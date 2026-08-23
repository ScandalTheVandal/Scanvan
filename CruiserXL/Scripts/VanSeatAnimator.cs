using GameNetcodeStuff;
using ScandalLib.Scripts;
using ScanVan.Patches;
using ScanVan.Utils;

namespace ScanVan.Scripts;

public class VanSeatAnimator : VehicleSeatAnimator
{
    public CruiserXLController vehicleController = null!;

    public override void OnPlayerLeaveGame()
    {
        if (seatTrigger == vehicleController.driverSeatTrigger) vehicleController.OnDriverLeaveGameServerRpc(seatTrigger.playerScriptInSpecialAnimation);
        else if (seatTrigger == vehicleController.middlePassengerSeatTrigger) vehicleController.OnMiddlePassengerLeaveGameRpc(seatTrigger.playerScriptInSpecialAnimation);
        else if (seatTrigger == vehicleController.passengerSeatTrigger) vehicleController.OnPassengerLeaveGameRpc(seatTrigger.playerScriptInSpecialAnimation);
        else return;
    }

    public override void ResetPlayerData(PlayerControllerB player)
    {
        player.ladderCameraHorizontal = 0f;
    }
}
