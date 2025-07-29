using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FilterManager.Core;

public static class Gfx
{
  private const float Padding = 3f;

  private const float ButtonHeight = 24f;

  public static readonly Color InactiveColor = new(0.5f, 0.5f, 0.5f);
  public static readonly Color SelectedColor = new(0.35f, 1f, 0.3f);
  public static readonly Color PartialColor = new(1f, 0.7f, 0.55f);
  public static readonly Color CriticalColor = new(1f, 0.3f, 0.35f);

  public static bool DrawButton(Rect rect, string label, bool active = true)
  {
    var lastColor = GUI.color;
    if (!active) { GUI.color = InactiveColor; }
    var clicked = Widgets.ButtonText(rect, label);
    GUI.color = lastColor;

    if (!active || !clicked) { return false; }
    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();

    return true;
  }

  public static void DrawButtons()
  {
    Widgets.DrawBoxSolid(FilterWindow.Rect.ContractedBy(1f * Prefs.UIScale).TopPartPixels(ButtonHeight + Padding).Rounded(), Widgets.MenuSectionBGFillColor);
    var buttonRect = new Rect(FilterWindow.Rect.x + Padding, FilterWindow.Rect.y + Padding, ((FilterWindow.Rect.width - 2f - Padding) / 2f) - Padding, ButtonHeight);

    if (DrawButton(buttonRect, "FilterManager.Invert".TranslateSimple())) { FilterWindow.Invert(); }

    if (!DrawButton(new Rect(buttonRect.xMax + Padding, buttonRect.y, buttonRect.width, buttonRect.height), "FilterManager.Presets".TranslateSimple())) { return; }

    if (PresetWindow.Instance?.IsOpen ?? false) { PresetWindow.Instance.Close(); }
    else { Find.WindowStack!.Add(new PresetWindow()); }
  }

  public static void ShowConfirmDialog(string text, Action action)
  {
    Find.WindowStack!.Add(new Dialog_Confirm(text, action));
  }

  private class Dialog_Confirm : Dialog_MessageBox
  {
    public override Vector2 InitialSize => new(400f, 120f);

    public Dialog_Confirm(string text, Action action) : base(text, "Yes".TranslateSimple(), action, "No".TranslateSimple(), buttonADestructive: true)
    { }
  }
}
