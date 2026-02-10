using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(ForestGiantAI))]
internal static class ForestGiantAIPatches
{
    [HarmonyPatch(nameof(ForestGiantAI.AnimationEventA))]
    [HarmonyPrefix]
    static bool AnimationEventA_Prefix(ForestGiantAI __instance)
    {
        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        if (playerControllerB == null)
            return false;

        if (References.truckController == null)
            return true;

        // do not allow fall death in the truck
        if (PlayerUtils.isPlayerInCab ||
            PlayerUtils.isPlayerInStorage ||
            PlayerUtils.seatedInTruck)
            return false;

        // not in our truck, run vanilla logic
        return true;
    }

    [HarmonyPatch(nameof(ForestGiantAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(ForestGiantAI __instance, Collider other)
    {
        if ((__instance.inSpecialAnimationWithPlayer != null || __instance.inEatingPlayerAnimation) ||
            __instance.stunNormalizedTimer >= 0f ||
            __instance.currentBehaviourStateIndex == 2)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inEatingPlayerAnimation, false);
        if (playerControllerB == null )
            return true;

        if (References.truckController == null)
            return true;
        CruiserXLController controller = References.truckController;

        // check if the player is seated in our truck
        if (VehicleUtils.IsPlayerSeatedInVehicle(controller))
        {
            // player is protected, so do not allow the kill
            if (VehicleUtils.IsSeatedPlayerProtected(playerControllerB, controller))
                return false;
            return true; // allow vanilla logic to run (no inVehicleAnimation check)
        }

        // not seated in our truck, but within the vehicle bounds
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            if (VehicleUtils.IsPlayerProtectedByVehicle(playerControllerB, controller))
                return false; // player is protected, so do not allow the kill

            return true; // player is not protected, allow vanilla logic to run
        }

        // not in our truck, run vanilla logic
        return true;
    }
}