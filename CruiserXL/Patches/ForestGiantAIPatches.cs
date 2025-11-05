using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(ForestGiantAI))]
internal static class ForestGiantAIPatches
{
    [HarmonyPatch("OnCollideWithPlayer")]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(ForestGiantAI __instance, Collider other)
    {
        if (__instance.inSpecialAnimationWithPlayer != null || __instance.inEatingPlayerAnimation)
            return true;

        if (__instance.stunNormalizedTimer >= 0f)
            return true;

        if (__instance.currentBehaviourStateIndex == 2)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inEatingPlayerAnimation, false);
        if (playerControllerB == null || playerControllerB != GameNetworkManager.Instance.localPlayerController)
            return false;

        if (References.truckController == null)
            return true;

        if (!VehicleUtils.IsPlayerNearTruck(playerControllerB, References.truckController))
            return true;

        if (!VehicleUtils.MeetsSpecialConditionsToCheck())
            return false;

        // not in our truck, run vanilla logic
        if (!VehicleUtils.IsPlayerInTruck(playerControllerB, References.truckController))
            return true;
        // this check is also important to prevent returning false if the player isn't in our truck

        // check if the player is protected in our truck
        if (VehicleUtils.IsPlayerProtectedByTruck(playerControllerB, References.truckController))
        {
            // player is protected, so do not allow the grab
            //Plugin.Logger.LogMessage("Giant Collide A");
            return false;
        }
        //Plugin.Logger.LogMessage("Giant Collide B");
        playerControllerB.CancelSpecialTriggerAnimations();
        //if (__instance.currentBehaviourStateIndex == 1)
        //    __instance.GrabPlayerServerRpc((int)playerControllerB.playerClientId);
        return true;
        // force grab, otherwise if we return true, the original physicsParent condition takes prescedent and causes inconsistent behaviour.
    }
}