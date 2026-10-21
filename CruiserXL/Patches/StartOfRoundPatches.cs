using ScanVan.Managers;
using ScanVan.Networking;
using ScanVan.Utils;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;


namespace ScanVan.Patches;

[HarmonyPatch]
public static class StartOfRoundPatches
{
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
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
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SyncAlreadyHeldObjectsServerRpc))]
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
            controller.SendClientSyncData((ulong)joiningClientId);
        }
        catch (Exception e)
        {
            Plugin.LogError("Exception caught sending saved Van data:\n" + e);
        }
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.LoadAttachedVehicle))]
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
            if (SaveManager.TryLoad<int>(SaveManager.SavedVanVariant, out var variant))
            {
                controller.vanVariant = (VanVariant)variant;
                controller.SetVanVariant();
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
                if (controller.vanVariant != VanVariant.Creasman)
                {
                    controller.voiceModule.ignitionChimeStarted = ignition;
                    controller.voiceModule.ignitionChimeFinished = ignition;
                }
                controller.disableAnimations = !ignition;

                controller.ignitionOn = ignition;
                controller.keyIsInIgnition = ignition;

                controller.SetIgnitionKeyValues(setKeyInHand: false, setKeyInSlot: ignition);
                controller.SetIgnition(ignition, ignition);
                controller.SetIgnitionInteractTrigger();

                if (ignition)
                {
                    controller.dashboardSweepCoroutine = controller.StartCoroutine(controller.DashboardSweepCoroutine());
                    controller.TrySetMirrorsFolded(setFolded: ignition);
                }
                if (controller.hasVehicleDisplay)
                {
                    controller.SetSymbolActive(controller.vehicleDisplay, controller.vehicleDisplayLight, ignition);
                }
                controller.SetWindowInteractTriggers(isIgnitionOn: ignition);
                controller.ignitionCollider.enabled = ignition;
                const string IGNITION_ANIM = "SA_IgnitionAnim";
                controller.ignitionAnimator.SetInteger(IGNITION_ANIM, ignition ? 1 : 0);
            }
            if (SaveManager.TryLoad<float>(SaveManager.SavedVanSteering, out var wheelPosition))
            {
                controller.syncedSteeringWheelRotation = wheelPosition;
                controller.steeringWheelAnimFloat = wheelPosition;
            }
            if (SaveManager.TryLoad<int>(SaveManager.SavedVanHealth, out var vanHealth))
            {
                controller.carHP = vanHealth;
            }
            if (SaveManager.TryLoad<int>(SaveManager.SavedVanServiceAlert, out var voiceAlert))
            {
                controller.voiceModule.randomServiceAlert = voiceAlert;
            }
            if (SaveManager.TryLoad<bool>(SaveManager.SavedVanAltAlerts, out var altVoiceAlerts))
            {
                controller.voiceModule.useAltAudio = altVoiceAlerts;
            }
            if (SaveManager.TryLoad<float>(SaveManager.SavedVanMileage, out var odometerMileage))
            {
                controller.odometerValue = odometerMileage;
            }
        }
        catch (Exception e)
        {
            Plugin.LogError("Exception caught loading saved Van data:\n" + e);
        }
    }
}