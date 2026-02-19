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

namespace CruiserXL.Behaviour;

public class EVAModule : NetworkBehaviour
{
    [Header("EVA System")]

    public CruiserXLController controller = null!;
    public AudioClip[] voiceAudioClips = null!;
    public AudioSource voiceAudio = null!;

    public AudioClip sixBeepAlert = null!;
    public AudioClip singleAlert = null!;
    public AudioClip highToneAlert = null!;
    public AudioClip highToneAlertAlt = null!;
    public AudioClip doorAlertAlt = null!;

    public Coroutine audioTimedCoroutine = null!;
    public TextMeshPro clusterScreen = null!;

    public readonly Queue<(int clipId, bool thank, bool special, bool ignition, int specialType)> clientClipQueue = new();
    public bool clientIsPlaying;

    public string[] clusterTexts = null!;
    public bool[] audioClipsJustPlayed = null!;
    public bool[] audioClipsInQueue = null!;
    public bool isPlayingBeeps;
    public bool isPlayingOnEngineStart;
    public int currentClipId;
    public int randomOverheatClipToPlay;
    public bool isSpecialAlert;
    public bool isKeysForgotten;
    public float alertSystemInterval;

    public bool hasWarnedChargeSystemLow;
    public bool hasWarnedEngineCritical;
    public bool hasWarnedFuelLevelLow;
    public bool hasWarnedTransmissionLevelLow;
    public bool hasWarnedCoolantLevelLow;
    public bool hasWarnedEngineOilLevelLow;
    public bool hasWarnedEngineTemperature;
    public bool hasAlertedOnEngineStart;
    public bool hasJustPlayedSixBeepChime;

