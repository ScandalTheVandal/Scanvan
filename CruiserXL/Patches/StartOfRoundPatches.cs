using ScanVan.Managers;
using ScanVan.Networking;
using ScanVan.Utils;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;


namespace ScanVan.Patches;

[HarmonyPatch(typeof(StartOfRound))]
public static class StartOfRoundPatches
{
    [HarmonyPatch(nameof(StartOfRound.Awake))]
    [HarmonyPrefix]
    static void StartOfRound_Pre_Awake(StartOfRound __instance)
    {
        ScanVanNetworker.Create();
        if (RadioManager._stations.Count == 0)
        {
            RadioManager.GetRadioStations().Forget();
        }
    }

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/StartOfRound.cs
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Network/Patches/StartOfRound.cs
    /// </summary>
    [HarmonyPatch(nameof(StartOfRound.SyncAlreadyHeldObjectsServerRpc))]
    [HarmonyPostfix]
    static void StartOfRound_Post_SyncAlreadyHeldObjectsServerRpc(StartOfRound __instance, int joiningClientId)
    {
        if (!__instance.attachedVehicle || __instance.attachedVehicle is not CruiserXLController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.LogError("Attempted to send client data, but the Van is null? please report this to Scandal.");
                return;
            }
            controller.SendClientSyncData();
        }
        catch (Exception e)
        {
            Plugin.LogError("Exception caught sending saved Van data:\n" + e);
        }
    }

    [HarmonyPatch(nameof(StartOfRound.LoadAttachedVehicle))]
    [HarmonyPostfix]
    static void StartOfRound_Post_LoadAttachedVehicle(StartOfRound __instance)
    {
        if (!__instance.attachedVehicle || __instance.attachedVehicle is not CruiserXLController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.LogError("Attempted to load saved data, but the Van is null? please report this to Scandal.");
                return;
            }
            if (SaveManager.TryLoad<bool>(SaveManager.SavedVanVariant, out var variant))
            {
                controller.isSpecialVariant = variant;
                controller.SetVariant(controller.isSpecialVariant);
            }
            if (SaveManager.TryLoad<Vector3>(SaveManager.SavedVanPosition, out var position) &&
                SaveManager.TryLoad<Vector3>(SaveManager.SavedVanRotation, out var rotation))
            {
                controller.transform.rotation = Quaternion.Euler(rotation);
                controller.transform.position = StartOfRound.Instance.elevatorTransform.TransformPoint(position);
            }
            if (SaveManager.TryLoad<int>(SaveManager.SavedVanTurbo, out var turbos))
            {
                controller.turboBoosts = turbos;
            }
            if (SaveManager.TryLoad<bool>(SaveManager.SavedVanIgnition, out var ignition))
            {
                controller.voiceModule.ignitionChimeStarted = ignition;
                controller.voiceModule.ignitionChimeFinished = ignition;

                controller.disableAnimations = !ignition;
                controller.inIgnitionAnimation = !ignition;
                controller.ignitionOn = ignition;

                controller.ignitionOn = ignition;
                controller.keyIsInIgnition = ignition;

                controller.SetIgnitionValues(trying: false, keyInHand: false, keyInSlot: ignition);
                controller.SetIgnition(ignition, ignition);
                controller.SetFrontCabinLightOn(setOn: ignition);
                controller.SetIgnitionTrigger();

                if (ignition) controller.dashboardSweepCoroutine = controller.StartCoroutine(controller.DashboardSweepCoroutine());
                controller.SetSymbolActive(controller.vehicleDisplay, controller.vehicleDisplayLight, ignition);
                controller.driversSideWindow.interactable = ignition;
                controller.passengersSideWindow.interactable = ignition;
                controller.ignitionCollider.enabled = ignition;
                controller.ignitionAnimator.SetInteger("SAIgnition_Anim", ignition ? 1 : 0);
            }
            if (SaveManager.TryLoad<float>(SaveManager.SavedVanSteering, out var wheelPosition))
            {
                controller.syncedSteeringWheelRotation = wheelPosition;
                controller.steeringWheelAnimFloat = wheelPosition;
            }
            if (SaveManager.TryLoad<int>(SaveManager.SavedVanHealth, out var carHealth))
            {
                controller.carHP = carHealth;
            }
            if (SaveManager.TryLoad<int>(SaveManager.SavedVanServiceAlert, out var voiceAlert))
            {
                controller.voiceModule.randomServiceAlert = voiceAlert;
            }
            if (SaveManager.TryLoad<bool>(SaveManager.SavedVanAltAlerts, out var altVoiceAlerts))
            {
                controller.voiceModule.useAltAudio = altVoiceAlerts;
            }
        }
        catch (Exception e)
        {
            Plugin.LogError("Exception caught loading saved Van data:\n" + e);
        }
    }
}