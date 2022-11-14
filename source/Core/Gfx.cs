using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FilterManager.Core
{
  internal static class Gfx
  {
    public const float ButtonHeight = 24f;

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
      var rect = LastState.Rect.ContractedBy(1f);

      var font = Text.Font;
      Text.Font = GameFont.Tiny;

      var buttonWidth = rect.width / 2f;

      if (DrawButton(new Rect(rect.x, rect.y, buttonWidth, ButtonHeight), "FilterManager.Button.Invert".Translate())) { LastState.InvertSelection(); }
      if (DrawButton(new Rect(rect.xMax - buttonWidth, rect.y, buttonWidth, ButtonHeight), "FilterManager.Button.Presets".Translate()))
      {
        if (PresetWindow.Instance?.IsOpen ?? false) { PresetWindow.Instance.Close(); }
        else { Find.WindowStack.Add(new PresetWindow()); }
      }

      Text.Font = font;
    }
  }
}
