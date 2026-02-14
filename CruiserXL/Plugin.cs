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
using CruiserXL.Managers;
using CruiserXL.Utils;
using System.IO;
using CruiserXL.Compatibility;

namespace CruiserXL
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("voxx.LethalElementsPlugin", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        private static bool initialized;

        internal static VehicleControls VehicleControlsInstance = null!;

        public void Awake()
        {
            if (initialized)
            {
                return;
            }
            initialized = true;
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

            References.truckOtherPlayerAnimator = PlayerAnimationBundle.LoadAsset<RuntimeAnimatorController>("truckOtherPlayerMetarig.controller");
            if (References.truckOtherPlayerAnimator != null)
            {
                Logger.LogInfo("[AssetBundle] Successfully loaded runtime controller: truckOtherPlayerMetarig");
            }
            else
            {
                Logger.LogError("[AssetBundle] Failed to load runtime controller: truckOtherPlayerMetarig");
                return;
            }

            VehicleControlsInstance = new VehicleControls();
            UserConfig.InitConfig();

            NetcodePatcher();
            Patch();

            RadioManager.PreloadStations();
            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            if (CompatibilityUtils.IsModInstalled("voxx.LethalElementsPlugin"))
            {
                LethalElementsCompatibility.PatchAllElements(Harmony);
            }

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }

        private void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }

    internal class VehicleControls : LcInputActions
    {
        [InputAction(KeyboardControl.W, Name = "Gas Pedal", GamepadControl = GamepadControl.RightTrigger)]
        public InputAction GasPedalKey { get; set; } = null!;

        [InputAction(KeyboardControl.A, Name = "Steer Left", GamepadControl = GamepadControl.LeftStick)]
        public InputAction SteerLeftKey { get; set; } = null!;

        [InputAction(KeyboardControl.S, Name = "Brake", GamepadControl = GamepadControl.LeftTrigger)]
        public InputAction BrakePedalKey { get; set; } = null!;

        [InputAction(KeyboardControl.D, Name = "Steer Right", GamepadControl = GamepadControl.RightStick)]
        public InputAction SteerRightKey { get; set; } = null!;

        [InputAction(KeyboardControl.Space, Name = "Jump")]
        public InputAction JumpKey { get; set; } = null!;

        [InputAction(MouseControl.ScrollUp, Name = "Shift Gear Forward", GamepadControl = GamepadControl.LeftShoulder)]
        public InputAction GearShiftForwardKey { get; set; } = null!;

        [InputAction(MouseControl.ScrollDown, Name = "Shift Gear Backward", GamepadControl = GamepadControl.RightShoulder)]
        public InputAction GearShiftBackwardKey { get; set; } = null!;

        [InputAction(KeyboardControl.F, Name = "Headlamps")]
        public InputAction ToggleHeadlightsKey { get; set; } = null!;

        [InputAction(KeyboardControl.H, Name = "Horn")]
        public InputAction ActivateHornKey { get; set; } = null!;

        [InputAction(KeyboardControl.None, Name = "Wipers")]
        public InputAction ToggleWipersKey { get; set; } = null!;
    }
}
