using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FilterManager.Core;

public class Preset
{
  public string Name { get; }

  public Dictionary<ThingDef, bool> Things { get; }
  public Dictionary<SpecialThingFilterDef, bool> Filters { get; }

  public FloatRange? AllowedHitPointsPercents { get; }
  public QualityRange? AllowedQualityLevels { get; }

  public bool Integrated { get; }

  public Preset(string name, Dictionary<ThingDef, bool> things, Dictionary<SpecialThingFilterDef, bool> filters, FloatRange? hitpoints = null, QualityRange? quality = null)
  {
    Name = name;

    Things = things;
    Filters = filters;

    AllowedHitPointsPercents = hitpoints;
    AllowedQualityLevels = quality;
  }

  public Preset(string name, Dictionary<ThingDef, bool> things) : this(name, things, new Dictionary<SpecialThingFilterDef, bool>()) => Integrated = true;

  public static IEnumerable<Preset> BuildIntegrated() =>
  [
    BuildFromPredicate("FilterManager.IntegratedPreset.Degradable".TranslateSimple(), static def => def.CanEverDeteriorate && def.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 0),
    BuildFromPredicate("FilterManager.IntegratedPreset.Rottable".TranslateSimple(), static def => def.HasComp(typeof(CompRottable)))
  ];

  private static Preset BuildFromPredicate(string name, Func<ThingDef, bool> predicate) => new(name, ThingCategoryNodeDatabase.RootNode!.catDef!.DescendantThingDefs.Distinct().Where(predicate).ToDictionary(static def => def, static _ => true));

  public void Apply(bool? target, bool invert, bool? fallback, bool setName)
  {
    if (setName) { PresetWindow.FocusName(Name); }
    FilterWindow.Apply(this, target, invert, fallback);
  }

  public void Overwrite()
  {
    if (Integrated) { throw new Exception("Tried to overwrite integrated preset"); }

    Storage.SavePreset(Name);
  }

  public void Delete()
  {
    if (Integrated) { return; }

    Storage.DeletePreset(this);
  }
}
