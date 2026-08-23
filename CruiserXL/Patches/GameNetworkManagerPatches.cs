using System;
using ScanVan.Networking;
using ScanVan.Utils;
using HarmonyLib;

namespace ScanVan.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
public static class GameNetworkManagerPatches
{
    [HarmonyPatch(nameof(GameNetworkManager.Start))]
    [HarmonyPostfix]
    public static void GameNetworkManager_Post_Start()
    {
        ScanVanNetworker.Init();
    }

    [HarmonyPatch(nameof(GameNetworkManager.SaveItemsInShip))]
    [HarmonyPostfix]
    static void GameNetworkManager_Post_SaveItemsInShip(GameNetworkManager __instance)
    {
        try
        {
            if (StartOfRound.Instance.attachedVehicle && StartOfRound.Instance.attachedVehicle is CruiserXLController controller)
            {
                SaveManager.Save(SaveManager.SavedVanRotation, controller.magnetTargetRotation.eulerAngles);
                SaveManager.Save(SaveManager.SavedVanPosition, controller.magnetTargetPosition);
                SaveManager.Save(SaveManager.SavedVanTurbo, controller.turboBoosts);
                SaveManager.Save(SaveManager.SavedVanVariant, (int)controller.vanVariant);
                SaveManager.Save(SaveManager.SavedVanIgnition, controller.ignitionStarted);
                SaveManager.Save(SaveManager.SavedVanSteering, controller.steeringWheelAnimFloat);
                SaveManager.Save(SaveManager.SavedVanHealth, controller.carHP);
                SaveManager.Save(SaveManager.SavedVanServiceAlert, controller.voiceModule.randomServiceAlert);
                SaveManager.Save(SaveManager.SavedVanAltAlerts, controller.voiceModule.useAltAudio);
                SaveManager.Save(SaveManager.SavedVanMileage, controller.odometerValue);

                Plugin.LogMessage("Successfully saved Van data.");
            }
            else
            {
                SaveManager.Delete(SaveManager.SavedVanRotation);
                SaveManager.Delete(SaveManager.SavedVanPosition);
                SaveManager.Delete(SaveManager.SavedVanTurbo);
                SaveManager.Delete(SaveManager.SavedVanVariant);
                SaveManager.Delete(SaveManager.SavedVanIgnition);
                SaveManager.Delete(SaveManager.SavedVanSteering);
                SaveManager.Delete(SaveManager.SavedVanHealth);
                SaveManager.Delete(SaveManager.SavedVanServiceAlert);
                SaveManager.Delete(SaveManager.SavedVanAltAlerts);
                SaveManager.Delete(SaveManager.SavedVanMileage);

                Plugin.LogMessage("Successfully deleted Van data.");
            }
        }
        catch (Exception e)
        {
            Plugin.LogError("Exception caught saving Van data:\n" + e);
        }
    }
}