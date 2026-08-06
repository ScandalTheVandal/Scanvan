using GameNetcodeStuff;
using HarmonyLib;
using ScanVan.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;
using VoxxWeatherPlugin.Patches;
using VoxxWeatherPlugin.Utils;
using VoxxWeatherPlugin.Weathers;

namespace ScanVan.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
[HarmonyPatch]
public static class LethalElementsCompatibility
{
    public static bool IsVanCabinEnclosed(CruiserXLController controller)
    {
        return !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue &&
               !controller.frontLeftWindow.isWindowOpen && !controller.frontRightWindow.isWindowOpen &&
               !controller.windshieldBroken;
    }

    public static bool IsVanBlowerOnAndHot(CruiserXLController controller)
    {
        return controller.ignitionStarted && 
               controller.heaterOn && 
               controller.isHeaterWarm && 
               !controller.isHeaterCold;
    }

    public static bool IsVanBlowerOnAndCool(CruiserXLController controller)
    {
        return controller.ignitionStarted &&
               controller.heaterOn &&
               !controller.isHeaterWarm &&
               controller.isHeaterCold;
    }

    public static bool IsIndoorLighting(PlayerControllerB playerScript)
    {
        return playerScript.currentAudioTrigger != null && playerScript.currentAudioTrigger.insideLighting;
    }

    [HarmonyPatch(typeof(PlayerEffectsManager), nameof(PlayerEffectsManager.SetPlayerTemperature))]
    [HarmonyPrefix]
    public static void PlayerEffectsManager_Pre_SetPlayerTemperature(PlayerEffectsManager __instance, float temperatureDelta)
    {
        if (HeatwaveWeather.Instance == null || !HeatwaveWeather.Instance.IsActive)
            return;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);

        if (!playerSeated &&
            !playerOnVan &&
            !playerInCab &&
            !playerInStorage)
            return;

