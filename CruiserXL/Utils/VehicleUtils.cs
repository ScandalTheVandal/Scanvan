using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;

namespace CruiserXL.Utils;
public static class VehicleUtils
{
    public static float lastCheckTime = 0f;
    public static float cooldown = 0.25f;

    public static bool MeetsSpecialConditionsToCheck()
    {
        if (Time.realtimeSinceStartup - lastCheckTime < cooldown)
            return false;

        lastCheckTime = Time.realtimeSinceStartup;
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

    public static bool IsPlayerInVehicleBounds(PlayerControllerB player, CruiserXLController vehicle)
    {
        if (!IsPlayerNearTruck(player, vehicle))
            return false;

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
        //CapsuleCollider cabCollider = vehicle.cabinPoint;
        //Collider storageCollider = vehicle.storageCompartment;

        // cache checks
        Vector3 physicsClosest = physicsCollider.ClosestPoint(playerTransform);
        //Vector3 cabClosest = cabCollider.ClosestPoint(playerTransform);
        //Vector3 storageClosest = storageCollider.ClosestPoint(playerTransform);

        bool inPhysics = physicsClosest == playerTransform;
        //bool inCab = cabClosest == playerTransform;
        //bool inStorage = storageClosest == playerTransform;

        if (player == vehicle.currentDriver || // player is the driver
            player == vehicle.currentMiddlePassenger || // player is the middle passenger
            player == vehicle.currentPassenger) // player is the passenger
            return true;

        // player is within the physics regions bounds, and they're not within the cab nor the storage compartment
        if (playerOverride == null && inPhysics)
            return true;

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

        if ((player == vehicle.currentDriver && driverDoorOpen) || // player is the driver and their door is open
            (player == vehicle.currentMiddlePassenger && (driverDoorOpen || passengerDoorOpen)) || // player is the middle passenger and either side door is open
            (player == vehicle.currentPassenger && passengerDoorOpen)) // player is the passenger and their door is open
            return false;

        if (playerOverride == null) // not seated
        {
            // variables
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

            // player is within the physics regions bounds, and they're not within the cab nor the storage compartment
            if (inPhysics && !inCab && !inStorage)
                return false;

            // player is within the cabin, and either door is open
            if (inCab && (driverDoorOpen || passengerDoorOpen))
                return false;

            // player is within the storage compartment, and the back door is open or the side door is open
            if (inStorage && (backDoorOpen || sideDoorOpen))
                return false;
        }
        return true;
    }
}