using CruiserXL.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection.Emit;
using BepInEx.Bootstrap;
using UnityEngine.UIElements;
using CruiserXL.Managers;
using Cysharp.Threading.Tasks;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerBPatches
{
    private static bool usingSeatCam = false;

    // Here we want to backup the players original animator, although I am an idiot,
    // as StartOfRound has both the local client animator and the other-client animator
    // as references, so some refactor is in order
    [HarmonyPostfix]
    [HarmonyPatch("ConnectClientToPlayerObject")]
    static void Start_Postfix(PlayerControllerB __instance)
    {
        if (References.originalPlayerAnimator != null) return;

        References.originalPlayerAnimator = GameObject.Instantiate(__instance.playerBodyAnimator.runtimeAnimatorController);
        References.originalPlayerAnimator.name = "metarig";
    }

    // This is required to prevent rendering first person arms in the mirrors,
    // however I have noticed that half the time this does not work, so idk,
    // needs investigation
    [HarmonyPatch("SpawnPlayerAnimation")]
    [HarmonyPostfix]
    static void SpawnPlayerAnimation_Postfix(ref PlayerControllerB __instance)
    {
        if (__instance != GameNetworkManager.Instance.localPlayerController) return;

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
                $"Retry attempting to preload stations.. " + $"You may need to reboot your game!", 
                isWarning: true);
            RadioManager.GetRadioStations().Forget();
        }
    }

    // The Scanvans custom player animator splits vehicle animations into 3 layers, 
    // effectively adding 2 extra layers to the animator. If the hash list size 
    // doesn’t match the animators layer count, Unity throws index errors. 
    // This patch ensures the hash lists are enlargened to the correct size 
    // before syncing animations.
    [HarmonyPatch("UpdatePlayerAnimationsToOtherClients")]
    [HarmonyPrefix]
    public static bool UpdatePlayerAnimationsToOtherClients_Prefix(PlayerControllerB __instance, Vector2 moveInputVector)
    {
        if (__instance == null ||
            !__instance.isPlayerControlled ||
            __instance.isPlayerDead)
            return false;

        if (__instance.playerBodyAnimator == null)
            return false;

        if (__instance.currentAnimationStateHash.Count != __instance.playerBodyAnimator.layerCount ||
            __instance.previousAnimationStateHash.Count != __instance.playerBodyAnimator.layerCount)
        {
            __instance.updatePlayerAnimationsInterval = 0f;
            __instance.currentAnimationStateHash = new List<int>(new int[__instance.playerBodyAnimator.layerCount]);
            __instance.previousAnimationStateHash = new List<int>(new int[__instance.playerBodyAnimator.layerCount]);
            return false;
        }
        return true;
    }

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/PlayerController.cs
    /// </summary>
    [HarmonyPatch("Update")]
    [HarmonyPostfix]
    public static void Update_Postfix(PlayerControllerB __instance)
    {
        if (__instance != GameNetworkManager.Instance.localPlayerController) return; 

        Vector3 cameraOffset = Vector3.zero;
        bool validTruck = __instance.inVehicleAnimation && __instance.currentTriggerInAnimationWith && __instance.currentTriggerInAnimationWith.overridePlayerParent;
        if (validTruck && __instance.currentTriggerInAnimationWith.overridePlayerParent.TryGetComponent<CruiserXLController>(out var controller))
        {
            usingSeatCam = true;

            // The default offset below is pretty much the "sweet-spot" where the camera lines
            // up with the employees visor model.
            cameraOffset = new Vector3(0f, 0.118f, -0.05f) * UserConfig.SeatBoostScale.Value;
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
            
            // Extremely hacky method to prevent leaning through a closed window, while
            // still allowing leaning through the cabin window, since you cannot directly
            // clamp both left and right directions seperately from each-other.
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
        else if (!__instance.inVehicleAnimation && usingSeatCam == true)
        {
            usingSeatCam = false;
            __instance.gameplayCamera.transform.localPosition = Vector3.zero;
            __instance.horizontalClamp = 70f;
        }
    }

    // This fixes a really annoying visual bug with the players model, as 
    // various parts such as the first person arms can become disaligned
    // and cause obvious visual problems such as the ignition key not 
    // aligning properly during the ignition animation, or even causing
    // the players body to shift backwards, resulting in their hands
    // not visually holding anything, which looks bad.
    [HarmonyPatch("LateUpdate")]
    [HarmonyPostfix]
    private static void LateUpdate_Postfix(PlayerControllerB __instance)
    {
        if (!PlayerUtils.SanityCheck(__instance)) 
            return;

        bool validTruck = __instance.inVehicleAnimation &&
            __instance.currentTriggerInAnimationWith &&
            __instance.currentTriggerInAnimationWith.overridePlayerParent;

        if (validTruck &&
            __instance.currentTriggerInAnimationWith.overridePlayerParent.TryGetComponent<CruiserXLController>(out var controller))
        {
            // fix players first-person arms orientation after interacting with certain objects (i.e. terminal, start round lever) causing visual issues such as the ignition-key animation being off
            __instance.playerModelArmsMetarig.parent.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            __instance.playerModelArmsMetarig.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            __instance.localArmsTransform.localPosition = new Vector3(0, -0.008f, -0.43f);
            __instance.localArmsTransform.localRotation = Quaternion.Euler(84.78056f, 0f, 0f);
            __instance.playerBodyAnimator.transform.localPosition = controller.playerPositionOffset;
            __instance.playerBodyAnimator.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        }
    }

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/PlayerController.cs
    /// </summary>
    [HarmonyPatch("PlaceGrabbableObject")]
    [HarmonyPostfix]
    static void PlaceGrabbableObject_Postfix(GrabbableObject placeObject)
    {
        ScanNodeProperties scanNode = placeObject.GetComponentInChildren<ScanNodeProperties>();

        //Add a rigidbody to the scanNode so it'll be scannable when attached to the cruiser
        if (scanNode && !scanNode.GetComponent<Rigidbody>())
        {
            var rb = scanNode.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }
    }

    /// <summary>
    ///  Available from CruiserImproved, licensed under MIT License.
    ///  Source: https://github.com/digger1213/CruiserImproved/blob/main/source/Patches/PlayerController.cs
    /// </summary>
    [HarmonyPatch("SetHoverTipAndCurrentInteractTrigger")]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> SetHoverTipAndCurrentInteractTrigger_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //Fix untagged interactables from carrying the last valid interactable the player looked at (caused the sunvisors interact prompt to carry over the whole WindshieldInteractBlocker)
        var codes = new List<CodeInstruction>(instructions);

        if (Chainloader.PluginInfos.ContainsKey("DiggC.CruiserImproved"))
            return codes;

        if (Chainloader.PluginInfos.ContainsKey("CruiserImproved"))
            return codes;

        var get_layer = PatchUtils.Method(typeof(GameObject), "get_layer");

        int insertIndex = PatchUtils.LocateCodeSegment(0, codes, [
            new(OpCodes.Callvirt, get_layer),
        new(OpCodes.Ldc_I4_S, 0x1E),
        new(OpCodes.Beq),
        ]);

        if (insertIndex == -1)
        {
            Plugin.Logger.LogWarning("Could not transpile SetHoverTipAndCurrentInteractTrigger!");
            return codes;
        }

        var jumpDestination = codes[insertIndex + 2].operand;

        insertIndex += 3; //after the searched sequence

        var insertMethod = PatchUtils.Method(typeof(PlayerControllerBPatches), nameof(ValidRayHit));

        /*
         *  IL Code (adding new condition to the end of the existing if statement beginning with Physics.Raycast):
         *  
         *  && PlayerControllerPatches.ValidRayHit(this)
         */

        codes.InsertRange(insertIndex, [
            new(OpCodes.Ldarg_0),
        new(OpCodes.Call, insertMethod),
        new(OpCodes.Brfalse, jumpDestination)
            ]);

        return codes;
    }

    static bool ValidRayHit(PlayerControllerB player)
    {
        //Return true if the looked at object is a valid interactable
        string tag = player.hit.collider.tag;
        return tag == "PhysicsProp" || tag == "InteractTrigger";
    }
}