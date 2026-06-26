using HarmonyLib;
using ScanVan.Utils;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(BushWolfEnemy))]
public static class BushWolfEnemyPatches
{
    [HarmonyPatch(nameof(BushWolfEnemy.Update))]
    [HarmonyPostfix]
    static void Update_Postfix(BushWolfEnemy __instance)
    {
        if (__instance.targetPlayer == null)
            return;
        if (__instance.targetPlayer.isPlayerDead || !__instance.targetPlayer.isPlayerControlled ||
            __instance.targetPlayer.inAnimationWithEnemy || __instance.stunNormalizedTimer > 0f) return;

        CruiserXLController controller = References.vanController;
        if (controller == null)
            return;

        bool isOccupant = controller.currentDriver == __instance.targetPlayer ||
                          controller.currentMiddlePassenger == __instance.targetPlayer ||
                          controller.currentPassenger == __instance.targetPlayer;

        if (isOccupant && VehicleUtils.IsSeatedPlayerProtected(playerController: __instance.targetPlayer, vanController: controller, checkWindows: true))
        {
            __instance.agent.speed = 0f;
            __instance.CancelReelingPlayerIn();
            if (__instance.IsOwner && __instance.tongueLengthNormalized < -0.25f)
            {
                __instance.SwitchToBehaviourState(0);
                return;
            }
            return;
        }

        var targetData = PlayerControllerBPatches.playerData[__instance.targetPlayer];
        if (targetData.playerRidingInVanCab && !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue ||
            targetData.playerRidingInVanStorage && !controller.liftGateOpen && !controller.sideDoorOpen)
        {
            __instance.agent.speed = 0f;
            __instance.CancelReelingPlayerIn();
            if (__instance.IsOwner && __instance.tongueLengthNormalized < -0.25f)
            {
                __instance.SwitchToBehaviourState(0);
                return;
            }
            return;
        }
    }
}