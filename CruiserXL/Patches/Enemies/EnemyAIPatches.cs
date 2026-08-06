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
    static void EnemyAI_Post_PlayerIsTargetable(EnemyAI __instance, PlayerControllerB playerScript, bool cannotBeInShip, bool overrideInsideFactoryCheck, bool checkForMineshaftStartTile, ref bool __result)
    {
        if (__instance is not BushWolfEnemy)
            return;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerScript, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerScript, vanController, usePlayerData: true);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerScript, vanController, usePlayerData: true);
        bool storageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);

        if (playerSeated)
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerScript, vanController, checkWindows: true))
            {
                __result = false;
                return;
            }

            return;
        }

        bool protectedInCab =
            playerInCab &&
            VehicleUtils.IsPlayerProtectedByVan(playerScript, vanController, checkWindows: true);

        bool protectedInStorage =
            playerInStorage &&
            storageEnclosed;

        if (protectedInCab || protectedInStorage)
            __result = false;
    }
}