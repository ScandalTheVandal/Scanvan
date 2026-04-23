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

    public float checkInterval = -0.1f;

    public bool IsItRaining()
    {
        if ((GlobalReferences.wesleyHurricaneRainObj != null && GlobalReferences.wesleyHurricaneRainObj.activeSelf) || 
            (GlobalReferences.wesleyForsakenRainObj != null && GlobalReferences.wesleyForsakenRainObj.activeSelf)) return true;
        return TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Rainy ||
               TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Flooded ||
               TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Stormy;
    }

    public void Start()
    {
        for (int i = 0; i < insideOcclusion.Length; i++)
        {
            insideOcclusion[i].lowPassOverride = 3500f;
        }
        for (int i = 0; i < outsideOcclusion.Length; i++)
        {
            outsideOcclusion[i].lowPassOverride = 3500f;
        }
    }

    public void LateUpdate()
    {
        if (controller == null)
            return;

        if (checkInterval > 0.5f)
        {
            checkInterval = 0f;
            return;
        }
        checkInterval += Time.deltaTime;

        bool inTruck = PlayerUtils.isPlayerOnTruck && (PlayerUtils.seatedInTruck || PlayerUtils.isPlayerInCab || PlayerUtils.isPlayerInStorage);
        bool roofRainAudioActive = IsItRaining() && inTruck;
        bool soundAudible = controller.driverSideDoor.boolValue ||
                            controller.driversSideWindowTrigger.boolValue ||
                            controller.passengerSideDoor.boolValue ||
                            controller.passengersSideWindowTrigger.boolValue ||
                            controller.windshieldBroken;
        bool storageOpen = controller.sideDoorOpen || controller.liftGateOpen;

        controller.SetVehicleAudioProperties(controller.roofRainAudio, roofRainAudioActive, 0, 1f, 3f, useVolumeInsteadOfPitch: true);
        controller.roofRainAudio.spatialBlend = Mathf.MoveTowards(controller.roofRainAudio.spatialBlend, roofRainAudioActive ? 0f : 1f, 4f * Time.deltaTime);

        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[i];
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;

            if (player == null ||
                player.isPlayerDead ||
                !player.isPlayerControlled ||
                player.isUnderwater ||
                player.sinkingValue > 0.73f)
                continue;

            if (player == localPlayer)
                continue;

            var data = PlayerControllerBPatches.GetData(player);
            if (data == null)
                continue;

            if (player.currentVoiceChatIngameSettings == null ||
                player.currentVoiceChatIngameSettings.voiceAudio == null)
                continue;

            if (!player.currentVoiceChatIngameSettings.voiceAudio.TryGetComponent<OccludeAudio>(out var audioOcclusion))
                continue;

            bool playerInCab = data.isPlayerInCab;
            bool playerInStorage = data.isPlayerInStorage;
            bool playerInVehicle = playerInCab || playerInStorage;

            bool muffled = false;
            if (inTruck)
            {
                if (playerInVehicle)
                {
                    muffled = false;
                }
                else
                {
                    if (PlayerUtils.isPlayerInCab)
                        muffled = !soundAudible;
                    else if (PlayerUtils.isPlayerInStorage)
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
            player.voiceMuffledByEnemy = muffled;
            audioOcclusion.overridingLowPass = muffled;
            if (muffled) audioOcclusion.lowPassOverride = 600f;
        }

        for (int i = 0; i < insideOcclusion.Length; i++)
        {
            if (inTruck)
                insideOcclusion[i].overridingLowPass = false;
            else
                insideOcclusion[i].overridingLowPass = !soundAudible;
        }

        for (int i = 0; i < outsideOcclusion.Length; i++)
        {
            if (inTruck)
                outsideOcclusion[i].overridingLowPass = !soundAudible;
            else
                outsideOcclusion[i].overridingLowPass = false;
        }
    }
}
