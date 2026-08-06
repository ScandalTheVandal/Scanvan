using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(RadMechAI))]
public static class RadMechAIPatches
{
    [HarmonyPatch(nameof(RadMechAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool RadMechAI_Pre_OnCollideWithPlayer(RadMechAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController =
            __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerController == null)
            return true;

        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);

        if (playerSeated)
            return !VehicleUtils.IsSeatedPlayerProtected(playerController, vanController, checkWindows: true, windshieldCheck: true);

        if (!playerOnVan)
            return true;

        if (!playerInCab)
            return true;

        return !VehicleUtils.IsPlayerProtectedByVan(playerController, vanController, checkWindows: true, windshieldCheck: true);
    }
}