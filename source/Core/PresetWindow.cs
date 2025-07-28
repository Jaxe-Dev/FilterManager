using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace FilterManager.Core;

internal class PresetWindow : Window
{
  private const float Width = 360f;

  private const float SectionPadding = 8f;
  private const float ScrollbarWidth = 20f;

  private const string NameEntryControl = Mod.Id + ".NameEntry";
  private const float NameEntryLabelWidth = 60f;
  private const int NameEntryMaxLength = 32;

  private const float RightButtonWidth = 60f;

  public static PresetWindow? Instance;

  private static readonly Regex NameEntryRegex = new("^[^*<>[\\]]*$");

  public static bool StayOpen { get; set; }

  private Vector2 _scrollPosition;
  private Rect _scrollRect;

  private string? _nameEntry;
  private bool _focusNameEntry = true;

  private Preset? _matched;

  public PresetWindow()
  {
    Instance = this;

    doCloseX = true;
    layer = WindowLayer.SubSuper;
  }

  protected override void SetInitialSizeAndPosition()
  {
    var lastRect = LastState.Rect;
    lastRect.position /= Prefs.UIScale;

    var rect = GUIUtility.GUIToScreenRect(lastRect);
    windowRect = new Rect(rect.xMax + GenUI.GapTiny, rect.y, Width, rect.height).Rounded();
  }

  public override void WindowUpdate()
  {
    base.WindowUpdate();
    if (!StayOpen)
    {
      Close();
      return;
    }

    StayOpen = false;
  }

  public override void DoWindowContents(Rect rect)
  {
    var font = Text.Font;
    Text.Font = GameFont.Tiny;

    rect.xMax -= ScrollbarWidth;
    rect.yMin += SectionPadding;

    DrawTop(rect.TopPartPixels(Text.LineHeight));
    rect.yMin += Text.LineHeight + SectionPadding;

    DrawPresets(rect);

    Text.Font = font;
  }

  private void DrawTop(Rect rect)
  {
    Widgets.Label(new Rect(rect.x, rect.y, NameEntryLabelWidth, rect.height), "FilterManager.Preset.Label".TranslateSimple());

    GUI.SetNextControlName(NameEntryControl);
    var nameEntry = Widgets.TextField(new Rect(rect.x + NameEntryLabelWidth, rect.y, rect.width - (NameEntryLabelWidth + RightButtonWidth), Text.LineHeight), _nameEntry, NameEntryMaxLength, NameEntryRegex) ?? "";
    if (nameEntry.Length <= NameEntryMaxLength) { _nameEntry = nameEntry; }

    if (_focusNameEntry)
    {
      GUI.FocusControl(NameEntryControl);
      (GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor)?.MoveTextEnd();

      _focusNameEntry = false;
    }

    if (Gfx.DrawButton(new Rect(rect.xMax - RightButtonWidth, rect.y, RightButtonWidth, Text.LineHeight), "FilterManager.Button.Save".TranslateSimple(), !string.IsNullOrWhiteSpace(_nameEntry))) { Storage.SavePreset(_nameEntry!); }
  }

  private void DrawPresets(Rect rect)
  {
    if (_scrollRect == default) { _scrollRect = new Rect(rect.x, rect.y + Text.LineHeight + SectionPadding, rect.width, 99999f); }

    var buttonWidth = _scrollRect.width - RightButtonWidth;
    var y = 0f;

    rect.xMax += ScrollbarWidth;
    Widgets.BeginScrollView(rect, ref _scrollPosition, _scrollRect);

    var selected = false;
    foreach (var preset in Storage.Presets.ToArray())
    {
      var color = GUI.color;
      if (LastState.MatchesPreset(preset))
      {
        GUI.color = Gfx.SelectedColor;

        if (!selected)
        {
          if (_matched != preset) { _nameEntry = preset.Integrated ? null : preset.Name; }
          _matched = preset;
          selected = true;
        }
      }
      else if (!preset.Integrated && _nameEntry == preset.Name) { GUI.color = Gfx.CriticalColor; }

      if (Gfx.DrawButton(new Rect(_scrollRect.x, _scrollRect.y + y, buttonWidth, Text.LineHeight), preset.Integrated ? $"<i>{preset.Name}</i>" : preset.Name)) { preset.Set(null, false, true); }
      if (Gfx.DrawButton(new Rect(_scrollRect.xMax - RightButtonWidth, _scrollRect.y + y, RightButtonWidth, Text.LineHeight), "...")) { PresetMenu.Draw(preset); }

      GUI.color = color;

      y += Text.LineHeight;
    }

    Widgets.EndScrollView();

    _scrollRect.height = y;
  }

  public void SetName(string? name = null)
  {
    _nameEntry = name ?? null;
    _focusNameEntry = true;
  }
}
