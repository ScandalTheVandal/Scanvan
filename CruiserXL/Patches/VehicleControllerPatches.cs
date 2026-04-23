using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(VehicleController))]
public static class VehicleControllerPatches
{
    // thank you MattyMatty, and DiFFoZ for helping me with this!!
    [HarmonyPatch(nameof(VehicleController.AddEngineOil))]
    [HarmonyPrefix]
    static bool AddEngineOil_Prefix(VehicleController __instance, bool __runOriginal)
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

    [HarmonyPatch(nameof(VehicleController.AddTurboBoost))]
    [HarmonyPrefix]
    static bool AddTurboBoost_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.AddTurboBoost();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.StartMagneting))]
    [HarmonyPrefix]
    static bool StartMagneting_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        // need to investigate some stuff regarding this
        //vehicle.StartMagneting();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.CollectItemsInTruck))]
    [HarmonyPrefix]
    static bool CollectItemsInTruck_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.CollectItemsInTruck();
        return false;
    }


    [HarmonyPatch(nameof(VehicleController.DestroyCar))]
    [HarmonyPrefix]
    static bool DestroyCar_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.DestroyCar();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.ExitDriverSideSeat))]
    [HarmonyPrefix]
    static bool ExitDriverSideSeat_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.ExitDriverSideSeat();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.ExitPassengerSideSeat))]
    [HarmonyPrefix]
    static bool ExitPassengerSideSeat_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.ExitPassengerSideSeat();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.CarReactToObstacle))]
    [HarmonyPrefix]
    static bool CarReactToObstacle_Prefix(VehicleController __instance, bool __runOriginal, ref Vector3 vel, ref Vector3 position, ref Vector3 impulse, ref CarObstacleType type, ref float obstacleSize, ref EnemyAI enemyScript, ref bool dealDamage)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.CarReactToObstacle(vel, position, impulse, type, obstacleSize, enemyScript, dealDamage);
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.DealPermanentDamage))]
    [HarmonyPrefix]
    static bool DealPermanentDamage_Prefix(VehicleController __instance, bool __runOriginal, ref int damageAmount, ref Vector3 damagePosition)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.DealPermanentDamage(damageAmount, damagePosition);
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.DamagePlayerInVehicle))]
    [HarmonyPrefix]
    static bool DamagePlayerInVehicle_Prefix(VehicleController __instance, bool __runOriginal, ref Vector3 vel, ref float magnitude)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.DamagePlayerInVehicle(vel, magnitude);
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.SetInternalStress))]
    [HarmonyPrefix]
    static bool SetInternalStress_Prefix(VehicleController __instance, bool __runOriginal, ref float carStressIncrease)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.SetInternalStress(carStressIncrease);
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.ToggleHeadlightsLocalClient))]
    [HarmonyPrefix]
    static bool ToggleHeadlightsLocalClient_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.ToggleHeadlightsLocalClient();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.SetHeadlightMaterial))]
    [HarmonyPrefix]
    static bool SetHeadlightMaterial_Prefix(VehicleController __instance, bool __runOriginal, ref bool on)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        return false;
    }

    [HarmonyPatch(nameof(VehicleController.SpringDriverSeatLocalClient))]
    [HarmonyPrefix]
    static bool SpringDriverSeatLocalClient_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        return false;
    }

    [HarmonyPatch(nameof(VehicleController.SetRadioOnLocalClient))]
    [HarmonyPrefix]
    static bool SetRadioOnLocalClient_Prefix(VehicleController __instance, bool __runOriginal, ref bool on, ref bool setClip)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.SetRadioOnLocalClient(on, setClip);
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.SwitchRadio))]
    [HarmonyPrefix]
    static bool SwitchRadio_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.SwitchRadio();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.ChangeRadioStation))]
    [HarmonyPrefix]
    static bool ChangeRadioStation_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.ChangeRadioStation();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.StartTryCarIgnition))]
    [HarmonyPrefix]
    static bool StartTryCarIgnition_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.StartTryCarIgnition();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.CancelTryCarIgnition))]
    [HarmonyPrefix]
    static bool CancelTryCarIgnition_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.CancelTryCarIgnition();
        return false;
    }

    [HarmonyPatch(nameof(VehicleController.RemoveKeyFromIgnition))]
    [HarmonyPrefix]
    static bool RemoveKeyFromIgnition_Prefix(VehicleController __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance is not CruiserXLController vehicle)
            return true;

        vehicle.RemoveKeyFromIgnition();
        return false;
    }
}