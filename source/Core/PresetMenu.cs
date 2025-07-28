using System.Collections.Generic;
using Verse;

namespace FilterManager.Core;

internal static class PresetMenu
{
  public static void Draw(Preset preset)
  {
    var options = new List<FloatMenuOption>
    {
      new(preset.Name, null),
      new("FilterManager.Button.PresetMenu.UseInverse".TranslateSimple(), () => preset.Set(null, true, true)),

      new("FilterManager.Button.PresetMenu.CopyAllowed".TranslateSimple(), () => preset.Set(true, false, false)),
      new("FilterManager.Button.PresetMenu.CopyAllowedInverse".TranslateSimple(), () => preset.Set(true, true, false)),
      new("FilterManager.Button.PresetMenu.CopyForbidden".TranslateSimple(), () => preset.Set(false, false, false)),
      new("FilterManager.Button.PresetMenu.CopyForbiddenInverse".TranslateSimple(), () => preset.Set(false, true, false))
    };

    if (!preset.Integrated)
    {
      options.Add(new FloatMenuOption("FilterManager.Button.PresetMenu.Overwrite".TranslateSimple().Colorize(Gfx.CriticalColor), preset.Overwrite));
      options.Add(new FloatMenuOption("FilterManager.Button.PresetMenu.Delete".TranslateSimple().Colorize(Gfx.CriticalColor), preset.Delete));
    }

    Find.WindowStack!.Add(new FloatMenuUnsorted(options));
  }

  private sealed class FloatMenuUnsorted : FloatMenu
  {
    public FloatMenuUnsorted(List<FloatMenuOption> options) : base(options) => this.options = options;
  }
}
