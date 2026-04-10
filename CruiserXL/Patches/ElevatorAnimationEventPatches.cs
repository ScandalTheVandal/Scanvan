using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(ElevatorAnimationEvents))]
public static class ElevatorAnimationEventsPatches
{
    [HarmonyPatch(nameof(ElevatorAnimationEvents.ElevatorFullyRunning))]
    [HarmonyPrefix]
    static void ElevatorFullyRunning_PlayerSafety_Prefix()
    {
        if (References.truckController == null) return;
        if (!References.truckController.magnetedToShip) return;

        // save players who are on the magneted truck from being abandoned
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (PlayerUtils.seatedInTruck || PlayerUtils.isPlayerOnTruck)
            localPlayer.isInElevator = true;
    }

    [HarmonyPatch(nameof(ElevatorAnimationEvents.ElevatorFullyRunning))]
    [HarmonyPrefix]
    static void ElevatorFullyRunning_ItemSafety_Prefix()
    {
        References.justDepartedIntoOrbit = true;

        CruiserXLController controller = References.truckController;
        if (controller != null && controller.magnetedToShip)
        {
            controller.CollectItemsInTruck();
        }
        References.justDepartedIntoOrbit = false;
    }
}
