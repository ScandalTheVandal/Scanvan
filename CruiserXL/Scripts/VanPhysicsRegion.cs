using GameNetcodeStuff;
using ScandalLib.Interfaces;
using ScandalLib.Patches.Enemies;
using ScandalLib.Scripts;
using ScanVan.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace ScanVan.Scripts;

public class VanPhysicsRegion : ExtendedPhysicsRegion
{
    public CruiserXLController vehicleController = null!;

    // must null check before calling
    internal bool IsLocalPlayerInShelter()
    {
        return (vehicleController.vanCabinZone.playerInZone && vehicleController.IsCabinEnclosed()) || 
               (vehicleController.vanStorageZone.playerInZone && vehicleController.IsStorageEnclosed());
    }

    public override bool IsTargetPlayerInVehicle(PlayerControllerB playerScript, EnemyAI enemyAi)
    {
        if (playerScript is not IPlayerManager playerInterface)
            return false;
        switch (enemyAi)
        {
            case GiantKiwiAI:
                return vehicleController.IsPlayerSeated(playerScript) ||
                    ((vehicleController.IsPlayerInCabin(playerScript) && vehicleController.IsCabinEnclosed()) ||
                     (vehicleController.IsPlayerInStorage(playerScript) && vehicleController.IsStorageEnclosed()) ||
                     vehicleController.ontopOfTruckCollider.ClosestPoint(playerScript.transform.position) == playerScript.transform.position);
            default:
                return false;
        }
    }

