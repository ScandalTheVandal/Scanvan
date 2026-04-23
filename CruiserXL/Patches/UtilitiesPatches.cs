using ScanVan.Networking;
using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(ScandalsTweaks.Utils.Utilities))]
public static class UtilitiesPatches
{
    [HarmonyPatch(nameof(ScandalsTweaks.Utils.Utilities.ShouldAllowSightForVehicle))]
    [HarmonyPrefix]
    private static bool ShouldAllowSightForVehicle_Prefix(ref PlayerControllerB player, ref bool __result)
    {
        if (References.truckController == null)
            return true;
        CruiserXLController controller = References.truckController;

        var data = PlayerControllerBPatches.GetData(player);
        bool isOccupant = controller.currentDriver == player ||
                          controller.currentMiddlePassenger == player ||
                          controller.currentPassenger == player ||
                          data.isPlayerInCab;

        if (!isOccupant)
            return true;

        __result = SCVNetworker.Instance.OldBirdSight.Value;
        return false;
    }
}
