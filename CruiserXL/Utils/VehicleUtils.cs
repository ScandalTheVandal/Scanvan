using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine.InputSystem;

namespace CruiserXL.Utils;
public static class VehicleUtils
{
    public static bool IsPlayerInVehicleBounds(PlayerControllerB player, CruiserXLController vehicle)
    {
        // variables
        Vector3 playerTransform = player.transform.position;
        Transform playerOverride = player.overridePhysicsParent;
        Collider physicsCollider = vehicle.vehicleBounds;

        // cache checks
        Vector3 physicsClosest = physicsCollider.ClosestPoint(playerTransform);
        bool inPhysics = physicsClosest == playerTransform;

        // player is within the physics regions bounds, and they're not seated
        if (playerOverride == null && inPhysics)
            return true;

        return false;
    }
    public static bool IsPlayerInTruck(PlayerControllerB player, CruiserXLController vehicle)
    {
        // variables
        Vector3 playerTransform = player.transform.position;
        Transform playerOverride = player.overridePhysicsParent;
        Collider physicsCollider = vehicle.vehicleBounds;
        CapsuleCollider cabCollider = vehicle.cabinPoint;
        Collider storageCollider = vehicle.storageCompartment;

        // cache checks
        Vector3 physicsClosest = physicsCollider.ClosestPoint(playerTransform);
        Vector3 cabClosest = cabCollider.ClosestPoint(playerTransform);
        Vector3 storageClosest = storageCollider.ClosestPoint(playerTransform);

        bool inPhysics = physicsClosest == playerTransform;
        bool inCab = cabClosest == playerTransform;
        bool inStorage = storageClosest == playerTransform;

        // player is the driver
        if (player == vehicle.currentDriver)
            return true;

        // player is the middle passenger
        if (player == vehicle.currentMiddlePassenger)
            return true;

        // player is the passenger
        if (player == vehicle.currentPassenger)
            return true;

        // player is within the physics regions bounds, and they're not within the cab nor the storage compartment
        if (playerOverride == null && inPhysics && !inCab && !inStorage)
            return true;

        // player is within the cabin
        if (playerOverride == null && inCab)
            return true;

        // player is within the storage compartment
        if (playerOverride == null && inStorage)
            return true;

        //// player is the driver
        //if (player == vehicle.currentDriver)
        //    return true;

        //// player is the middle passenger
        //if (player == vehicle.currentMiddlePassenger)
        //    return true;

        //// player is the passenger
        //if (player == vehicle.currentPassenger)
        //    return true;

        //// player is within the physics regions bounds, and they're not within the cab nor the storage compartment
        //if (playerOverride == null &&
        //    physicsCollider.ClosestPoint(playerTransform) == playerTransform &&
        //    storageCollider.ClosestPoint(playerTransform) != playerTransform &&
        //    cabCollider.ClosestPoint(playerTransform) != playerTransform)
        //    return true;

        //// player is within the cabin
        //if (playerOverride == null &&
        //    cabCollider.ClosestPoint(playerTransform) == playerTransform)
        //    return true;

        //// player is within the storage compartment
        //if (playerOverride == null &&
        //    storageCollider.ClosestPoint(playerTransform) == playerTransform)
        //    return true;

        return false;
    }

    public static bool IsPlayerProtectedByTruck(PlayerControllerB player, CruiserXLController vehicle)
    {
        // to-do
        if (!vehicle.hasEnclosedRoof)
            return false;

        // variables
        bool driverDoorOpen = vehicle.driverSideDoor.boolValue;
        bool passengerDoorOpen = vehicle.passengerSideDoor.boolValue;
        bool backDoorOpen = vehicle.liftGateOpen;
        bool sideDoorOpen = vehicle.rightSideDoor.boolValue;

        Vector3 playerTransform = player.transform.position;
        Transform playerOverride = player.overridePhysicsParent;
        Collider physicsCollider = vehicle.vehicleBounds;
        CapsuleCollider cabCollider = vehicle.cabinPoint;
        Collider storageCollider = vehicle.storageCompartment;

        // cache checks
        Vector3 physicsClosest = physicsCollider.ClosestPoint(playerTransform);
        Vector3 cabClosest = cabCollider.ClosestPoint(playerTransform);
        Vector3 storageClosest = storageCollider.ClosestPoint(playerTransform);

        bool inPhysics = physicsClosest == playerTransform;
        bool inCab = cabClosest == playerTransform;
        bool inStorage = storageClosest == playerTransform;

        // player is the driver and their door is open
        if (player == vehicle.currentDriver && driverDoorOpen)
            return false;

        // player is the middle passenger and either side door is open
        if (player == vehicle.currentMiddlePassenger && (driverDoorOpen || passengerDoorOpen))
            return false;

        // player is the passenger and their door is open
        if (player == vehicle.currentPassenger && passengerDoorOpen)
            return false;

        // player is within the physics regions bounds, and they're not within the cab nor the storage compartment
        if (playerOverride == null && inPhysics && !inCab && !inStorage)
            return false;

        // player is within the cabin, and either door is open
        if (playerOverride == null && inCab && (driverDoorOpen || passengerDoorOpen))
            return false;

        // player is within the storage compartment, and the back door is open or the side door is open
        if (playerOverride == null && inStorage && (backDoorOpen || sideDoorOpen))
            return false;

        //// player is the driver and their door is open
        //if (player == vehicle.currentDriver && driverDoorOpen)
        //    return false;

        //// player is the middle passenger and either side door is open
        //if (player == vehicle.currentMiddlePassenger && (driverDoorOpen || passengerDoorOpen))
        //    return false;

        //// player is the passenger and their door is open
        //if (player == vehicle.currentPassenger && passengerDoorOpen)
        //    return false;

        //// player is within the physics regions bounds, and they're not within the cab nor the storage compartment
        //if (playerOverride == null &&
        //    physicsCollider.ClosestPoint(playerTransform) == playerTransform &&
        //    storageCollider.ClosestPoint(playerTransform) != playerTransform &&
        //    cabCollider.ClosestPoint(playerTransform) != playerTransform)
        //    return false;

        //// player is within the cabin, and either door is open
        //if (playerOverride == null &&
        //    cabCollider.ClosestPoint(playerTransform) == playerTransform &&
        //    (driverDoorOpen || passengerDoorOpen))
        //    return false;

        //// player is within the storage compartment, and the back door is open or the side door is open
        //if (playerOverride == null &&
        //    storageCollider.ClosestPoint(playerTransform) == playerTransform &&
        //    (backDoorOpen || sideDoorOpen))
        //    return false;

        return true;
    }
}