using CruiserXL.Utils;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CruiserXL.Behaviour;

// a more universal way of having custom seat animations
public class VehiclePlayerSeat : MonoBehaviour
{
    public RuntimeAnimatorController cachedPlayerAnimatorController = null!;
    public Animator thisPlayerAnimator = null!; // this client/both

    public void ReplacePlayerAnimator(PlayerControllerB playerController, bool isLocalPlayer, InteractTrigger seatTrigger)
    {
        // safeguarding
        if (playerController == null ||
            playerController.isPlayerDead ||    
            !playerController.isPlayerControlled)
        {
            if (playerController != null) 
                playerController.playerBodyAnimator.runtimeAnimatorController = isLocalPlayer ? StartOfRound.Instance.localClientAnimatorController : StartOfRound.Instance.otherClientsAnimatorController;
            cachedPlayerAnimatorController = null!;
            thisPlayerAnimator = null!;
            return;
        }

        if (isLocalPlayer)
            UncrouchPlayer(playerController);

        // reset the players local data
        PlayerUtils.ResetPlayerData(playerController);

        // save a reference of the players current animator
        cachedPlayerAnimatorController = null!;
        cachedPlayerAnimatorController = GameObject.Instantiate(playerController.playerBodyAnimator.runtimeAnimatorController);
        cachedPlayerAnimatorController.name = "metarig";
        thisPlayerAnimator = playerController.playerBodyAnimator;

        // save the parameters of the current animator
        if (isLocalPlayer)
            PlayerUtils.StoreParameters(playerController.playerBodyAnimator);

        // apply the animator from our references
        playerController.playerBodyAnimator.runtimeAnimatorController = References.truckPlayerAnimator;

        if (!isLocalPlayer)
        {
            playerController.playerBodyAnimator.ResetTrigger(PlayerUtils.stopAnimationID);
            playerController.playerBodyAnimator.ResetTrigger(seatTrigger.animationString);
            playerController.playerBodyAnimator.SetTrigger(seatTrigger.animationString);
        }
    }

    public void ReturnPlayerAnimator(PlayerControllerB playerController, bool isLocalPlayer, InteractTrigger seatTrigger)
    {
        // safeguarding
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled)
        {
            if (playerController != null) 
                playerController.playerBodyAnimator.runtimeAnimatorController = isLocalPlayer ? StartOfRound.Instance.localClientAnimatorController : StartOfRound.Instance.otherClientsAnimatorController;
            cachedPlayerAnimatorController = null!;
            thisPlayerAnimator = null!;
            return;
        }

        // reapply the original players animator, if it exists
        if (cachedPlayerAnimatorController != null)
        {
            playerController.playerBodyAnimator.runtimeAnimatorController = cachedPlayerAnimatorController;

            // restore the original parameters for the original animator
            if (isLocalPlayer)
                PlayerUtils.RestoreParameters(playerController.playerBodyAnimator);
        }
        else
        {
            // fallback
            if (isLocalPlayer)
                playerController.playerBodyAnimator.runtimeAnimatorController = StartOfRound.Instance.localClientAnimatorController;
            else
                playerController.playerBodyAnimator.runtimeAnimatorController = StartOfRound.Instance.otherClientsAnimatorController;
        }

        if (isLocalPlayer)
        {
            playerController.playerBodyAnimator.ResetTrigger(PlayerUtils.stopAnimationID); // fix the standing up bug
            UncrouchPlayer(playerController);
        }

        // reset the players local data
        PlayerUtils.ResetPlayerData(playerController);

        // clear old references
        cachedPlayerAnimatorController = null!;
        thisPlayerAnimator = null!;
    }

    public static void UncrouchPlayer(PlayerControllerB player)
    {
        player.isCrouching = false;
        player.playerBodyAnimator.SetBool("crouching", false);
    }
}
