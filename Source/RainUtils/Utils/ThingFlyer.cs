using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace RainUtils.Utils;

public static class ThingFlyer
{
    // Makes a PawnFlyer with a generic Thing held in it instead of a pawn. Does not save pawn information.
    // Unfortunately has to use reflection to access some fields since they're private/protected.
    public static PawnFlyer MakeThingFlyer(ThingDef flyingDef, Thing thing, IntVec3 destCell, Vector3? startPos = null, 
        EffecterDef flightEffecterDef = null, SoundDef landingSound = null)
    {
        var flyer = (PawnFlyer)ThingMaker.MakeThing(flyingDef);
        var traverse = Traverse.Create(flyer);
        traverse.Field("startVec").SetValue(startPos ?? thing.TrueCenter());
        traverse.Field("destCell").SetValue(destCell);
        traverse.Field("flightDistance").SetValue(thing.Position.DistanceTo(destCell));
        traverse.Field("flightEffecterDef").SetValue(flightEffecterDef);
        traverse.Field("soundLanding").SetValue(landingSound);
        flyer.Rotation = thing.Rotation;
        
        if (thing.Spawned) thing.DeSpawn(DestroyMode.WillReplace);
        if (traverse.Field("innerContainer").GetValue<ThingOwner<Thing>>().TryAdd(thing)) return flyer;
        
        Log.Error("[RainUtils] - Failed to add Thing " + thing.ToStringSafe() + " to a flyer, destroying.");
        thing.Destroy();

        return flyer;
    }
}