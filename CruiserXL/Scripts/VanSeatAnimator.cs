using GameNetcodeStuff;
using ScandalsTweaks.Scripts;
using ScanVan.Patches;
using ScanVan.Utils;

namespace ScanVan.Scripts;

public class VanSeatAnimator : VehicleSeatAnimator
{
    public CruiserXLController vehicleController = null!;

    public override void OnPlayerLeaveGame()
    {
        if (seatTrigger == vehicleController.driverSeatTrigger) vehicleController.OnDriverLeaveGameServerRpc((int)seatTrigger.playerScriptInSpecialAnimation.playerClientId);
        else if (seatTrigger == vehicleController.middlePassengerSeatTrigger) vehicleController.OnMiddlePassengerLeaveGameRpc((int)seatTrigger.playerScriptInSpecialAnimation.playerClientId);
        else if (seatTrigger == vehicleController.passengerSeatTrigger) vehicleController.OnPassengerLeaveGameRpc((int)seatTrigger.playerScriptInSpecialAnimation.playerClientId);
        else return;
    }

    public override void ResetPlayerData(PlayerControllerB player)
    {
        player.ladderCameraHorizontal = 0f;
    }

    public override void SetPlayerSeated(bool setSeated, PlayerControllerB localPlayer)
    {
        PlayerUtils.isSeatedInVan = setSeated;
    }
}
