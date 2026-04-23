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
using Woecust.ImmersiveVisor;

namespace ScanVan.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>

public class ImmersiveVisorCompatibility
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void PatchAllCompatibilityMethods(Harmony harmony)
    {
        ApplyVisorPatch(harmony);
    }

    [HarmonyPrefix]
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void ApplyVisorPatch(Harmony harmony)
    {
        var linecastMethod = AccessTools.Method(typeof(VisorRainState), nameof(VisorRainState.LineCastForCeiling));
        var prefixLinecastMethod = AccessTools.Method(typeof(ImmersiveVisorCompatibility), nameof(LineCastForCeiling_Prefix));

        harmony.Patch(linecastMethod, prefix: new HarmonyMethod(prefixLinecastMethod));
    }

    public static bool LineCastForCeiling_Prefix(VisorRainState __instance, ref bool __result)
    {
        if (References.truckController == null)
            return true;
        CruiserXLController controller = References.truckController;

        if (PlayerUtils.seatedInTruck || PlayerUtils.isPlayerInCab || PlayerUtils.isPlayerInStorage)
        {
            __result = true;
            return false;
        }
        return true;
    }
}
