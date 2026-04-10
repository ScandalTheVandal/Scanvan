using HarmonyLib;
using CruiserXL.Utils;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(RoundManager))]
public class RoundManagerPatches
{
    [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
    [HarmonyPrefix]
    static void DespawnPropsAtEndOfRound_Prefix(bool despawnAllItems = false)
    {
        References.isDespawningProps = true;
    }

    [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
    [HarmonyPostfix]
    static void DespawnPropsAtEndOfRound_Postfix(bool despawnAllItems = false)
    {
        References.isDespawningProps = false;
    }
}
