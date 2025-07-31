using System.Collections.Generic;
using Verse;

namespace FilterManager.Core;

public static class PresetMenu
{
  public static void Draw(Preset preset, bool unused)
  {
    if (unused)
    {
      Find.WindowStack!.Add(new FloatMenuUnsorted([new FloatMenuOption("FilterManager.NotApplicable".Translate(preset.Name), null)]));
      return;
    }

    var options = new List<FloatMenuOption>
    {
      new(preset.Name, null),
      new("FilterManager.Use".TranslateSimple(), () => preset.Apply(null, false, false, true)),
      new("FilterManager.UseInverse".TranslateSimple(), () => preset.Apply(null, true, true, true)),
      new("FilterManager.CheckAllowedOnly".TranslateSimple(), () => preset.Apply(true, false, null, false)),
      new("FilterManager.UncheckAllowedOnly".TranslateSimple(), () => preset.Apply(true, true, null, false))
    };

    if (!preset.Integrated)
    {
      options.Add(new FloatMenuOption("FilterManager.Overwrite".TranslateSimple().Colorize(Gfx.CriticalColor), preset.Overwrite));
      options.Add(new FloatMenuOption("FilterManager.Delete".TranslateSimple().Colorize(Gfx.CriticalColor), preset.Delete));
    }

    Find.WindowStack!.Add(new FloatMenuUnsorted(options));
  }

  private sealed class FloatMenuUnsorted : FloatMenu
  {
    public FloatMenuUnsorted(List<FloatMenuOption> options) : base(options) => this.options = options;
  }
}
