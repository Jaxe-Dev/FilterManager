using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using RimWorld;
using Verse;

namespace FilterManager.Core;

public static class Storage
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
  private static readonly SortedDictionary<string, Preset> IntegratedPresetsDictionary = new(StringComparer.OrdinalIgnoreCase);

  public static IEnumerable<Preset> Presets => IntegratedPresetsDictionary.Values.Concat(PresetsDictionary.Values.OrderByDescending(static preset => preset.Integrated).ThenBy(static preset => preset.Name));

  private static void AddIntegrated()
  {
    foreach (var preset in Preset.BuildIntegrated()) { IntegratedPresetsDictionary[preset.Name] = preset; }
  }

  private static void AddPreset(Preset preset)
  {
    PresetsDictionary[preset.Name] = preset;
    Save();
  }

  public static void SavePreset(string name)
  {
    var preset = FilterWindow.CreatePreset(name);

    if (PresetsDictionary.ContainsKey(preset.Name))
    {
      Gfx.ShowConfirmDialog("FilterManager.OverwriteConfirm".Translate(preset.Name), () =>
      {
        AddPreset(preset);
        PresetWindow.FocusName(preset.Name);
      });
    }
    else { AddPreset(preset); }
  }

  public static void DeletePreset(Preset preset) => Gfx.ShowConfirmDialog("FilterManager.DeleteConfirm".Translate(preset.Name), () =>
  {
    PresetsDictionary.Remove(preset.Name);
    Save();
    PresetWindow.FocusName(null);
  });

  public static bool CanSave(string? name) => !string.IsNullOrWhiteSpace(name) && !IntegratedPresetsDictionary.ContainsKey(name!);

  public static void Load()
  {
    PresetsDictionary.Clear();
    IntegratedPresetsDictionary.Clear();

    AddIntegrated();

    Mod.ConfigFile.Refresh();
    if (!Mod.ConfigFile.Exists) { return; }

    var document = XDocument.Load(Mod.ConfigFile.FullName);
    var presets = document.Element(XElementPresets);

    foreach (var element in presets?.Elements(XElementPreset) ?? [])
    {
      var name = element.Attribute(XAttributeName)?.Value!;
      if (string.IsNullOrWhiteSpace(name))
      {
        Mod.Warning("Failed to load unnamed preset");
        continue;
      }

      try
      {
        var things = LoadDefs<ThingDef>(element.Element(XElementThings));
        var filters = LoadDefs<SpecialThingFilterDef>(element.Element(XElementFilters));
        var hitpoints = LoadHitpoints(element.Element(XElementHitpoints));
        var quality = LoadQuality(element.Element(XElementQuality));

        PresetsDictionary[name] = new Preset(name, things, filters, hitpoints, quality);
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
    catch (Exception exception) { throw new Exception("Error saving presets", exception); }
  }

  private static void SerializeDefs<T>(string group, string item, IEnumerable<KeyValuePair<T, bool>> dictionary, XElement root) where T : Def
  {
    var element = new XElement(group);
    foreach (var entry in dictionary) { element.Add(new XElement(item, new XAttribute(XAttributeName, entry.Key!.defName)) { Value = XmlConvert.ToString(entry.Value) }); }
    if (element.HasElements) { root.Add(element); }
  }

  private static XElement SerializePreset(Preset preset)
  {
    var element = new XElement(XElementPreset, new XAttribute(XAttributeName, preset.Name));

    if (preset.AllowedHitPointsPercents is not null) { element.Add(new XElement(XElementHitpoints) { Value = preset.AllowedHitPointsPercents.Value.ToString() }); }
    if (preset.AllowedQualityLevels is not null) { element.Add(new XElement(XElementQuality) { Value = preset.AllowedQualityLevels.Value.ToString() }); }

    SerializeDefs(XElementFilters, XElementFilter, preset.Filters.Where(static def => !def.Value), element);
    SerializeDefs(XElementThings, XElementThing, preset.Things.Where(static def => def.Value), element);

    return element;
  }
}
