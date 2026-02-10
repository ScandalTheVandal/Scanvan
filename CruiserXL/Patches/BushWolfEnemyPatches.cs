using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(BushWolfEnemy))]
public static class BushWolfEnemyPatches
{
    [HarmonyPatch(nameof(BushWolfEnemy.Update))]
    [HarmonyPrefix]
    static void Update_Prefix(BushWolfEnemy __instance)
    {
        if (__instance.currentBehaviourStateIndex != 2 || __instance.targetPlayer == null ||
            __instance.timeSinceKillingPlayer < 2f || __instance.timeSinceTakingDamage < 0.35f ||
            __instance.failedTongueShoot)
            return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;

        // check if the player is seated in our truck & player is protected, cancel the grab if so
        if (VehicleUtils.IsPlayerSeatedInVehicle(controller) &&
            VehicleUtils.IsSeatedPlayerProtected(__instance.targetPlayer, controller))
        {
            // recycled vanilla logic
            __instance.agent.speed = 0f;
            __instance.CancelReelingPlayerIn();
            if (__instance.IsOwner && __instance.tongueLengthNormalized < -0.25f)
                __instance.SwitchToBehaviourState(0);
            return;
        }

        // not seated in our truck, but within the vehicle bounds & player is protected, cancel the grab if so
        if (VehicleUtils.IsPlayerInVehicleBounds() &&
            VehicleUtils.IsPlayerProtectedByVehicle(__instance.targetPlayer, controller))
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