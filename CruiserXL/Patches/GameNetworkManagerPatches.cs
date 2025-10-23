using System;
using CruiserXL.Utils;
using HarmonyLib;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatches
{
    [HarmonyPatch("SaveItemsInShip")]
    [HarmonyPostfix]
    static void SaveItemsInShip_Postfix(GameNetworkManager __instance)
    {
        //save Scanvan data if we have one
        //try
        //{
        //    if (StartOfRound.Instance.attachedVehicle && StartOfRound.Instance.attachedVehicle.TryGetComponent<CruiserXLController>(out var controller))
        //    {
        //        SaveManager.Save("AttachedVehicleRotation", controller.magnetTargetRotation.eulerAngles);
        //        SaveManager.Save("AttachedVehiclePosition", controller.magnetTargetPosition);
        //        SaveManager.Save("AttachedVehicleTurbo", controller.turboBoosts);
        //        SaveManager.Save("AttachedVehicleIgnition", controller.ignitionStarted);
        //        SaveManager.Save("AttachedVehicleSteeringRotation", controller.steeringWheelAnimFloat);
        //        SaveManager.Save("AttachedVehicleRadioSeed", controller.randomRadioSeed);
        //        SaveManager.Save("AttachedVehicleHP", controller.carHP);
        //        Plugin.Logger.LogMessage("Successfully saved Scanvan data.");
        //    }
        //    else
        //    {
        //        SaveManager.Delete("AttachedVehicleRotation");
        //        SaveManager.Delete("AttachedVehiclePosition");
        //        SaveManager.Delete("AttachedVehicleTurbo");
        //        SaveManager.Delete("AttachedVehicleIgnition");
        //        SaveManager.Delete("AttachedVehicleSteeringRotation");
        //        SaveManager.Delete("AttachedVehicleRadioSeed");
        //        SaveManager.Delete("AttachedVehicleHP");
        //    }
        //}
        //catch (Exception e)
        //{
        //    Plugin.Logger.LogError("Exception caught saving Scanvan data:\n" + e);
        //}
    }
}