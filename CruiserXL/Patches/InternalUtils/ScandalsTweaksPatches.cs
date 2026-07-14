using GameNetcodeStuff;
using HarmonyLib;
using ScandalsTweaks.Patches;
using ScandalsTweaks.Utils;
using ScanVan.Networking;
using ScanVan.Patches;
using ScanVan.Utils;
using ScandalsTweaks.Compatibility;
using System.Threading;

namespace ScanVan.Patches.InternalUtils;

[HarmonyPatch]
public static class ScandalsTweaksPatches
{
    public static bool IsPlayerInVan(PlayerControllerB player, bool checkTrunk = false)
    {
        if (PlayerControllerBPatches.playerData[player].playerSeatedInVan ||
            PlayerControllerBPatches.playerData[player].playerRidingInVanCab ||
            (checkTrunk && PlayerControllerBPatches.playerData[player].playerRidingInVanStorage))
            return true;

        return false;
    }

    [HarmonyPatch(typeof(JLLCompatibility), nameof(JLLCompatibility.CanPlayerBeBlown))]
    [HarmonyPrefix]
    private static bool CanPlayerBeBlown_Prefix(PlayerControllerB player, ref bool __result)
    {
        if (IsPlayerInVan(player, true))
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(GlobalUtilities), nameof(GlobalUtilities.ShouldAllowSightForVehicle))]
    [HarmonyPrefix]
    private static bool ShouldAllowSightForVehicle_Prefix(PlayerControllerB player, EnemyAI enemy, ref bool __result)
    {
        if (IsPlayerInVan(player, false))
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(GiantKiwiAI_Patches), nameof(GiantKiwiAI_Patches.IsTargetPlayerInVehicle))]
    [HarmonyPrefix]
    private static bool IsTargetPlayerInVehicle_Prefix(GiantKiwiAI giantKiwiAi, VehicleController vehicleController, ref bool __result)
    {
        if (vehicleController is not CruiserXLController controller)
            return true;

        var targetData = PlayerControllerBPatches.playerData[giantKiwiAi.targetPlayer];
        bool targetInTruck = targetData.playerSeatedInVan ||
                             targetData.playerRidingInVanCab ||
                            (targetData.playerRidingInVanStorage && !controller.liftGateOpen) ||
                             controller.ontopOfTruckCollider.ClosestPoint(giantKiwiAi.targetPlayer.transform.position) ==
                             giantKiwiAi.targetPlayer.transform.position;

        if (targetInTruck)
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Landmine_Patches), nameof(Landmine_Patches.ShouldCheckCustomKnockback))]
    [HarmonyPrefix]
    private static bool ShouldCheckCustomKnockback_Prefix(ref bool __result)
    {
        if (PlayerUtils.isSeatedInVan)
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Landmine_Patches), nameof(Landmine_Patches.CanPlayerBeKnockedBack))]
    [HarmonyPrefix]
    private static bool CanPlayerBeKnockedBack_Prefix(ref bool __result)
    {
        if (PlayerUtils.isSeatedInVan && UserConfig.PreventKnockback.Value)
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Landmine_Patches), nameof(Landmine_Patches.CurrentForceMultiplier))]
    [HarmonyPrefix]
    private static bool CurrentForceMultiplier_Prefix(ref float __result)
    {
        if (References.vanController != null)
        {
            __result = 1f;
            return false;
        }
        return true;
    }
}
