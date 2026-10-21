using HarmonyLib;
using LethalMin;
using System.Runtime.CompilerServices;
using System;
using UnityEngine;

namespace ScanVan.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
public static class LethalMinCompatibility
{
    [HarmonyPatch(typeof(PikminVehicleController), nameof(PikminVehicleController.InitializeReferences))]
    [HarmonyPrefix]
    public static bool PikminVehicleController_Pre_InitializeReferences(PikminVehicleController __instance)
    {
        if (__instance.TryGetComponent<CruiserXLController>(out var controller))
        {
            __instance.controller = controller;
            __instance.PointsRegion = controller.collisionTrigger.insideTruckNavMeshBounds;
            __instance.PikminCheckRegion = controller.storageCompartment;
            __instance.PikminWarpPoint = new GameObject("Pikmin Warp Point").transform;
            __instance.PikminWarpPoint.SetParent(__instance.transform);
            __instance.PikminWarpPoint.localPosition = new Vector3(0f, -2f, -6.55f);
            __instance.PikminWarpPoint.localScale = new Vector3(1f, 1f, 1f);
            __instance.OriginalWTLocalPosition = __instance.PikminWarpPoint.localPosition;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(CruiserXLCollisionTrigger), nameof(CruiserXLCollisionTrigger.OnTriggerEnter))]
    [HarmonyPrefix]
    public static bool CruiserXLCollisionTrigger_Pre_OnTriggerEnter(CruiserXLCollisionTrigger __instance, Collider other)
    {
        try
        {
            if (other.gameObject.CompareTag("Enemy") && other.gameObject.TryGetComponent<PikminCollisionDetect>(out _))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Plugin.LogError(string.Format("Error in 'CruiserXLCollisionTrigger.OnTriggerEnterPrefix': {0}", e));
            return true;
        }
        return true;
    }
}
