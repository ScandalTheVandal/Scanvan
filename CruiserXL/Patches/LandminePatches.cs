using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using ScandalsTweaks.Patches;

namespace ScanVan.Patches;

[HarmonyPatch]
public static class LandminePatches
{
    [HarmonyPatch(typeof(Landmine), nameof(Landmine.SpawnExplosion))]
    [HarmonyPostfix]
    private static void Landmine_Post_SpawnExplosion(Landmine __instance, ref Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;
        if (!vanController.IsSpawned || !vanController.hasBeenSpawned)
            return;
        if (vanController.carDestroyed || vanController.magnetedToShip)
            return;
        if (vanController.keyIgnitionCoroutine != null || vanController.ignitionOn)
            return;
        if (vanController.alarmDebounce)
            return;

        if (Vector3.Distance(vanController.transform.position, explosionPosition) > 7.5f)
            return;
        if ((float)Random.Range(9, 85) > 12f)
            return;

        vanController.alarmDebounce = true;
        vanController.TryBeginAlarm();
    }

    [HarmonyPatch(typeof(Landmine), nameof(Landmine.SpawnExplosion))]
    [HarmonyPrefix]
    private static void Landmine_Pre_SpawnExplosion(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerNearVan(playerController, vanController))
            return;

        bool playerSeated = vanController.IsPlayerSeated(playerController);
        bool playerInCab = vanController.IsPlayerInCabin(playerController);
        bool playerInStorage = vanController.IsPlayerInStorage(playerController);

        bool explosionOutsideStorage =
            vanController.storageCompartment.ClosestPoint(explosionPosition) !=
            explosionPosition;

        bool isProbablyLightning =
            !goThroughCar &&
            !spawnExplosionEffect &&
            killRange == 2.4f &&
            damageRange == 5f &&
            nonLethalDamage == 1 &&
            physicsForce == 0f;

        if (!isProbablyLightning)
            return;

        if ((playerInStorage && explosionOutsideStorage) ||
            playerInCab ||
            playerSeated)
        {
            killRange = -1f;
            damageRange = -1f;
        }
    }
}