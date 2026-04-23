using BepInEx.Configuration;
using HarmonyLib;
using NAudio.Dsp;
using System.Collections.Generic;
using System.Reflection;

namespace ScanVan.Utils;

internal class UserConfig
{
    // Host
    internal static ConfigEntry<bool> StreamerRadio = null!;
    internal static ConfigEntry<bool> OldBirdSight = null!;

    // General
    internal static ConfigEntry<bool> SeatBoostEnabled = null!;
    internal static ConfigEntry<float> SeatBoostScale = null!;
    internal static ConfigEntry<bool> PreventKnockback = null!;
    internal static ConfigEntry<float> VoiceAlertVolume = null!;

    // QOL
    internal static ConfigEntry<bool> RecenterWheel = null!;
    internal static ConfigEntry<bool> SmoothWheel = null!;

    // Experimental
    //internal static ConfigEntry<bool> MuffleVoices = null!;

    internal static void InitConfig()
    {
        ConfigFile config = Plugin.Instance.Config;
        config.SaveOnConfigSet = false;

        // Host
        StreamerRadio = config.Bind("Host", "DMCA Radio", true, "[Host] If true, will enable streamer-friendly music on the radio, and disable the live-radio system");
        OldBirdSight = config.Bind("Host", "Enemy sight", true, "[Host] If true, will allow enemies such as Old Birds to see players in the front seats");

        // General
        SeatBoostEnabled = config.Bind("General", "Enable Seat Boost", true, "Should the camera be boosted when sat in the truck?");
        AcceptableValueRange<float> seatScale = new(1.0f, 2.0f);
        SeatBoostScale = config.Bind("General", "Seat Boost Scale", 1.0f, new ConfigDescription("How much to boost the seat up? (Default: 1.0)", seatScale));
        PreventKnockback = config.Bind("General", "Prevent Seated Knockback", true, "Prevent explosions kicking you out of the front seats?");
        AcceptableValueRange<float> alertVolume = new(0.25f, 1.0f);
        VoiceAlertVolume = config.Bind("General", "Voice Alert Volume", 1.0f, new ConfigDescription("How much to reduce the voice alert volume by? (Full volume: 1.0)", alertVolume));

        // QOL
        RecenterWheel = config.Bind("Quality Of Life", "Automatically Center Wheel", false, "Should the wheel be automatically re-centered?");
        SmoothWheel = config.Bind("Quality Of Life", "Smooth Wheel", true, "Should the wheels inputs be smoothened?");

        // Experimental
        //MuffleVoices = config.Bind("Experimental", "Muffle Player Voices", true, "Should player voices be muffled when in-side the truck with the doors shut? or vise-versa.");

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
