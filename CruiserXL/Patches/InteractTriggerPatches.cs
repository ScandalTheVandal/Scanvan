using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;
using System;
using System.Collections;
using CruiserXL.Utils;
using System.Numerics;
using CruiserXL.Behaviour;

namespace CruiserXL.Patches;

[HarmonyPatch(typeof(InteractTrigger))]
public class InteractTriggerPatches
{
    private static VehiclePlayerSeat currentPlayerSeat = null!;
    public static Coroutine specialInteractCoroutine = null!;

    public static float interactTime = 0.6f;
    public static float timeSinceSpecialInteraction;

    [HarmonyPatch(nameof(InteractTrigger.Interact))]
    [HarmonyPrefix]
    static bool Interact_Prefix(InteractTrigger __instance, Transform playerTransform, bool __runOriginal)
    {
        if (!__runOriginal)
            // somebody else has redirected the function ignore the call
            return false;

        PlayerControllerB playerController = playerTransform.GetComponent<PlayerControllerB>();
        playerController.playerBodyAnimator.ResetTrigger(PlayerUtils.stopAnimationID); // fix the standing up bug

        if (!__instance.setVehicleAnimation || 
            __instance.overridePlayerParent == null)
            return true;

        if (!__instance.overridePlayerParent.TryGetComponent<CruiserXLController>(out var controller))
            return true;

        if (specialInteractCoroutine != null)
            controller.StopCoroutine(specialInteractCoroutine);

        timeSinceSpecialInteraction = Time.realtimeSinceStartup;

        if (playerController.inSpecialInteractAnimation && 
            !playerController.isClimbingLadder && 
            !__instance.allowUseWhileInAnimation)
            return false;

        if (__instance.interactCooldown)
        {
            if (__instance.currentCooldownValue >= 0f)
                return false;

            __instance.currentCooldownValue = __instance.cooldownTime;
        }

        playerController.ResetFallGravity();
        __instance.onInteract.Invoke(playerController);
        __instance.onInteractEarly.Invoke(null);
        return false;
    }

    public static IEnumerator SpecialTruckInteractAnimation(InteractTrigger trigger, PlayerControllerB playerController, CruiserXLController controller, VehiclePlayerSeat playerSeat)
    {
        PlayerUtils.playerAnimator = playerController.playerBodyAnimator;
        playerSeat.ReplacePlayerAnimator(playerController, true, trigger);
        currentPlayerSeat = playerSeat;

        trigger.UpdateUsedByPlayerServerRpc((int)playerController.playerClientId);
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

        trigger.playerScriptInSpecialAnimation.overridePhysicsParent = trigger.overridePlayerParent;
        trigger.playerScriptInSpecialAnimation.inVehicleAnimation = trigger.setVehicleAnimation;

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

    [HarmonyPatch(nameof(InteractTrigger.StopSpecialAnimation))]
    [HarmonyPrefix]
    static bool StopSpecialAnimation_Prefix(InteractTrigger __instance, bool __runOriginal)
    {
        if (!__runOriginal)
            return false;

        if (__instance.lockedPlayer == null)
            return true;

        PlayerControllerB playerController = __instance.lockedPlayer.GetComponent<PlayerControllerB>();

        if (playerController != GameNetworkManager.Instance.localPlayerController)
            return true;

        if (!__instance.setVehicleAnimation || 
            __instance.overridePlayerParent == null)
            return true;

        if (!__instance.overridePlayerParent.TryGetComponent<CruiserXLController>(out var controller))
            return true;

        if (specialInteractCoroutine != null)
        {
            __instance.StopCoroutine(specialInteractCoroutine);
            specialInteractCoroutine = null!;
        }

        if (currentPlayerSeat == null)
            return true;

        currentPlayerSeat.ReturnPlayerAnimator(playerController, true, __instance);
        return true;
    }
}
