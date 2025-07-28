using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RimWorld;
using Verse;

namespace FilterManager.Core;

internal static class Storage
{
  private const string XAttributeName = "Name";
  private const string XElementPresets = "Presets";
  private const string XElementPreset = "Preset";
  private const string XElementHitpoints = "Hitpoints";
  private const string XElementQuality = "Quality";
  private const string XElementFilters = "Filters";
  private const string XElementFilter = "Filter";
  private const string XElementThings = "Things";
  private const string XElementThing = "Thing";

  private static readonly SortedDictionary<string, Preset> PresetsDictionary = new(StringComparer.OrdinalIgnoreCase);
  public static IEnumerable<Preset> Presets => PresetsDictionary.Values.OrderByDescending(static preset => preset.Integrated).ThenBy(static preset => preset.Name);

  private static void BuildIntegrated()
  {
    if (ThingCategoryNodeDatabase.RootNode?.catDef is null) { return; }

    AddIntegrated("FilterManager.IntegratedPreset.Degradable".TranslateSimple(), null, ThingCategoryNodeDatabase.RootNode.catDef.DescendantThingDefs.Distinct().ToDictionary(static def => def, static def => def.CanEverDeteriorate && def.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 0));
    AddIntegrated("FilterManager.IntegratedPreset.Rottable".TranslateSimple(), null, ThingCategoryNodeDatabase.RootNode.catDef.DescendantThingDefs.Distinct().ToDictionary(static def => def, static def => def.HasComp(typeof(CompRottable))));
  }

  private static void AddIntegrated(string name, Dictionary<SpecialThingFilterDef, bool>? filters, Dictionary<ThingDef, bool> things)
  {
    var preset = new Preset(name, filters, things);
    if (preset.Filters.Count + preset.Things.Count == 0) { return; }

    PresetsDictionary[preset.Name] = preset;
  }

  private static void AddPreset(string name)
  {
    var preset = new Preset(name);
    if (preset.Filters.Count + preset.Things.Count == 0) { return; }

    PresetsDictionary[preset.Name] = preset;

    Save();
  }

  public static void SavePreset(string name)
  {
    if (PresetsDictionary.ContainsKey(name)) { Gfx.ShowConfirmDialog("FilterManager.Button.OverwriteConfirm".Translate(name), () => AddPreset(name)); }
    else { AddPreset(name); }
  }

  public static void DeletePreset(Preset preset) => Gfx.ShowConfirmDialog("FilterManager.Button.DeleteConfirm".Translate(preset.Name), () =>
  {
    PresetsDictionary.Remove(preset.Name);
    Save();
  });

  public static void Load()
  {
    PresetsDictionary.Clear();

    BuildIntegrated();

    Mod.ConfigFile.Refresh();
    if (!Mod.ConfigFile.Exists) { return; }

    var document = XDocument.Load(Mod.ConfigFile.FullName);
    var presets = document.Element(XElementPresets);

    foreach (var element in presets?.Elements(XElementPreset) ?? [])
    {
      var name = element.Attribute(XAttributeName)?.Value;
      if (string.IsNullOrWhiteSpace(name))
      {
        Mod.Warning("Failed to load unnamed preset");
        continue;
      }

      try
      {
        var hitpoints = LoadHitpoints(element.Element(XElementHitpoints));
        var quality = LoadQuality(element.Element(XElementQuality));

        var specials = LoadDefs<SpecialThingFilterDef>(element.Element(XElementFilters));
        var things = LoadDefs<ThingDef>(element.Element(XElementThings));

        PresetsDictionary[name!] = new Preset(name!, hitpoints, quality, specials, things);
      }
      catch { Mod.Warning($"Failed to load preset '{name}'"); }
    }
  }

  private static FloatRange? LoadHitpoints(XElement? root)
  {
    if (root == null) { return null; }
    try { return FloatRange.FromString(root.Value); }
    catch
    {
      Mod.Warning($"Failed to load hitpoint range value '{root.Value}'");
      return null;
    }
  }

  private static QualityRange? LoadQuality(XElement? root)
  {
    if (root == null) { return null; }
    try { return QualityRange.FromString(root.Value); }
    catch
    {
      Mod.Warning($"Failed to load quality range value '{root.Value}'");
      return null;
    }
  }

  private static Dictionary<T, bool> LoadDefs<T>(XElement? root) where T : Def
  {
    var dictionary = new Dictionary<T, bool>();
    if (root == null) { return dictionary; }
    foreach (var element in root.Elements().Where(static element => element.Name == XElementThing || element.Name == XElementFilter))
    {
      var defName = element.Attribute(XAttributeName)?.Value;

      if (string.IsNullOrWhiteSpace(defName))
      {
        Mod.Log("Skipping unnamed def");
        continue;
      }

      var def = DefDatabase<T>.GetNamed(defName, false);
      if (def is null)
      {
        Mod.Log($"Skipping missing def '{defName}'");
        continue;
      }

      dictionary.Add(def, XmlConvert.ToBoolean(element.Value));
    }
    return dictionary;
  }

  private static void Save()
  {
    var document = new XDocument();

    var presets = new XElement(XElementPresets);

    try
    {
      foreach (var preset in Presets.Where(static preset => !preset.Integrated)) { presets.Add(SerializePreset(preset)); }
      document.Add(presets);

      Mod.ConfigFile.Directory!.Create();
      document.Save(Mod.ConfigFile.FullName);
    }
    catch (Exception exception) { throw Mod.Exception("Error saving presets", exception); }
  }

  private static void SerializeDefs<T>(string group, string item, Dictionary<T, bool> dictionary, XElement root) where T : Def
  {
    var element = new XElement(group);
    foreach (var entry in dictionary) { element.Add(new XElement(item, new XAttribute(XAttributeName, entry.Key!.defName)) { Value = XmlConvert.ToString(entry.Value) }); }
    if (element.HasElements) { root.Add(element); }
  }

  private static XElement SerializePreset(Preset preset)
  {
    var element = new XElement(XElementPreset, new XAttribute(XAttributeName, preset.Name));

    if (preset.AllowedHitPointsPercents != null) { element.Add(new XElement(XElementHitpoints) { Value = preset.AllowedHitPointsPercents.Value.ToString() }); }
    if (preset.AllowedQualityLevels != null) { element.Add(new XElement(XElementQuality) { Value = preset.AllowedQualityLevels.Value.ToString() }); }

    SerializeDefs(XElementFilters, XElementFilter, preset.Filters, element);
    SerializeDefs(XElementThings, XElementThing, preset.Things, element);

    return element;
  }
}
