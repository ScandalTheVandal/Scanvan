using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(ElevatorAnimationEvents))]
public static class ElevatorAnimationEventsPatches
{
    [HarmonyPatch(nameof(ElevatorAnimationEvents.ElevatorFullyRunning))]
    [HarmonyPrefix]
    static void ElevatorFullyRunning_PlayerSafety_Prefix()
    {
        CruiserXLController controller = References.vanController;
        if (controller == null) 
            return;
        if (!controller.magnetedToShip) 
            return;

        TryDespawnItemsInVan(vanController: controller);

        // save players who are on the magneted truck from being abandoned
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (PlayerUtils.isSeatedInVan || VehicleUtils.IsPlayerInVanBounds(controller))
            localPlayer.isInElevator = true;
    }

    static void TryDespawnItemsInVan(CruiserXLController vanController)
    {
        if (!RoundManager.Instance.IsServer)
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
                Plugin.Logger.LogDebug($"ScanVan: Item \"{vanItem.name}\" (#{vanItem.GetInstanceID()}) lost in orbit");
                if (vanItem.NetworkObject != null && vanItem.NetworkObject.IsSpawned)
                    vanItem.NetworkObject.Despawn();
                else
                    Object.Destroy(vanItem.gameObject);
            }
        }
    }
}
