using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using System.Diagnostics;
using System;
using Unity.Netcode;
using ScanVan.Managers;
using ScanVan.Utils;
using System.IO;
using ScanVan.Compatibility;
using BepInEx.Bootstrap;
using ScanVan.Networking;
using System.Runtime.CompilerServices;
using ScanVan.Patches.Enemies;
using ScanVan.Patches.InternalUtils;
using ScanVan.Patches;

namespace ScanVan
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(CompatibilityUtils.GUID_LETHAL_LEVEL_LOADER, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CompatibilityUtils.GUID_SCANDALLIB, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CompatibilityUtils.GUID_SCANDALS_TWEAKS, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CompatibilityUtils.GUID_INPUT_UTILS, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CompatibilityUtils.GUID_LETHALELEMENTS, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CompatibilityUtils.GUID_LETHALMIN, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CompatibilityUtils.GUID_IMMERSIVE_VISOR, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(CompatibilityUtils.GUID_FIRST_PERSON_VIEW, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        internal static VehicleControls VehicleControlsInstance = null!;

        internal static bool initialized = false;

        public void Awake()
        {
            if (initialized)
            {
                return;
            }
            initialized = true;
            Logger = base.Logger;
            Instance = this;

            VehicleControlsInstance = new VehicleControls();
            UserConfig.InitConfig();

            NetworkVariableInitalizer.Init();
            Patch();

            RadioManager.PreloadStations();
            LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            LogDebug("Patching...");

            CompatibilityUtils.Init(Harmony);

            Harmony.PatchAll(type: typeof(ScandalsTweaksPatches));
            Harmony.PatchAll(type: typeof(BaboonBirdAIPatches));
            Harmony.PatchAll(type: typeof(CadaverGrowthAIPatches));
            //Harmony.PatchAll(type: typeof(EnemyAIPatches));
            Harmony.PatchAll(type: typeof(ForestGiantAIPatches));
            Harmony.PatchAll(type: typeof(GiantKiwiAIPatches));
            Harmony.PatchAll(type: typeof(MaskedPlayerEnemyPatches));
            Harmony.PatchAll(type: typeof(MouthDogAIPatches));
            Harmony.PatchAll(type: typeof(PumaAIPatches));
            Harmony.PatchAll(type: typeof(RadMechAIPatches));
            Harmony.PatchAll(type: typeof(RedLocustBeesPatches));
            Harmony.PatchAll(type: typeof(SandWormAIPatches));
            Harmony.PatchAll(type: typeof(ElevatorAnimationEventsPatches));
            Harmony.PatchAll(type: typeof(GameNetworkManagerPatches));
            Harmony.PatchAll(type: typeof(GrabbableObjectPatches));
            Harmony.PatchAll(type: typeof(HUDManagerPatches));
            Harmony.PatchAll(type: typeof(LandminePatches));
            Harmony.PatchAll(type: typeof(PlayerControllerBPatches));
            Harmony.PatchAll(type: typeof(StartOfRoundPatches));
            Harmony.PatchAll(type: typeof(VehicleControllerPatches));

            LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            LogDebug("Finished unpatching!");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogMessage(string messageLog)
        {
            Logger.LogMessage(messageLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogDebug(string debugLog)
        {
            Logger.LogDebug(debugLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogInfo(string infoLog)
        {
            Logger.LogInfo(infoLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogWarning(string warningLog)
        {
            Logger.LogWarning(warningLog);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void LogError(string errorLog)
        {
            Logger.LogError(errorLog);
        }
    }
}
