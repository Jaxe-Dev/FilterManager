using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace FilterManager.Core;

internal static class Gfx
{
  public const float OffsetHeight = ButtonHeight;
  public const float Padding = 3f;

  private const float ButtonHeight = 24f;

  public static readonly Color SelectedColor = new(0.35f, 1f, 0.3f);
  public static readonly Color CriticalColor = new(1f, 0.3f, 0.35f);

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
    Widgets.DrawBoxSolid(LastState.Rect.ContractedBy(1f * Prefs.UIScale).TopPartPixels(ButtonHeight + Padding).Rounded(), Widgets.MenuSectionBGFillColor);
    var buttonRect = new Rect(LastState.Rect.x + Padding, LastState.Rect.y + Padding, ((LastState.Rect.width - 2f - Padding) / 2f) - Padding, ButtonHeight);

    if (DrawButton(buttonRect, "FilterManager.Button.Invert".TranslateSimple())) { LastState.InvertSelection(); }

    if (!DrawButton(new Rect(buttonRect.xMax + Padding, buttonRect.y, buttonRect.width, buttonRect.height), "FilterManager.Button.Presets".TranslateSimple())) { return; }

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
