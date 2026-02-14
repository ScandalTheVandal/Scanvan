using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(RadMechAI))]
internal static class RadMechAIPatches
{
    // patch to protect players from being grabbed
    // by an old-bird, if they're in our truck, and they're
    // considered 'protected'.
    [HarmonyPatch(nameof(RadMechAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(RadMechAI __instance, Collider other)
    {
        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerControllerB == null)
            return true;

        if (References.truckController == null)
            return true;
        CruiserXLController controller = References.truckController;

        // check if the player is seated in our truck
        if (VehicleUtils.IsPlayerSeatedInVehicle(controller))
        {
            // windshield is missing, so allow the grab
            if (controller.windshieldBroken && controller.averageVelocity.magnitude <= 10f)
                return false;
            // player is protected, so do not allow the grab
            if (VehicleUtils.IsSeatedPlayerProtected(playerControllerB, controller))
                return false;
            return true; // allow vanilla logic to run
        }

        // not seated in our truck, but within the vehicle bounds
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            // windshield is missing, so allow the grab
            if (PlayerUtils.isPlayerInCab && (controller.windshieldBroken && controller.averageVelocity.magnitude <= 10f))
                return false;
            if (VehicleUtils.IsPlayerProtectedByVehicle(playerControllerB, controller))
                return false; // player is protected, so do not allow the grab

            return true; // player is not protected, allow vanilla logic to run
        }

        // not in our truck, run vanilla logic
        return true;
    }
}