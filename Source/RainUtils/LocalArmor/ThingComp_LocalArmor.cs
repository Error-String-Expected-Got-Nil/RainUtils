using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

// ReSharper disable InconsistentNaming

namespace RainUtils.LocalArmor
{
    public class ThingComp_LocalArmor : ThingComp
    {
        public readonly Dictionary<BodyPartDef, LocalArmorInfo> ArmoredParts = 
            new Dictionary<BodyPartDef, LocalArmorInfo>();
        
        public override void Initialize(CompProperties properties)
        {
            props = properties;

            foreach (var info in ((CompProperties_LocalArmor)props).localArmorInfo
                     .Where(info => info.targetPart != null))
            {
                // Silently catch any errors for a key already existing since that's already warned about by the
                // config errors.
                try
                {
                    ArmoredParts.Add(info.targetPart, info);
                }
                catch (ArgumentException) { }
            }
        }
    }
    
    public class CompProperties_LocalArmor : CompProperties
    {
        public List<LocalArmorInfo> localArmorInfo = new List<LocalArmorInfo>();

        public CompProperties_LocalArmor()
        {
            compClass = typeof(ThingComp_LocalArmor);
        }

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (var configError in base.ConfigErrors(parentDef))
                yield return configError;

            if (parentDef.thingClass != typeof(Pawn))
            {
                yield return parentDef.defName + " has LocalArmor comp but is not a pawn";
                yield break;
            }
            
            if (localArmorInfo.Empty())
            {
                yield return parentDef.defName + " has LocalArmor comp with no defined localArmorInfo";
                yield break;
            }

            var seenParts = new List<BodyPartDef>();
            foreach (var info in localArmorInfo)
            {
                if (info.targetPart == null)
                    yield return parentDef.defName + " has LocalArmor comp with LocalArmorInfo with null targetPart";
                else if (seenParts.Contains(info.targetPart))
                    yield return parentDef.defName + " has LocalArmor comp with LocalArmorInfo with targetPart that " +
                                 "has already defined in a previous LocalArmorInfo";
                else
                    seenParts.Add(info.targetPart);
            }
        }
    }

    public class LocalArmorInfo
    {
        public BodyPartDef targetPart = null;
        public float armorSharp = 0f;
        public float armorBlunt = 0f;
    }
}