using ScanVan.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using ScanVan.Managers;
using Cysharp.Threading.Tasks;
using ScanVan.Behaviour;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using System.Runtime.CompilerServices;
using ScanVan.Networking;
using TMPro;
using UnityEngine.InputSystem;
using System;

namespace ScanVan.Patches;

[HarmonyPatch]
public static class PlayerControllerBPatches
{
    private static bool usingSeatCamera;

    // i'm not sure if this actually works, i'll need to look into this
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SpawnPlayerAnimation))]
    [HarmonyPostfix]
    static void PlayerControllerB_Post_SpawnPlayerAnimation(PlayerControllerB __instance)
    {
        if (__instance != GameNetworkManager.Instance.localPlayerController) 
            return;

        PlayerControllerB player = __instance;
        __instance.GetComponentInChildren<LODGroup>().enabled = false;

        player.thisPlayerModel.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        player.localVisor.GetComponentInChildren<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        player.thisPlayerModelLOD1.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        player.thisPlayerModelLOD2.enabled = false;

        player.thisPlayerModel.gameObject.layer = 23;
        player.thisPlayerModelArms.gameObject.layer = 5;
    }

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
    [HarmonyPrefix]
    public static void PlayerControllerB_Prefix_Update(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
            return;

        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        bool playerSeated = vanController.IsPlayerSeated(__instance);
        if (!playerSeated)
            return;

        if (usingSeatCamera && !playerSeated)
        {
            usingSeatCamera = false;
            __instance.gameplayCamera.transform.localPosition = Vector3.zero;
            __instance.horizontalClamp = 70f;
            return;
        }
        if (playerSeated && UserConfig.PreventKnockback.Value)
        {
            __instance.externalForceAutoFade = Vector3.zero;
            __instance.externalForces = Vector3.zero;
        }
    }

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/PlayerController.cs
    /// </summary>
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.Update))]
    [HarmonyPostfix]
    public static void PlayerControllerB_Post_Update(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
            return;

        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        if (!vanController.IsPlayerSeated(__instance))
            return;

        usingSeatCamera = true;
        // this default offset is pretty much the "sweet-spot" where the camera lines
        // up with the employees visor model
        Vector3 cameraOffset = Vector3.zero;
        if (UserConfig.SeatBoostEnabled.Value) cameraOffset = new Vector3(0f, 0.09f * UserConfig.SeatBoostScale.Value, 0f);
        Vector3 lookFlat = __instance.gameplayCamera.transform.localRotation * Vector3.forward;
        lookFlat.y = 0;
        float angleToBack = Vector3.Angle(lookFlat, Vector3.back);
        if (angleToBack < 70 && __instance != vanController.currentMiddlePassenger)
        {
            // if we're looking backwards, offset the camera to the side ('leaning')
            cameraOffset.x = Mathf.Sign(lookFlat.x) * ((70f - angleToBack) / 70f);
        }
        __instance.gameplayCamera.transform.localPosition = cameraOffset;

        // extremely hacky method to prevent leaning through a closed window, while
        // still allowing leaning through the cabin window, since you cannot directly
        // clamp both left and right directions seperately from each-other
        if (__instance == vanController.currentDriver)
        {
            __instance.horizontalClamp = vanController.frontLeftWindow.isWindowOpen ? 163f : 105f;
            if (__instance.ladderCameraHorizontal > 0)
            {
                __instance.horizontalClamp = 163f;
                return;
            }
        }
        else if (__instance == vanController.currentPassenger)
        {
            __instance.horizontalClamp = vanController.frontRightWindow.isWindowOpen ? 163f : 105f;
            if (__instance.ladderCameraHorizontal < 0)
            {
                __instance.horizontalClamp = 163f;
                return;
            }
        }
    }
}