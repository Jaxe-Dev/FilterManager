using System.Collections.Generic;
using FilterManager.Core;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FilterManager.Patch;

[HarmonyPatch(typeof(ThingFilterUI), nameof(ThingFilterUI.DoThingFilterConfigWindow))]
public static class Verse_ThingFilterUI_DoThingFilterConfigWindow
{
  [HarmonyPriority(0)]
  private static void Prefix(ref Rect rect, ThingFilter filter, ThingFilter? parentFilter = null, IEnumerable<ThingDef>? forceHiddenDefs = null, IEnumerable<SpecialThingFilterDef>? forceHiddenFilters = null)
  {
    FilterWindow.Update(rect, filter, parentFilter, forceHiddenDefs, forceHiddenFilters);
    Widgets.DrawMenuSection(rect);

    rect.yMin += Text.LineHeight;

    PresetWindow.StayOpen = true;
  }

  [HarmonyPostfix, HarmonyPriority(800)]
  private static void Postfix1(ref Rect rect) => rect.yMin -= Text.LineHeight;

  [HarmonyPostfix, HarmonyPriority(0)]
  private static void Postfix2(ref Rect rect)
  {
    rect.yMin += Text.LineHeight;

    Gfx.DrawButtons();
  }
}
