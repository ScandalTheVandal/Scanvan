using CruiserXL.Utils;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(Landmine))]
public class LandminePatches
{
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    [HarmonyPrefix]
    private static void SpawnExplosion_Prefix(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        if (!spawnExplosionEffect && killRange == 2.4f && 
            damageRange == 5f && nonLethalDamage == 50f &&
            physicsForce == 0f && overridePrefab == null &&
            !goThroughCar)
        {
            Plugin.Logger.LogError("lightning!");
            if (PlayerUtils.isPlayerInCab || PlayerUtils.seatedInTruck ||
                PlayerUtils.isPlayerInStorage)
            {
                killRange = 0f;
                damageRange = 0f;
            }
        }
    }

    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    [HarmonyPostfix]
    private static void SpawnExplosion_Postfix(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;

        if (!controller.IsSpawned ||
            !controller.hasBeenSpawned ||
            controller.alarmDebounce ||
            controller.ignitionStarted)
            return;

        if (Vector3.Distance(controller.transform.position, explosionPosition) > 5f)
            return;

        if ((float)UnityEngine.Random.Range(9, 85) > 17f)
            return;

        controller.alarmDebounce = true;
        controller.TryBeginAlarm();
    }
}
