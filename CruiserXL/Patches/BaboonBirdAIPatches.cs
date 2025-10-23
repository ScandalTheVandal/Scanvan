using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(BaboonBirdAI))]
internal class BaboonBirdAIPatches
{
    [HarmonyPatch("OnCollideWithPlayer")]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(BaboonBirdAI __instance, Collider other)
    {
        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inSpecialAnimation || __instance.doingKillAnimation, false);
        // safety check
        if (playerControllerB == null || playerControllerB != GameNetworkManager.Instance.localPlayerController)
            return false;

        // check there is one of our trucks on the map
        if (References.truckController == null)
            return true;

        // variables
        Vector3 enemyTransform = __instance.transform.position;
        Collider storageCollider = References.truckController.storageCompartment;

        // cache checks
        Vector3 storageClosest = storageCollider.ClosestPoint(enemyTransform);

        bool inStorage = (storageClosest - enemyTransform).sqrMagnitude < 0.001f;

        // not in our truck, run vanilla logic
        if (!VehicleUtils.IsPlayerInTruck(playerControllerB, References.truckController))
            return true;

        // if the baboon hawk is locked in the back, but the player is not in the back, but the baboon hawk 'grazes' them from inside, prevent the bullshit kill
        if (!VehicleUtils.IsPlayerInTruck(playerControllerB, References.truckController) && !playerControllerB.inVehicleAnimation &&
            inStorage)
            return false;

        // check if the player is protected in our truck, but we need to structure this differently so we can die in the back if the baboon hawk is also in the back
        if (VehicleUtils.IsPlayerProtectedByTruck(playerControllerB, References.truckController) && playerControllerB.inVehicleAnimation)
        {
            // only if the player is seated in the cab should they be protected, regardless of door state (will adjust later, but this is okay for now)
            return false;
        }

        // if the baboon hawk is in the back with the player and they're not seated, allow the baboon hawk to damage them
        if (VehicleUtils.IsPlayerInTruck(playerControllerB, References.truckController) && !playerControllerB.inVehicleAnimation && 
            inStorage)
            return true;

        // otherwise, return false, since they are protected
        return false;
    }
}
