using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(ElevatorAnimationEvents))]
public static class ElevatorAnimationEventsPatches
{
    [HarmonyPatch(nameof(ElevatorAnimationEvents.ElevatorFullyRunning))]
    [HarmonyPrefix]
    static void ElevatorAnimationEvents_Pre_ElevatorFullyRunning()
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null || !vanController.magnetedToShip)
            return;

        TryDespawnItemsInVan(vanController);

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);
        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);

        if (playerSeated || playerOnVan || playerInStorage || playerInCab)
            playerController.isInElevator = true;
    }

    static void TryDespawnItemsInVan(CruiserXLController vanController)
    {
        if (!RoundManager.Instance.NetworkManager.IsServer)
            return;

        Bounds onTopOfVanBounds = vanController.ontopOfTruckCollider.bounds;
        Bounds inBackOfVanBounds = vanController.storageCompartment.bounds;

        //GrabbableObject[] vanItems = vanController.GetComponentsInChildren<GrabbableObject>();
        foreach (GrabbableObject vanItem in References.itemsInTruck)
        {
            if (References.itemsInTruck.Count == 0 || vanItem == null) 
                continue;

            if ((inBackOfVanBounds.ClosestPoint(vanItem.transform.position) == vanItem.transform.position && vanController.liftGateOpen) || 
                onTopOfVanBounds.ClosestPoint(vanItem.transform.position) == vanItem.transform.position)
            {
                Plugin.LogDebug($"Item \"{vanItem.name}\" (#{vanItem.GetInstanceID()}) lost in orbit.");
                if (vanItem.NetworkObject != null && vanItem.NetworkObject.IsSpawned)
                    vanItem.NetworkObject.Despawn();
                else
                    Object.Destroy(vanItem.gameObject);
            }
        }
    }
}
