using HarmonyLib;
using FirstPersonView;

namespace ScanVan.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
[HarmonyPatch]
public static class FirstPersonViewCompatibility
{
    [HarmonyPatch(typeof(CruiserXLController), nameof(CruiserXLController.SetIgnitionKey))]
    [HarmonyPrefix]
    public static void HaulerController_Pre_SetCarKeyEffects(CruiserXLController __instance, ref bool localUseBodyHands)
    {
        if (!LocalBodyViewController.LocalBodyShown)
            return;
        localUseBodyHands = true;
    }
}