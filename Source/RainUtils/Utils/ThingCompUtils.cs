using System;
using System.Collections.Generic;
using HarmonyLib;
using Verse;

namespace RainUtils.Utils;

// Utilities for hot-adding/removing a ThingComp from a ThingWithComps. You generally should not be doing this, but if
// you ever have a genuine need for it, these methods should allow it to be done safely. Uses reflection in some cases
// though, so don't do it frequently.
public static class ThingCompUtils
{
    public static void AddThingComp(this ThingWithComps thing, Type compType, CompProperties properties)
    {
        var comp = InstantiateThingComp(thing, compType, properties);
        
        if (!thing.AllComps.Empty())
        {
            thing.AllComps.Add(comp);
            return;
        }

        var comps = new List<ThingComp> { comp };
        Traverse.Create(thing).Field("comps").SetValue(comps);
    }

    public static void AddThingComp<T>(this ThingWithComps thing, CompProperties properties) where T : ThingComp
    {
        AddThingComp(thing, typeof(T), properties);
    }

    public static void RemoveThingComp(this ThingWithComps thing, ThingComp comp)
    {
        thing.AllComps.Remove(comp);
        if (thing.AllComps.Empty())
            Traverse.Create(thing).Field("comps").SetValue(null);
    }

    // Does nothing if target thing has no comp of given type.
    public static void RemoveThingComp<T>(this ThingWithComps thing) where T : ThingComp
    {
        var comp = thing.GetComp<T>();
        if (comp != null)
            RemoveThingComp(thing, comp);
    }

    public static ThingComp InstantiateThingComp(ThingWithComps parent, Type compType, CompProperties properties)
    {
        var comp = (ThingComp) Activator.CreateInstance(compType);
        comp.parent = parent;
        comp.Initialize(properties);
        return comp;
    }

    public static ThingComp InstantiateThingComp<T>(ThingWithComps parent, CompProperties properties)
        where T : ThingComp
    {
        return InstantiateThingComp(parent, typeof(T), properties);
    }
}