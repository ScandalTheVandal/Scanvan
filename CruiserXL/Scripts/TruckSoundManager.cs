using ScanVan.Patches;
using ScanVan.Utils;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ScandalsTweaks.Utils;

namespace ScanVan.Behaviour;

public class TruckSoundManager : MonoBehaviour
{
    public CruiserXLController controller = null!;

    public OccludeAudio[] insideOcclusion = null!;
    public OccludeAudio[] outsideOcclusion = null!;

    public float checkInterval;

    public void Start()
    {
        for (int i = 0; i < insideOcclusion.Length; i++)
        {
            insideOcclusion[i].lowPassOverride = 3500f;
        }
        for (int j = 0; j < outsideOcclusion.Length; j++)
        {
            outsideOcclusion[j].lowPassOverride = 3500f;
        }
    }

    public void LateUpdate()
    {
        if (controller == null)
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

        var perspectiveData = PlayerControllerBPatches.playerData[perspectivePlayer];

        bool perspectiveInCab = perspectiveData?.playerRidingInVanCab ?? false;
        bool perspectiveInStorage = perspectiveData?.playerRidingInVanStorage ?? false;
        bool perspectiveInVehicle = perspectiveInCab || perspectiveInStorage;

        controller.roofRainAudioActive = GlobalUtilities.IsItRaining() && perspectiveInVehicle;
        bool soundAudible = controller.driverSideDoor.boolValue ||
                            controller.driversSideWindowTrigger.boolValue ||
                            controller.passengerSideDoor.boolValue ||
                            controller.passengersSideWindowTrigger.boolValue ||
                            controller.windshieldBroken;

        bool storageOpen = controller.sideDoorOpen || controller.liftGateOpen;

        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[i];

            if (player == null ||
                player.isPlayerDead ||
                !player.isPlayerControlled ||
                player.isUnderwater ||
                player.sinkingValue > 0.73f)
                continue;

            if (player == localPlayerController)
                continue;

            var playerData = PlayerControllerBPatches.playerData[player];
            if (playerData == null)
                continue;

            if (player.currentVoiceChatIngameSettings?.voiceAudio == null)
                continue;

            if (!player.currentVoiceChatIngameSettings.voiceAudio.TryGetComponent<OccludeAudio>(out var audioOcclusion))
                continue;

            bool playerInCab = playerData.playerRidingInVanCab;
            bool playerInStorage = playerData.playerRidingInVanStorage;
            bool playerInVehicle = playerInCab || playerInStorage;

            if (playerInVehicle || perspectiveInVehicle)
            {
                if (player.speakingToWalkieTalkie && perspectivePlayer.holdingWalkieTalkie)
                {
                    playerData.applyVoiceEffects = false;
                    player.voiceMuffledByEnemy = false;
                    audioOcclusion.overridingLowPass = true;
                    audioOcclusion.lowPassOverride = 4000f;
                    continue;
                }
            }

            if (!playerInVehicle && !perspectiveInVehicle)
            {
                if (playerData.applyVoiceEffects)
                {
                    playerData.applyVoiceEffects = false;
                    player.voiceMuffledByEnemy = false;
                    audioOcclusion.overridingLowPass = false;
                }
                continue;
            }
            playerData.applyVoiceEffects = true;

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

        for (int i = 0; i < insideOcclusion.Length; i++)
        {
            if (perspectiveInVehicle)
                insideOcclusion[i].overridingLowPass = false;
            else
                insideOcclusion[i].overridingLowPass = !soundAudible;
        }

        for (int i = 0; i < outsideOcclusion.Length; i++)
        {
            if (perspectiveInVehicle)
                outsideOcclusion[i].overridingLowPass = !soundAudible;
            else
                outsideOcclusion[i].overridingLowPass = false;
        }
    }
}