    public bool pendingDriverDoorThanked;
    public bool pendingPassengerDoorThanked;
    public bool pendingSideDoorThanked;
    public bool pendingBackDoorThanked;

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
            bool meetsSpecialAlertConditions = !isSpecialAlert && !isKeysForgotten && hasJustPlayedSixBeepChime;
            if (audioClipsInQueue[i] && !audioClipsJustPlayed[i] && meetsSpecialAlertConditions && audioTimedCoroutine == null)
            {
                audioTimedCoroutine = StartCoroutine(PlayAudioClip(i, false, false, false, 0));
                PlayAudioClipRpc(i, false, false, false, 0);
            }
        }
    }

    private void SetIgnitionClips()
    {
        if (isSpecialAlert && isKeysForgotten &&
            !controller.headlightsContainer.activeSelf &&
            !controller.keyIsInIgnition)
        {
            isSpecialAlert = false;
            isKeysForgotten = false;

            ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);

            SetThankYouClip();
            return;
        }
        if (currentClipId != (int)ElectronicVoiceAlert.ThankYou)
        {
            if (!isSpecialAlert && !isKeysForgotten &&
            controller.currentDriver == null &&
            controller.headlightsContainer.activeSelf &&
            !controller.ignitionStarted && controller.keyIsInIgnition)
            {
                isSpecialAlert = true;
                isKeysForgotten = true;
                SetSpecialAlert(-1, -1);
                return;
            }

            if (!isSpecialAlert && !controller.ignitionStarted &&
                controller.currentDriver == null &&
                controller.headlightsContainer.activeSelf)
            {
                isSpecialAlert = true;
                SetSpecialAlert((int)ElectronicVoiceAlert.HeadlampsOn, 1);
                return;
            }
            if (!isKeysForgotten && controller.keyIsInIgnition && 
                !controller.ignitionStarted && controller.currentDriver == null)
            {
                isKeysForgotten = true;
                SetSpecialAlert((int)ElectronicVoiceAlert.KeyInIgnition, 2);
                return;
            }
        }
        if (isSpecialAlert && (!controller.headlightsContainer.activeSelf || controller.ignitionStarted) && 
            currentClipId != (int)ElectronicVoiceAlert.ThankYou)
        {
            isSpecialAlert = false;
            isKeysForgotten = false;
            ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
            SetThankYouClip();
            return;
        }
        if (isKeysForgotten && !controller.keyIsInIgnition && currentClipId != (int)ElectronicVoiceAlert.ThankYou)
        {
            isKeysForgotten = false;
            isSpecialAlert = false;
            ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
            SetThankYouClip();
            return;
        }
        if ((isKeysForgotten || isSpecialAlert) && 
            !controller.driverSideDoor.boolValue && controller.currentDriver != null)
        {
            isKeysForgotten = false;
            isSpecialAlert = false;
            if (isPlayingBeeps)
            {
                hasJustPlayedSixBeepChime = false;
                hasAlertedOnEngineStart = false;
            }
            if (isPlayingOnEngineStart)
            {
                hasAlertedOnEngineStart = false;
                ResetAudioClip(ElectronicVoiceAlert.AllSystemsOk);
            }
            StopAudioClipIfPlaying();
            return;
        }
        if (controller.keyIsInIgnition)
        {
            if (hasJustPlayedSixBeepChime)
            {
                // only trigger "all systems OK" if the car is above 27 hp
                if (!hasAlertedOnEngineStart &&
                    !WasClipPlayed(ElectronicVoiceAlert.AllSystemsOk) && controller.carHP > 27)
                {
                    hasAlertedOnEngineStart = true;
                    ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                    ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
                    SetClipInQueue(ElectronicVoiceAlert.AllSystemsOk);
                    return;
                }
                else if (controller.carHP <= 27)
                {
                    hasAlertedOnEngineStart = true;
                    ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                    ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
                    audioClipsJustPlayed[(int)ElectronicVoiceAlert.AllSystemsOk] = true;
                    return;
                }
            }
            else if (!hasJustPlayedSixBeepChime && currentClipId != (int)ElectronicVoiceAlert.ThankYou &&
                !isSpecialAlert && !isKeysForgotten)
            {
                // play the six, long beeps upon vehicle startup
                hasJustPlayedSixBeepChime = true;
                hasAlertedOnEngineStart = false;
                ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
                ResetAudioClip(ElectronicVoiceAlert.AllSystemsOk);
                SetIgnitionChime();
                return;
            }
            return;
        }
        WipeVoiceModuleMemory();
        ResetAudioClip(ElectronicVoiceAlert.ThankYou);
        hasJustPlayedSixBeepChime = false;
        hasAlertedOnEngineStart = false;
        randomOverheatClipToPlay = 0;
    }

    private void StopAudioClipIfPlaying()
    {
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
            audioTimedCoroutine = null!;
        }
        voiceAudio.clip = null;
        currentClipId = -1;
        clusterScreen.text = "";
        StopAudioClipRpc();
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void StopAudioClipRpc()
    {
        if (GameNetworkManager.Instance.localPlayerController == null)
            return;

        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
            audioTimedCoroutine = null!;
        }
        voiceAudio.clip = null;
        currentClipId = -1;
        clusterScreen.text = "";
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

        SetLowHealthAlertClips();
        SetOverheatingAudioClips();
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
            if (doorOpen && controller.averageVelocity.magnitude > 4f)
            {
                if (!audioClipsInQueue[doorClipId] &&
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

    private void SetLowHealthAlertClips()
    {
        var healthAlerts = new List<(Func<bool> hasWarned, Action setQueued, Action resetWarned, int threshold, ElectronicVoiceAlert alertType)>
        {
            (() => hasWarnedFuelLevelLow, () => hasWarnedFuelLevelLow = true, () => hasWarnedFuelLevelLow = false, 27, ElectronicVoiceAlert.FuelLevel),
            (() => hasWarnedTransmissionLevelLow, () => hasWarnedTransmissionLevelLow = true, () => hasWarnedTransmissionLevelLow = false, 23, ElectronicVoiceAlert.TransFluidLevel),
            (() => hasWarnedCoolantLevelLow, () => hasWarnedCoolantLevelLow = true, () => hasWarnedCoolantLevelLow = false, 19, ElectronicVoiceAlert.EngineCoolantLevel),
            (() => hasWarnedEngineOilLevelLow, () => hasWarnedEngineOilLevelLow = true, () => hasWarnedEngineOilLevelLow = false, 15, ElectronicVoiceAlert.EngineOilLevel),
            (() => hasWarnedEngineCritical, () => hasWarnedEngineCritical = true, () => hasWarnedEngineCritical = false, 12, ElectronicVoiceAlert.EngineOilPressure)
        };

        foreach (var (hasWarned, setQueued, resetWarned, threshold, alertType) in healthAlerts)
        {
            if (controller.carHP <= threshold)
            {
                if (!hasWarned())
                {
                    setQueued();
                    SetClipInQueue(alertType);
                }
            }
            else
            {
                resetWarned();
                ResetAudioClip(alertType);
            }
        }
    }

    private void SetOverheatingAudioClips()
    {
        if (controller.isHoodOnFire && controller.carHP <= 15)
        {
            if (randomOverheatClipToPlay == 0 && !hasWarnedEngineTemperature)
            {
                hasWarnedEngineTemperature = true;
                int clipIndex = UnityEngine.Random.Range(15, 17);
                randomOverheatClipToPlay = clipIndex;
                audioClipsInQueue[randomOverheatClipToPlay] = true;
            }
            return;
        }
        hasWarnedEngineTemperature = false;
        randomOverheatClipToPlay = 0;
        ResetAudioClip(ElectronicVoiceAlert.EngineTempAboveNormal);
        ResetAudioClip(ElectronicVoiceAlert.EngineOverheating);
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
            voiceAudio.time < voiceAudio.clip.length / 2f && currentClipId != -1)
        {
            audioClipsJustPlayed[currentClipId] = false;
            audioClipsInQueue[currentClipId] = true;
            // requeue the audio clip, if it was interrupted, and it hasn't played more than half the clip
        }
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
        }
        audioTimedCoroutine = StartCoroutine(PlayAudioClip(2, true, false, false, 0));
        PlayAudioClipRpc(2, true, false, false, -1);
    }

    public void SetSpecialAlert(int clipToPlay, int clipType)
    {
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
        }
        audioTimedCoroutine = StartCoroutine(PlayAudioClip(clipToPlay, false, true, false, clipType));
        PlayAudioClipRpc(clipToPlay, false, true, false, clipType);
    }

    public void SetIgnitionChime()
    {
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
        }
        audioTimedCoroutine = StartCoroutine(PlayAudioClip(-1, false, false, true, 0));
        PlayAudioClipRpc(-1, false, false, true, -1);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void PlayAudioClipRpc(int clipToPlay, bool isThankYouClip, bool isSpecialWarning, bool isIgnitionChime, int specialType)
    {
        if (GameNetworkManager.Instance.localPlayerController == null)
            return;

        if (clientClipQueue.Count >= voiceAudioClips.Length)
            clientClipQueue.Clear();

        if (isSpecialWarning || isThankYouClip)
        {
            CancelVoiceAudioCoroutine();
            clientClipQueue.Clear();
        }
        clientClipQueue.Enqueue((clipToPlay, isThankYouClip, isSpecialWarning, isIgnitionChime, specialType));
        if (!clientIsPlaying)
            StartCoroutine(PlayAudioClipsAfterQueue());
    }

    public void CancelVoiceAudioCoroutine()
    {
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
            audioTimedCoroutine = null!;
        }
    }

    private IEnumerator PlayAudioClipsAfterQueue()
    {
        clientIsPlaying = true;
        while (clientClipQueue.Count > 0)
        {
            var (clipId, isThankYouClip, isSpecialWarning, isIgnitionChime, clipType) = clientClipQueue.Dequeue();
            if (audioTimedCoroutine != null)
                StopCoroutine(audioTimedCoroutine);
            audioTimedCoroutine = StartCoroutine(PlayAudioClip(clipId, isThankYouClip, isSpecialWarning, isIgnitionChime, clipType));
            yield return new WaitUntil(() => audioTimedCoroutine == null);
        }
        clientIsPlaying = false;
    }


    public IEnumerator PlayAudioClip(int clipId, bool playThankAudio, bool isSpecialWarning, bool isIgnitionChime, int clipType)
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
            yield return PlaySpecialAlert(clipId, clipType);
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
        isPlayingBeeps = true;
        yield return new WaitForSeconds(0.25f);

        clusterScreen.fontSize = controller.isSpecial ? 1.78f : 2.1f;
        clusterScreen.text = controller.isSpecial ? "sys status: <booting>" : "service due!";
        voiceAudio.Stop();
        voiceAudio.clip = sixBeepAlert;
        voiceAudio.Play();

        yield return new WaitForSeconds(voiceAudio.clip.length + 0.45f);
        clusterScreen.fontSize = 2.1f;
        audioTimedCoroutine = null!;
        clusterScreen.text = null;
        currentClipId = -1;
        isPlayingBeeps = false;
        voiceAudio.clip = null;
    }

    private IEnumerator PlaySpecialAlert(int clipId, int clipType)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (clipId != -1)
            {
                audioClipsInQueue[clipId] = false;
                audioClipsJustPlayed[clipId] = true;
            }
            else if (clipId == -1 && clipType == -1)
            {
                audioClipsInQueue[(int)ElectronicVoiceAlert.HeadlampsOn] = false;
                audioClipsInQueue[(int)ElectronicVoiceAlert.KeyInIgnition] = false;

                audioClipsJustPlayed[(int)ElectronicVoiceAlert.HeadlampsOn] = true;
                audioClipsJustPlayed[(int)ElectronicVoiceAlert.KeyInIgnition] = true;
            }
        }
        yield return new WaitForSeconds(0.3f);
        voiceAudio.loop = false;
        if (clipId == -1 && clipType == -1)
        {
            while (true)
            {
                yield return SetScreenTextAfterDelay((int)ElectronicVoiceAlert.KeyInIgnition, 0.08f);

                voiceAudio.Stop();
                voiceAudio.PlayOneShot(singleAlert);
                yield return new WaitForSeconds(singleAlert.length);

                voiceAudio.clip = voiceAudioClips[(int)ElectronicVoiceAlert.KeyInIgnition];
                voiceAudio.Play();
                yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);

                voiceAudio.Stop();
                voiceAudio.clip = highToneAlertAlt;
                voiceAudio.Play();

                yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);

                yield return SetScreenTextAfterDelay((int)ElectronicVoiceAlert.HeadlampsOn, 0.08f);

                voiceAudio.Stop();
                voiceAudio.PlayOneShot(singleAlert);
                yield return new WaitForSeconds(singleAlert.length);

                voiceAudio.clip = voiceAudioClips[(int)ElectronicVoiceAlert.HeadlampsOn];
                voiceAudio.Play();
                yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);

                voiceAudio.Stop();
                voiceAudio.clip = highToneAlert;
                voiceAudio.Play();
                yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);
            }
        }
        else
        {
            while (true)
            {
                yield return SetScreenTextAfterDelay(clipId, 0.08f);

                voiceAudio.Stop();
                voiceAudio.PlayOneShot(singleAlert);
                yield return new WaitForSeconds(singleAlert.length);

                voiceAudio.clip = voiceAudioClips[clipId];
                voiceAudio.Play();
                yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);

                voiceAudio.Stop();
                voiceAudio.clip = clipType == 1 ? highToneAlert : highToneAlertAlt;
                voiceAudio.Play();
                yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);
            }
        }
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
        voiceAudio.clip = null;
    }

    private IEnumerator PlayVoiceAlert(int clipId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            audioClipsInQueue[clipId] = false;
            audioClipsJustPlayed[clipId] = true;
        }
        if (clipId == (int)ElectronicVoiceAlert.AllSystemsOk)
        {
            isPlayingOnEngineStart = true;
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
        isPlayingOnEngineStart = false;
        voiceAudio.clip = null;
    }

    private IEnumerator SetScreenTextAfterDelay(int clipId, float delay)
    {
        clusterScreen.text = null;
        yield return new WaitForSeconds(delay);
        clusterScreen.text = clusterTexts[clipId];
    }

    public void WipeVoiceModuleMemory()
    {
        hasWarnedEngineCritical = false;
        hasWarnedFuelLevelLow = false;
        hasWarnedTransmissionLevelLow = false;
        hasWarnedCoolantLevelLow = false;
        hasWarnedEngineOilLevelLow = false;
        hasAlertedOnEngineStart = false;
        hasWarnedEngineTemperature = false;

        var resetAlerts = new[] {0, 10, 14, 11, 12, 15, 16, 3, 4, 17};
        foreach (var index in resetAlerts)
        {
            audioClipsJustPlayed[index] = false;
            audioClipsInQueue[index] = false;
        }
    }
}
