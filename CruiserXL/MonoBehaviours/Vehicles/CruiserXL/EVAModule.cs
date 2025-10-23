using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using static Unity.Properties.TypeUtility;

namespace CruiserXL.MonoBehaviours.Vehicles.CruiserXL;

//[MovedFrom("CruiserXL.MonoBehaviours.Vehicles.CruiserXL")]
public class EVAModule : NetworkBehaviour
{
    [Header("EVA System")]

    public CruiserXLController controller = null!;
    // Voice alerts
    public AudioClip[] voiceAudioClips = null!;
    public AudioSource voiceAudio = null!;

    // Misc chimes & beeps
    public AudioClip sixBeepAlert = null!;
    public AudioClip singleAlert = null!;
    public AudioClip highToneAlert = null!;

    // Extra misc stuff
    public Coroutine audioTimedCoroutine = null!;
    public TextMeshPro clusterScreen = null!;

    public string[] clusterTexts = null!;
    public bool[] audioClipsJustPlayed = null!;
    public bool[] audioClipsInQueue = null!;
    public int currentClipId;
    public int randomOverheatClipToPlay;
    public bool isSpecialAlert;
    public float alertSystemInterval;

    private bool hasWarnedEngineCritical;
    private bool hasWarnedFuelLevelLow;
    private bool hasWarnedTransmissionLevelLow;
    private bool hasWarnedCoolantLevelLow;
    private bool hasWarnedEngineOilLevelLow;
    private bool hasAlertedOnEngineStart;
    private bool hasJustPlayedSixBeepChime;
    private bool pendingDriverDoorThanked;
    private bool pendingPassengerDoorThanked;
    private bool pendingSideDoorThanked;
    private bool pendingBackDoorThanked;
    private bool pendingKeysInIgnitionThanked;

    public void LateUpdate()
    {
        if (controller == null || !controller.IsSpawned ||
            !NetworkManager.Singleton.IsHost || controller.carDestroyed) return;

        if (alertSystemInterval <= 0.25f)
        {
            alertSystemInterval += Time.deltaTime;
            return;
        }

        PlayQueuedClips();
        SetIgnitionClips();
        SetHealthAlertClips();
        SetDoorAlertClips();

        alertSystemInterval = -0.01f;
    }

