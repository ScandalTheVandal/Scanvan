using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;

namespace CruiserXL.Utils;

internal class UserConfig
{
    // Host
    internal static ConfigEntry<bool> StreamerRadio = null!;

    // General
    internal static ConfigEntry<bool> SeatBoostEnabled = null!;
    internal static ConfigEntry<float> SeatBoostScale = null!;
    internal static ConfigEntry<bool> PreventKnockback = null!;

    // QOL
    internal static ConfigEntry<bool> RecenterWheel = null!;

    internal static void InitConfig()
    {
        ConfigFile config = Plugin.Instance.Config;
        config.SaveOnConfigSet = false;

        // Host
        StreamerRadio = config.Bind("Host", "DMCA Radio", true, "[Host] If true, will enable streamer-friendly music on the radio, and disable the live-radio system");

        // General
        SeatBoostEnabled = config.Bind("General", "Enable Seat Boost", true, "Should the camera be boosted when sat in the truck?");
        AcceptableValueRange<float> seatScale = new(1.0f, 2.0f);
        SeatBoostScale = config.Bind("General", "Seat Boost Scale", 1.0f, new ConfigDescription("How much to boost the seat up? (Default: 1.0)", seatScale));
        PreventKnockback = config.Bind("General", "Prevent Seated Knockback", false, "Prevent explosions kicking you out of the front seats?");

        // QOL
        RecenterWheel = config.Bind("Quality Of Life", "Automatically Center Wheel", false, "Should the wheel be automatically re-centered?");

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
