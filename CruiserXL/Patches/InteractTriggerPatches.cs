using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine.InputSystem.XR;
using System.ComponentModel;
using CruiserXL.Utils;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(InteractTrigger))]
public class InteractTriggerPatches
{
    public static Coroutine specialInteractCoroutine = null!;
    public static float interactTime = 0.6f;
    public static float timeSinceSpecialInteraction;

    [HarmonyPatch("Interact")]
    [HarmonyPrefix]
    static bool Interact_Prefix(InteractTrigger __instance, Transform playerTransform, bool __runOriginal)
    {
        if (!__runOriginal)
            //somebody else has redirected the function ignore the call
            return false;

        PlayerControllerB playerController = playerTransform.GetComponent<PlayerControllerB>();
        playerController.playerBodyAnimator.ResetTrigger("SA_stopAnimation"); // Fix the standing up bug

        if (!__instance.setVehicleAnimation)
            return true;

        if (__instance.overridePlayerParent == null)
            return true;

        if (!__instance.overridePlayerParent.TryGetComponent<CruiserXLController>(out var controller))
            return true;

        if (specialInteractCoroutine != null)
            controller.StopCoroutine(specialInteractCoroutine);

        timeSinceSpecialInteraction = Time.realtimeSinceStartup;

        if (playerController.inSpecialInteractAnimation && !playerController.isClimbingLadder && !__instance.allowUseWhileInAnimation)
            return false;

        if (__instance.interactCooldown)
        {
            if (__instance.currentCooldownValue >= 0f)
                return false;

            __instance.currentCooldownValue = __instance.cooldownTime;
        }

        playerController.ResetFallGravity();
        playerController.Crouch(false);
        __instance.onInteract.Invoke(playerController);
        __instance.onInteractEarly.Invoke(null);

        //specialInteractCoroutine = __instance.StartCoroutine(SpecialTruckInteractAnimation(__instance, playerController, controller, __instance != controller.driverSeatTrigger));
        return false;
    }

    public static IEnumerator SpecialTruckInteractAnimation(InteractTrigger trigger, PlayerControllerB playerController, CruiserXLController controller, bool isPassenger)
    {
        Plugin.Logger.LogMessage("truck: RUNNING REPLACEMENT COROUTINE!!!");

        if (!isPassenger)
        {
            // save a reference of the players current animator
            PlayerUtils.localDriverCachedAnimatorController = null!;
            PlayerUtils.localDriverCachedAnimatorController = GameObject.Instantiate(playerController.playerBodyAnimator.runtimeAnimatorController);
            PlayerUtils.localDriverCachedAnimatorController.name = "metarig";
            PlayerUtils.playerAnimator = playerController.playerBodyAnimator;

            // save the parameters of the current animator
            PlayerUtils.StoreParameters();

            // apply the animator from our references
            if (References.truckPlayerAnimator != null)
                playerController.playerBodyAnimator.runtimeAnimatorController = References.truckPlayerAnimator;

            PlayerUtils.playerAnimator.Update(0.0f);
        }
        trigger.UpdateUsedByPlayerServerRpc((int)playerController.playerClientId);
        //trigger.onInteractEarly.Invoke(null);
        trigger.isPlayingSpecialAnimation = true;
        trigger.lockedPlayer = playerController.thisPlayerBody;
        trigger.playerScriptInSpecialAnimation = playerController;
        if (trigger.clampLooking)
        {
            trigger.playerScriptInSpecialAnimation.minVerticalClamp = trigger.minVerticalClamp;
            trigger.playerScriptInSpecialAnimation.maxVerticalClamp = trigger.maxVerticalClamp;
            trigger.playerScriptInSpecialAnimation.horizontalClamp = trigger.horizontalClamp;
            trigger.playerScriptInSpecialAnimation.clampLooking = true;
        }
        if (trigger.overridePlayerParent)
        {
            trigger.playerScriptInSpecialAnimation.overridePhysicsParent = trigger.overridePlayerParent;
        }
        if (trigger.setVehicleAnimation)
        {
            trigger.playerScriptInSpecialAnimation.inVehicleAnimation = true;
        }
        if (trigger.hidePlayerItem && trigger.playerScriptInSpecialAnimation.currentlyHeldObjectServer != null)
        {
            trigger.playerScriptInSpecialAnimation.currentlyHeldObjectServer.EnableItemMeshes(false);
        }
        playerController.UpdateSpecialAnimationValue(true, (short)trigger.playerPositionNode.eulerAngles.y, 0f, false);
        playerController.inSpecialInteractAnimation = true;
        playerController.currentTriggerInAnimationWith = trigger;
        playerController.playerBodyAnimator.ResetTrigger(trigger.animationString);
        playerController.playerBodyAnimator.SetTrigger(trigger.animationString);
        HUDManager.Instance.ClearControlTips();
        if (!trigger.stopAnimationManually)
        {
            yield return new WaitForSeconds(trigger.animationWaitTime);
            trigger.StopSpecialAnimation();
        }
        yield return new WaitForSeconds(interactTime);
        specialInteractCoroutine = null!;
        yield break;
    }

    [HarmonyPatch("StopSpecialAnimation")]
    [HarmonyPrefix]
    static bool StopSpecialAnimation_bool_prefix(InteractTrigger __instance)
    {
        if (__instance.lockedPlayer == null)
            return true;

        PlayerControllerB playerController = __instance.lockedPlayer.GetComponent<PlayerControllerB>();

        if (playerController != GameNetworkManager.Instance.localPlayerController)
            return true;

        playerController.playerBodyAnimator.ResetTrigger("SA_stopAnimation");

        if (!__instance.setVehicleAnimation)
            return true;

        if (__instance.overridePlayerParent == null)
            return true;

        if (!__instance.overridePlayerParent.TryGetComponent<CruiserXLController>(out var controller))
            return true;

        if (__instance != controller.driverSeatTrigger)
            return true;

        if (specialInteractCoroutine != null)
        {
            __instance.StopCoroutine(specialInteractCoroutine);
            specialInteractCoroutine = null!;
        }

        // reapply the original players animator, if it exists
        if (PlayerUtils.localDriverCachedAnimatorController != null)
        {
            playerController.playerBodyAnimator.runtimeAnimatorController = PlayerUtils.localDriverCachedAnimatorController;

            // restore the original parameters for the original animator
            PlayerUtils.RestoreParameters();
        }
        else
        {
            playerController.playerBodyAnimator.runtimeAnimatorController = StartOfRound.Instance.localClientAnimatorController;
        }
        PlayerUtils.playerAnimator.Update(0.0f);

        // clear old references
        PlayerUtils.localDriverCachedAnimatorController = null!;
        PlayerUtils.playerAnimator = null!;

        return true;
    }
}
