using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(EnemyAI))]
public static class EnemyAIPatches
{
    [HarmonyPatch(nameof(EnemyAI.PlayerIsTargetable))]
    [HarmonyPostfix]
    static void PlayerIsTargetable_Postfix(EnemyAI __instance, PlayerControllerB playerScript, bool cannotBeInShip, bool overrideInsideFactoryCheck, ref bool __result)
    {
        if (__instance is not BushWolfEnemy bushWolf) return;
        if (References.truckController == null) return;
        CruiserXLController controller = References.truckController;
        var data = PlayerControllerBPatches.GetData(playerScript);

        bool isOccupant = controller.currentDriver == playerScript ||
                          controller.currentMiddlePassenger == playerScript ||
                          controller.currentPassenger == playerScript;

        if (isOccupant && VehicleUtils.IsSeatedPlayerProtected(playerScript, controller))
            __result = false;

        if ((data.isPlayerInCab && !controller.driverSideDoor.boolValue && !controller.passengerSideDoor.boolValue) || 
            (data.isPlayerInStorage && !controller.liftGateOpen && !controller.sideDoorOpen))
            __result = false;
    }
}