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

namespace ScanVan
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("imabatby.lethallevelloader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("scandal.scandalstweaks", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("voxx.LethalElementsPlugin", BepInDependency.DependencyFlags.SoftDependency)] 
    [BepInDependency("NoteBoxz.LethalMin", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("ImmersiveVisor", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        internal static VehicleControls VehicleControlsInstance = null!;

        public void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            AssetBundle PlayerAnimationBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location), "scanvananimationbundle"));
            if (PlayerAnimationBundle == null)
            {
                Logger.LogError("[AssetBundle] Failed to load asset bundle: scanvananimationbundle");
                return;
            }

            References.truckPlayerAnimator = PlayerAnimationBundle.LoadAsset<RuntimeAnimatorController>("truckPlayerMetarig.controller");
            if (References.truckPlayerAnimator != null)
            {
                Logger.LogInfo("[AssetBundle] Successfully loaded runtime controller: truckPlayerMetarig");
            }
            else
            {
                Logger.LogError("[AssetBundle] Failed to load runtime controller: truckPlayerMetarig");
                return;
            }

            VehicleControlsInstance = new VehicleControls();
            UserConfig.InitConfig();

            NetworkVariableInitalizer.Init();
            Patch();

            RadioManager.PreloadStations();
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            if (IsModPresent("voxx.LethalElementsPlugin")) LethalElementsCompatibility.PatchAllCompatibilityMethods(Harmony);
            if (IsModPresent("NoteBoxz.LethalMin")) LethalMinCompatibility.PatchAllCompatibilityMethods(Harmony);
            if (IsModPresent("ImmersiveVisor")) ImmersiveVisorCompatibility.PatchAllCompatibilityMethods(Harmony);

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }

        internal static bool IsModPresent(string name)
        {
            return Chainloader.PluginInfos.ContainsKey(name);
        }
    }
}
