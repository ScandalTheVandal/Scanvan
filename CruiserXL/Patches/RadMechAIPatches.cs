using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(RadMechAI))]
internal static class RadMechAIPatches
{
    // Special function to protect players from being grabbed
    // by an old-bird, if they're in our truck, and they're
    // considered 'protected'.
    [HarmonyPatch("OnCollideWithPlayer")]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(RadMechAI __instance, Collider other)
    {
        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        if (playerControllerB == null || !playerControllerB.isPlayerControlled || playerControllerB.isPlayerDead)
            return true;

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
            return false;
        }
        // run vanilla logic
        return true;
    }
}