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
    public static void Start_Postfix()
    {
        ScanVanNetworker.Init();
    }

    [HarmonyPatch(nameof(GameNetworkManager.SaveItemsInShip))]
    [HarmonyPostfix]
    static void SaveItemsInShip_Postfix(GameNetworkManager __instance)
    {
        try
        {
            if (StartOfRound.Instance.attachedVehicle && StartOfRound.Instance.attachedVehicle is CruiserXLController controller)
            {
                SaveManager.Save("AttachedVehicleRotation", controller.magnetTargetRotation.eulerAngles);
                SaveManager.Save("AttachedVehiclePosition", controller.magnetTargetPosition);
                SaveManager.Save("AttachedVehicleTurbo", controller.turboBoosts);
                SaveManager.Save("AttachedVehicleVariant", controller.isSpecial);
                SaveManager.Save("AttachedVehicleIgnition", controller.ignitionStarted);
                SaveManager.Save("AttachedVehicleSteeringRotation", controller.steeringWheelAnimFloat);
                SaveManager.Save("AttachedVehicleHealth", controller.carHP);

                Plugin.Logger.LogMessage("ScanVan: Successfully saved Scanvan data.");
            }
            else
            {
                SaveManager.Delete("AttachedVehicleRotation");
                SaveManager.Delete("AttachedVehiclePosition");
                SaveManager.Delete("AttachedVehicleTurbo");
                SaveManager.Delete("AttachedVehicleVariant");
                SaveManager.Delete("AttachedVehicleIgnition");
                SaveManager.Delete("AttachedVehicleSteeringRotation");
                SaveManager.Delete("AttachedVehicleHealth");

                Plugin.Logger.LogMessage("ScanVan: Successfully deleted Scanvan data.");
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("ScanVan: Exception caught saving Scanvan data:\n" + e);
        }
    }
}