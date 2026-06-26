using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(EnemyAI))]
public static class EnemyAIPatches
{
    [HarmonyPatch(nameof(EnemyAI.PlayerIsTargetable))]
    [HarmonyPostfix]
    static void PlayerIsTargetable_Postfix(EnemyAI __instance, PlayerControllerB playerScript, bool cannotBeInShip, bool overrideInsideFactoryCheck, bool checkForMineshaftStartTile, ref bool __result)
    {
        if (__instance is not BushWolfEnemy)
            return;

        CruiserXLController controller = References.vanController;
        if (controller == null)
            return;

        var playerData = PlayerControllerBPatches.playerData[playerScript];
        bool isOccupant = controller.currentDriver == playerScript ||
                          controller.currentMiddlePassenger == playerScript ||
                          controller.currentPassenger == playerScript;

        if (isOccupant && VehicleUtils.IsSeatedPlayerProtected(playerController: playerScript, vanController: controller, checkWindows: true))
            __result = false;

        if (playerData.playerRidingInVanCab && !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue ||
            playerData.playerRidingInVanStorage && !controller.liftGateOpen && !controller.sideDoorOpen)
            __result = false;
    }
}