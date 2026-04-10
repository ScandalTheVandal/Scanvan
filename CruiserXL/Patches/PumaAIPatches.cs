using CruiserXL.Utils;
using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(PumaAI))]
internal static class PumaAIPatches
{
    [HarmonyPatch(nameof(PumaAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(PumaAI __instance, Collider other)
    {
        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);

        if (playerControllerB == null)
            return true;

        if (References.truckController == null)
            return true;
        CruiserXLController controller = References.truckController;
        var avgSpeed = controller.averageVelocity.magnitude;

        // check if the player is seated in our truck
        if (VehicleUtils.IsPlayerSeatedInVehicle(controller))
        {
            if (avgSpeed >= 2f)
                return false;

            // player is protected, so do not allow the kill
            if (VehicleUtils.IsSeatedPlayerProtected(playerControllerB, controller))
                return false;
            return true; // allow vanilla logic to run
        }

        bool enemyInTruck = VehicleUtils.IsEnemyInVehicle(__instance, controller);
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            // enemy is not in the back with the player
            if (PlayerUtils.isPlayerInStorage && !enemyInTruck)
                return false;

            // player is just riding on the truck
            if (!PlayerUtils.isPlayerInStorage && !PlayerUtils.isPlayerInCab)
                return avgSpeed < 2f;

            // player is standing in the cab
            if (PlayerUtils.isPlayerInCab)
            {
                // vehicle is going slow and either side door is open
                if (avgSpeed < 2f &&
                    (controller.driverSideDoor.boolValue || controller.passengerSideDoor.boolValue))
                {
                    return true;
                }
                return false;
            }
            return true;
        }
        else
        {
            if (enemyInTruck)
                return false;
        }
        return true; // run vanilla logic
    }
}