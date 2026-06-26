using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(CadaverGrowthAI))]
public static class CadaverGrowthAIPatches
{
    // OnEnable would be too early to check this (?)
    [HarmonyPatch(nameof(CadaverGrowthAI.Start))]
    [HarmonyPostfix]
    static void Start_Postfix(CadaverGrowthAI __instance)
    {
        if (References.cadaverGrowthAI == null)
            References.cadaverGrowthAI = __instance;
    }

    [HarmonyPatch(nameof(CadaverGrowthAI.OnDisable))]
    [HarmonyPostfix]
    static void OnDisable_Postfix(CadaverGrowthAI __instance)
    {
        if (References.cadaverGrowthAI == __instance)
            References.cadaverGrowthAI = null!;
    }
}
