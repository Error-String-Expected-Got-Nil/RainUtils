using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleMultipleEnumeration

namespace RainUtils.LocalArmor
{
    [HarmonyPatch(typeof(ArmorUtility))]
    public class Patch_ArmorUtility
    {
        private static readonly MethodInfo ApplyArmorMethod = AccessTools.Method(typeof(ArmorUtility), 
            "ApplyArmor");

        private static readonly MethodInfo CheckLocalArmorMethod = AccessTools.Method(typeof(Patch_ArmorUtility),
            nameof(CheckLocalArmor));
        
        [HarmonyPatch(nameof(ArmorUtility.GetPostArmorDamage))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Patch_GetPostArmorDamage(IEnumerable<CodeInstruction> instructions, 
            ILGenerator generator)
        {
            // When I was doing Rain World modding, tutorials always told you to use this neat thing that BepInEx
            // modding had called an ILCursor when making a transpiler. It was quite helpful, and I was sure that
            // Harmony must have an equivalent- and it does, the CodeMatcher. I would have remade it myself if it
            // didn't already exist. Very handy tool.
            var cursor = new CodeMatcher(instructions, generator);
            
            // Transpiler procedure:
            // 1 - Seek to end of method
            // 2 - Find first call of ApplyArmor going backwards. This is usually where natural armor is applied.
            // 3 - Create label on first instruction afterwards
            // 4 - Seek to first instruction for loading arguments to the ApplyArmor call
            // 5 - Insert instructions to load all arguments for CheckLocalArmor and call it
            // 6 - Insert branch on true to skip the original ApplyArmor call if it was already handled
            cursor
                .End() // 1
                .SearchBackwards(inst => inst.Calls(ApplyArmorMethod)) // 2
                .Advance(1)
                .CreateLabel(out var postApplyArmor) // 3
                .Advance(-12); // 4

            if (cursor.IsValid)
                return cursor
                    .Insert(
                        CodeInstruction.LoadArgument(1, true), // Damage amount
                        CodeInstruction.LoadArgument(2), // Armor penetration
                        CodeInstruction.LoadArgument(4), // DamageDef
                        CodeInstruction.LoadArgument(0), // Pawn
                        CodeInstruction.LoadLocal(6, true), // Metal armor bool
                        CodeInstruction.LoadArgument(3), // BodyPartRecord
                        new CodeInstruction(OpCodes.Call, CheckLocalArmorMethod), // 5
                        new CodeInstruction(OpCodes.Brtrue, postApplyArmor) // 6
                    )
                    .Instructions();
            
            Log.Error("[RainUtils] - Error in Patch_ArmorUtility transpiler! Invalid cursor position. Using " +
                      "original method with no changes instead.");
            return instructions;
        }
        
        // Returns true if there was relevant local armor, false if there was not. Calculates post-armor damage if
        // there was.
        public static bool CheckLocalArmor(ref float damageAmount, float armorPen, ref DamageDef damageDef, Pawn pawn, 
            ref bool metalArmor, BodyPartRecord part)
        {
            if (!(pawn.GetComp<ThingComp_LocalArmor>()?.ArmoredParts.TryGetValue(part.def, out var info) ?? false))
                return false;

            float? armorRating = null;
            if (damageDef.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Sharp)
                armorRating = info.armorSharp;
            else if (damageDef.armorCategory.armorRatingStat == StatDefOf.ArmorRating_Blunt)
                armorRating = info.armorBlunt;

            if (armorRating == null) return false;

            var arguments = new object[] { damageAmount, armorPen, armorRating, null, damageDef, pawn, metalArmor };
            ApplyArmorMethod.Invoke(null, arguments);

            damageAmount = (float)arguments[0];
            damageDef = (DamageDef)arguments[4];
            metalArmor = (bool)arguments[6];
            
            return true;
        }
    }
}