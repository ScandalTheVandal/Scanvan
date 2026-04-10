using CruiserXL.Utils;
using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(GiantKiwiAI))]
internal static class GiantKiwiAIPatches
{
    [HarmonyPatch(nameof(GiantKiwiAI.NavigateTowardsTargetPlayer))]
    [HarmonyPrefix]
    static bool NavigateTowardsTargetPlayer_Prefix(GiantKiwiAI __instance)
    {
        if (References.truckController == null)
            return true;
        CruiserXLController controller = References.truckController;

        // this is super hacky
        if (__instance.setDestinationToPlayerInterval <= 0f)
        {
            if (Vector3.Distance(__instance.targetPlayer.transform.position,
                controller.transform.position) < 10f)
            {
                __instance.setDestinationToPlayerInterval = 0.25f;

                bool isOccupant = controller.currentDriver == __instance.targetPlayer || 
                                  controller.currentMiddlePassenger == __instance.targetPlayer || 
                                  controller.currentPassenger == __instance.targetPlayer;

                bool inTruckBounds = controller.vehicleBounds.ClosestPoint(
                    __instance.targetPlayer.transform.position) == 
                    __instance.targetPlayer.transform.position;
                bool onTopOfTruck = controller.ontopOfTruckCollider.ClosestPoint(
                    __instance.targetPlayer.transform.position) == 
                    __instance.targetPlayer.transform.position;
                bool inTruckStorage = PlayerUtils.isPlayerInStorage;
                bool inTruckCab = PlayerUtils.isPlayerInCab && !PlayerUtils.seatedInTruck;

                int areaMask = -1;
                if (isOccupant ||
                    inTruckBounds ||
                    inTruckStorage ||
                    inTruckCab ||
                    onTopOfTruck)
                {
                    __instance.targetPlayerIsInTruck = true;
                    areaMask = -33;
                }
                __instance.destination = RoundManager.Instance.GetNavMeshPosition(__instance.targetPlayer.transform.position,
                    RoundManager.Instance.navHit, 5.5f,
                    areaMask);
                return false;
            }
            return true;
        }
        return true;
    }

    [HarmonyPatch(nameof(GiantKiwiAI.IsEggInsideClosedTruck))]
    [HarmonyPrefix]
    static bool IsEggInsideClosedTruck_Prefix(GiantKiwiAI __instance, KiwiBabyItem egg, bool closedTruck, ref bool __result)
    {
        if (References.truckController == null)
            return true;
        CruiserXLController controller = References.truckController;

        if (egg.parentObject == controller.physicsRegion.parentNetworkObject.transform)
        {
            __result = (!closedTruck ||
                !controller.liftGateOpen);
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(GiantKiwiAI.AnimationEventB))]
    [HarmonyPrefix]
    static void AnimationEventB_Prefix(GiantKiwiAI __instance)
    {
        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;

        if (playerControllerB == null || 
            !playerControllerB.isPlayerControlled || 
            playerControllerB.isPlayerDead)
            return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;
        var avgSpeed = controller.averageVelocity.magnitude;

        // check if the player is seated in our truck
        if (VehicleUtils.IsPlayerSeatedInVehicle(controller))
        {
            // reset the timer, to prevent the kiwi from damaging the player, i guess
            if (avgSpeed < 10f && VehicleUtils.IsSeatedPlayerProtected(playerControllerB, controller))
            {
                __instance.timeSinceHittingPlayer = 0.4f;
                return;
            }
            else if (avgSpeed >= 10f)
            {
                __instance.timeSinceHittingPlayer = 0.4f;
            }
        }

        bool enemyInTruck = VehicleUtils.IsEnemyInVehicle(__instance, controller);
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            // enemy is not in the back with the player
            if (PlayerUtils.isPlayerInStorage && !enemyInTruck)
                __instance.timeSinceHittingPlayer = 0.4f;

            // player is standing in the cab
            if (PlayerUtils.isPlayerInCab)
            {
                if (avgSpeed >= 10f ||
                    (!controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue && 
                    !controller.driversSideWindowTrigger.boolValue && !controller.passengersSideWindowTrigger.boolValue))
                {
                    __instance.timeSinceHittingPlayer = 0.4f;
                }
            }
        }
        else
        {
            // reset the timer, to prevent the kiwi from damaging the player
            if (enemyInTruck)
                __instance.timeSinceHittingPlayer = 0.4f;
        }
    }
}