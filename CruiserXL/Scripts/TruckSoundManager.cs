using ScanVan.Patches;
using ScanVan.Utils;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ScandalLib.Utils;
using System.Linq;
using ScandalLib.Interfaces;

namespace ScanVan.Behaviour;

public class TruckSoundManager : MonoBehaviour
{
    public CruiserXLController controller = null!;

    public AudioSource[] dopplerAudios = null!;

    public OccludeAudio[] insideOcclusion = null!;
    public OccludeAudio[] outsideOcclusion = null!;

    private bool isAudioDopplerActive = true;
    private bool isRainAudioActive;

    private bool isInsideOccluded;
    private bool isOutsideOccluded;

    private float checkInterval;

    public void Start()
    {
        for (int i = 0; i < insideOcclusion.Length; i++)
        {
            insideOcclusion[i].lowPassOverride = 2500f;
        }
        for (int j = 0; j < outsideOcclusion.Length; j++)
        {
            outsideOcclusion[j].lowPassOverride = 3500f;
        }
    }

    public void Update()
    {
        if (controller == null || !controller.IsSpawned)
            return;

        if (checkInterval < 0.5f)
        {
            checkInterval += Time.deltaTime;
            return;
        }
        checkInterval = 0f;

        PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
        if (localPlayerController == null)
            return;

        PlayerControllerB perspectivePlayer = localPlayerController;
        if (localPlayerController.isPlayerDead && localPlayerController.spectatedPlayerScript != null)
        {
            perspectivePlayer = localPlayerController.spectatedPlayerScript;
        }

        bool vehicleDestroyed = controller.carDestroyed;

        bool inDoorLighting = perspectivePlayer.currentAudioTrigger != null && perspectivePlayer.currentAudioTrigger.insideLighting;
        bool perspectiveOnVehicle = controller.IsPlayerAttached(perspectivePlayer);
        bool perspectiveInCab = controller.IsPlayerInCabinOrSeated(perspectivePlayer);
        bool perspectiveInStorage = controller.IsPlayerInStorage(perspectivePlayer);
        bool perspectiveInVehicle = perspectiveInCab || perspectiveInStorage;
        bool perspectiveInOrOnVehicle = perspectiveOnVehicle || perspectiveInVehicle;
        bool perspectiveInVanNotIndoors = perspectiveInVehicle && !inDoorLighting;

        UpdateRainAudio(perspectiveInVanNotIndoors);
        UpdateDopplerAudioEffect(perspectiveInOrOnVehicle && !vehicleDestroyed);

        bool soundAudible = vehicleDestroyed || 
                            controller.driverSideDoor.boolValue ||
                            controller.frontLeftWindow.isWindowOpen ||
                            controller.passengerSideDoor.boolValue ||
                            controller.frontRightWindow.isWindowOpen ||
                            controller.windshieldBroken;

        bool storageOpen = vehicleDestroyed || 
                           controller.sideDoorOpen || 
                           controller.liftGateOpen;

        var allPlayerScriptsSOR = StartOfRound.Instance.allPlayerScripts;
        for (int i = 0; i < allPlayerScriptsSOR.Length; i++)
        {
            PlayerControllerB player = allPlayerScriptsSOR[i];

            if (player == null ||
                player.isPlayerDead ||
                !player.isPlayerControlled ||
                player.isUnderwater ||
                player.sinkingValue > 0.73f)
                continue;

            if (player == localPlayerController)
                continue;

            if (player is not IPlayerManager playerInterface)
                continue;

            if (player.currentVoiceChatIngameSettings?.voiceAudio == null)
                continue;

            if (!player.currentVoiceChatIngameSettings.voiceAudio.TryGetComponent<OccludeAudio>(out var audioOcclusion))
                continue;

            bool playerInCab = controller.IsPlayerInCabinOrSeated(player);
            bool playerInStorage = controller.IsPlayerInStorage(player);
            bool playerInVehicle = playerInCab || playerInStorage;

            if (playerInVehicle || perspectiveInVehicle)
            {
                if (player.speakingToWalkieTalkie && perspectivePlayer.holdingWalkieTalkie)
                {
                    playerInterface.ApplyVoiceEffects = false;
                    player.voiceMuffledByEnemy = false;
                    audioOcclusion.overridingLowPass = true;
                    audioOcclusion.lowPassOverride = 4000f;
                    continue;
                }
            }

            if (!playerInVehicle && !perspectiveInVehicle)
            {
                if (playerInterface.ApplyVoiceEffects)
                {
                    playerInterface.ApplyVoiceEffects = false;
                    player.voiceMuffledByEnemy = false;
                    audioOcclusion.overridingLowPass = false;
                }
                continue;
            }
            playerInterface.ApplyVoiceEffects = true;

            bool muffled = false;
            if (perspectiveInVehicle)
            {
                if (playerInVehicle)
                {
                    muffled = false;
                }
                else
                {
                    if (perspectiveInCab)
                        muffled = !soundAudible;
                    else if (perspectiveInStorage)
                        muffled = !storageOpen;
                }
            }
            else
            {
                if (playerInVehicle)
                {
                    if (playerInCab)
                        muffled = !soundAudible;
                    else if (playerInStorage)
                        muffled = !storageOpen;
                }
                else
                {
                    muffled = false;
                }
            }

            if (perspectiveInVehicle && playerInVehicle)
            {
                if ((perspectiveInCab && playerInCab) ||
                    (perspectiveInStorage && playerInStorage))
                {
                    muffled = false;
                }
            }

            player.voiceMuffledByEnemy = muffled;
            audioOcclusion.overridingLowPass = muffled;
            if (muffled) audioOcclusion.lowPassOverride = 600f;
        }
        UpdateInsideOcclusion(!perspectiveInVehicle && !soundAudible);
        UpdateOutsideOcclusion(perspectiveInVehicle && !soundAudible);
    }

    public void UpdateInsideOcclusion(bool setOccluded)
    {
        if (isInsideOccluded == setOccluded)
            return;

        isInsideOccluded = setOccluded;
        for (int i = 0; i < insideOcclusion.Length; i++)
        {
            insideOcclusion[i].overridingLowPass = setOccluded;
        }
    }

    public void UpdateOutsideOcclusion(bool setOccluded)
    {
        if (isOutsideOccluded == setOccluded)
            return;

        isOutsideOccluded = setOccluded;
        for (int i = 0; i < outsideOcclusion.Length; i++)
        {
            outsideOcclusion[i].overridingLowPass = setOccluded;
        }
    }

    public void UpdateDopplerAudioEffect(bool setDoppler)
    {
        if (isAudioDopplerActive == setDoppler)
            return;

        isAudioDopplerActive = setDoppler;
        float level = setDoppler ? 0f : 1f;
        for (int i = 0; i < dopplerAudios.Length; i++)
        {
            dopplerAudios[i].dopplerLevel = level;
        }
    }

    public void UpdateRainAudio(bool perspectiveInVanNotIndoors)
    {
        if (controller.carDestroyed)
        {
            if (isRainAudioActive)
                isRainAudioActive = false;
            return;
        }
        bool setRainAudio = Utilities.IsItRaining() && perspectiveInVanNotIndoors;
        if (isRainAudioActive == setRainAudio)
            return;
        isRainAudioActive = setRainAudio;
        controller.roofRainAudioActive = setRainAudio;
    }
}
