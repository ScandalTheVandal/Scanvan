using GameNetcodeStuff;
using ScanVan.Patches;
using UnityEngine;

namespace ScanVan.Utils;
public static class VehicleUtils
{
    internal static CruiserXLController vanController = null!;

    public static bool IsEnemyInVan(EnemyAI enemyScript, BoxCollider navMeshBounds)
    {
        return (navMeshBounds.ClosestPoint(enemyScript.transform.position) == enemyScript.transform.position) ||
               (navMeshBounds.ClosestPoint(enemyScript.agent.destination) == enemyScript.agent.destination);
    }

    public static bool IsVanStorageEnclosed(CruiserXLController vanScript)
    {
        return !vanScript.liftGateOpen &&
               !vanScript.sideDoorOpen;
    }

    public static bool IsPlayerSeatedInVan(PlayerControllerB playerScript, CruiserXLController vanScript)
    {
        return vanScript != null && playerScript?.overridePhysicsParent == vanScript?.transform;
    }

    public static bool IsPlayerInVanBounds(PlayerControllerB playerScript, CruiserXLController vanScript, bool usePlayerData = false)
    {
        return usePlayerData
            ? PlayerControllerBPatches.playerData[playerScript].playerRidingOnVan
            : vanScript.vanZone.playerInZone;
    }

    public static bool IsPlayerInVanCabin(PlayerControllerB playerScript, CruiserXLController vanScript, bool usePlayerData = false)
    {
        return usePlayerData
            ? PlayerControllerBPatches.playerData[playerScript].playerRidingInVanCab
            : vanScript.vanCabinZone.playerInZone;
    }

    public static bool IsPlayerInVanStorage(PlayerControllerB playerScript, CruiserXLController vanScript, bool usePlayerData = false)
    {
        return usePlayerData
            ? PlayerControllerBPatches.playerData[playerScript].playerRidingInVanStorage
            : vanScript.vanStorageZone.playerInZone;
    }

    public static bool IsPlayerOnOutsideOfVan(PlayerControllerB playerScript, CruiserXLController vanScript, bool usePlayerData = false)
    {
        return IsPlayerInVanBounds(playerScript, vanScript, usePlayerData) &&
               !IsPlayerInVanCabin(playerScript, vanScript, usePlayerData) &&
               !IsPlayerInVanStorage(playerScript, vanScript, usePlayerData) &&
               !IsPlayerSeatedInVan(playerScript, vanScript);
    }

    public static bool IsVanMovingFastEnough(CruiserXLController vanScript, float threshold)
    {
        return vanScript.averageVelocity.magnitude > threshold;
    }

    public static bool IsSeatedPlayerProtected(PlayerControllerB playerScript, CruiserXLController vanScript, bool checkWindows = false, bool windshieldCheck = false, bool velocityCheck = false, float velocityMagnitude = 0f)
    {
        if (velocityCheck && IsVanMovingFastEnough(vanScript, velocityMagnitude))
            return true;

        if (windshieldCheck && vanScript.windshieldBroken)
            return false;

        bool leftSideOpen =
            vanScript.driverSideDoor.boolValue ||
            (checkWindows && vanScript.frontLeftWindow.isWindowOpen);

        bool rightSideOpen =
            vanScript.passengerSideDoor.boolValue ||
            (checkWindows && vanScript.frontRightWindow.isWindowOpen);

        if ((playerScript == vanScript.currentDriver && leftSideOpen) ||
            (playerScript == vanScript.currentMiddlePassenger && (leftSideOpen || rightSideOpen)) ||
            (playerScript == vanScript.currentPassenger && rightSideOpen))
        {
            return false;
        }

        return true;
    }

    public static bool IsPlayerProtectedByVan(PlayerControllerB playerScript, CruiserXLController vanScript, bool checkWindows = false, bool windshieldCheck = false, bool velocityCheck = false, float velocityMagnitude = 0f, bool usePlayerData = false)
    {
        if (vanScript.carDestroyed)
            return false;

        if (velocityCheck && IsVanMovingFastEnough(vanScript, velocityMagnitude))
            return true;

        bool frontDoorsOpen =
            vanScript.driverSideDoor.boolValue ||
            vanScript.passengerSideDoor.boolValue;

        bool frontWindowsOpen =
            vanScript.frontLeftWindow.isWindowOpen ||
            vanScript.frontRightWindow.isWindowOpen;

        if (IsPlayerInVanCabin(playerScript, vanScript, usePlayerData))
        {
            if (frontDoorsOpen ||
                (checkWindows && frontWindowsOpen) ||
                (windshieldCheck && vanScript.windshieldBroken))
            {
                return false;
            }
        }
        else if (IsPlayerInVanStorage(playerScript, vanScript, usePlayerData))
        {
            if (!IsVanStorageEnclosed(vanScript))
                return false;
        }
        else if (IsPlayerOnOutsideOfVan(playerScript, vanScript, usePlayerData))
        {
            return false;
        }

        return true;
    }

    public static bool IsPlayerNearVan(PlayerControllerB playerController, CruiserXLController vanController)
    {
        Vector3 vehicleTransform = vanController.mainRigidbody.position;
        Vector3 playerTransform = playerController.transform.position;

        return Vector3.Distance(playerTransform, vehicleTransform) <= 10f;
    }
}