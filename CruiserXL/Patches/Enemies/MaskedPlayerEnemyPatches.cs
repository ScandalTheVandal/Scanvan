using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(MaskedPlayerEnemy))]
public static class MaskedPlayerEnemyPatches
{
    [HarmonyPatch(nameof(MaskedPlayerEnemy.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(MaskedPlayerEnemy __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController controller = References.vanController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation || __instance.startingKillAnimationLocalClient || !__instance.enemyEnabled, false);
        if (playerControllerB == null)
            return true;

        if (VehicleUtils.IsPlayerSeatedInVan())
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController: playerControllerB, vanController: controller, checkWindows: false, windshieldCheck: false, velocityCheck: true, velocityMagnitude: 5f))
            {
                return false;
            }
            return true;
        }
        bool enemyInVan = VehicleUtils.IsEnemyInVan(enemyScript: __instance, vanController: controller);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(vanController: controller);
        bool backDoorsOpen = controller.liftGateOpen || controller.sideDoorOpen;
        if (VehicleUtils.IsPlayerInVanBounds(vanController: controller))
        {
            if (playerInStorage && !backDoorsOpen && !enemyInVan || !playerInStorage && enemyInVan)
            {
                return false;
            }
            if (VehicleUtils.IsPlayerProtectedByVan(playerController: playerControllerB, vanController: controller, checkWindows: false, windshieldCheck: false, velocityCheck: true, velocityMagnitude: 5f))
            {
                return false;
            }
            return true;
        }
        if (enemyInVan)
        {
            return false;
        }
        return true;
    }
}