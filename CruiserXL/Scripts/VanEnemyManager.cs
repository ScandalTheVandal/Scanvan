using GameNetcodeStuff;
using ScandalLib.Interfaces;
using ScandalsTweaks.Scripts;
using UnityEngine;

namespace ScanVan.Scripts;

public class VanEnemyManager : SVehicleEnemyManager
{
    public CruiserXLController vehicleController = null!;

    public override bool ShouldAllowSightThroughVehicle(EnemyAI enemyScript, PlayerControllerB playerScript)
    {
        return vehicleController != null && playerScript is IPlayerManager playerInterface && (vehicleController.IsPlayerSeated(playerScript) || playerInterface.CurrentPlayerZone == vehicleController.vanCabinZone);
    }
}
