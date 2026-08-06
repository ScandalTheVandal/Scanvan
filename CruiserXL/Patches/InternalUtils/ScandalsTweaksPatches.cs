using GameNetcodeStuff;
using HarmonyLib;
using ScandalsTweaks.Patches;
using ScandalsTweaks.Utils;
using ScanVan.Networking;
using ScanVan.Patches;
using ScanVan.Utils;
using ScandalsTweaks.Compatibility;
using System.Threading;
using ScandalsTweaks.Patches.Enemies;

namespace ScanVan.Patches.InternalUtils;

[HarmonyPatch]
public static class ScandalsTweaksPatches
{
    public static bool IsPlayerInVan(PlayerControllerB player, bool checkStorage = false)
    {
        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return false;

        if (VehicleUtils.IsPlayerSeatedInVan(player, vanController) ||
            VehicleUtils.IsPlayerInVanCabin(player, vanController, usePlayerData: true))
        {
            return true;
        }

        return checkStorage &&
               VehicleUtils.IsPlayerInVanStorage(player, vanController, usePlayerData: true);
    }

    [HarmonyPatch(typeof(JLLCompatibility), nameof(JLLCompatibility.CanBlowPlayer))]
    [HarmonyPrefix]
    private static bool JLLCompatibility_Pre_CanBlowPlayer(PlayerControllerB player, ref bool __result)
    {
        if (IsPlayerInVan(player, true))
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(JLLCompatibility), nameof(JLLCompatibility.CanDamagePlayer))]
    [HarmonyPrefix]
    private static bool JLLCompatibility_Pre_CanDamagePlayer(PlayerControllerB player, ref bool __result)
    {
        if (IsPlayerInVan(player, true))
        {
            __result = false;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(Utilities), nameof(Utilities.ShouldAllowSightThroughVehicle))]
    [HarmonyPrefix]
    private static bool Utilities_Pre_ShouldAllowSightThroughVehicle(PlayerControllerB player, EnemyAI enemy, ref bool __result)
    {
        if (IsPlayerInVan(player, false))
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(GiantKiwiAIPatches), nameof(GiantKiwiAIPatches.IsTargetPlayerInVehicle))]
    [HarmonyPrefix]
    private static bool GiantKiwiAIPatches_Pre_IsTargetPlayerInVehicle(GiantKiwiAI giantKiwiAi, VehicleController vehicleController, ref bool __result)
    {
        if (vehicleController is not CruiserXLController vanController)
            return true;

        PlayerControllerB targetPlayer = giantKiwiAi.targetPlayer;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(targetPlayer, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(targetPlayer, vanController, usePlayerData: true);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(targetPlayer, vanController, usePlayerData: true);
        bool storageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);

        bool playerOnVan =
            vanController.ontopOfTruckCollider.ClosestPoint(targetPlayer.transform.position) ==
            targetPlayer.transform.position;

        if (playerSeated ||
            playerInCab ||
            (playerInStorage && storageEnclosed) ||
            playerOnVan)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(BushWolfEnemyPatches), nameof(BushWolfEnemyPatches.IsTargetPlayerProtected))]
    [HarmonyPrefix]
    private static bool BushWolfEnemyPatches_Pre_IsTargetPlayerProtected(PlayerControllerB player, VehicleController vehicle, ref bool __result)
    {
        if (vehicle is not CruiserXLController vanController)
            return true;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(player, vanController);
        bool playerInCab = VehicleUtils.IsPlayerInVanCabin(player, vanController, usePlayerData: true);
        bool playerInStorage = VehicleUtils.IsPlayerInVanStorage(player, vanController, usePlayerData: true);
        bool storageEnclosed = VehicleUtils.IsVanStorageEnclosed(vanController);

        if (playerSeated)
        {
            if (VehicleUtils.IsSeatedPlayerProtected(player, vanController, checkWindows: false))
            {
                __result = true;
                return false;
            }
            return true;
        }

        if (playerInStorage && storageEnclosed)
        {
            __result = true;
            return false;
        }

        if (!playerInCab)
            return true;

        if (VehicleUtils.IsPlayerProtectedByVan(player, vanController, checkWindows: false))
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(ScandalsTweaks.Patches.LandminePatches), nameof(ScandalsTweaks.Patches.LandminePatches.ShouldCheckCustomKnockback))]
    [HarmonyPrefix]
    private static bool LandminePatches_Pre_ShouldCheckCustomKnockback(ref bool __result)
    {
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        CruiserXLController vanController = VehicleUtils.vanController;
        if (VehicleUtils.IsPlayerSeatedInVan(playerController, vanController))
        {
            __result = true;
            return false;
        }
        return true;
    }

    [HarmonyPatch(typeof(ScandalsTweaks.Patches.LandminePatches), nameof(ScandalsTweaks.Patches.LandminePatches.CanPlayerBeKnockedBack))]
    [HarmonyPrefix]
    private static bool LandminePatches_Pre_CanPlayerBeKnockedBack(ref bool __result)
    {
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        CruiserXLController vanController = VehicleUtils.vanController;
        if (VehicleUtils.IsPlayerSeatedInVan(playerController, vanController) && UserConfig.PreventKnockback.Value)
        {
            __result = false;
            return false;
        }
        return true;
    }
}