    public override bool IsPlayerProtectedFromEnemy(PlayerControllerB playerScript, EnemyAI enemyAi)
    {
        switch (enemyAi)
        {
            case BaboonBirdAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: true, checkWindshield: true, velocityCheck: true, velocityThreshold: 8f, enemyCanBeInVan: true);
            case ForestGiantAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: true, checkWindshield: true, velocityCheck: true, velocityThreshold: 10f);
            case GiantKiwiAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: true, checkWindshield: true, velocityCheck: true, velocityThreshold: 12f, enemyCanBeInVan: true);
            case MaskedPlayerEnemy:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: false, checkWindshield: false, velocityCheck: true, velocityThreshold: 4f, enemyCanBeInVan: true);
            case MouthDogAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: false, checkWindshield: true, velocityCheck: true, velocityThreshold: 12f);
            case PumaAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: true, checkWindshield: true, velocityCheck: true, velocityThreshold: 9f, enemyCanBeInVan: true);
            case RadMechAI:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: true, checkWindshield: true, velocityCheck: true, velocityThreshold: 10f);
            case RedLocustBees:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: true, checkWindshield: true, velocityCheck: true, velocityThreshold: 8f, enemyCanBeInVan: true);
            case SandWormAI:
                return vehicleController.IsPlayerSeated(playerScript) ||
                       vehicleController.IsPlayerInStorage(playerScript) ||
                       vehicleController.IsPlayerInCabin(playerScript);
            case BushWolfEnemy:
                return IsPlayerProtectedByVehicle(playerScript, enemyAi, checkWindows: false, checkWindshield: false, velocityCheck: true, velocityThreshold: 6f, enemyCanBeInVan: true);
            default:
                return false;
        }
    }

    internal bool IsPlayerProtectedByVehicle(PlayerControllerB playerScript, EnemyAI enemyAi, bool checkWindows = false, bool checkWindshield = false, bool velocityCheck = false, float velocityThreshold = 0f, bool enemyCanBeInVan = false)
    {
        var enemyInVan = enemyCanBeInVan && IsEnemyInVan(enemyAi, vehicleController.collisionTrigger.insideTruckNavMeshBounds);

        var playerOnVan = vehicleController.IsPlayerAttached(playerScript);
        var playerInCab = vehicleController.IsPlayerInCabin(playerScript);
        var playerInStorage = vehicleController.IsPlayerInStorage(playerScript);
        var playerSeated = vehicleController.IsPlayerSeated(playerScript);
        var storageEnclosed = vehicleController.IsStorageEnclosed();

        if (playerSeated)
            return IsSeatedPlayerProtected(playerScript, checkWindows, checkWindshield, velocityCheck, velocityThreshold);

        if (!playerOnVan)
            return !enemyInVan;

        if (playerInStorage && storageEnclosed && !enemyInVan)
            return true;

        if (enemyInVan && playerInStorage)
            return false;

        if (!playerInCab)
            return false;

        if (velocityCheck)
        {
            float vehicleSpeed = vehicleController.averageVelocity.magnitude;
            if (vehicleSpeed >= velocityThreshold)
                return true;
        }

        return IsPlayerProtectedByVan(playerScript, checkWindows, checkWindshield, velocityCheck, velocityThreshold);
    }

    public bool IsSeatedPlayerProtected(PlayerControllerB playerScript, bool checkWindows = false, bool windshieldCheck = false, bool velocityCheck = false, float velocityThreshold = 0f)
    {
        float vehicleSpeed = vehicleController.averageVelocity.magnitude;
        if (velocityCheck && vehicleSpeed >= velocityThreshold)
            return true;

        if (windshieldCheck && vehicleController.windshieldBroken)
            return false;

        bool leftSideOpen =
            vehicleController.driverSideDoor.boolValue ||
            (checkWindows && vehicleController.frontLeftWindow.isWindowOpen);

        bool rightSideOpen =
            vehicleController.passengerSideDoor.boolValue ||
            (checkWindows && vehicleController.frontRightWindow.isWindowOpen);

        if ((playerScript == vehicleController.currentDriver && leftSideOpen) ||
            (playerScript == vehicleController.currentMiddlePassenger && (leftSideOpen || rightSideOpen)) ||
            (playerScript == vehicleController.currentPassenger && rightSideOpen))
        {
            return false;
        }

        return true;
    }

    public bool IsPlayerProtectedByVan(PlayerControllerB playerScript, bool checkWindows = false, bool windshieldCheck = false, bool velocityCheck = false, float velocityThreshold = 0f)
    {
        if (vehicleController.carDestroyed)
            return false;

        float vehicleSpeed = vehicleController.averageVelocity.magnitude;
        if (velocityCheck && vehicleSpeed >= velocityThreshold)
            return true;

        bool frontDoorsOpen =
            vehicleController.driverSideDoor.boolValue ||
            vehicleController.passengerSideDoor.boolValue;

        bool frontWindowsOpen =
            vehicleController.frontLeftWindow.isWindowOpen ||
            vehicleController.frontRightWindow.isWindowOpen;

        var playerOnVan = vehicleController.IsPlayerAttached(playerScript);
        var playerInCab = vehicleController.IsPlayerInCabin(playerScript);
        var playerInStorage = vehicleController.IsPlayerInStorage(playerScript);
        var storageEnclosed = vehicleController.IsStorageEnclosed();

        if (playerInCab)
        {
            if (frontDoorsOpen ||
                (checkWindows && frontWindowsOpen) ||
                (windshieldCheck && vehicleController.windshieldBroken))
            {
                return false;
            }
        }
        else if (playerInStorage)
        {
            if (storageEnclosed)
                return false;
        }
        else if (playerOnVan)
        {
            return false;
        }

        return true;
    }

    // This can be used for smaller functions, one-liners, etc.
    // No-Inlining is used for soft dependancy functions. Only when it's not a Harmony patch otherwise TypeLoadExceptions can occur!
    // Thanks T-Rizzle =]
    public bool IsEnemyInVan(EnemyAI? enemyScript, BoxCollider? navMeshBounds)
    {
        return enemyScript != null && navMeshBounds != null &&
              (navMeshBounds.ClosestPoint(enemyScript.transform.position) == enemyScript.transform.position ||
               navMeshBounds.ClosestPoint(enemyScript.agent.destination) == enemyScript.agent.destination);
    }

    public override bool IsPlayerProtectedFromKnockback(PlayerControllerB playerScript)
    {
        return vehicleController != null && vehicleController.IsPlayerSeated(playerScript) && UserConfig.PreventKnockback.Value;
    }

    public override bool IsLocalPlayerCooled()
    {
        return vehicleController != null && vehicleController.IsBlowerOnAndCool() && IsLocalPlayerInShelter();
    }

    public override bool IsLocalPlayerHeated()
    {
        return vehicleController != null && vehicleController.IsBlowerOnAndHot() && IsLocalPlayerInShelter();
    }

    public override float GetHeatTransferRateLocalPlayer(bool acceleratedHeatTransfer)
    {
        return vehicleController != null && vehicleController.IsBlowerOn() ? (acceleratedHeatTransfer ? 1.15f * vehicleController.heaterSpeed : 0.25f * vehicleController.heaterSpeed) : 1f;
    }

    public override bool IsLocalPlayerSheltered()
    {
        return vehicleController != null && IsLocalPlayerInShelter();
    }
}
