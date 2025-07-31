using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace FilterManager.Core;

public class PresetWindow : Window
{
  private const float Width = 360f;

  private const float SectionPadding = 8f;
  private const float ScrollbarWidth = 20f;

  private const string NameEntryControl = Mod.Id + ".NameEntry";
  private const int NameEntryMaxLength = 32;

  private const float SideWidth = 60f;

  public static PresetWindow? Instance;

  private static readonly Regex NameEntryRegex = new("^[^*<>[\\]]*$");
  private static bool _focusNameEntry = true;

  private static Vector2 _scrollPosition;
  private static Rect _scrollRect;

  private static Preset? _matched;

  private static string? _nameEntry;

  public static bool StayOpen { get; set; }

  public PresetWindow()
  {
    doCloseX = true;
    layer = WindowLayer.SubSuper;

    Instance = this;

    _nameEntry = null;
    _focusNameEntry = true;

    _matched = null;
  }

  public static void FocusName(string? name)
  {
    _nameEntry = name;
    _focusNameEntry = true;
  }

  private static void Save() => Storage.SavePreset(_nameEntry!);

  private static void DrawTop(Rect rect)
  {
    Widgets.Label(new Rect(rect.x, rect.y, SideWidth, rect.height), "FilterManager.Preset".TranslateSimple());

    GUI.SetNextControlName(NameEntryControl);
    var name = Widgets.TextField(new Rect(rect.x + SideWidth, rect.y, rect.width - (SideWidth * 2f), Text.LineHeight), _nameEntry, NameEntryMaxLength, NameEntryRegex) ?? "";
    if (name.Length <= NameEntryMaxLength) { _nameEntry = name; }

    if (_focusNameEntry)
    {
      GUI.FocusControl(NameEntryControl);
      var editor = GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl) as TextEditor;
      editor?.MoveTextEnd();
      editor?.SelectNone();

      _focusNameEntry = false;
    }

    if (Gfx.DrawButton(new Rect(rect.xMax - SideWidth, rect.y, SideWidth, Text.LineHeight), "FilterManager.Save".TranslateSimple(), Storage.CanSave(_nameEntry))) { Save(); }
  }

  private static void DrawPresets(Rect rect)
  {
    if (_scrollRect == default) { _scrollRect = new Rect(rect.x, rect.y + Text.LineHeight + SectionPadding, rect.width, 99999f); }

    var buttonWidth = _scrollRect.width - SideWidth;
    var y = 0f;

    rect.xMax += ScrollbarWidth;
    Widgets.BeginScrollView(rect, ref _scrollPosition, _scrollRect);

    var fullMatch = (bool?)null;
    foreach (var preset in Storage.Presets)
    {
      var match = FilterWindow.MatchesPreset(preset);
      var lastColor = GUI.color;
      GUI.color = GetMatchColor(match) ?? lastColor;

      if (match is MatchResult.Full or MatchResult.Partial)
      {
        if (fullMatch is null)
        {
          if (_matched != preset) { FocusName(preset.Name); }
          _matched = preset;
          fullMatch = match is MatchResult.Full;
        }
      }
      else if (!preset.Integrated && _nameEntry == preset.Name) { GUI.color = Gfx.CriticalColor; }

      if (Gfx.DrawButton(new Rect(_scrollRect.x, _scrollRect.y + y, buttonWidth, Text.LineHeight), preset.Integrated ? $"<i>{preset.Name}</i>" : preset.Name))
      {
        if (match == MatchResult.NotApplicable) { PresetMenu.Draw(preset, true); }
        else { preset.Apply(null, false, false, true); }
      }

      if (Gfx.DrawButton(new Rect(_scrollRect.xMax - SideWidth, _scrollRect.y + y, SideWidth, Text.LineHeight), "...")) { PresetMenu.Draw(preset, match is MatchResult.NotApplicable); }
      GUI.color = lastColor;

      y += Text.LineHeight;
    }

    Widgets.EndScrollView();

    _scrollRect.height = y;
  }

  private static Color? GetMatchColor(MatchResult match) => match switch
  {
    MatchResult.Full => Gfx.SelectedColor,
    MatchResult.Partial => Gfx.PartialColor,
    MatchResult.NotApplicable => Gfx.InactiveColor,
    _ => null
  };

  protected override void SetInitialSizeAndPosition()
  {
    var lastRect = FilterWindow.Rect;
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

  public enum MatchResult
  {
    NotApplicable,
    None,
    Partial,
    Full
  }
}
