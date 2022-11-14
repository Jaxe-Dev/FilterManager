using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace FilterManager.Core
{
  internal class Preset
  {
    public string Name { get; }

    public readonly Dictionary<SpecialThingFilterDef, bool> Filters;
    public readonly Dictionary<ThingDef, bool> Things;

    public readonly FloatRange? AllowedHitPointsPercents;
    public readonly QualityRange? AllowedQualityLevels;

    public readonly bool Integrated;

    public Preset(string name, Dictionary<SpecialThingFilterDef, bool> filters, Dictionary<ThingDef, bool> things)
    {
      Integrated = true;

      Name = $"<i>{name}</i>";

      Filters = filters ?? new Dictionary<SpecialThingFilterDef, bool>();
      Things = things ?? new Dictionary<ThingDef, bool>();
    }

    public Preset(string name, FloatRange? hitpoints, QualityRange? quality, Dictionary<SpecialThingFilterDef, bool> filters, Dictionary<ThingDef, bool> things)
    {
      Name = name;
      AllowedHitPointsPercents = hitpoints;
      AllowedQualityLevels = quality;
      Filters = filters;
      Things = things;
    }

    public Preset(string name)
    {
      Name = name;

      Filters = new Dictionary<SpecialThingFilterDef, bool>();
      Things = new Dictionary<ThingDef, bool>();

      try
      {
        AllowedHitPointsPercents = LastState.ActiveFilter.allowedHitPointsConfigurable ? LastState.ActiveFilter.AllowedHitPointsPercents : (FloatRange?) null;
        AllowedQualityLevels = LastState.ActiveFilter.allowedQualitiesConfigurable ? LastState.ActiveFilter.AllowedQualityLevels : (QualityRange?) null;

        Filters = LastState.GetFiltersAllowed();
        Things = LastState.GetThingsAllowed();
      }
      catch (Exception exception) { throw Mod.Exception("Error creating preset", exception); }
    }

    public void Set(bool? limit, bool invert)
    {
      PresetWindow.Instance.SetName(Integrated ? null : Name);

      if (AllowedHitPointsPercents != null && LastState.ActiveFilter.allowedHitPointsConfigurable) { LastState.ActiveFilter.AllowedHitPointsPercents = AllowedHitPointsPercents.Value; }
      if (AllowedQualityLevels != null && LastState.ActiveFilter.allowedQualitiesConfigurable) { LastState.ActiveFilter.AllowedQualityLevels = AllowedQualityLevels.Value; }

      LastState.SetAllowed(Filters);
      LastState.SetAllowed(Things, limit, invert);
    }

    public void Overwrite()
    {
      if (Integrated) { return; }

      PresetWindow.Instance.SetName(Name);
      Storage.AddPreset(Name);
    }

    public void Delete()
    {
      if (Integrated) { return; }

      PresetWindow.Instance.SetName();

      Storage.DeletePreset(this);
    }
  }
}
