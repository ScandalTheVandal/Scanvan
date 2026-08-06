    using GameNetcodeStuff;
    using HarmonyLib;
    using ScanVan.Utils;
    using UnityEngine;

    namespace ScanVan.Patches.Enemies;

    [HarmonyPatch(typeof(RedLocustBees))]
    public static class RedLocustBeesPatches
    {
    [HarmonyPatch(nameof(RedLocustBees.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static void RedLocustBees_Pre_OnCollideWithPlayer(RedLocustBees __instance, Collider other)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        PlayerControllerB playerController =
            __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerController == null)
            return;

        BoxCollider truckNavMeshBounds = vanController.collisionTrigger.insideTruckNavMeshBounds;
        bool enemyInVan = VehicleUtils.IsEnemyInVan(__instance, truckNavMeshBounds);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool storageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);

        if (playerSeated)
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController, vanController, checkWindows: true, windshieldCheck: false, velocityCheck: true, velocityMagnitude: 5f))
                __instance.timeSinceHittingPlayer = 0f;
            return;
        }

        if (!playerOnVan)
        {
            if (enemyInVan)
                __instance.timeSinceHittingPlayer = 0f;
            return;
        }

        bool protectedByStorage =
            playerInStorage &&
            storageEnclosed &&
            !enemyInVan;

        if (protectedByStorage)
        {
            __instance.timeSinceHittingPlayer = 0f;
            return;
        }

        if (enemyInVan && playerInStorage)
            return;

        if (!playerInCab)
            return;

        if (VehicleUtils.IsPlayerProtectedByVan(playerController, vanController, checkWindows: true, windshieldCheck: false, velocityCheck: true, velocityMagnitude: 5f))
            __instance.timeSinceHittingPlayer = 0f;
    }
}