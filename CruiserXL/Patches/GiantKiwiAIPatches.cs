using CruiserXL.Utils;
using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(GiantKiwiAI))]
internal static class GiantKiwiAIPatches
{
    [HarmonyPatch("NavigateTowardsTargetPlayer")]
    [HarmonyPrefix]
    static bool NavigateTowardsTargetPlayer_Prefix(GiantKiwiAI __instance)
    {
        if (References.truckController == null)
            return true;

        // Sort of recycled vanilla logic, but to work with out
        // Lift-gate open bool
        if (__instance.setDestinationToPlayerInterval <= 0f)
        {
            if (Vector3.Distance(__instance.targetPlayer.transform.position,
                References.truckController.transform.position) < 10f)
            {
                __instance.setDestinationToPlayerInterval = 0.25f;
                bool isOccupant = References.truckController.currentDriver == __instance.targetPlayer ||
                    References.truckController.currentPassenger == __instance.targetPlayer;

                bool isInTruck = References.truckController.storageCompartment.ClosestPoint(
                    __instance.targetPlayer.transform.position) ==
                    __instance.targetPlayer.transform.position;

                int areaMask = -1;
                if (isOccupant || (isInTruck && !References.truckController.liftGateOpen) ||
                    References.truckController.ontopOfTruckCollider.ClosestPoint(
                    __instance.targetPlayer.transform.position) ==
                    __instance.targetPlayer.transform.position)
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

    [HarmonyPatch("IsEggInsideClosedTruck")]
    [HarmonyPrefix]
    static bool IsEggInsideClosedTruck_Prefix(GiantKiwiAI __instance, KiwiBabyItem egg, bool closedTruck, ref bool __result)
    {
        if (References.truckController == null)
            return true;

        if (egg.parentObject == References.truckController.physicsRegion.parentNetworkObject.transform)
        {
            __result = (!closedTruck ||
                !References.truckController.liftGateOpen);
            return false;
        }
        return true;
    }

    [HarmonyPatch("AnimationEventB")]
    [HarmonyPrefix]
    static void AnimationEventB_Prefix(GiantKiwiAI __instance)
    {
        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;

        if (playerControllerB == null || !playerControllerB.isPlayerControlled || playerControllerB.isPlayerDead)
            return;

        // check there is one of our trucks on the map
        if (References.truckController == null)
            return;

        if (!VehicleUtils.IsPlayerNearTruck(playerControllerB, References.truckController))
            return;

        if (!VehicleUtils.MeetsSpecialConditionsToCheck())
            return;

        // not in our truck, run vanilla logic
        if (!VehicleUtils.IsPlayerInTruck(playerControllerB, References.truckController))
            return;

        // check if the player is protected in our truck
        if (VehicleUtils.IsPlayerProtectedByTruck(playerControllerB, References.truckController))
        {
            // idk if this works but it's worth a try
            __instance.timeSinceHittingPlayer = 0.4f;
            return;
        }
    }
}