using CruiserXL.Events;
using CruiserXL.Networking;
using CruiserXL.Utils;
using HarmonyLib;
using System;
using UnityEngine;


namespace CruiserXL.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public static class StartOfRoundPatches
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPostfix]
    static void Awake_Postfix(StartOfRound __instance)
    {
        SCVNetworker.Create();
    }

    /// <summary>
    ///  Available from RadioFurniture, licensed under GNU General Public License.
    ///  Source: https://github.com/legoandmars/RadioFurniture/tree/master/RadioFurniture
    /// </summary>
    [HarmonyPatch(nameof(StartOfRound.EndOfGame))]
    [HarmonyPrefix]
    static void EndOfGame_Prefix(ref SelectableLevel ___currentLevel)
    {
        if (___currentLevel.currentWeather == LevelWeatherType.Stormy)
        {
            WeatherEvents.StormEnd();
        }
    }

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/StartOfRound.cs
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Network/Patches/StartOfRound.cs
    /// </summary>
    [HarmonyPatch(nameof(StartOfRound.SyncAlreadyHeldObjectsServerRpc))]
    [HarmonyPostfix]
    static void SyncAlreadyHeldObjectsServerRpc_Postfix(StartOfRound __instance, int joiningClientId)
    {
        if (!__instance.attachedVehicle || __instance.attachedVehicle is not CruiserXLController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.Logger.LogError("attempted to send client data, but the truck is null? please report this to Scandal.");
                return;
            }
            controller.SendClientSyncData();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("exception caught sending saved Scanvan data:\n" + e);
        }
    }

    [HarmonyPatch(nameof(StartOfRound.LoadAttachedVehicle))]
    [HarmonyPostfix]
    static void LoadAttachedVehicle_Postfix(StartOfRound __instance)
    {
        if (!__instance.attachedVehicle ||  __instance.attachedVehicle is not CruiserXLController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.Logger.LogError("attempted to load saved data, but the truck is null? please report this to Scandal.");
                return;
            }

            SaveManager.TryLoad<bool>("AttachedVehicleVariant", out var variant);
            SaveManager.TryLoad<Vector3>("AttachedVehicleRotation", out var rotation);
            SaveManager.TryLoad<Vector3>("AttachedVehiclePosition", out var position);
            SaveManager.TryLoad<int>("AttachedVehicleTurbo", out var turbos);
            SaveManager.TryLoad<bool>("AttachedVehicleIgnition", out var ignition);
            SaveManager.TryLoad<float>("AttachedVehicleSteeringRotation", out var wheelPosition);
            SaveManager.TryLoad<int>("AttachedVehicleGear", out var carGear);
            SaveManager.TryLoad<int>("AttachedVehicleHealth", out var carHealth);
            SaveManager.TryLoad<bool>("AttachedVehicleWindshield", out var carWindow);
            SaveManager.TryLoad<bool>("AttachedVehicleWindshieldBroken", out var carWindowBroken);

            controller.isSpecial = variant;
            controller.SetVariant(controller.isSpecial);

            controller.transform.rotation = Quaternion.Euler(rotation);
            controller.transform.position = StartOfRound.Instance.elevatorTransform.TransformPoint(position);

            controller.turboBoosts = turbos;

            controller.voiceModule.hasJustPlayedSixBeepChime = ignition;
            controller.keyIsInIgnition = ignition;
            controller.SetFrontCabinLightOn(setOn: ignition);
            controller.SetIgnition(ignition);
            if (ignition) controller.dashboardSymbolPreStartup = controller.StartCoroutine(controller.PreIgnitionSymbolCheck());
            controller.driversSideWindow.interactable = ignition;
            controller.passengersSideWindow.interactable = ignition;

            controller.syncedWheelRotation = wheelPosition;
            controller.steeringWheelAnimFloat = wheelPosition;
            controller.autoGear = (TruckGearShift)carGear;
            controller.carHP = carHealth;

            if (carWindow) controller.ShatterWindshield();
            if (carWindowBroken) controller.BreakWindshield();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("exception caught loading saved Scanvan data:\n" + e);
        }
    }
}