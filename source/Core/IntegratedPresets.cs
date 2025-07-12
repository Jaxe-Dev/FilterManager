using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FilterManager.Core;

internal static class IntegratedPresets
{
  public static void Build()
  {
    Storage.AddIntegrated("FilterManager.IntegratedPreset.Degradable".Translate(), null, GetDegradable());
    Storage.AddIntegrated("FilterManager.IntegratedPreset.Rottable".Translate(), null, GetRottable());
  }

  private static Dictionary<ThingDef, bool> GetDegradable() => ThingCategoryNodeDatabase.RootNode!.catDef!.DescendantThingDefs.Distinct().ToDictionary(static def => def, static def => def.CanEverDeteriorate && def.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 0);
  private static Dictionary<ThingDef, bool> GetRottable() => ThingCategoryNodeDatabase.RootNode!.catDef!.DescendantThingDefs.Distinct().ToDictionary(static def => def, static def => def.HasComp(typeof(CompRottable)));
}
