using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(ElevatorAnimationEvents))]
public static class ElevatorAnimationEventsPatches
{
    [HarmonyPatch("ElevatorFullyRunning")]
    [HarmonyPrefix]
    static void ElevatorFullyRunning_Prefix()
    {
        if (References.truckController == null) return;
        if (!References.truckController.magnetedToShip) return;

        // save players who are on the magneted truck from being abandoned
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (References.truckController.localPlayerInControl || 
            References.truckController.localPlayerInMiddlePassengerSeat || 
            References.truckController.localPlayerInPassengerSeat)
        {
            localPlayer.isInElevator = true;
            return;
        }

        if (localPlayer.physicsParent == null) return;
        if (localPlayer.physicsParent.TryGetComponent<CruiserXLController>(out var vehicle))
            localPlayer.isInElevator = true;
    }
}
