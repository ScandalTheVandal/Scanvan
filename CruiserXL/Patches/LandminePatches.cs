using CruiserXL.Utils;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(Landmine))]
public class LandminePatches
{
    [HarmonyPatch(nameof(Landmine.SpawnExplosion))]
    [HarmonyPrefix]
    private static void SpawnExplosion_Prefix(Landmine __instance, Vector3 explosionPosition, bool spawnExplosionEffect, ref float killRange, ref float damageRange, int nonLethalDamage, float physicsForce, GameObject overridePrefab, bool goThroughCar)
    {
        if (PlayerUtils.isPlayerInCab || 
            PlayerUtils.isPlayerInStorage)
        {
            killRange = 0f;
            damageRange = 0f;
        }
    }
}
