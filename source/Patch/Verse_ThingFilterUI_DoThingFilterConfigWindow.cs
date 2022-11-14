using System.Collections.Generic;
using FilterManager.Core;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FilterManager.Patch
{
  [HarmonyPatch(typeof(ThingFilterUI), nameof(ThingFilterUI.DoThingFilterConfigWindow))]
  internal static class Verse_ThingFilterUI_DoThingFilterConfigWindow
  {
    private const float Offset = Gfx.ButtonHeight - 1f;

    [HarmonyPriority(0)]
    private static void Prefix(ref Rect rect, ThingFilter filter, ThingFilter parentFilter = null, IEnumerable<ThingDef> forceHiddenDefs = null, IEnumerable<SpecialThingFilterDef> forceHiddenFilters = null)
    {
      LastState.Update(rect, filter, parentFilter, forceHiddenDefs, forceHiddenFilters);
      Widgets.DrawMenuSection(rect);

      rect.yMin += Offset;
    }

    [HarmonyPostfix, HarmonyPriority(800)]
    private static void Postfix1(ref Rect rect) => rect.yMin -= Offset;

    [HarmonyPostfix, HarmonyPriority(0)]
    private static void Postfix2(ref Rect rect)
    {
      rect.yMin += Offset;

      Gfx.DrawButtons();
    }
  }
}
