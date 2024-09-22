using System.Reflection;
using HarmonyLib;
using Verse;

namespace RainUtils.Utils;

public static class PawnUtils
{
    private static readonly FieldInfo JittererField = AccessTools.Field(typeof(Pawn_DrawTracker), "jitterer");
    
    public static JitterHandler GetJitterer(this Pawn_DrawTracker drawTracker)
    {
        return (JitterHandler)JittererField.GetValue(drawTracker);
    }
}