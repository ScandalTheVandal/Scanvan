using ScanVan.Utils;
using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(HUDManager))]
public static class HUDManagerPatches
{
    [HarmonyPatch(nameof(HUDManager.HelmetCondensationDrops))]
    [HarmonyPostfix]
    private static void HUDManager_Post_HelmetCondensationDrops(HUDManager __instance)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;

        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerController, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerController, vanController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerController, vanController);

        if (playerInCab || playerInStorage || playerSeated)
            __instance.increaseHelmetCondensation = false;
    }
}
