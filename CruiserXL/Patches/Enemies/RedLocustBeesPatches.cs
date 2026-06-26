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
        static void OnCollideWithPlayer_Prefix(RedLocustBees __instance, Collider other)
        {
            CruiserXLController controller = References.vanController;
            if (controller == null)
                return;

            PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
            if (playerControllerB == null)
                return;

            if (VehicleUtils.IsPlayerSeatedInVan())
            {
                if (VehicleUtils.IsSeatedPlayerProtected(playerController: playerControllerB, vanController: controller, checkWindows: true, windshieldCheck: false, velocityCheck: true, velocityMagnitude: 5f))
                {
                    __instance.timeSinceHittingPlayer = 0f;
                }
                return;
            }

            bool enemyInVan = VehicleUtils.IsEnemyInVan(enemyScript: __instance, vanController: controller);
            bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(vanController: controller);
            bool backDoorsOpen = controller.liftGateOpen || controller.sideDoorOpen;
            if (VehicleUtils.IsPlayerInVanBounds(vanController: controller))
            {
                if (playerInStorage && !backDoorsOpen && !enemyInVan || !playerInStorage && enemyInVan)
                {
                    __instance.timeSinceHittingPlayer = 0f;
                    return;
                }
                if (VehicleUtils.IsPlayerProtectedByVan(playerController: playerControllerB, vanController: controller, checkWindows: true, windshieldCheck: false, velocityCheck: true, velocityMagnitude: 5f))
                {
                    __instance.timeSinceHittingPlayer = 0f;
                    return;
                }
                return;
            }
            if (enemyInVan)
            {
                __instance.timeSinceHittingPlayer = 0f;
                return;
            }
        }
    }