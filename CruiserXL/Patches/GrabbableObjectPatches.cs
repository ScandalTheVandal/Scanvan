using HarmonyLib;
using ScanVan.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScanVan.Patches;

[HarmonyPatch]
public static class GrabbableObjectPatches
{
    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Update))]
    [HarmonyPostfix]
    private static void GrabbableObject_Post_Update(GrabbableObject __instance)
    {
        if (__instance == null)
            return;

        if (GameNetworkManager.Instance.localPlayerController == null)
            return;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        bool isInVan = __instance.transform.parent == vanController.transform;
        if (!isInVan || __instance.isHeld || __instance.isHeldByEnemy)
        {
            References.itemsInTruck.Remove(__instance);
            return;
        }
        References.itemsInTruck.Add(__instance);
    }

    [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.OnDestroy))]
    [HarmonyPostfix]
    private static void GrabbableObject_Post_OnDestroy(GrabbableObject __instance)
    {
        References.itemsInTruck.Remove(__instance);
    }
}
