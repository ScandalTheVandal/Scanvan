using ScanVan.Utils;
using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(GiantKiwiAI))]
public static class GiantKiwiAIPatches
{
    [HarmonyPatch(nameof(GiantKiwiAI.IsEggInsideClosedTruck))]
    [HarmonyPrefix]
    static bool GiantKiwiAI_Pre_IsEggInsideClosedTruck(GiantKiwiAI __instance, KiwiBabyItem egg, bool closedTruck, ref bool __result, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController controller = VehicleUtils.vanController;
        if (controller == null)
            return true;

        if (egg.parentObject == controller.physicsRegion.parentNetworkObject.transform)
        {
            __result = !controller.liftGateOpen;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(GiantKiwiAI.AnimationEventB))]
    [HarmonyPrefix]
    static void GiantKiwiAI_Pre_AnimationEventB(GiantKiwiAI __instance)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (playerController == null ||
            !playerController.isPlayerControlled ||
            playerController.isPlayerDead)
            return;

        BoxCollider truckNavMeshBounds = vanController.collisionTrigger.insideTruckNavMeshBounds;
        bool enemyInVan = VehicleUtils.IsEnemyInVan(__instance, truckNavMeshBounds);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool storageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);

        if (playerSeated)
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController, vanController, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f))
                __instance.timeSinceHittingPlayer = 0.4f;
            return;
        }

        if (!playerOnVan)
        {
            if (enemyInVan)
                __instance.timeSinceHittingPlayer = 0.4f;
            return;
        }

        bool protectedByStorage =
            playerInStorage &&
            storageEnclosed &&
            !enemyInVan;

        if (protectedByStorage)
        {
            __instance.timeSinceHittingPlayer = 0.4f;
            return;
        }

        if (enemyInVan && playerInStorage)
            return;

        if (VehicleUtils.IsPlayerProtectedByVan(playerController, vanController, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f))
            __instance.timeSinceHittingPlayer = 0.4f;
    }
}