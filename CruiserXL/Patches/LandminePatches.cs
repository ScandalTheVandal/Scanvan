using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(Landmine))]
public class LandminePatches
{
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    [HarmonyPostfix]
    private static void SpawnExplosion_Postfix(Landmine __instance, ref Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;

        if (!controller.IsSpawned ||
            !controller.hasBeenSpawned ||
            controller.alarmDebounce ||
            controller.ignitionStarted ||
            controller.carDestroyed)
            return;

        if (Vector3.Distance(controller.transform.position, explosionPosition) > 5f)
            return;

        if ((float)UnityEngine.Random.Range(9, 85) > 17f)
            return;

        controller.alarmDebounce = true;
        controller.TryBeginAlarm();
    }
}

[HarmonyPatch(typeof(ScandalsTweaks.Patches.LandminePatches))]
public class ExternalLandminePatches
{
    [HarmonyPatch(nameof(ScandalsTweaks.Patches.LandminePatches.DoesVehicleExist))]
    [HarmonyPrefix]
    private static bool DoesVehicleExist_Prefix(ref bool __result)
    {
        if (References.truckController != null)
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(ScandalsTweaks.Patches.LandminePatches.CanPlayerBeKnockedBackInVehicle))]
    [HarmonyPrefix]
    private static bool CanPlayerBeKnockedBackInVehicle_Prefix(ref bool __result)
    {
        if (PlayerUtils.seatedInTruck && UserConfig.PreventKnockback.Value)
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(nameof(ScandalsTweaks.Patches.LandminePatches.GetCurrentCachedVehicle))]
    [HarmonyPrefix]
    private static void GetCurrentCachedVehicle_Prefix()
    {
        if (References.truckController != null)
        {
            ScandalsTweaks.Utils.References.vehicleController = References.truckController;
        }
    }

    [HarmonyPatch(nameof(ScandalsTweaks.Patches.LandminePatches.CurrentForceMultiplier))]
    [HarmonyPrefix]
    private static bool CurrentForceMultiplier_Prefix(ref float __result)
    {
        if (References.truckController != null)
        {
            __result = 5f;
            return false;
        }
        return true;
    }
}
