using BepInEx.Bootstrap;
using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using VoxxWeatherPlugin.Patches;
using System.Runtime.CompilerServices;
using VoxxWeatherPlugin.Weathers;
using VoxxWeatherPlugin.Utils;
using UnityEngine.VFX;

namespace CruiserXL.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
public class LethalElementsCompatibility
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAllElements(Harmony harmony)
    {
        ApplyElementsPatch(harmony);
    }

    [HarmonyPrefix]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ApplyElementsPatch(Harmony harmony)
    {
        var originalSetTempMethod = AccessTools.Method(typeof(PlayerEffectsManager), nameof(PlayerEffectsManager.SetPlayerTemperature));
        var originalHeatwaveStopMethod = AccessTools.Method(typeof(HeatwavePatches), nameof(HeatwavePatches.CheckConditionsForHeatingStop));
        var originalHeatwavePauseMethod = AccessTools.Method(typeof(HeatwavePatches), nameof(HeatwavePatches.CheckConditionsForHeatingPause));
        var originalUpdateMethod = AccessTools.Method(typeof(BlizzardWeather), nameof(BlizzardWeather.Update));
        var originalVFXUpdateMethod = AccessTools.Method(typeof(SnowfallVFXManager), nameof(SnowfallVFXManager.Update));
        var originalSetZoneMethod = AccessTools.Method(typeof(BlizzardWeather), nameof(BlizzardWeather.SetColdZoneState));
        var originalBaseSetZoneMethod = AccessTools.Method(typeof(SnowfallWeather), nameof(SnowfallWeather.SetColdZoneState));

        // patching my own shit???
        // hell yeah i am
        var originalVehicleStartMethod = AccessTools.Method(typeof(CruiserXLController), nameof(CruiserXLController.Start));
        var originalVehicleFixedUpdateMethod = AccessTools.Method(typeof(CruiserXLController), nameof(CruiserXLController.FixedUpdate));

        var prefixSetTempMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(SetPlayerTemperature_Heatwave));
        var prefixHeatwaveStopMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(CheckConditionsForHeatingStop_Prefix));
        var prefixHeatwavePauseMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(CheckConditionsForHeatingPause_Prefix));
        var prefixUpdateMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(Update_Prefix));
        var postfixVFXUpdateMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(VFXUpdate_Postfix));
        var prefixSetZoneMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(SetColdZoneState_Prefix));
        var prefixBaseSetZoneMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(SetBaseColdZoneState_Prefix));

        // prefixing my own shit, yeeaaaaahh
        var prefixVehicleStartMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(VehicleSnowTracksPatch_Prefix));
        var prefixVehicleFixedUpdateMethod = AccessTools.Method(typeof(LethalElementsCompatibility), nameof(VehicleSnowTracksFixedUpdatePatch_Prefix));
        
        harmony.Patch(originalHeatwaveStopMethod, prefix: new HarmonyMethod(prefixHeatwaveStopMethod));
        harmony.Patch(originalHeatwavePauseMethod, prefix: new HarmonyMethod(prefixHeatwavePauseMethod));
        harmony.Patch(originalSetTempMethod, prefix: new HarmonyMethod(prefixSetTempMethod));
        harmony.Patch(originalUpdateMethod, prefix: new HarmonyMethod(prefixUpdateMethod));
        harmony.Patch(originalVFXUpdateMethod, postfix: new HarmonyMethod(postfixVFXUpdateMethod));
        harmony.Patch(originalSetZoneMethod, prefix: new HarmonyMethod(prefixSetZoneMethod));
        harmony.Patch(originalBaseSetZoneMethod, prefix: new HarmonyMethod(prefixBaseSetZoneMethod));

        harmony.Patch(originalVehicleStartMethod, prefix: new HarmonyMethod(prefixVehicleStartMethod));
        harmony.Patch(originalVehicleFixedUpdateMethod, prefix: new HarmonyMethod(prefixVehicleFixedUpdateMethod));
    }

    // hacky method to alter the heat transfer rate during heatwave
    public static void SetPlayerTemperature_Heatwave(PlayerEffectsManager __instance, float temperatureDelta)
    {
        if (HeatwaveWeather.Instance == null || !HeatwaveWeather.Instance.IsActive) 
            return;
        if (References.truckController == null)
            return;
        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.isPlayerInCab &&
            !PlayerUtils.isPlayerInStorage)
            return;

        CruiserXLController controller = References.truckController;
        bool isCabEnclosed = !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue &&
             !controller.driversSideWindowTrigger.boolValue && !controller.passengersSideWindowTrigger.boolValue;
        bool isStorageEnclosed = !controller.liftGateOpen && !controller.sideDoorOpen;
        bool isHeaterOnAndCool = controller.ignitionStarted && controller.heaterOn && !controller.isHeaterWarm && controller.isHeaterCold;
        bool isHeaterOnAndWarm = controller.ignitionStarted && controller.heaterOn && controller.isHeaterWarm && !controller.isHeaterCold;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.isPlayerInCab && !PlayerUtils.isPlayerInStorage;
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        bool inDoorLighting = localPlayer.currentAudioTrigger != null && localPlayer.currentAudioTrigger.insideLighting;

        if ((PlayerUtils.isPlayerInCab && isCabEnclosed) || 
            (PlayerUtils.isPlayerInStorage && isStorageEnclosed))
        {
            if (!inDoorLighting)
            {
                if (isHeaterOnAndCool) PlayerEffectsManager.heatTransferRate = 0.25f * controller.heaterSpeed;
                else if (isHeaterOnAndWarm) PlayerEffectsManager.heatTransferRate = 1.15f * controller.heaterSpeed;
                else PlayerEffectsManager.heatTransferRate = 0.75f;
            }
            else PlayerEffectsManager.heatTransferRate = 1f;
        }
        else PlayerEffectsManager.heatTransferRate = 1f;
        if (outsideOfTruck) PlayerEffectsManager.heatTransferRate = 1f;
    }

    public static void VehicleSnowTracksPatch_Prefix(CruiserXLController __instance)
    {
        SnowTrackersManager.AddFootprintTracker(__instance, 6f, 0.75f, 1f, new Vector3(0, 0, -1f));
    }

    public static void VehicleSnowTracksFixedUpdatePatch_Prefix(CruiserXLController __instance)
    {
        if (!SnowPatches.IsSnowActive())
        {
            return;
        }

        bool enableTracker = __instance.FrontLeftWheel.isGrounded ||
                                __instance.FrontRightWheel.isGrounded ||
                                __instance.BackLeftWheel.isGrounded ||
                                __instance.BackRightWheel.isGrounded;
        SnowTrackersManager.UpdateFootprintTracker(__instance, enableTracker);
    }

    public static void VFXUpdate_Postfix()
    {
        if (References.truckController == null)
            return;

        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.isPlayerInCab &&
            !PlayerUtils.isPlayerInStorage)
            return;

        if (PlayerUtils.isPlayerInCab || PlayerUtils.isPlayerInStorage)
        {
            PlayerEffectsManager.isUnderSnow = false;
            SnowfallVFXManager.snowMovementHindranceMultiplier = 1f;
        }
    }

    public static bool SetColdZoneState_Prefix(BlizzardWeather __instance)
    {
        if (References.truckController == null)
            return true;
        if (PlayerUtils.isPlayerOnTruck || 
            PlayerUtils.isPlayerInCab || 
            PlayerUtils.isPlayerInStorage)
            return false;
        return true;
    }

    public static bool SetBaseColdZoneState_Prefix(SnowfallWeather __instance)
    {
        if (BlizzardWeather.Instance == null)
            return true;
        if (References.truckController == null)
            return true;
        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.isPlayerInCab &&
            !PlayerUtils.isPlayerInStorage)
            return true;

        CruiserXLController controller = References.truckController;
        bool isCabEnclosed = !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue &&
            !controller.driversSideWindowTrigger.boolValue && !controller.passengersSideWindowTrigger.boolValue;
        bool isStorageEnclosed = !controller.liftGateOpen && !controller.sideDoorOpen;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.isPlayerInCab && !PlayerUtils.isPlayerInStorage;
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        bool inDoorLighting = localPlayer.currentAudioTrigger != null && localPlayer.currentAudioTrigger.insideLighting;

        if (PlayerUtils.isPlayerInCab)
        {
            if (isCabEnclosed)
            {
                if (controller.ignitionStarted && controller.heaterOn) 
                    PlayerEffectsManager.isInColdZone = controller.isHeaterCold;
                else PlayerEffectsManager.isInColdZone = !inDoorLighting;
            }
            else
            {
                PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(localPlayer, controller) && BlizzardWeather.Instance.isLocalPlayerInWind;
            }
            return false;
        }
        else if (PlayerUtils.isPlayerInStorage)
        {
            if (isStorageEnclosed)
            {
                if (controller.ignitionStarted && controller.heaterOn)
                    PlayerEffectsManager.isInColdZone = controller.isHeaterCold;
                else PlayerEffectsManager.isInColdZone = !inDoorLighting;
            }
            else
            {
                PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(localPlayer, controller) && BlizzardWeather.Instance.isLocalPlayerInWind;
            }
            return false;
        }
        else if (outsideOfTruck)
        {
            PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(localPlayer, controller) && BlizzardWeather.Instance.isLocalPlayerInWind;
            return false;
        }
        return true;
    }

    public static bool Update_Prefix(BlizzardWeather __instance)
    {
        if (References.truckController == null)
            return true;

        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.isPlayerInCab &&
            !PlayerUtils.isPlayerInStorage)
            return true;

        CruiserXLController controller = References.truckController;
        bool isCabEnclosed = !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue &&
            !controller.driversSideWindowTrigger.boolValue && !controller.passengersSideWindowTrigger.boolValue;
        bool isStorageEnclosed = !controller.liftGateOpen && !controller.sideDoorOpen;
        bool inCabOrStorage = PlayerUtils.isPlayerInCab || PlayerUtils.isPlayerInStorage;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.isPlayerInCab && !PlayerUtils.isPlayerInStorage;
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        bool inDoorLighting = localPlayer.currentAudioTrigger != null && localPlayer.currentAudioTrigger.insideLighting;

        SnowfallWeather.Instance?.Update();

        if (inCabOrStorage)
        {
            if ((PlayerUtils.isPlayerInCab && isCabEnclosed) || 
                (PlayerUtils.isPlayerInStorage && isStorageEnclosed)) __instance.isPlayerInBlizzard = false;
            else __instance.isPlayerInBlizzard = __instance.isLocalPlayerInWind;

            if (controller.ignitionStarted && controller.heaterOn) 
                PlayerEffectsManager.heatTransferRate = (0.25f * controller.heaterSpeed);
            else PlayerEffectsManager.heatTransferRate = 0.75f;
        }

        if (outsideOfTruck)
        {
            PlayerEffectsManager.heatTransferRate = 1f;
            __instance.isPlayerInBlizzard = __instance.isLocalPlayerInWind;
        }
        return false;
    }

    public static bool IsWindAllowedVehicle(PlayerControllerB localPlayer, CruiserXLController controller)
    {
        if (localPlayer.currentAudioTrigger != null &&
                localPlayer.currentAudioTrigger.insideLighting) return false;

        bool isCabEnclosed = !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue &&
            !controller.driversSideWindowTrigger.boolValue && !controller.passengersSideWindowTrigger.boolValue;
        bool isStorageEnclosed = !controller.liftGateOpen && !controller.sideDoorOpen;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.isPlayerInCab && !PlayerUtils.isPlayerInStorage;

        if ((PlayerUtils.isPlayerInCab && !isCabEnclosed) || 
            (PlayerUtils.isPlayerInStorage && !isStorageEnclosed))
        {
            return true;
        }
        else if (outsideOfTruck)
        {
            return true;
        }
        return false;
    }

    public static bool CheckConditionsForHeatingStop_Prefix(PlayerControllerB playerController, ref bool __result)
    {
        if (References.truckController == null)
            return true;
        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.isPlayerInCab &&
            !PlayerUtils.isPlayerInStorage)
            return true;

        CruiserXLController controller = References.truckController;
        bool isCabEnclosed = !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue &&
             !controller.driversSideWindowTrigger.boolValue && !controller.passengersSideWindowTrigger.boolValue;
        bool isStorageEnclosed = !controller.liftGateOpen && !controller.sideDoorOpen;
        bool isHeaterOnAndCool = controller.ignitionStarted && controller.heaterOn && !controller.isHeaterWarm && controller.isHeaterCold;
        bool isHeaterOnAndWarm = controller.ignitionStarted && controller.heaterOn && controller.isHeaterWarm && !controller.isHeaterCold;
        bool outsideOfTruck = PlayerUtils.isPlayerOnTruck && !PlayerUtils.isPlayerInCab && !PlayerUtils.isPlayerInStorage;
        bool inDoorLighting = playerController.currentAudioTrigger != null && playerController.currentAudioTrigger.insideLighting;

        if ((PlayerUtils.isPlayerInCab && isCabEnclosed) ||
            (PlayerUtils.isPlayerInStorage && isStorageEnclosed))
        {
            if (isHeaterOnAndCool) __result = true;
            else if (isHeaterOnAndWarm) __result = false;
            else __result = false;
        }
        else
        {
            __result = inDoorLighting ? true : false;
        }
        if (outsideOfTruck) __result = inDoorLighting ? true : false;
        return false;
    }

    public static bool CheckConditionsForHeatingPause_Prefix(PlayerControllerB playerController, ref bool __result)
    {
        if (References.truckController == null)
            return true;
        if (!PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.isPlayerInCab &&
            !PlayerUtils.isPlayerInStorage)
            return true;

        __result = false;
        return false;
    }
}
