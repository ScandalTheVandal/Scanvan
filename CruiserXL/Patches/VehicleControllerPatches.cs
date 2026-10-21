using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches;

[HarmonyPatch]
public static class VehicleControllerPatches
{
    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.DisableVehicleCollisionForAllPlayers))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_DisableVehicleCollisionForAllPlayers(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.DisableVehicleCollisionForAllPlayers();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.EnableVehicleCollisionForAllPlayers))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_EnableVehicleCollisionForAllPlayers(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.EnableVehicleCollisionForAllPlayers();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SetVehicleCollisionForPlayer))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_SetVehicleCollisionForPlayer(VehicleController __instance, bool __runOriginal, bool setEnabled, PlayerControllerB player)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.SetVehicleCollisionForPlayer(setEnabled, player);
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SetBackDoorOpen))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_SetBackDoorOpen(VehicleController __instance, bool __runOriginal, bool open)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.SetBackDoorOpen(open);
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SetFrontCabinLightOn))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_SetFrontCabinLightOn(VehicleController __instance, bool __runOriginal, bool setOn)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.SetFrontCabinLightOn(setOn);
        return false;

    }

    // thank you MattyMatty, and DiFFoZ for helping me with this!!
    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.AddEngineOil))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_AddEngineOil(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            // somebody else has redirected the function ignore the call
            return false;

        if (__instance is not CruiserXLController vehicle)
            // not us run the original code
            return true;

        // our class run our code, and skip original.
        vehicle.AddEngineOil();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.AddTurboBoost))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_AddTurboBoost(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.AddTurboBoost();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.StartMagneting))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_StartMagneting(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController)
            return true;

        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.CollectItemsInTruck))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_CollectItemsInTruck(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController)
            return true;

        return false;
    }


    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.DestroyCar))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_DestroyCar(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.DestroyCar();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.ExitDriverSideSeat))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_ExitDriverSideSeat(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController)
            return true;

        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.ExitPassengerSideSeat))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_ExitPassengerSideSeat(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController)
            return true;

        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.CarReactToObstacle))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_CarReactToObstacle(VehicleController __instance, bool __runOriginal, ref Vector3 vel, ref Vector3 position, ref Vector3 impulse, ref CarObstacleType type, ref float obstacleSize, ref EnemyAI enemyScript, ref bool dealDamage)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.CarReactToObstacle(vel, position, impulse, type, obstacleSize, enemyScript, dealDamage);
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.DealPermanentDamage))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_DealPermanentDamage(VehicleController __instance, bool __runOriginal, ref int damageAmount, ref Vector3 damagePosition)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.DealPermanentDamage(damageAmount, damagePosition);
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.DamagePlayerInVehicle))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_DamagePlayerInVehicle(VehicleController __instance, bool __runOriginal, ref Vector3 vel, ref float magnitude)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.DamagePlayerInVehicle(vel, magnitude);
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SetInternalStress))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_SetInternalStress(VehicleController __instance, bool __runOriginal, ref float carStressIncrease)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.SetInternalStress(carStressIncrease);
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.ToggleHeadlightsLocalClient))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_ToggleHeadlightsLocalClient(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.ToggleHeadlightsLocalClient();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SetHeadlightMaterial))]
    [HarmonyPrefix]
    static bool SetHeadlightMaterial_Prefix(VehicleController __instance, bool __runOriginal, ref bool on)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController)
            return true;

        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SpringDriverSeatLocalClient))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_SpringDriverSeatLocalClient(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController)
            return true;

        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SetRadioOnLocalClient))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_SetRadioOnLocalClient(VehicleController __instance, bool __runOriginal, ref bool on, ref bool setClip)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController)
            return true;

        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.SwitchRadio))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_SwitchRadio(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.SwitchRadio();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.ChangeRadioStation))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_ChangeRadioStation(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.ChangeRadioStation(true);
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.StartTryCarIgnition))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_StartTryCarIgnition(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.StartTryCarIgnition();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.CancelTryCarIgnition))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_CancelTryCarIgnition(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.CancelTryCarIgnition();
        return false;
    }

    [HarmonyPatch(typeof(VehicleController), nameof(VehicleController.RemoveKeyFromIgnition))]
    [HarmonyPrefix]
    static bool VehicleController_Pre_RemoveKeyFromIgnition(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.RemoveKeyFromIgnition();
        return false;
    }
}