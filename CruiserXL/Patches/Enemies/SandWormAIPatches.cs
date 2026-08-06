using GameNetcodeStuff;
using HarmonyLib;
using ScanVan.Utils;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(SandWormAI))]
public static class SandWormAIPatches
{
    [HarmonyPatch(nameof(SandWormAI.EatPlayer))]
    [HarmonyPrefix]
    static bool SandWormAI_Pre_EatPlayer(SandWormAI __instance, PlayerControllerB playerScript, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        bool playerOnVan = VehicleUtils.IsPlayerInVanBounds(playerScript, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(playerScript, vanController);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(playerScript, vanController);
        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(playerScript, vanController);

        if (playerSeated)
            return false;

        if (!playerOnVan)
            return true;

        return !playerInCab &&
               !playerInStorage;
    }
}