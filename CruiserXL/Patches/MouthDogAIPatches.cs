using CruiserXL;
using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(MouthDogAI))]
public static class MouthDogAIPatches
{
    [HarmonyPatch("OnCollideWithPlayer")]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(MouthDogAI __instance, Collider other)
    {
        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inKillAnimation, false);
        if (playerControllerB == null || playerControllerB != GameNetworkManager.Instance.localPlayerController)
            return false;

        if (References.truckController == null)
            return true;

        // not in our truck, run vanilla logic
        if (!VehicleUtils.IsPlayerInTruck(playerControllerB, References.truckController))
            return true;
        // this check is also important to prevent returning false if the player isn't in our truck

        // check if the player is protected in our truck
        if (VehicleUtils.IsPlayerProtectedByTruck(playerControllerB, References.truckController))
        {
            // player is protected, so do not allow the kill
            return false;
        }
        if (__instance.currentBehaviourStateIndex == 3 && 
            playerControllerB.inVehicleAnimation)
        {
            playerControllerB.CancelSpecialTriggerAnimations();
            playerControllerB.inAnimationWithEnemy = __instance;
            __instance.KillPlayerServerRpc((int)playerControllerB.playerClientId);
            return false;
            // force kill the player, otherwise the original inVehicleAnimation check takes prescedant, which we don't want
        }
        return true;
        // otherwise, allow vanilla code to run (we use a seperate bool for the rear lift gate protection check, so this should work fine
    }
}
