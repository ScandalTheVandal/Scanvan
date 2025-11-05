using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(BushWolfEnemy))]
public static class BushWolfEnemyPatches
{
    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    static void Update_Prefix(BushWolfEnemy __instance)
    {
        if (__instance.currentBehaviourStateIndex != 2)
            return;

        if (__instance.targetPlayer == null)
            return;

        if (__instance.timeSinceKillingPlayer < 2f || __instance.timeSinceTakingDamage < 0.35f)
            return;

        if (__instance.failedTongueShoot)
            return;

        if (References.truckController == null)
            return;

        if (!VehicleUtils.IsPlayerNearTruck(__instance.targetPlayer, References.truckController))
            return;

        if (!VehicleUtils.MeetsSpecialConditionsToCheck())
            return;

        // not in our truck, run vanilla logic
        if (!VehicleUtils.IsPlayerInTruck(__instance.targetPlayer, References.truckController))
            return;

        // check if the player is protected in our truck
        if (VehicleUtils.IsPlayerProtectedByTruck(__instance.targetPlayer, References.truckController))
        {
            // recycled vanilla logic
            __instance.agent.speed = 0f;
            __instance.CancelReelingPlayerIn();
            if (__instance.IsOwner && __instance.tongueLengthNormalized < -0.25f)
                __instance.SwitchToBehaviourState(0);
            return;
        }
    }
}