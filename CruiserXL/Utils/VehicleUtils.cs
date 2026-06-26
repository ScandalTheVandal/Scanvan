using GameNetcodeStuff;
using UnityEngine;

namespace ScanVan.Utils;
public static class VehicleUtils
{
    public static bool IsEnemyInVan(EnemyAI enemyScript, CruiserXLController vanController)
    {
        if ((vanController.collisionTrigger.insideTruckNavMeshBounds.ClosestPoint(enemyScript.transform.position) == enemyScript.transform.position) ||
            (vanController.collisionTrigger.insideTruckNavMeshBounds.ClosestPoint(enemyScript.agent.destination) == enemyScript.agent.destination))
            return true;
        return false;
    }

    public static bool IsPlayerInVanBounds(CruiserXLController vanController)
    {
        return vanController.vanZone.playerInZone;
    }

    public static bool IsPlayerInVanCabin(CruiserXLController vanController)
    {
        return vanController.vanCabinZone.playerInZone;
    }

    public static bool IsPlayerInVanStorage(CruiserXLController vanController)
    {
        return vanController.vanStorageZone.playerInZone;
    }

    public static bool IsPlayerSeatedInVan()
    {
        return PlayerUtils.isSeatedInVan;
    }

    public static bool IsSeatedPlayerProtected(PlayerControllerB playerController, CruiserXLController vanController, bool checkWindows = false, bool windshieldCheck = false, bool velocityCheck = false, float velocityMagnitude = 0f)
    {
        float avgVel = vanController.averageVelocity.magnitude;

        if (velocityCheck && avgVel > velocityMagnitude)
            return true;

        bool windshieldBroken = vanController.windshieldBroken;

        if (windshieldCheck && windshieldBroken)
            return false;

        bool leftSideOpen = vanController.driverSideDoor.boolValue || (checkWindows && vanController.driversSideWindowTrigger.boolValue);
        bool rightSideOpen = vanController.passengerSideDoor.boolValue || (checkWindows && vanController.passengersSideWindowTrigger.boolValue);

        if ((playerController == vanController.currentDriver && leftSideOpen) ||
            (playerController == vanController.currentMiddlePassenger && (leftSideOpen || rightSideOpen)) ||
            (playerController == vanController.currentPassenger && rightSideOpen))
            return false;

        return true;
    }

    public static bool IsPlayerProtectedByVan(PlayerControllerB playerController, CruiserXLController vanController, bool checkWindows = false, bool windshieldCheck = false, bool velocityCheck = false, float velocityMagnitude = 0f)
    {
        if (vanController.carDestroyed)
            return false;

        float avgVel = vanController.averageVelocity.magnitude;

        if (velocityCheck && avgVel > velocityMagnitude)
            return true;

        bool windshieldBroken = vanController.windshieldBroken;

        bool frontDoorsOpen = vanController.driverSideDoor.boolValue || 
                              vanController.passengerSideDoor.boolValue;

        bool frontWindowsOpen = vanController.driversSideWindowTrigger.boolValue ||
                                vanController.passengersSideWindowTrigger.boolValue;

        bool backDoorOpen = vanController.liftGateOpen;
        bool sideDoorOpen = vanController.sideDoorOpen;

        if (IsPlayerInVanCabin(vanController) && 
            frontDoorsOpen || 
            (checkWindows && frontWindowsOpen) || 
            (windshieldCheck && windshieldBroken))
            return false;
        else if (IsPlayerInVanStorage(vanController) && 
            (backDoorOpen || sideDoorOpen))
            return false;
        else if (IsPlayerInVanBounds(vanController) &&
            !IsPlayerInVanCabin(vanController) &&
            !IsPlayerInVanStorage(vanController))
            return false;

        return true;
    }

    public static bool IsPlayerNearVan(PlayerControllerB playerController, CruiserXLController vanController)
    {
        Vector3 vehicleTransform = vanController.mainRigidbody.position;
        Vector3 playerTransform = playerController.transform.position;

        if (Vector3.Distance(playerTransform, vehicleTransform) > 10f)
            return false;

        return true;
    }
}