using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine.InputSystem;

namespace CruiserXL.Utils;
public static class PlayerUtils
{
    public static Animator playerAnimator = null!;
    public static RuntimeAnimatorController localDriverCachedAnimatorController = null!;
    public static RuntimeAnimatorController driverCachedAnimatorController = null!;

    private static float[] storedParameters = new float[0];
    private static bool[] storedBools = new bool[0];
    private static int[] storedInts = new int[0];
    private static AnimationInfo[] storedAnimations = new AnimationInfo[0];

    private struct AnimationInfo
    {
        public string stateName;
        public float normalizedTime;
    }

    public static void ResetHUDToolTips(PlayerControllerB player)
    {
        if (!SanityCheck(player)) return;
        if (player.currentlyHeldObjectServer != null)
        {
            player.currentlyHeldObjectServer.SetControlTipsForItem();
            return;
        }
        HUDManager.Instance.ClearControlTips();
    }

    public static bool SanityCheck(PlayerControllerB player)
    {
        if (player == null) return false;
        if (player.isPlayerDead) return false;
        if (!player.isPlayerControlled) return false;
        return true;
    }

    public static void ReplaceClientPlayerAnimator(int playerId)
    {
        // find the player
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];

        // safeguarding
        if (!SanityCheck(playerController)) return;

        // save a reference of the players current animator
        driverCachedAnimatorController = null!;
        driverCachedAnimatorController = GameObject.Instantiate(playerController.playerBodyAnimator.runtimeAnimatorController);
        driverCachedAnimatorController.name = "metarigOtherPlayers";
        playerAnimator = playerController.playerBodyAnimator;

        if (References.truckOtherPlayerAnimator != null)
            playerController.playerBodyAnimator.runtimeAnimatorController = References.truckOtherPlayerAnimator;
    }

    public static void StoreParameters()
    {
        var parameters = playerAnimator.parameters;
        storedParameters = new float[parameters.Length];
        storedBools = new bool[parameters.Length];
        storedInts = new int[parameters.Length];
        storedAnimations = new AnimationInfo[playerAnimator.layerCount];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    storedParameters[i] = playerAnimator.GetFloat(param.name);
                    break;
                case AnimatorControllerParameterType.Bool:
                    storedBools[i] = playerAnimator.GetBool(param.name);
                    break;
                case AnimatorControllerParameterType.Int:
                    storedInts[i] = playerAnimator.GetInteger(param.name);
                    break;
            }
        }

        // Store current animations for each layer
        for (int layer = 0; layer < playerAnimator.layerCount; layer++)
        {
            var stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(layer);
            var animInfo = new AnimationInfo
            {
                stateName = stateInfo.fullPathHash.ToString(),
                normalizedTime = stateInfo.normalizedTime
            };
            storedAnimations[layer] = animInfo;
        }
    }

    public static void ReturnClientPlayerAnimator(int playerId)
    {
        // find the player
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];

        // safeguarding
        if (!SanityCheck(playerController))
        {
            // clear old references
            driverCachedAnimatorController = null!;
            playerAnimator = null!;
            return;
        }

        // reapply the original players animator, if it exists
        playerController.playerBodyAnimator.runtimeAnimatorController =
            driverCachedAnimatorController ?? StartOfRound.Instance.otherClientsAnimatorController;

        // clear old references
        driverCachedAnimatorController = null!;
        playerAnimator = null!;
    }

    public static void RestoreParameters()
    {
        var parameters = playerAnimator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    playerAnimator.SetFloat(param.name, storedParameters[i]);
                    break;
                case AnimatorControllerParameterType.Bool:
                    playerAnimator.SetBool(param.name, storedBools[i]);
                    break;
                case AnimatorControllerParameterType.Int:
                    playerAnimator.SetInteger(param.name, storedInts[i]);
                    break;
            }
        }

        // Restore animations for each layer
        for (int layer = 0; layer < playerAnimator.layerCount; layer++)
        {
            var animInfo = storedAnimations[layer];
            playerAnimator.Play(int.Parse(animInfo.stateName), layer, animInfo.normalizedTime);
        }
    }

}