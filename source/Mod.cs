using System;
using System.IO;
using FilterManager.Core;
using HarmonyLib;
using Verse;

namespace FilterManager;

[StaticConstructorOnStartup]
internal static class Mod
{
  public const string Id = "FilterManager";
  public const string Name = "Filter Manager";
  public const string Version = "1.8";

  public static readonly FileInfo ConfigFile = new(Path.Combine(GenFilePaths.ConfigFolderPath, Id, "Presets.xml"));

  static Mod()
  {
    new Harmony(Id).PatchAll();

    Storage.Load();

    Log("Initialized");
  }

  public static void Log(string message) => Verse.Log.Message(PrefixMessage(message));
  public static void Warning(string message) => Verse.Log.Warning(PrefixMessage(message));
  private static string PrefixMessage(string message) => $"[{Name} v{Version}] {message}";

  public static Exception Exception(string message, Exception exception) => new(PrefixMessage(message), exception);
}
