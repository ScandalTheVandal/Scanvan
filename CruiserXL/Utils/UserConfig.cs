using BepInEx.Configuration;
using HarmonyLib;
using NAudio.Dsp;
using System.Collections.Generic;
using System.Reflection;

namespace ScanVan.Utils;

public class UserConfig
{
    // DEBUG
    public static ConfigEntry<bool> EnableDebugLogging = null!;

    // Host
    public static ConfigEntry<bool> StreamerRadio = null!;

    // General
    public static ConfigEntry<bool> MirrorsEnabled = null!;
    public static ConfigEntry<bool> SeatBoostEnabled = null!;
    public static ConfigEntry<float> SeatBoostScale = null!;
    public static ConfigEntry<bool> PreventKnockback = null!;
    public static ConfigEntry<float> VoiceAlertVolume = null!;

    // QOL
    public static ConfigEntry<bool> RecenterWheel = null!;
    public static ConfigEntry<bool> SmoothWheel = null!;

    internal static void InitConfig()
    {
        ConfigFile config = Plugin.Instance.Config;
        config.SaveOnConfigSet = false;

        // DEBUG
        EnableDebugLogging = config.Bind("Debug", "Debug-Logging", false, "If true, activates debug logging");

        // Host
        StreamerRadio = config.Bind("Host", "Public-Domain Music", true, "[Host] If true, the radio will only be able to play vintage public-domain music");

        // General
        MirrorsEnabled = config.Bind("General", "Enable Mirrors", true, "Enable rendering for the side mirrors? (may impact performance for low end hardware)");
        SeatBoostEnabled = config.Bind("General", "Enable Seat Boost", true, "Should the camera be boosted when sat in the truck?");
        AcceptableValueRange<float> seatScale = new(1.0f, 2.0f);
        SeatBoostScale = config.Bind("General", "Seat Boost Scale", 1.0f, new ConfigDescription("How much to boost the seat up? (Default: 1.0)", seatScale));
        PreventKnockback = config.Bind("General", "Prevent Seated Knockback", true, "Prevent explosions kicking you out of the front seats?");
        AcceptableValueRange<float> alertVolume = new(0.5f, 1.0f);
        VoiceAlertVolume = config.Bind("General", "Voice Alert Volume", 1.0f, new ConfigDescription("How much to reduce the voice alert volume by? (Full volume: 1.0)", alertVolume));

        // QOL
        RecenterWheel = config.Bind("Quality Of Life", "Automatically Center Wheel", false, "Should the wheel be automatically re-centered?");
        SmoothWheel = config.Bind("Quality Of Life", "Smooth Wheel", true, "Should the wheels inputs be smoothened?");

        ClearOrphanedEntries(config);
        config.Save();
        config.SaveOnConfigSet = true;
    }

    static void ClearOrphanedEntries(ConfigFile config)
    {
        PropertyInfo orphanedEntriesProp = AccessTools.Property(typeof(ConfigFile), "OrphanedEntries");
        var orphanedEntries = (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(config);
        orphanedEntries.Clear();
    }
}
