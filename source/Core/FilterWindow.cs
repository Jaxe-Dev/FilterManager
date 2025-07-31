using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace FilterManager.Core;

public static class FilterWindow
{
  private static ThingFilter _active = null!;
  private static ThingFilter? _parent;

  private static ThingCategoryDef _rootCategory = null!;

  private static IEnumerable<ThingDef>? _forceHiddenDefs;
  private static IEnumerable<SpecialThingFilterDef>? _forceHiddenFilters;

  public static Rect Rect { get; private set; }

  public static void Update(Rect rect, ThingFilter activeFilter, ThingFilter? parentFilter, IEnumerable<ThingDef>? forceHiddenDefs, IEnumerable<SpecialThingFilterDef>? forceHiddenFilters)
  {
    Rect = rect;

    _active = activeFilter;
    _parent = parentFilter;

    _rootCategory = (parentFilter?.DisplayRootCategory ?? activeFilter.RootNode!).catDef;

    _forceHiddenDefs = forceHiddenDefs;
    _forceHiddenFilters = forceHiddenFilters;
  }

  public static void Apply(Preset preset, bool? target, bool invert, bool? fallback)
  {
    if (fallback is true || (invert && fallback is false)) { _active.SetAllowAll(_parent); }
    else if (fallback is false) { _active.SetDisallowAll(); }

    if (_parent is null)
    {
      if (preset.AllowedHitPointsPercents is not null) { _active.AllowedHitPointsPercents = preset.AllowedHitPointsPercents.Value; }
      if (preset.AllowedQualityLevels is not null) { _active.AllowedQualityLevels = preset.AllowedQualityLevels.Value; }
    }
    else
    {
      if (_parent.allowedHitPointsConfigurable) { _active.AllowedHitPointsPercents = preset.AllowedHitPointsPercents ?? _parent.AllowedHitPointsPercents; }
      if (_parent.allowedQualitiesConfigurable) { _active.AllowedQualityLevels = preset.AllowedQualityLevels ?? _parent.AllowedQualityLevels; }
    }

    if (target is null)
    {
      foreach (var entry in preset.Filters) { _active.SetAllow(entry.Key, entry.Value); }
    }

    foreach (var entry in preset.Things.Where(entry => target is null || entry.Value == target.Value)) { _active.SetAllow(entry.Key, invert != entry.Value); }
  }

  public static void Invert()
  {
    foreach (var def in GetThings()) { _active.SetAllow(def, !_active.Allows(def)); }
  }

  public static PresetWindow.MatchResult MatchesPreset(Preset preset)
  {
    var things = 0;
    var defined = 0;
    var invalid = 0;

    var thingDefs = GetThings();
    foreach (var def in thingDefs)
    {
      if (preset.Things.TryGetValue(def, out var presetValue)) { defined++; }

      var activeValue = _active.Allows(def);
      if (activeValue != presetValue) { invalid++; }
      things++;
    }

    if ((defined == 0 && preset.Things.Count > 0) || (preset.Integrated && defined == things)) { return PresetWindow.MatchResult.NotApplicable; }
    if (preset.Things.Count > defined && !preset.Integrated) { return PresetWindow.MatchResult.Partial; }

    if (invalid > 0) { return PresetWindow.MatchResult.None; }

    if (GetSpecialFilters().Any(def => preset.Filters.GetValueOrDefault(def, true) != _active.Allows(def))) { return PresetWindow.MatchResult.Partial; }

    var hitpointsValid = !(_parent?.allowedHitPointsConfigurable ?? true) || (preset.AllowedHitPointsPercents is null && _parent?.AllowedHitPointsPercents == _active.AllowedHitPointsPercents) || preset.AllowedHitPointsPercents == _active.AllowedHitPointsPercents;
    var qualityValid = !(_parent?.allowedQualitiesConfigurable ?? true) || (preset.AllowedQualityLevels is null && _parent?.AllowedQualityLevels == _active.AllowedQualityLevels) || preset.AllowedQualityLevels == _active.AllowedQualityLevels;

    return hitpointsValid && qualityValid ? PresetWindow.MatchResult.Full : PresetWindow.MatchResult.Partial;
  }

  public static Preset CreatePreset(string name) => new(name, GetThingDictionary(), GetSpecialFilterDictionary(), GetHitpoints(), GetQuality());

  private static IEnumerable<ThingDef> GetThings() => _rootCategory.DescendantThingDefs.Distinct().Where(static def => !_forceHiddenDefs?.Contains(def) ?? true);
  private static Dictionary<ThingDef, bool> GetThingDictionary() => GetThings().Where(static def => _active.Allows(def)).ToDictionary(static def => def, static _ => true);

  private static IEnumerable<SpecialThingFilterDef> GetSpecialFilters() => _rootCategory.DescendantSpecialThingFilterDefs.Distinct().Where(static def => def.configurable && (!_forceHiddenFilters?.Contains(def) ?? true));
  private static Dictionary<SpecialThingFilterDef, bool> GetSpecialFilterDictionary() => GetSpecialFilters().Where(static def => !_active.Allows(def)).ToDictionary(static def => def, static _ => false);

  private static FloatRange? GetHitpoints() => (_parent?.allowedHitPointsConfigurable ?? true) && _parent?.AllowedHitPointsPercents != _active.AllowedHitPointsPercents ? _active.AllowedHitPointsPercents : null;
  private static QualityRange? GetQuality() => (_parent?.allowedQualitiesConfigurable ?? true) && _parent?.AllowedQualityLevels != _active.AllowedQualityLevels ? _active.AllowedQualityLevels : null;
}
