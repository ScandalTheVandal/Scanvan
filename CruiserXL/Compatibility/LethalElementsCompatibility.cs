using GameNetcodeStuff;
using HarmonyLib;
using ScanVan.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;
using VoxxWeatherPlugin.Patches;
using VoxxWeatherPlugin.Utils;
using VoxxWeatherPlugin.Weathers;

namespace ScanVan.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
public static class LethalElementsCompatibility
{
    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.Start))]
    [HarmonyPostfix]
    public static void CruiserXLController_Post_Start(CruiserXLController __instance)
    {
        SnowTrackersManager.AddFootprintTracker(__instance, 6f, 0.75f, 1f, new Vector3(0, 0, -1f));
    }

    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.FixedUpdate))]
    [HarmonyPostfix]
    public static void CruiserXLController_Post_FixedUpdate(CruiserXLController __instance)
    {
        if (!SnowPatches.IsSnowActive())
        {
            return;
        }
        SnowTrackersManager.UpdateFootprintTracker(__instance, !__instance.allWheelsAirborne);
    }
}