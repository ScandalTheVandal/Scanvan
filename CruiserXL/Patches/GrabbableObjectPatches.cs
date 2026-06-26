using HarmonyLib;
using ScanVan.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(GrabbableObject))]
public static class GrabbableObjectPatches
{
    [HarmonyPatch(nameof(GrabbableObject.Update))]
    [HarmonyPostfix]
    private static void Update_Postfix(GrabbableObject __instance)
    {
        if (__instance == null)
            return;

        if (GameNetworkManager.Instance.localPlayerController == null)
            return;

        CruiserXLController vanController = References.vanController;
        if (vanController == null)
            return;

        if (__instance.transform.parent != vanController.transform)
            return;

        bool isInVan = __instance.transform.parent == vanController.transform;
        if (!isInVan || __instance.isHeld || __instance.isHeldByEnemy)
        {
            References.itemsInTruck.Remove(__instance);
            return;
        }
        References.itemsInTruck.Add(__instance);
    }

    [HarmonyPatch(nameof(GrabbableObject.OnDestroy))]
    [HarmonyPostfix]
    private static void OnDestroy_Postfix(GrabbableObject __instance)
    {
        References.itemsInTruck.Remove(__instance);
    }
}
