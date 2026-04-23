using ScanVan.Utils;
using GameNetcodeStuff;
using UnityEngine;
using HarmonyLib;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(RedLocustBees))]
internal static class RedLocustBeesPatches
{
    [HarmonyPatch(nameof(RedLocustBees.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static void OnCollideWithPlayer_Prefix(RedLocustBees __instance, ref Collider other)
    {
        if (References.truckController == null)
            return;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, false, false);
        if (playerControllerB == null)
            return;

        CruiserXLController controller = References.truckController;
        var avgSpeed = controller.averageVelocity.magnitude;

        // check if the player is seated in our truck
        if (VehicleUtils.IsPlayerSeatedInVehicle(controller))
        {
            // reset the timer, to prevent the bees from damaging the player, i guess
            if (avgSpeed < 10f && VehicleUtils.IsSeatedPlayerProtected(playerControllerB, controller))
            {
                __instance.timeSinceHittingPlayer = 0f;
                return;
            }
            else if (avgSpeed >= 10f)
            {
                __instance.timeSinceHittingPlayer = 0f;
            }
        }

        bool enemyInTruck = VehicleUtils.IsEnemyInVehicle(__instance, controller);
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            // enemy is not in the back with the player
            if (PlayerUtils.isPlayerInStorage && !enemyInTruck)
                __instance.timeSinceHittingPlayer = 0f;

            // player is standing in the cab
            if (PlayerUtils.isPlayerInCab)
            {
                if (avgSpeed >= 10f ||
                    (!controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue && 
                    !controller.driversSideWindowTrigger.boolValue && !controller.passengersSideWindowTrigger.boolValue))
                {
                    __instance.timeSinceHittingPlayer = 0f;
                }
            }
        }
        else
        {
            // reset the timer, to prevent the bees from damaging the player
            if (enemyInTruck)
                __instance.timeSinceHittingPlayer = 0f;
        }
    }
}