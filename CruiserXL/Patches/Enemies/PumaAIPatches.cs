using ScanVan.Utils;
using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(PumaAI))]
public static class PumaAIPatches
{
    [HarmonyPatch(nameof(PumaAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool PumaAI_Pre_OnCollideWithPlayer(PumaAI __instance, Collider other, bool __runOriginal)
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

        BoxCollider truckNavMeshBounds = vanController.collisionTrigger.insideTruckNavMeshBounds;
        bool enemyInVan = VehicleUtils.IsEnemyInVan(__instance, truckNavMeshBounds);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool storageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);

        if (playerSeated)
            return !VehicleUtils.IsSeatedPlayerProtected(playerController, vanController, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f);

        if (!playerOnVan)
            return !enemyInVan;

        bool protectedByStorage =
            playerInStorage &&
            storageEnclosed &&
            !enemyInVan;

        if (protectedByStorage)
            return false;

        if (enemyInVan && playerInStorage)
            return true;

        if (!playerInCab)
            return true;

        return !VehicleUtils.IsPlayerProtectedByVan(playerController, vanController, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f);
    }
}