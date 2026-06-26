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
    static bool OnCollideWithPlayer_Prefix(PumaAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController controller = References.vanController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerControllerB == null)
            return true;

        if (VehicleUtils.IsPlayerSeatedInVan())
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController: playerControllerB, vanController: controller, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f))
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
            if (VehicleUtils.IsPlayerProtectedByVan(playerController: playerControllerB, vanController: controller, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f))
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