        bool playerInCabOrSeated = playerSeated || playerInCab;
        bool isCabEnclosed = IsVanCabinEnclosed(vanController);
        bool isStorageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);
        bool isHeaterOnAndCool = IsVanBlowerOnAndCool(vanController);
        bool isHeaterOnAndWarm = IsVanBlowerOnAndHot(vanController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfVan(playerController, vanController);
        bool inDoorLighting = IsIndoorLighting(playerController);

        if ((playerInCabOrSeated && isCabEnclosed) || 
             (playerInStorage && isStorageEnclosed))
        {
            if (!inDoorLighting)
            {
                if (isHeaterOnAndCool) PlayerEffectsManager.heatTransferRate = 0.25f * vanController.heaterSpeed;
                else if (isHeaterOnAndWarm) PlayerEffectsManager.heatTransferRate = 1.15f * vanController.heaterSpeed;
                else PlayerEffectsManager.heatTransferRate = 0.75f;
            }
            else PlayerEffectsManager.heatTransferRate = 1f;
        }
        else PlayerEffectsManager.heatTransferRate = 1f;
        if (outsideOfTruck) PlayerEffectsManager.heatTransferRate = 1f;
    }

    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.Start))]
    [HarmonyPostfix]
    public static void CruiserXLController_Post_Start(CruiserXLController __instance)
    {
        SnowTrackersManager.AddFootprintTracker(__instance, 6f, 0.75f, 1f, new Vector3(0, 0, -1f));
    }

    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.FixedUpdate))]
    [HarmonyPostfix]
    public static void CruiserXLController_Post_FixedUpdate(CruiserXLController __instance)
    {
        if (!SnowPatches.IsSnowActive())
        {
            return;
        }
        SnowTrackersManager.UpdateFootprintTracker(__instance, !__instance.allWheelsAirborne);
    }

    [HarmonyPatch(typeof(SnowfallVFXManager), nameof(SnowfallVFXManager.Update))]
    [HarmonyPostfix]
    public static void SnowfallVFXManager_Post_Update_VFX()
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);

        if (!playerSeated &&
            !playerOnVan &&
            !playerInCab &&
            !playerInStorage)
            return;

        bool playerInCabOrStorage = playerSeated || playerInCab || playerInStorage;
        if (playerInCabOrStorage)
        {
            PlayerEffectsManager.isUnderSnow = false;
            SnowfallVFXManager.snowMovementHindranceMultiplier = 1f;
        }
    }

    [HarmonyPatch(typeof(BlizzardWeather), nameof(BlizzardWeather.SetColdZoneState))]
    [HarmonyPrefix]
    public static bool BlizzardWeather_Pre_SetColdZoneState(BlizzardWeather __instance)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);

        if (playerSeated ||
            playerOnVan ||
            playerInCab ||
            playerInStorage)
            return false;

        return true;
    }

    [HarmonyPatch(typeof(SnowfallWeather), nameof(SnowfallWeather.SetColdZoneState))]
    [HarmonyPrefix]
    public static bool SnowfallWeather_Pre_SetColdZoneState(SnowfallWeather __instance)
    {
        if (BlizzardWeather.Instance == null)
            return true;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);

        if (!playerSeated &&
            !playerOnVan &&
            !playerInCab &&
            !playerInStorage)
            return true;

        bool playerInCabOrSeated = playerSeated || playerInCab;
        bool isCabEnclosed = IsVanCabinEnclosed(vanController);
        bool isStorageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfVan(playerController, vanController);
        bool inDoorLighting = IsIndoorLighting(playerController);

        if (playerInCabOrSeated)
        {
            if (isCabEnclosed)
            {
                if (vanController.ignitionStarted && vanController.heaterOn)
                    PlayerEffectsManager.isInColdZone = vanController.isHeaterCold;
                else PlayerEffectsManager.isInColdZone = !inDoorLighting;
            }
            else
            {
                PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(playerController, vanController) && BlizzardWeather.Instance.isLocalPlayerInWind;
            }
            return false;
        }
        else if (playerInStorage)
        {
            if (isStorageEnclosed)
            {
                if (vanController.ignitionStarted && vanController.heaterOn)
                    PlayerEffectsManager.isInColdZone = vanController.isHeaterCold;
                else PlayerEffectsManager.isInColdZone = !inDoorLighting;
            }
            else
            {
                PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(playerController, vanController) && BlizzardWeather.Instance.isLocalPlayerInWind;
            }
            return false;
        }
        else if (outsideOfTruck)
        {
            PlayerEffectsManager.isInColdZone = IsWindAllowedVehicle(playerController, vanController) && BlizzardWeather.Instance.isLocalPlayerInWind;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(BlizzardWeather), nameof(BlizzardWeather.Update))]
    [HarmonyPrefix]
    public static bool BlizzardWeather_Pre_Update(BlizzardWeather __instance)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);

        if (!playerSeated &&
            !playerOnVan &&
            !playerInCab &&
            !playerInStorage)
            return true;

        bool isCabEnclosed = IsVanCabinEnclosed(vanController);
        bool isStorageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);
        bool playerInCabOrStorage = playerSeated || playerInCab || playerInStorage;
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfVan(playerController, vanController);
        bool inDoorLighting = IsIndoorLighting(playerController);
        bool playerInCabOrSeated = playerSeated || playerInCab;

        SnowfallWeather.Instance?.Update();

        if (playerInCabOrStorage)
        {
            if ((playerInCabOrSeated && isCabEnclosed) || (playerInStorage && isStorageEnclosed)) __instance.isPlayerInBlizzard = false;
            else __instance.isPlayerInBlizzard = __instance.isLocalPlayerInWind;

            if (vanController.ignitionStarted && vanController.heaterOn)
                PlayerEffectsManager.heatTransferRate = (0.25f * vanController.heaterSpeed);
            else PlayerEffectsManager.heatTransferRate = 0.75f;
        }
        if (outsideOfTruck)
        {
            PlayerEffectsManager.heatTransferRate = 1f;
            __instance.isPlayerInBlizzard = __instance.isLocalPlayerInWind;
        }
        return false;
    }

    public static bool IsWindAllowedVehicle(PlayerControllerB playerController, CruiserXLController vanController)
    {
        if (IsIndoorLighting(playerController)) return false;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInCabOrSeated = playerSeated || playerInCab;
        bool isCabEnclosed = IsVanCabinEnclosed(vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);
        bool isStorageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfVan(playerController, vanController);

        if ((playerInCabOrSeated && !isCabEnclosed) || 
            (playerInStorage && !isStorageEnclosed))
        {
            return true;
        }
        else if (outsideOfTruck)
        {
            return true;
        }
        return false;
    }
   
    [HarmonyPatch(typeof(HeatwavePatches), nameof(HeatwavePatches.CheckConditionsForHeatingStop))]
    [HarmonyPrefix]
    public static bool HeatwavePatches_Pre_CheckConditionsForHeatingStop(PlayerControllerB playerController, ref bool __result)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);

        if (!playerSeated &&
            !playerOnVan &&
            !playerInCab &&
            !playerInStorage)
            return true;

        bool playerInCabOrSeated = playerSeated || playerInCab;
        bool isCabEnclosed = IsVanCabinEnclosed(vanController);
        bool isStorageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);
        bool isHeaterOnAndCool = IsVanBlowerOnAndCool(vanController);
        bool isHeaterOnAndWarm = IsVanBlowerOnAndHot(vanController);
        bool outsideOfTruck = VehicleUtils.IsPlayerOnOutsideOfVan(playerController, vanController);
        bool inDoorLighting = IsIndoorLighting(playerController);

        if ((playerInCabOrSeated && isCabEnclosed) ||
            (playerInStorage && isStorageEnclosed))
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

    [HarmonyPatch(typeof(HeatwavePatches), nameof(HeatwavePatches.CheckConditionsForHeatingPause))]
    [HarmonyPrefix]
    public static bool HeatwavePatches_Pre_CheckConditionsForHeatingPause(PlayerControllerB playerController, ref bool __result)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);

        if (!playerSeated &&
            !playerOnVan &&
            !playerInCab &&
            !playerInStorage)
            return true;

        __result = false;
        return false;
    }
}