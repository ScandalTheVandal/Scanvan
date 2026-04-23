using BepInEx.Bootstrap;
using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.VFX;
using LethalMin;

namespace ScanVan.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>

public class LethalMinCompatibility
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAllCompatibilityMethods(Harmony harmony)
    {
        ApplyPikminPatch(harmony);
    }

    [HarmonyPrefix]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ApplyPikminPatch(Harmony harmony)
    {
        var vehicleCollisionMethod = AccessTools.Method(typeof(CruiserXLCollisionTrigger), nameof(CruiserXLCollisionTrigger.OnTriggerEnter));
        var prefixvehicleCollisionMethod = AccessTools.Method(typeof(LethalMinCompatibility), nameof(OnTriggerEnter_Prefix));

        var pikminControllerMethod = AccessTools.Method(typeof(PikminVehicleController), nameof(PikminVehicleController.InitializeReferences));
        var prefixPikminControllerMethod = AccessTools.Method(typeof(LethalMinCompatibility), nameof(InitializeReferences_Prefix));

        harmony.Patch(vehicleCollisionMethod, prefix: new HarmonyMethod(prefixvehicleCollisionMethod));
        harmony.Patch(pikminControllerMethod, prefix: new HarmonyMethod(prefixPikminControllerMethod));
    }

    public static bool InitializeReferences_Prefix(PikminVehicleController __instance)
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

    public static bool OnTriggerEnter_Prefix(CruiserXLCollisionTrigger __instance, ref Collider other)
    {
        try
        {
            PikminCollisionDetect detect;
            if (other.gameObject.CompareTag("Enemy") && other.gameObject.TryGetComponent<PikminCollisionDetect>(out detect))
            {
                return false;
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(string.Format("Error in CruiserXLCollisionTrigger.OnTriggerEnterPrefix: {0}", e));
            return true;
        }
        return true;
    }
}
