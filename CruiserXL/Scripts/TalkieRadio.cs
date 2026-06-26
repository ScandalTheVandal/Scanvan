using GameNetcodeStuff;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Interactions;
using static UnityEngine.GraphicsBuffer;

namespace ScanVan.Behaviour;

/// <summary>
///  Available from Itolib, licensed under MIT License.
///  Source: https://github.com/pacoito123/LC_itolib/blob/main/itolib/Behaviours/Interactables/InteractTalkable.cs
/// </summary>
public class TalkieRadio : InteractTrigger
{
    [Space(5.0f)]
    [Header("Talkie Radio")]
    [Tooltip("")]
    [SerializeField] private UnityEvent<PlayerControllerB> onStartTalking = new();

    [Tooltip("")]
    [SerializeField] private UnityEvent<PlayerControllerB> onStopTalking = new();

    private bool isActive;

    public void Reset()
    {
        hoverTip = "Transmit voice : [LMB]";
        interactable = true;
        oneHandedItemAllowed = false;
        twoHandedItemAllowed = false;

        holdInteraction = true;
        timeToHold = 1.0f;
        timeToHoldSpeedMultiplier = 0.0f;
        holdTip = "Transmitting voice...";

        cooldownTime = 0.5f;
    }

    public void Awake()
    {
        holdingInteractEvent.AddListener(EnableWalkieLocal);
        onStopInteract.AddListener(DisableWalkieLocal);
    }

    public void EnableWalkieLocal(float _)
    {
        if (isActive)
        {
            return;
        }
        isActive = true;

        if (IsSpawned)
        {
            EnableWalkieRpc(GameNetworkManager.Instance.localPlayerController);
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void EnableWalkieRpc(NetworkBehaviourReference playerReference)
    {
        if (playerReference.TryGet(out PlayerControllerB player))
        {
            player.holdingWalkieTalkie = true;
            player.speakingToWalkieTalkie = true;

            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.UpdatePlayerVoiceEffects();
            }

            onStartTalking.Invoke(player);
        }
    }

    public void DisableWalkieLocal(PlayerControllerB _)
    {
        isActive = false;

        if (IsSpawned)
        {
            DisableWalkieRpc(GameNetworkManager.Instance.localPlayerController);
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void DisableWalkieRpc(NetworkBehaviourReference playerReference)
    {
        if (playerReference.TryGet(out PlayerControllerB player))
        {
            onStopTalking.Invoke(player);

            player.holdingWalkieTalkie = false;
            player.speakingToWalkieTalkie = false;

            if (StartOfRound.Instance != null)
            {
                StartOfRound.Instance.UpdatePlayerVoiceEffects();
            }
        }
    }
}
