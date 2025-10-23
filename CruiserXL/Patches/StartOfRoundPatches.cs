using CruiserXL.Events;
using CruiserXL.Utils;
using HarmonyLib;
using System;
using UnityEngine;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public static class StartOfRoundPatches
{
    // Used for the radio static effect.
    [HarmonyPatch("EndOfGame")]
    [HarmonyPrefix]
    static void EndOfGame(ref SelectableLevel ___currentLevel)
    {
        if (___currentLevel.currentWeather == LevelWeatherType.Stormy)
        {
            WeatherEvents.StormEnd();
        }
    }

    // None of the stuff below works, it broke one 
    // day and I never got round to fixing it.
    // Maybe another time.

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/StartOfRound.cs
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Network/Patches/StartOfRound.cs
    /// </summary>
    [HarmonyPatch("SyncAlreadyHeldObjectsServerRpc")]
    [HarmonyPostfix]
    static void SyncAlreadyHeldObjectsServerRpc(StartOfRound __instance, int joiningClientId)
    {
        //if (!__instance.attachedVehicle || __instance.attachedVehicle is not CruiserXLController) return;
        //try
        //{
        //    if (__instance.attachedVehicle.TryGetComponent<CruiserXLController>(out var controller))
        //    {
        //        controller.SendClientSyncData();
        //    }
        //}
        //catch (Exception e)
        //{
        //    Plugin.Logger.LogError("Exception caught sending saved Scanvan data:\n" + e);
        //}
    }

    [HarmonyPatch("LoadAttachedVehicle")]
    [HarmonyPostfix]
    static void LoadAttachedVehicle_Postfix(StartOfRound __instance)
    {
        //if (!__instance.attachedVehicle || __instance.attachedVehicle is not CruiserXLController) return;
        //try
        //{
        //    if (__instance.attachedVehicle.TryGetComponent<CruiserXLController>(out var controller))
        //    {
        //        if (SaveManager.TryLoad<Vector3>("AttachedVehicleRotation", out var rotation))
        //        {
        //            controller.transform.rotation = Quaternion.Euler(rotation);
        //        }
        //        if (SaveManager.TryLoad<Vector3>("AttachedVehiclePosition", out var position))
        //        {
        //            controller.transform.position = StartOfRound.Instance.elevatorTransform.TransformPoint(position);
        //        }
        //        if (SaveManager.TryLoad<int>("AttachedVehicleTurbo", out var turbos))
        //        {
        //            controller.turboBoosts = turbos;
        //        }
        //        if (SaveManager.TryLoad<bool>("AttachedVehicleIgnition", out var ignition))
        //        {
        //            controller.hasAlertedOnEngineStart = ignition;
        //            controller.pendingKeysInIgnitionThanked = ignition;
        //            controller.EVAAudioClipsJustPlayed[4] = ignition;

        //            controller.SetIgnition(ignition);
        //            controller.keyIsInIgnition = ignition;
        //            controller.SetFrontCabinLightOn(setOn: ignition);
        //        }
        //        if (SaveManager.TryLoad<float>("AttachedVehicleSteeringRotation", out var wheelPosition))
        //        {
        //            controller.syncedWheelRotation = wheelPosition;
        //            controller.steeringWheelAnimFloat = wheelPosition;
        //        }
        //        if (SaveManager.TryLoad<int>("AttachedVehicleRadioSeed", out var radioSeed))
        //        {
        //            controller.randomRadioSeed = radioSeed;
        //            controller.ScrambleRadioClipOrder(controller.randomRadioSeed);
        //        }
        //        if (SaveManager.TryLoad<int>("AttachedVehicleHP", out var carHealth))
        //        {
        //            controller.carHP = carHealth;
        //        }
        //    }
        //}
        //catch (Exception e)
        //{
        //    Plugin.Logger.LogError("Exception caught loading saved Scanvan data:\n" + e);
        //}
    }
}