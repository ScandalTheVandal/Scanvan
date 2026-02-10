using GameNetcodeStuff;
using UnityEngine;

namespace CruiserXL.Utils;
public static class VehicleUtils
{
    public static float lastCheckTime = 0f;
    public static float cooldown = 0.25f;

    // kind of unused
    public static bool MeetsSpecialConditionsToCheck()
    {
        if (Time.realtimeSinceStartup - lastCheckTime < cooldown)
            return false;

        lastCheckTime = Time.realtimeSinceStartup;
        return true;
    }

    public static bool IsEnemyInVehicle(EnemyAI enemyScript, CruiserXLController controller)
    {
        if ((controller.collisionTrigger.insideTruckNavMeshBounds.ClosestPoint(enemyScript.transform.position) == enemyScript.transform.position) ||
            (controller.collisionTrigger.insideTruckNavMeshBounds.ClosestPoint(enemyScript.agent.destination) == enemyScript.agent.destination))
            return true;
        return false;
    }

    public static bool IsPlayerInVehicleBounds()
    {
        return PlayerUtils.isPlayerOnTruck;
    }

    public static bool IsPlayerSeatedInVehicle(CruiserXLController controller)
    {
        return PlayerUtils.seatedInTruck;
    }

    public static bool IsSeatedPlayerProtected(PlayerControllerB player, CruiserXLController controller)
    {
        bool driverSideEnclosed = controller.driverSideDoor.boolValue;
        bool passengerSideEnclosed = controller.passengerSideDoor.boolValue;

        if ((player == controller.currentDriver && driverSideEnclosed) ||
            (player == controller.currentMiddlePassenger && (driverSideEnclosed || passengerSideEnclosed)) ||
            (player == controller.currentPassenger && passengerSideEnclosed))
            return false;
        return true;
    }

    public static bool IsPlayerProtectedByVehicle(PlayerControllerB player, CruiserXLController controller)
    {
        if (controller.carDestroyed)
            return false;

        bool driverDoorOpen = controller.driverSideDoor.boolValue;
        bool passengerDoorOpen = controller.passengerSideDoor.boolValue;
        bool backDoorOpen = controller.liftGateOpen;
        bool sideDoorOpen = controller.rightSideDoor.boolValue;

        if (PlayerUtils.isPlayerInCab && (driverDoorOpen || passengerDoorOpen))
            return false;
        else if (PlayerUtils.isPlayerInStorage && (backDoorOpen || sideDoorOpen))
            return false;
        else if (PlayerUtils.isPlayerOnTruck &&
            !PlayerUtils.isPlayerInCab &&
            !PlayerUtils.isPlayerInStorage)
            return false;

        return true;
    }

    public static bool IsPlayerNearTruck(PlayerControllerB player, CruiserXLController vehicle)
    {
        Vector3 vehicleTransform = vehicle.transform.position;
        Vector3 playerTransform = player.transform.position;

        if (Vector3.Distance(playerTransform, vehicleTransform) > 10f)
            return false;

        return true;
    }
}