using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using CruiserXL.Managers;
using Cysharp.Threading.Tasks;
using CruiserXL.Behaviour;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerBPatches
{
    public static float checkInterval;

    public class PlayerControllerBData
    {
        public float syncLookInputInterval;
        public float vehicleCameraHorizontal;
        public float lastVehicleCameraHorizontal;
        public int currentCarAnimation = -1;

        public bool isPlayerOnTruck;
        public bool isPlayerInCab;
        public bool isPlayerInStorage;
    }

    public static Dictionary<PlayerControllerB, PlayerControllerBData> playerData = new();


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

    public static PlayerControllerBData GetData(PlayerControllerB player)
    {
        if (!playerData.TryGetValue(player, out var data))
        {
            data = new PlayerControllerBData();
            playerData[player] = data;
        }
        return data;
    }

    [HarmonyPatch(nameof(PlayerControllerB.Awake))]
    [HarmonyPostfix]
    static void Awake_Postfix(PlayerControllerB __instance)
    {
        RemoveStalePlayerData();
        if (!playerData.ContainsKey(__instance))
        {
            playerData.Add(__instance, new PlayerControllerBData());
        }
    }

    [HarmonyPatch(nameof(PlayerControllerB.UpdatePlayerAnimationsToOtherClients))]
    [HarmonyPrefix]
    static bool UpdatePlayerAnimationsToOtherClients_Prefix(PlayerControllerB __instance, Vector2 moveInputVector, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return true;

        if (PlayerUtils.disableAnimationSync) return false;
        return true;
    }

    [HarmonyPatch(nameof(PlayerControllerB.TeleportPlayer))]
    [HarmonyPostfix]
    static void TeleportPlayer_Postfix(PlayerControllerB __instance, Vector3 pos, bool withRotation = false, float rot = 0f, bool allowInteractTrigger = false, bool enableController = true)
    {
        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        if (References.truckController == null)
            return;

        PlayerUtils.isPlayerInCab = false;
        PlayerUtils.isPlayerOnTruck = false;
        PlayerUtils.isPlayerInStorage = false;
    }

    // i'm not sure if this actually works, i'll need to look into this
    [HarmonyPatch(nameof(PlayerControllerB.SpawnPlayerAnimation))]
    [HarmonyPostfix]
    static void SpawnPlayerAnimation_Postfix(ref PlayerControllerB __instance)
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

        if (RadioManager._stations.Count == 0)
        {
            HUDManager.Instance.DisplayTip("Radio failed to preload!", 
                $"This could be an issue with the RadioBrowser API!", 
                isWarning: false);
            RadioManager.GetRadioStations().Forget();
        }
    }

    [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
    [HarmonyPostfix]
    public static void SyncZoneStateLateUpdate_Postfix(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
            return;

        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;

        if (checkInterval < 0.3f)
        {
            checkInterval += Time.deltaTime;
            return;
        }
        checkInterval = 0f;
        var data = GetData(__instance);

        if (data.isPlayerOnTruck != PlayerUtils.isPlayerOnTruck || 
            data.isPlayerInCab != PlayerUtils.isPlayerInCab || 
            data.isPlayerInStorage != PlayerUtils.isPlayerInStorage)
        {
            data.isPlayerOnTruck = PlayerUtils.isPlayerOnTruck;
            data.isPlayerInCab = PlayerUtils.isPlayerInCab;
            data.isPlayerInStorage = PlayerUtils.isPlayerInStorage;

            controller.SyncPlayerZoneRpc(
                (int)__instance.playerClientId,
                PlayerUtils.isPlayerOnTruck,
                PlayerUtils.isPlayerInCab,
                PlayerUtils.isPlayerInStorage);
        }
    }

    [HarmonyPatch(nameof(PlayerControllerB.Update))]
    [HarmonyPrefix]
    public static void Update_Prefix(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
            return;

        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        if (References.truckController == null)
            return;

        if (PlayerUtils.seatedInTruck && UserConfig.PreventKnockback.Value)
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
    public static void Update_Postfix(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
            return;

        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;

        Vector3 cameraOffset = Vector3.zero;
        bool inVehicle = __instance.inVehicleAnimation && __instance.currentTriggerInAnimationWith && __instance.currentTriggerInAnimationWith.overridePlayerParent;
        if (inVehicle && __instance.currentTriggerInAnimationWith.overridePlayerParent == controller.transform)
        {
            PlayerUtils.seatedInTruck = true;
            PlayerUtils.isPlayerInCab = true;
            PlayerUtils.isPlayerOnTruck = true;
            PlayerUtils.isPlayerInStorage = false;

            // this default offset is pretty much the "sweet-spot" where the camera lines
            // up with the employees visor model
            if (UserConfig.SeatBoostEnabled.Value) cameraOffset = new Vector3(0f, 0.118f, -0.05f) * UserConfig.SeatBoostScale.Value;
            Vector3 lookFlat = __instance.gameplayCamera.transform.localRotation * Vector3.forward;
            lookFlat.y = 0;

            float angleToBack = Vector3.Angle(lookFlat, Vector3.back);
            float cameraSpeed = 35f * Time.deltaTime;

            if (angleToBack < 70 && __instance != controller.currentMiddlePassenger)
            {
                // if we're looking backwards, offset the camera to the side ('leaning')
                cameraOffset.x = Mathf.Sign(lookFlat.x) * ((70f - angleToBack) / 70f);
            }
            __instance.gameplayCamera.transform.localPosition = cameraOffset;

            // extremely hacky method to prevent leaning through a closed window, while
            // still allowing leaning through the cabin window, since you cannot directly
            // clamp both left and right directions seperately from each-other
            if (__instance == controller.currentDriver)
            {
                __instance.horizontalClamp = controller.driversSideWindowTrigger.boolValue ? 163f : 105f;
                if (__instance.ladderCameraHorizontal > 0)
                {
                    __instance.horizontalClamp = 163f;
                    return;
                }
            }
            else if (__instance == controller.currentPassenger)
            {
                __instance.horizontalClamp = controller.passengersSideWindowTrigger.boolValue ? 163f : 105f;
                if (__instance.ladderCameraHorizontal < 0)
                {
                    __instance.horizontalClamp = 163f;
                    return;
                }
            }
        }
        else if (!__instance.inVehicleAnimation && PlayerUtils.seatedInTruck == true)
        {
            PlayerUtils.seatedInTruck = false;
            __instance.gameplayCamera.transform.localPosition = Vector3.zero;
            __instance.horizontalClamp = 70f;
        }
    }

    // this fixes a really annoying visual bug with the players model, as 
    // various parts such as the first person arms can become disaligned
    // and cause obvious visual problems such as the ignition key not 
    // aligning properly during the ignition animation, or even causing
    // the players body to shift backwards, resulting in their hands
    // not visually holding anything
    [HarmonyPatch(nameof(PlayerControllerB.LateUpdate))]
    [HarmonyPostfix]
    private static void LateUpdate_Postfix(PlayerControllerB __instance)
    {
        if (__instance == null ||
            __instance.isPlayerDead ||
            !__instance.isPlayerControlled)
            return;

        if (References.truckController == null)
            return;
        CruiserXLController controller = References.truckController;

        bool inVehicle = __instance.inVehicleAnimation &&
            __instance.currentTriggerInAnimationWith &&
            __instance.currentTriggerInAnimationWith.overridePlayerParent;

        if (inVehicle &&
            __instance.currentTriggerInAnimationWith.overridePlayerParent == controller.transform)
        {
            __instance.playerModelArmsMetarig.parent.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            __instance.playerModelArmsMetarig.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            __instance.localArmsTransform.localPosition = new Vector3(0, -0.008f, -0.43f);
            __instance.localArmsTransform.localRotation = Quaternion.Euler(84.78056f, 0f, 0f);
            __instance.playerBodyAnimator.transform.localPosition = controller.playerPositionOffset;
            __instance.playerBodyAnimator.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        }
    }
}