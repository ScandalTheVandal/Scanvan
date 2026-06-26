using ScanVan.Utils;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(HUDManager))]
public static class HUDManagerPatches
{
    [HarmonyPatch(nameof(HUDManager.HelmetCondensationDrops))]
    [HarmonyPostfix]
    private static void HelmetCondensationDrops_Postfix(HUDManager __instance)
    {
        CruiserXLController controller = References.vanController;
        if (controller == null)
            return;

        if (VehicleUtils.IsPlayerInVanCabin(controller) ||
            VehicleUtils.IsPlayerInVanStorage(controller))
        {
            __instance.increaseHelmetCondensation = false;
        }
    }
}
