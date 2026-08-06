using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(ForestGiantAI))]
public static class ForestGiantAIPatches
{
    [HarmonyPatch(nameof(ForestGiantAI.AnimationEventA))]
    [HarmonyPrefix]
    static bool ForestGiantAI_Pre_AnimationEventA(ForestGiantAI __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (playerController == null)
            return false;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);

        if (playerSeated ||
            playerInCab ||
            playerInStorage)
            return false;

        return true;
    }

    [HarmonyPatch(nameof(ForestGiantAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool ForestGiantAI_Pre_OnCollideWithPlayer(ForestGiantAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController =
            __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inEatingPlayerAnimation, false);
        if (playerController == null)
            return true;

        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);

        if (playerSeated)
            return !VehicleUtils.IsSeatedPlayerProtected(playerController, vanController, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 8f);

        if (!playerOnVan)
            return true;

        return !VehicleUtils.IsPlayerProtectedByVan(playerController, vanController, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 8f);
    }
}