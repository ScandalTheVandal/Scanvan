using GameNetcodeStuff;
using HarmonyLib;
using ScanVan.Utils;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(SandWormAI))]
public static class SandWormAIPatches
{
    [HarmonyPatch(nameof(SandWormAI.EatPlayer))]
    [HarmonyPrefix]
    static bool EatPlayer_Prefix(SandWormAI __instance, PlayerControllerB playerScript, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController controller = References.vanController;
        if (controller == null)
            return true;

        if (VehicleUtils.IsPlayerSeatedInVan())
        {
            return false;
        }
        if (VehicleUtils.IsPlayerInVanBounds(vanController: controller))
        {
            if (VehicleUtils.IsPlayerInVanCabin(vanController: controller) ||
                VehicleUtils.IsPlayerInVanStorage(vanController: controller))
            {
                return false;
            }
            return true;
        }
        return true;
    }
}