using GameNetcodeStuff;
using HarmonyLib;
using ScanVan.Utils;
using Woecust.ImmersiveVisor;

namespace ScanVan.Compatibility;

/// <summary>
///  Available from BrutalCompanyMinus, licensed under MIT licence.
///  Source: https://github.com/Sylkadi/BrutalCompanyMinus

///  Available from BrutalCompanyMinusExtraReborn, licensed under GNU General Public License.
///  Source: https://github.com/TheSoftDiamond/BrutalCompanyMinusExtraReborn
/// </summary>
[HarmonyPatch]
public static class ImmersiveVisorCompatibility
{
    [HarmonyPatch(typeof(VisorRainState), nameof(VisorRainState.LineCastForCeiling))]
    [HarmonyPrefix]
    public static bool VisorRainState_Pre_LineCastForCeiling(VisorRainState __instance, ref bool __result)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return true;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (VehicleUtils.IsPlayerSeatedInVan(playerController, vanController) || 
            VehicleUtils.IsPlayerInVanCabin(playerController, vanController) || 
            VehicleUtils.IsPlayerInVanStorage(playerController, vanController))
        {
            __result = true;
            return false;
        }
        return true;
    }
}