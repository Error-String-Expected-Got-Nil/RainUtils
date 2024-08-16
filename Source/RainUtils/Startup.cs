using System.Reflection;
using HarmonyLib;
using Verse;

namespace RainUtils
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup()
        {
            Harmony.DEBUG = false;
            var harmony = new Harmony("RainUtils");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}