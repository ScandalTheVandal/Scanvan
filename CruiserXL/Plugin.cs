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

namespace CruiserXL
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
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
            VehicleControlsInstance = new VehicleControls();

            UserConfig.InitConfig();
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
            }

            References.truckOtherPlayerAnimator = PlayerAnimationBundle.LoadAsset<RuntimeAnimatorController>("truckOtherPlayerMetarig.controller");
            if (References.truckOtherPlayerAnimator != null)
            {
                Logger.LogInfo("[AssetBundle] Successfully loaded runtime controller: truckOtherPlayerMetarig");
            }
            else
            {
                Logger.LogError("[AssetBundle] Failed to load runtime controller: truckOtherPlayerMetarig");
            }

            NetcodePatcher();
            Patch();

            RadioManager.PreloadStations();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        public void BindConfig<T>(ref ConfigEntry<T> config, string section, string key, T defaultValue, string description = "")
        {
            config = Config.Bind<T>(section, key, defaultValue, description);
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

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

    internal class UserConfig
    {
        internal static ConfigEntry<bool> AutoSwitchDriveReverse = null!;
        internal static ConfigEntry<bool> AutoSwitchFromParked = null!;
        internal static ConfigEntry<bool> PreventPushInPark = null!;
        internal static ConfigEntry<bool> AutoSwitchToParked = null!;
        internal static ConfigEntry<bool> RecenterWheel = null!;
        internal static ConfigEntry<int> RecenterWheelSpeed = null!;
        internal static ConfigEntry<float> SeatBoostScale = null!;
        //internal static ConfigEntry<int> ChanceToStartIgnition = null!;
        //internal static ConfigEntry<int> MaxTurboBoosts = null!;

        internal static void InitConfig()
        {
            Plugin.Instance.BindConfig(ref AutoSwitchDriveReverse, "Settings", "Automatic Gearbox", true, "Should the gear automatically switch between drive & reverse when pressing the forward/backwards buttons?");
            Plugin.Instance.BindConfig(ref AutoSwitchFromParked, "Settings", "Automatic Handbrake Release", false, "Should the gear automatically switch to drive/reverse from parked?");
            Plugin.Instance.BindConfig(ref PreventPushInPark, "Settings", "Prevent Push In Park", false, "Should push attempts be blocked if the engine is off & the gear selector is in the park position?");
            Plugin.Instance.BindConfig(ref AutoSwitchToParked, "Settings", "Automatic Handbrake Pull", false, "Should the gear automatically switch to parked when the key is taken from the ignition?");
            Plugin.Instance.BindConfig(ref RecenterWheel, "Settings", "Automatically Center Wheel", false, "Should the wheel be automatically re-centered?");
            AcceptableValueRange<int> recenterWheelSpeedRange = new AcceptableValueRange<int>(-1, 10);
            RecenterWheelSpeed = Plugin.Instance.Config.Bind("Settings", "Center Wheel Speed", -1, new ConfigDescription("How fast should the wheel be re-centered? (Default: -1.0, Instant: 0.0)", recenterWheelSpeedRange));
            AcceptableValueRange<float> seatBoostRange = new(1.0f, 2.0f);
            SeatBoostScale = Plugin.Instance.Config.Bind("Settings", "Seat Boost Scale", 1.0f, new ConfigDescription("How much to boost the seat up? (Default: 1.0)", seatBoostRange));
            //AcceptableValueRange<int> ignitionChanceRange = new AcceptableValueRange<int>(0, 101);
            //ChanceToStartIgnition = Plugin.Instance.Config.Bind("Settings", "Ignition Chance", 0, new ConfigDescription("What should the success chance for the ignition be? If set to 0 this will increase the chance each time the ignition is used. (Vanilla: 0)", ignitionChanceRange));
            //AcceptableValueRange<int> turboBoostsRange = new AcceptableValueRange<int>(1, 100);
            //MaxTurboBoosts = Plugin.Instance.Config.Bind("Settings", "Turbo Boosts", 5, new ConfigDescription("How many turbo boosts should you be able to have queued up at the same time? (Original: 5)", turboBoostsRange));
            //Plugin.maxTurboBoosts = MaxTurboBoosts.Value;
            //MaxTurboBoosts.SettingChanged += (_, _) => Plugin.maxTurboBoosts = MaxTurboBoosts.Value;
        }
    }

    internal class VehicleControls : LcInputActions
    {
        [InputAction(KeyboardControl.W, Name = "Gas Pedal", GamepadControl = GamepadControl.RightTrigger)]
        public InputAction GasPedalKey { get; set; } = null!;

        [InputAction(KeyboardControl.S, Name = "Brake", GamepadControl = GamepadControl.LeftTrigger)]
        public InputAction BrakePedalKey { get; set; } = null!;

        [InputAction(KeyboardControl.Space, Name = "Jump/Boost", GamepadControl = GamepadControl.ButtonNorth)]
        public InputAction TurboKey { get; set; } = null!;

        [InputAction(KeyboardControl.None, Name = "Jump")]
        public InputAction JumpKey { get; set; } = null!;

        [InputAction(KeyboardControl.None, Name = "Drive Forward", GamepadPath = "<Gamepad>/leftStick/up")]
        public InputAction MoveForwardsKey { get; set; } = null!;

        [InputAction(KeyboardControl.None, Name = "Drive Backward", GamepadPath = "<Gamepad>/leftStick/down")]
        public InputAction MoveBackwardsKey { get; set; } = null!;

        [InputAction(MouseControl.ScrollUp, Name = "Shift Gear Forward", GamepadControl = GamepadControl.LeftShoulder)]
        public InputAction GearShiftForwardKey { get; set; } = null!;

        [InputAction(MouseControl.ScrollDown, Name = "Shift Gear Backward", GamepadControl = GamepadControl.RightShoulder)]
        public InputAction GearShiftBackwardKey { get; set; } = null!;

        [InputAction(KeyboardControl.None, Name = "Center Steering Wheel")]
        public InputAction WheelCenterKey { get; set; } = null!;

        [InputAction(KeyboardControl.L, Name = "Headlights")]
        public InputAction ToggleHeadlightsKey { get; set; } = null!;

        [InputAction(KeyboardControl.H, Name = "Horn")]
        public InputAction ActivateHornKey { get; set; } = null!;

        [InputAction(KeyboardControl.None, Name = "Wipers")]
        public InputAction ToggleWipersKey { get; set; } = null!;

        //[InputAction(KeyboardControl.None, Name = "Magnet")]
        //public InputAction ToggleMagnetKey { get; set; } = null!;
    }
}
