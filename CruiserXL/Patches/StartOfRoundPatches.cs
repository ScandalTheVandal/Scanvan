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
    [HarmonyPostfix]
    static void Awake_Postfix(StartOfRound __instance)
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
    static void SyncAlreadyHeldObjectsServerRpc_Postfix(StartOfRound __instance, int joiningClientId)
    {
        if (!__instance.attachedVehicle || __instance.attachedVehicle is not CruiserXLController controller) return;
        try
        {
            if (controller == null)
            {
                Plugin.Logger.LogError("ScanVan: Attempted to send client data, but the van is null? please report this to Scandal.");
                return;
            }
            controller.SendClientSyncData();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("ScanVan: Exception caught sending saved Scanvan data:\n" + e);
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
                Plugin.Logger.LogError("ScanVan: Attempted to load saved data, but the van is null? please report this to Scandal.");
                return;
            }
            if (SaveManager.TryLoad<bool>("AttachedVehicleVariant", out var variant))
            {
                controller.isSpecial = variant;
                controller.SetVariant(controller.isSpecial);
            }
            if (SaveManager.TryLoad<Vector3>("AttachedVehiclePosition", out var position) &&
                SaveManager.TryLoad<Vector3>("AttachedVehicleRotation", out var rotation))
            {
                controller.transform.rotation = Quaternion.Euler(rotation);
                controller.transform.position = StartOfRound.Instance.elevatorTransform.TransformPoint(position);
            }
            if (SaveManager.TryLoad<int>("AttachedVehicleTurbo", out var turbos))
            {
                controller.turboBoosts = turbos;
            }
            if (SaveManager.TryLoad<bool>("AttachedVehicleIgnition", out var ignition))
            {
                controller.voiceModule.ignitionChimeStarted = ignition;
                controller.voiceModule.ignitionChimeFinished = ignition;

                controller.disableAnimations = !ignition;
                controller.inIgnitionAnimation = !ignition;
                controller.accessoryMode = ignition;

                controller.accessoryMode = ignition;
                controller.keyIsInIgnition = ignition;

                controller.SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: ignition);
                controller.SetIgnition(ignition, ignition);
                controller.SetFrontCabinLightOn(setOn: ignition);
                controller.TrySetCarIgnitionTriggers();

                if (ignition) controller.dashboardSymbolPreStartup = controller.StartCoroutine(controller.PreIgnitionSymbolCheck());
                controller.SetSymbolActive(controller.vehicleDisplay, controller.vehicleDisplayLight, ignition);
                controller.driversSideWindow.interactable = ignition;
                controller.passengersSideWindow.interactable = ignition;
                controller.ignitionCollider.enabled = ignition;
                controller.ignitionAnimator.SetInteger("SAIgnition_Anim", ignition ? 1 : 0);
            }
            if (SaveManager.TryLoad<float>("AttachedVehicleSteeringRotation", out var wheelPosition))
            {
                controller.syncedSteeringWheelRotation = wheelPosition;
                controller.steeringWheelAnimFloat = wheelPosition;
            }
            if (SaveManager.TryLoad<int>("AttachedVehicleHealth", out var carHealth))
            {
                controller.carHP = carHealth;
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError("ScanVan: Exception caught loading saved Scanvan data:\n" + e);
        }
    }
}