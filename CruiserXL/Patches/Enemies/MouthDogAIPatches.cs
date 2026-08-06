using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(MouthDogAI))]
public static class MouthDogAIPatches
{
    [HarmonyPatch(nameof(MouthDogAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool MouthDogAI_Pre_OnCollideWithPlayer(MouthDogAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController =
            __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation, false);
        if (playerController == null)
            return true;

        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);

        if (playerSeated)
            return !VehicleUtils.IsSeatedPlayerProtected(playerController, vanController, checkWindows: false, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f);

        if (!playerOnVan)
            return true;

        return !VehicleUtils.IsPlayerProtectedByVan(playerController, vanController, checkWindows: false, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f);
    }
}
