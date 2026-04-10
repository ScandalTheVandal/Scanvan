using CruiserXL.Utils;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(HUDManager))]
public class HUDManagerPatches
{
    [HarmonyPatch(nameof(HUDManager.HelmetCondensationDrops))]
    [HarmonyPostfix]
    private static void HelmetCondensationDrops_Postfix(HUDManager __instance)
    {
        if (References.truckController == null)
            return;

        if (PlayerUtils.isPlayerInCab ||
            PlayerUtils.isPlayerInStorage)
        {
            __instance.increaseHelmetCondensation = false;
        }
    }
}
