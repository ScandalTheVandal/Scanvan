using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using ScandalsTweaks.Utils;
using ScandalsTweaks.Patches;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(Landmine))]
public static class LandminePatches
{
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    [HarmonyPostfix]
    private static void SpawnExplosion_Postfix(Landmine __instance, ref Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        CruiserXLController controller = References.vanController;
        if (controller == null)
            return;

        if (!controller.IsSpawned ||
            !controller.hasBeenSpawned ||
            controller.alarmDebounce ||
            controller.ignitionStarted ||
            controller.carDestroyed ||
            controller.magnetedToShip)
            return;

        if (Vector3.Distance(controller.transform.position, explosionPosition) > 7.5f)
            return;

        if ((float)Random.Range(9, 85) > 12f)
            return;

        controller.alarmDebounce = true;
        controller.TryBeginAlarm();
    }

    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    [HarmonyPrefix]
    private static void SpawnExplosion_Prefix(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        CruiserXLController controller = References.vanController;
        if (controller == null)
            return;

        if (!VehicleUtils.IsPlayerNearVan(GameNetworkManager.Instance.localPlayerController, vanController: controller))
            return;

        if (!goThroughCar &&
            ((VehicleUtils.IsPlayerInVanStorage(controller) && controller.storageCompartment.ClosestPoint(explosionPosition) != explosionPosition) || 
            VehicleUtils.IsPlayerInVanCabin(controller) || PlayerUtils.isSeatedInVan))
        {
            killRange = -1f;
            damageRange = -1f;
        }
    }
}