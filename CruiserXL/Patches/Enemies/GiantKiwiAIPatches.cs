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
    static bool IsEggInsideClosedTruck_Prefix(GiantKiwiAI __instance, KiwiBabyItem egg, bool closedTruck, ref bool __result, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController controller = References.vanController;
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
    static void AnimationEventB_Prefix(GiantKiwiAI __instance)
    {
        CruiserXLController controller = References.vanController;
        if (controller == null)
            return;

        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        if (playerControllerB == null ||
            !playerControllerB.isPlayerControlled ||
            playerControllerB.isPlayerDead)
            return;


        if (VehicleUtils.IsPlayerSeatedInVan())
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController: playerControllerB, vanController: controller, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f))
            {
                __instance.timeSinceHittingPlayer = 0.4f;
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
                __instance.timeSinceHittingPlayer = 0.4f;
                return;
            }
            if (VehicleUtils.IsPlayerProtectedByVan(playerController: playerControllerB, vanController: controller, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 10f))
            {
                __instance.timeSinceHittingPlayer = 0.4f;
                return;
            }
            return;
        }
        if (enemyInVan)
        {
            __instance.timeSinceHittingPlayer = 0.4f;
            return;
        }
    }
}