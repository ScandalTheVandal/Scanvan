using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(BushWolfEnemy))]
public static class BushWolfEnemyPatches
{
    [HarmonyPatch(nameof(BushWolfEnemy.Start))]
    [HarmonyPrefix]
    static void Start_Postfix(BushWolfEnemy __instance)
    {
        if (__instance == null) return;
        References.kidnapperFox = __instance;
    }

    [HarmonyPatch(nameof(BushWolfEnemy.Update))]
    [HarmonyPostfix]
    static void Update_Postfix(BushWolfEnemy __instance)
    {
        if (!__instance.foundSpawningPoint || StartOfRound.Instance.livingPlayers == 0) return;
        if (!__instance.isEnemyDead) return;
        if (__instance.stunNormalizedTimer > 0f || __instance.matingCallTimer >= 0f) return;
        if (__instance.currentBehaviourStateIndex != 2) return;
        if (__instance.timeSinceKillingPlayer < 2f || __instance.timeSinceTakingDamage < 0.35f) return;
        if (__instance.failedTongueShoot) return;
        if (__instance.targetPlayer == null) return;
        if (__instance.targetPlayer.isPlayerDead || !__instance.targetPlayer.isPlayerControlled ||
            __instance.targetPlayer.inAnimationWithEnemy || __instance.stunNormalizedTimer > 0f) return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;

        bool isOccupant = controller.currentDriver == __instance.targetPlayer ||
                          controller.currentMiddlePassenger == __instance.targetPlayer ||
                          controller.currentPassenger == __instance.targetPlayer;

        if (isOccupant && VehicleUtils.IsSeatedPlayerProtected(__instance.targetPlayer, controller))
        {
            __instance.agent.speed = 0f;
            __instance.CancelReelingPlayerIn();
            if (__instance.IsOwner && __instance.tongueLengthNormalized < -0.25f)
            {
                __instance.SwitchToBehaviourState(0);
                return;
            }
        }

        var data = PlayerControllerBPatches.GetData(__instance.targetPlayer);
        if ((data.isPlayerInCab && !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue) || 
            (data.isPlayerInStorage && !controller.liftGateOpen && !controller.sideDoorOpen))
        {
            __instance.agent.speed = 0f;
            __instance.CancelReelingPlayerIn();
            if (__instance.IsOwner && __instance.tongueLengthNormalized < -0.25f)
            {
                __instance.SwitchToBehaviourState(0);
                return;
            }
        }
    }
}