    private void PlayQueuedClips()
    {
        for (int i = 0; i < audioClipsInQueue.Length; i++)
        {
            bool meetsSpecialAlertConditions = !isSpecialAlert && hasJustPlayedSixBeepChime;
            if (audioClipsInQueue[i] && !audioClipsJustPlayed[i] && meetsSpecialAlertConditions && audioTimedCoroutine == null)
            {
                audioTimedCoroutine = StartCoroutine(PlayAudioClip(i, false, false, false));
                PlayAudioClipServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, i, false, false, false);
            }
        }
    }

    private void SetIgnitionClips()
    {
        // Ignition on
        if (controller.keyIsInIgnition)
        {
            if (isSpecialAlert)
            {
                isSpecialAlert = false;
                ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);

                if (controller.lowBeamsOn || controller.highBeamsOn)
                {
                    SetThankYouClip();
                    return;
                }
            }

            if (hasJustPlayedSixBeepChime)
            {
                // Only trigger "All systems OK" once after engine start
                if (!hasAlertedOnEngineStart && controller.ignitionStarted &&
                    !WasClipPlayed(ElectronicVoiceAlert.AllSystemsOk) && controller.carHP > 27)
                {
                    hasAlertedOnEngineStart = true;
                    ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                    SetClipInQueue(ElectronicVoiceAlert.AllSystemsOk);
                    return;
                }
                else if (controller.carHP <= 27)
                {
                    hasAlertedOnEngineStart = true;
                    ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                    audioClipsJustPlayed[(int)ElectronicVoiceAlert.AllSystemsOk] = true;
                    return;
                }
            }
            else if (currentClipId != (int)ElectronicVoiceAlert.ThankYou)
            {
                // Play the six, long beeps upon vehicle startup
                hasJustPlayedSixBeepChime = true;
                ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                SetIgnitionChime();
                return;
            }
            return;
        }

        // Ignition off
        if (!isSpecialAlert)
        {
            if (currentClipId != (int)ElectronicVoiceAlert.ThankYou &&
                currentClipId != (int)ElectronicVoiceAlert.KeyInIgnition &&
                controller.currentDriver == null && !pendingKeysInIgnitionThanked &&
                (controller.lowBeamsOn || controller.highBeamsOn))
            {
                isSpecialAlert = true;
                SetSpecialAlert((int)ElectronicVoiceAlert.HeadlampsOn);
                return;
            }
        }
        else
        {
            if (currentClipId == (int)ElectronicVoiceAlert.HeadlampsOn && !controller.headlightsContainer.activeSelf)
            {
                isSpecialAlert = false;
                ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                SetThankYouClip();
                return;
            }
        }

        if (pendingKeysInIgnitionThanked && WasClipPlayed(ElectronicVoiceAlert.KeyInIgnition))
        {
            pendingKeysInIgnitionThanked = false;
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
            SetThankYouClip();
            return;
        }

        WipeVoiceModuleMemory();

        if (!controller.keyIsInIgnition)
        {
            ResetAudioClip(ElectronicVoiceAlert.ThankYou);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
            hasJustPlayedSixBeepChime = false;
        }

        randomOverheatClipToPlay = 0;
    }

    private void SetHealthAlertClips()
    {
        if (!controller.ignitionStarted)
        {
            randomOverheatClipToPlay = 0;
            ResetAudioClip(ElectronicVoiceAlert.EngineTempAboveNormal);
            ResetAudioClip(ElectronicVoiceAlert.EngineOverheating);
            return;
        }

        if (audioTimedCoroutine != null)
            return;

        if (IsHealthLowAndSetClip())
            return;

        SetOverheatingAudioClips();
        SetForgottenKeysClip();
        SetParkingBrakeAlert();
    }

    private void SetDoorAlertClips()
    {
        SetDoorAlertClip(controller.driverSideDoor.boolValue, 6, ref pendingDriverDoorThanked);
        SetDoorAlertClip(controller.passengerSideDoor.boolValue, 7, ref pendingPassengerDoorThanked);
        SetDoorAlertClip(controller.sideDoorOpen, 8, ref pendingSideDoorThanked);
        SetDoorAlertClip(controller.liftGateOpen, 9, ref pendingBackDoorThanked);
    }

    public void SetDoorAlertClip(bool doorOpen, int doorClipId, ref bool pendingDoorThanked)
    {
        if (controller.ignitionStarted)
        {
            if (doorOpen)
            {
                if (controller.averageVelocity.magnitude > 4f &&
                    !audioClipsInQueue[doorClipId] &&
                    !pendingDoorThanked)
                {
                    audioClipsInQueue[doorClipId] = true;
                    pendingDoorThanked = true;
                }
            }
            else if (!doorOpen && pendingDoorThanked)
            {
                SetThankYouClip();
                audioClipsInQueue[doorClipId] = false;
                audioClipsJustPlayed[doorClipId] = false;
                pendingDoorThanked = false;
            }
        }
        else
        {
            audioClipsInQueue[doorClipId] = false;
            pendingDoorThanked = false;
        }
    }

    private bool IsHealthLowAndSetClip()
    {
        var healthAlerts = new List<(Func<bool> hasWarned, Action setQueued, int threshold, ElectronicVoiceAlert alertType)>
        {
            (() => hasWarnedFuelLevelLow, () => hasWarnedFuelLevelLow = true, 27, ElectronicVoiceAlert.FuelLevel),
            (() => hasWarnedTransmissionLevelLow, () => hasWarnedTransmissionLevelLow = true, 23, ElectronicVoiceAlert.TransFluidLevel),
            (() => hasWarnedCoolantLevelLow, () => hasWarnedCoolantLevelLow = true, 19, ElectronicVoiceAlert.EngineCoolantLevel),
            (() => hasWarnedEngineOilLevelLow, () => hasWarnedEngineOilLevelLow = true, 15, ElectronicVoiceAlert.EngineOilLevel),
            (() => hasWarnedEngineCritical, () => hasWarnedEngineCritical = true, 12, ElectronicVoiceAlert.EngineOilPressure)
        };

        foreach (var (hasWarned, setQueued, threshold, alertType) in healthAlerts)
        {
            if (!hasWarned() && controller.carHP <= threshold)
            {
                setQueued();
                SetClipInQueue(alertType);
                return true;
            }
        }
        return false;
    }

    private void SetOverheatingAudioClips()
    {
        if (controller.carHP <= 10)
        {
            if (controller.isHoodOnFire && randomOverheatClipToPlay == 0)
            {
                randomOverheatClipToPlay = UnityEngine.Random.Range(15, 17);
                audioClipsInQueue[randomOverheatClipToPlay] = true;
                return;
            }
            randomOverheatClipToPlay = 0;
            ResetAudioClip(ElectronicVoiceAlert.EngineTempAboveNormal);
            ResetAudioClip(ElectronicVoiceAlert.EngineOverheating);
        }
        else if (!controller.isHoodOnFire && randomOverheatClipToPlay != 0)
        {
            randomOverheatClipToPlay = 0;
            ResetAudioClip(ElectronicVoiceAlert.EngineTempAboveNormal);
            ResetAudioClip(ElectronicVoiceAlert.EngineOverheating);
        }
    }

    private void SetForgottenKeysClip()
    {
        if (controller.currentDriver == null && !WasClipPlayed(ElectronicVoiceAlert.KeyInIgnition) && !pendingKeysInIgnitionThanked)
        {
            pendingKeysInIgnitionThanked = true;
            SetClipInQueue(ElectronicVoiceAlert.KeyInIgnition);
        }
        else if (controller.currentDriver != null && pendingKeysInIgnitionThanked)
        {
            pendingKeysInIgnitionThanked = false;
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
        }
    }

    private void SetParkingBrakeAlert()
    {
        if (controller.drivetrainModule.autoGear == TruckGearShift.Park && controller.drivePedalPressed && !WasClipPlayed(ElectronicVoiceAlert.ParkBrakeOn))
        {
            SetClipInQueue(ElectronicVoiceAlert.ParkBrakeOn);
        }
        else if (WasClipPlayed(ElectronicVoiceAlert.ParkBrakeOn) &&
                (controller.drivetrainModule.autoGear != TruckGearShift.Park || !controller.drivePedalPressed))
        {
            ResetAudioClip(ElectronicVoiceAlert.ParkBrakeOn);
        }
    }

    private void ResetAudioClip(ElectronicVoiceAlert alert)
    {
        int index = (int)alert;
        audioClipsInQueue[index] = false;
        audioClipsJustPlayed[index] = false;
    }

    private void SetClipInQueue(ElectronicVoiceAlert alert)
    {
        audioClipsInQueue[(int)alert] = true;
    }

    private bool WasClipPlayed(ElectronicVoiceAlert alert)
    {
        return audioClipsJustPlayed[(int)alert];
    }

    public void SetThankYouClip()
    {
        if (voiceAudio.clip != null && voiceAudio.isPlaying && voiceAudio.clip != voiceAudioClips[(int)ElectronicVoiceAlert.ThankYou] &&
            voiceAudio.clip != voiceAudioClips[(int)ElectronicVoiceAlert.HeadlampsOn] && voiceAudio.time < voiceAudio.clip.length / 2f)
        {
            audioClipsJustPlayed[currentClipId] = false;
            audioClipsInQueue[currentClipId] = true;
            // requeue the audio clip, if it was interrupted, and it hasn't played more than half the clip
        }
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
        }
        audioTimedCoroutine = StartCoroutine(PlayAudioClip(2, true, false, false));
        PlayAudioClipServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, 2, true, false, false);
    }

    public void SetSpecialAlert(int clipToPlay)
    {
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
        }
        audioTimedCoroutine = StartCoroutine(PlayAudioClip(clipToPlay, false, true, false));
        PlayAudioClipServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, clipToPlay, false, true, false);
    }

    public void SetIgnitionChime()
    {
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
        }
        audioTimedCoroutine = StartCoroutine(PlayAudioClip(-1, false, false, true));
        PlayAudioClipServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, -1, false, false, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayAudioClipServerRpc(int playerWhoSent, int clipToPlay, bool isThankYouClip, bool isSpecialWarning, bool isIgnitionChime)
    {
        PlayAudioClipClientRpc(playerWhoSent, clipToPlay, isThankYouClip, isSpecialWarning, isIgnitionChime);
    }

    [ClientRpc]
    public void PlayAudioClipClientRpc(int playerWhoSent, int clipToPlay, bool isThankYouClip, bool isSpecialWarning, bool isIgnitionChime)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
        }
        if (!isIgnitionChime)
        {
            if (isThankYouClip && !isSpecialWarning)
            {
                audioTimedCoroutine = StartCoroutine(PlayAudioClip(2, true, false, false));
                return;
            }
            audioTimedCoroutine = StartCoroutine(PlayAudioClip(clipToPlay, false, isSpecialWarning, false));
            return;
        }
        audioTimedCoroutine = StartCoroutine(PlayAudioClip(-1, false, false, true));
    }

    public IEnumerator PlayAudioClip(int clipId, bool playThankAudio, bool isSpecialWarning, bool isIgnitionChime)
    {
        currentClipId = clipId;
        voiceAudio.loop = false;

        if (isIgnitionChime)
        {
            yield return PlayIgnitionChime();
            yield break;
        }

        if (isSpecialWarning)
        {
            yield return PlaySpecialAlert(clipId);
            yield break;
        }

        if (playThankAudio)
        {
            yield return PlayThankYouAudio(clipId);
            yield break;
        }

        yield return PlayVoiceAlert(clipId);
    }

    private IEnumerator PlayIgnitionChime()
    {
        currentClipId = -1;
        yield return new WaitForSeconds(0.25f);

        clusterScreen.text = "service due!";
        voiceAudio.Stop();
        voiceAudio.clip = sixBeepAlert;
        voiceAudio.Play();

        yield return new WaitForSeconds(voiceAudio.clip.length + 0.45f);

        audioTimedCoroutine = null!;
        clusterScreen.text = null;
        currentClipId = -1;
    }

    private IEnumerator PlaySpecialAlert(int clipId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            audioClipsInQueue[clipId] = false;
            audioClipsJustPlayed[clipId] = true;
        }

        yield return SetScreenTextAfterDelay(clipId, 0.08f);

        voiceAudio.Stop();
        voiceAudio.PlayOneShot(singleAlert);
        yield return new WaitForSeconds(singleAlert.length);

        voiceAudio.clip = voiceAudioClips[clipId];
        voiceAudio.Play();
        yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);

        voiceAudio.loop = true;
        voiceAudio.Stop();
        voiceAudio.clip = highToneAlert;
        voiceAudio.Play();
    }

    private IEnumerator PlayThankYouAudio(int clipId)
    {
        clusterScreen.text = clusterTexts[clipId];

        voiceAudio.Stop();
        voiceAudio.clip = voiceAudioClips[2];
        voiceAudio.Play();

        yield return new WaitForSeconds(voiceAudio.clip.length + 0.45f);

        audioTimedCoroutine = null!;
        clusterScreen.text = null;
        currentClipId = -1;
    }

    private IEnumerator PlayVoiceAlert(int clipId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            audioClipsInQueue[clipId] = false;
            audioClipsJustPlayed[clipId] = true;
        }

        yield return SetScreenTextAfterDelay(clipId, 0.08f);

        voiceAudio.Stop();
        voiceAudio.PlayOneShot(singleAlert);
        yield return new WaitForSeconds(singleAlert.length);

        voiceAudio.clip = voiceAudioClips[clipId];
        voiceAudio.Play();

        yield return new WaitForSeconds(voiceAudio.clip.length + 0.45f);

        audioTimedCoroutine = null!;
        clusterScreen.text = null;
        currentClipId = -1;
    }

    private IEnumerator SetScreenTextAfterDelay(int clipId, float delay)
    {
        clusterScreen.text = null;
        yield return new WaitForSeconds(delay);
        clusterScreen.text = clusterTexts[clipId];
    }

    private void WipeVoiceModuleMemory()
    {
        hasWarnedEngineCritical = false;
        hasWarnedFuelLevelLow = false;
        hasWarnedTransmissionLevelLow = false;
        hasWarnedCoolantLevelLow = false;
        hasWarnedEngineOilLevelLow = false;
        hasAlertedOnEngineStart = false;

        var resetAlerts = new[] { 0, 10, 14, 11, 12, 15, 16, 3, 4, 17 };

        foreach (var index in resetAlerts)
        {
            audioClipsJustPlayed[index] = false;
            audioClipsInQueue[index] = false;
        }
    }
}
