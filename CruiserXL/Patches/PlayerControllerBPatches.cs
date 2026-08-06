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

namespace ScanVan.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
public static class PlayerControllerBPatches
{
    private static bool usingSeatCamera;

    public class PlayerControllerBData
    {
        public bool applyVoiceEffects = false;

        public bool playerRidingInVanCab;
        public bool playerRidingInVanStorage;
        public bool playerRidingOnVan;
    }

    public static Dictionary<PlayerControllerB, PlayerControllerBData> playerData = new();
    public static float checkInterval;

    // optimisation
    private static Quaternion armsMetarigParentRot = Quaternion.Euler(90f, 0f, 0f);
    private static Quaternion armsMetarigRot = Quaternion.Euler(-90f, 0f, 0f);

    private static Vector3 localArmsPos = new Vector3(0f, -0.008f, -0.43f);
    private static Quaternion localArmsRot = Quaternion.Euler(84.78056f, 0f, 0f);

    private static Vector3 playerBodyPos = Vector3.zero;
    private static Quaternion playerBodyRot = Quaternion.Euler(-90f, 0f, 0f);


    private static void RemoveStalePlayerData()
    {
        List<PlayerControllerB> playersToRemove = new();
        foreach (PlayerControllerB player in playerData.Keys)
        {
            if (!player)
            {
                playersToRemove.Add(player);
            }
        }

        foreach (PlayerControllerB player in playersToRemove)
        {
            playerData.Remove(player);
        }
    }

    [HarmonyPatch(nameof(PlayerControllerB.Awake))]
    [HarmonyPostfix]
    static void PlayerControllerB_Post_Awake(PlayerControllerB __instance)
    {
        RemoveStalePlayerData();
        if (!playerData.ContainsKey(__instance))
        {
            PlayerControllerBData thisData = new();
            playerData.Add(__instance, thisData);
        }
    }

    // i'm not sure if this actually works, i'll need to look into this
    [HarmonyPatch(nameof(PlayerControllerB.SpawnPlayerAnimation))]
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

    [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
    [HarmonyPrefix]
    public static void PlayerControllerB_Pre_LateUpdate(PlayerControllerB __instance)
    {
        if (__instance == null ||
            !__instance.isPlayerControlled ||
            __instance != GameNetworkManager.Instance.localPlayerController)
        {
            return;
        }
        SetPlayerVehicleZone(__instance);
    }

    private static void SetPlayerVehicleZone(PlayerControllerB playerController)
    {
        CruiserXLController vanController = VehicleUtils.vanController;

        var playerData = PlayerControllerBPatches.playerData[playerController];
        bool ridingInVanCab = vanController?.vanCabinZone.playerInZone ?? false;
        bool ridingInVanStorage = vanController?.vanStorageZone.playerInZone ?? false;
        bool ridingOnVan = vanController?.vanZone.playerInZone ?? false;

        if (playerData.playerRidingInVanCab == ridingInVanCab && 
            playerData.playerRidingInVanStorage == ridingInVanStorage &&
            playerData.playerRidingOnVan == ridingOnVan)
        {
            return;
        }

        playerData.playerRidingInVanCab = ridingInVanCab;
        playerData.playerRidingInVanStorage = ridingInVanStorage;
        playerData.playerRidingOnVan = ridingOnVan;
        ScanVanNetworker.Instance?.SyncPlayerZoneRpc(playerController.NetworkObject,
                                                 ridingInVanStorage,
                                                 ridingInVanCab,
                                                 ridingOnVan);
    }

    [HarmonyPatch(nameof(PlayerControllerB.Update))]
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

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(__instance, vanController);
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
    [HarmonyPatch(nameof(PlayerControllerB.Update))]
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

        if (!VehicleUtils.IsPlayerSeatedInVan(__instance, vanController))
            return;

        usingSeatCamera = true;
        Vector3 cameraOffset = Vector3.zero;

        // this default offset is pretty much the "sweet-spot" where the camera lines
        // up with the employees visor model
        //if (UserConfig.SeatBoostEnabled.Value) cameraOffset = new Vector3(0f, 0.118f, -0.05f) * UserConfig.SeatBoostScale.Value;
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

    // this fixes a really annoying visual bug with the players model, as 
    // various parts such as the first person arms can become disaligned
    // and cause obvious visual problems such as the ignition key not 
    // aligning properly during the ignition animation, or even causing
    // the players body to shift backwards, resulting in their hands
    // not visually holding anything such as the steering wheel
    [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
    [HarmonyPostfix]
    private static void PlayerControllerB_Post_LateUpdate(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
        {
            return;
        }

        CruiserXLController vanController = VehicleUtils.vanController;
        if (vanController == null)
            return;

        bool playerSeated = VehicleUtils.IsPlayerSeatedInVan(__instance, vanController);
        if (!playerSeated)
            return;

        var playerModelArmsRig = __instance.playerModelArmsMetarig;
        var localArmsTransform = __instance.localArmsTransform;
        var playerBodyAnimator = __instance.playerBodyAnimator.transform;

        playerModelArmsRig.parent.transform.localRotation = armsMetarigParentRot;
        playerModelArmsRig.localRotation = armsMetarigRot;
        localArmsTransform.localPosition = localArmsPos;
        localArmsTransform.localRotation = localArmsRot;
        playerBodyAnimator.localPosition = playerBodyPos;
        playerBodyAnimator.localRotation = playerBodyRot;
    }
}