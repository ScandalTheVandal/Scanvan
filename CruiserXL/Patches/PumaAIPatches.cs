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
    static void OnCollideWithPlayer_Prefix(PumaAI __instance)
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
            // reset the timer, to prevent the puma from damaging the player, i guess
            if (avgSpeed < 10f && VehicleUtils.IsSeatedPlayerProtected(playerControllerB, controller))
            {
                __instance.timeAtLastScratch = Time.realtimeSinceStartup;
                return;
            }
            else if (avgSpeed >= 10f)
            {
                __instance.timeAtLastScratch = Time.realtimeSinceStartup;
            }
        }

        bool enemyInTruck = VehicleUtils.IsEnemyInVehicle(__instance, controller);
        if (VehicleUtils.IsPlayerInVehicleBounds())
        {
            // enemy is not in the back with the player
            if (PlayerUtils.isPlayerInStorage && !enemyInTruck)
                __instance.timeAtLastScratch = 4f;

            // player is standing in the cab
            if (PlayerUtils.isPlayerInCab)
            {
                if (avgSpeed >= 10f ||
                    (!controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue && 
                    !controller.driversSideWindowTrigger.boolValue && !controller.passengersSideWindowTrigger.boolValue))
                {
                    __instance.timeAtLastScratch = Time.realtimeSinceStartup;
                }
            }
        }
        else
        {
            // reset the timer, to prevent the puma from damaging the player
            if (enemyInTruck)
                __instance.timeAtLastScratch = Time.realtimeSinceStartup;
        }
    }
}