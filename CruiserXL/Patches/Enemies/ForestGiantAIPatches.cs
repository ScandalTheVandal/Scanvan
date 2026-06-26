using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace ScanVan.Patches.Enemies;

[HarmonyPatch(typeof(ForestGiantAI))]
public static class ForestGiantAIPatches
{
    [HarmonyPatch(nameof(ForestGiantAI.AnimationEventA))]
    [HarmonyPrefix]
    static bool AnimationEventA_Prefix(ForestGiantAI __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController controller = References.vanController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
        if (playerControllerB == null)
            return false;

        // do not allow fall death in the truck
        if (VehicleUtils.IsPlayerInVanCabin(vanController: controller) ||
            VehicleUtils.IsPlayerInVanStorage(vanController: controller) ||
            VehicleUtils.IsPlayerSeatedInVan())
            return false;

        // not in our truck, run vanilla logic
        return true;
    }

    [HarmonyPatch(nameof(ForestGiantAI.OnCollideWithPlayer))]
    [HarmonyPrefix]
    static bool OnCollideWithPlayer_Prefix(ForestGiantAI __instance, Collider other, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        CruiserXLController controller = References.vanController;
        if (controller == null)
            return true;

        PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inEatingPlayerAnimation, false);
        if (playerControllerB == null)
            return true;

        if (VehicleUtils.IsPlayerSeatedInVan())
        {
            if (VehicleUtils.IsSeatedPlayerProtected(playerController: playerControllerB, vanController: controller, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 8f))
            {
                return false;
            }
            return true;
        }
        if (VehicleUtils.IsPlayerInVanBounds(vanController: controller))
        {
            if (VehicleUtils.IsPlayerProtectedByVan(playerController: playerControllerB, vanController: controller, checkWindows: true, windshieldCheck: true, velocityCheck: true, velocityMagnitude: 8f))
            {
                return false;
            }
            return true;
        }
        return true;
    }
}