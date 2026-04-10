using CruiserXL.Utils;
using HarmonyLib;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(ItemDropship))]
public class ItemDropshipPatches
{
    // optimisation stuff
    [HarmonyPatch(nameof(ItemDropship.Start))]
    [HarmonyPrefix]
    private static void Start_Prefix(ItemDropship __instance)
    {
        if (__instance == null) 
            return;

        References.itemShip = __instance;
    }
}
