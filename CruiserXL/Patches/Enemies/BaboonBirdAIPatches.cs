using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(BaboonBirdAI))]
public static class BaboonBirdAIPatches
{
    [HarmonyPatch(nameof(BaboonBirdAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool BaboonBirdAI_Pre_OnCollideWithPlayer(BaboonBirdAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController = 
            __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inSpecialAnimation || __instance.doingKillAnimation, false);
        if (playerController == null)
            return true;

        // LethalBots stuff
        bool useData = !VehicleUtils.IsPlayerLocal(playerController);

        BoxCollider truckNavMeshBounds = vanController.collisionTrigger.insideTruckNavMeshBounds;
        bool enemyInVan = VehicleUtils.IsEnemyInVan(__instance, truckNavMeshBounds);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController, useData);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController, useData);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController, useData);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool storageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);

        if (playerSeated)
            return !VehicleUtils.IsSeatedPlayerProtected(playerController, vanController, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 5f);

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

        return !VehicleUtils.IsPlayerProtectedByVan(playerController, vanController, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 5f, usePlayerData: useData);
    }
}
