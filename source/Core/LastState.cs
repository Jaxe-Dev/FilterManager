﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FilterManager.Core;

internal static class LastState
{
  private static TreeNode_ThingCategory _node = null!;

  private static IEnumerable<ThingDef>? _forceHiddenDefs;
  private static IEnumerable<SpecialThingFilterDef>? _forceHiddenFilters;

  public static DateTime Time { get; private set; }
  public static Rect Rect { get; private set; }

  public static ThingFilter ActiveFilter { get; private set; } = null!;
  public static ThingFilter? ParentFilter { get; set; }

  public static void Update(Rect rect, ThingFilter activeFilter, ThingFilter? parentFilter, IEnumerable<ThingDef>? forceHiddenDefs, IEnumerable<SpecialThingFilterDef>? forceHiddenFilters)
  {
    Time = DateTime.Now;

    Rect = rect;

    ActiveFilter = activeFilter;
    ParentFilter = parentFilter ?? activeFilter;

    _node = ParentFilter.DisplayRootCategory ?? ThingCategoryNodeDatabase.RootNode;

    _forceHiddenDefs = forceHiddenDefs;
    _forceHiddenFilters = forceHiddenFilters;
  }

  public static void InvertSelection()
  {
    if (ParentFilter is null) { return; }

    foreach (var def in ParentFilter.DisplayRootCategory!.catDef!.DescendantThingDefs!) { ActiveFilter.SetAllow(def, !ActiveFilter.Allows(def)); }
  }

  public static Dictionary<SpecialThingFilterDef, bool> GetFiltersAllowed() => _node.catDef!.ParentsSpecialThingFilterDefs.Concat(_node.catDef.DescendantSpecialThingFilterDefs).Distinct().Where(Visible).Distinct().ToDictionary(static def => def, static def => ActiveFilter.Allows(def));
  public static Dictionary<ThingDef, bool> GetThingsAllowed() => _node.catDef!.DescendantThingDefs.Distinct().Where(Visible).ToDictionary(static def => def, static def => ActiveFilter.Allows(def));

  public static void SetAllowed(Dictionary<SpecialThingFilterDef, bool> dictionary)
  {
    foreach (var entry in ParentFilter!.DisplayRootCategory!.catDef!.DescendantSpecialThingFilterDefs.SelectMany(def => dictionary.Where(entry => entry.Key == def))) { ActiveFilter.SetAllow(entry.Key, entry.Value); }
  }

  public static void SetAllowed(Dictionary<ThingDef, bool> dictionary, bool? limit, bool invert)
  {
    if (ParentFilter is null) { return; }

    foreach (var entry in from def in ParentFilter.DisplayRootCategory!.catDef!.DescendantThingDefs! from entry in dictionary.Where(entry => entry.Key == def) where limit == null || entry.Value == limit select entry) { ActiveFilter.SetAllow(entry.Key, entry.Value != invert); }
  }

  private static bool Visible(SpecialThingFilterDef def)
  {
    if (ParentFilter?.Allows(def) is false) { return false; }

    var filters = _node.catDef!.ParentsSpecialThingFilterDefs.Concat(_node.catDef.DescendantSpecialThingFilterDefs);
    var things = ParentFilter == null ? _node.catDef.DescendantThingDefs : _node.catDef.DescendantThingDefs.Where(ParentFilter.Allows);

    return (from filter in filters let hasThings = things.Any(thingDef => filter.Worker.CanEverMatch(thingDef)) where !hasThings select filter).Concat(_forceHiddenFilters ?? []).All(filter => filter != def);
  }

  private static bool Visible(ThingDef def) => def.PlayerAcquirable && (_forceHiddenDefs == null || !_forceHiddenDefs.Contains(def)) && (ParentFilter == null || (ParentFilter.Allows(def) && !ParentFilter.IsAlwaysDisallowedDueToSpecialFilters(def)));
}
