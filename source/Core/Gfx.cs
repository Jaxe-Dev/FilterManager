using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FilterManager.Core
{
  internal static class Gfx
  {
    private const float ButtonHeight = 24f;
    private const float ButtonPadding = 3f;

    public const float OffsetHeight = ButtonHeight;

    public static bool DrawButton(Rect rect, string label, bool active = true)
    {
      var color = GUI.color;
      if (!active) { GUI.color = Color.gray; }
      var clicked = Widgets.ButtonText(rect, label);
      GUI.color = color;

      if (!active || !clicked) { return false; }
      SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();

      return true;
    }

    public static void DrawButtons()
    {
      Widgets.DrawBoxSolid(LastState.Rect.ContractedBy(1f * Prefs.UIScale).TopPartPixels(ButtonHeight + ButtonPadding).Rounded(), Widgets.MenuSectionBGFillColor);
      var buttonRect = new Rect(LastState.Rect.x + ButtonPadding, LastState.Rect.y + ButtonPadding, ((LastState.Rect.width - 2f - ButtonPadding) / 2f) - ButtonPadding, ButtonHeight);

      if (DrawButton(buttonRect, "FilterManager.Button.Invert".Translate())) { LastState.InvertSelection(); }
      if (!DrawButton(new Rect(buttonRect.xMax + ButtonPadding, buttonRect.y, buttonRect.width, buttonRect.height), "FilterManager.Button.Presets".Translate())) { return; }

      if (PresetWindow.Instance?.IsOpen ?? false) { PresetWindow.Instance.Close(); }
      else { Find.WindowStack!.Add(new PresetWindow()); }
    }
  }
}
