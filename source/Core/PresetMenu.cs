using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FilterManager.Core
{
  internal static class PresetMenu
  {
    public static void Draw(Preset preset)
    {
      var options = new List<FloatMenuOption>
      {
        new FloatMenuOption(preset.Name, null),
        new FloatMenuOption("FilterManager.Button.PresetMenu.Apply".Translate(), () => preset.Set(null, false)),
        new FloatMenuOption("FilterManager.Button.PresetMenu.ApplyInverse".Translate(), () => preset.Set(null, true)),

        new FloatMenuOption("FilterManager.Button.PresetMenu.CheckAllowed".Translate(), () => preset.Set(true, false)),
        new FloatMenuOption("FilterManager.Button.PresetMenu.UncheckAllowed".Translate(), () => preset.Set(true, true)),
        new FloatMenuOption("FilterManager.Button.PresetMenu.CheckForbidden".Translate(), () => preset.Set(false, false)),
        new FloatMenuOption("FilterManager.Button.PresetMenu.UncheckForbidden".Translate(), () => preset.Set(false, true))
      };

      if (!preset.Integrated)
      {
        options.Add(new FloatMenuOption("FilterManager.Button.PresetMenu.Overwrite".Translate(), preset.Overwrite));
        options.Add(new FloatMenuOption("FilterManager.Button.PresetMenu.Delete".Translate(), preset.Delete));
      }

      Find.WindowStack.Add(new FloatMenuUnsorted(options));
    }

    private class FloatMenuUnsorted : FloatMenu
    {
      public FloatMenuUnsorted(List<FloatMenuOption> options) : base(options) => this.options = options;
    }
  }
}
