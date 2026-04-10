using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public Queue<(int clipId, bool thank, bool important, bool ignition, int importantType)> alertClipQueue = new();
    public bool audioIsPlaying;

    public string[] clusterTexts = null!;
    public bool[] audioClipsJustPlayed = null!;
    public bool isPlayingBeeps;
    public bool isPlayingOnEngineStart;
    public int currentClipId;
    public int randomOverheatClipToPlay;
    public bool isImportantAlert;
    public bool isKeysForgotten;
    public float alertSystemInterval;

    // help me
    public bool hasWarnedChargeSystemLow;
    public bool hasWarnedEngineCritical;
    public bool hasWarnedFuelLevelLow;
    public bool hasWarnedTransmissionLevelLow;
    public bool hasWarnedCoolantLevelLow;
    public bool hasWarnedEngineOilLevelLow;
    public bool hasWarnedEngineTemperature;
    public bool hasAlertedOnEngineStart;
    public bool hasJustPlayedSixBeepChime;

    public bool pendingParkingBrakeThanked;
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
        alertSystemInterval = 0f;
        TryPlayQueuedClips();
        SetIgnitionClips();
        SetHealthAlertClips();
        SetDoorAlertClips();
    }


    private void TryPlayQueuedClips()
    {
        if (alertClipQueue.Count >= voiceAudioClips.Length)
            alertClipQueue.Clear();

        if (alertClipQueue.Count == 0)
            return;

        if (audioTimedCoroutine != null)
            return;

        bool meetsConditionsToPlay = !isImportantAlert && !isKeysForgotten && hasJustPlayedSixBeepChime;
        if (!meetsConditionsToPlay)
            return;

        StartCoroutine(TryPlayAudioClipsInQueue());
    }

    private void SetIgnitionClips()
    {
        if (isImportantAlert && isKeysForgotten &&
            !controller.headlightsContainer.activeSelf &&
            !controller.keyIsInIgnition)
        {
            isImportantAlert = false;
            isKeysForgotten = false;

            ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);

            SetThankYouClip();
            return;
        }
        if (currentClipId != (int)ElectronicVoiceAlert.ThankYou)
        {
            if ((!isImportantAlert || !isKeysForgotten) && 
                controller.currentDriver == null &&
                controller.headlightsContainer.activeSelf &&
                !controller.ignitionStarted && controller.keyIsInIgnition)
            {
                isImportantAlert = true;
                isKeysForgotten = true;
                SetImportantAlert(-1, -1);
                return;
            }

            if (!isImportantAlert && !controller.ignitionStarted &&
                controller.currentDriver == null &&
                controller.headlightsContainer.activeSelf)
            {
                isImportantAlert = true;
                SetImportantAlert((int)ElectronicVoiceAlert.HeadlampsOn, 1);
                return;
            }
            if (!isKeysForgotten && controller.keyIsInIgnition && 
                !controller.ignitionStarted && controller.currentDriver == null)
            {
                isKeysForgotten = true;
                SetImportantAlert((int)ElectronicVoiceAlert.KeyInIgnition, 2);
                return;
            }
        }
        if (isImportantAlert && (!controller.headlightsContainer.activeSelf || controller.ignitionStarted) && 
            currentClipId != (int)ElectronicVoiceAlert.ThankYou)
        {
            isImportantAlert = false;
            isKeysForgotten = false;
            ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
            SetThankYouClip();
            return;
        }
        if (isKeysForgotten && !controller.keyIsInIgnition && currentClipId != (int)ElectronicVoiceAlert.ThankYou)
        {
            isKeysForgotten = false;
            isImportantAlert = false;
            ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
            SetThankYouClip();
            return;
        }
        if ((isKeysForgotten || isImportantAlert) && 
            !controller.driverSideDoor.boolValue && controller.currentDriver != null)
        {
            isKeysForgotten = false;
            isImportantAlert = false;
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
        if (controller.electricsOn)
        {
            if (hasJustPlayedSixBeepChime)
            {
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
                !isImportantAlert && !isKeysForgotten)
            {
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
        ResetMemory();
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

        SetLowHealthAlert(ref hasWarnedFuelLevelLow, 27, ElectronicVoiceAlert.FuelLevel);
        SetLowHealthAlert(ref hasWarnedTransmissionLevelLow, 23, ElectronicVoiceAlert.TransFluidLevel);
        SetLowHealthAlert(ref hasWarnedCoolantLevelLow, 19, ElectronicVoiceAlert.EngineCoolantLevel);
        SetLowHealthAlert(ref hasWarnedEngineOilLevelLow, 15, ElectronicVoiceAlert.EngineOilLevel);
        SetLowHealthAlert(ref hasWarnedEngineCritical, 12, ElectronicVoiceAlert.EngineOilPressure);

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
                if (!IsClipInQueue(doorClipId) &&
                    !pendingDoorThanked)
                {
                    SetClipInQueue((ElectronicVoiceAlert)doorClipId);
                    pendingDoorThanked = true;
                }
            }
            else if (!doorOpen && pendingDoorThanked)
            {
                SetThankYouClip();
                RemoveClipFromQueue(doorClipId);
                audioClipsJustPlayed[doorClipId] = false;
                pendingDoorThanked = false;
            }
        }
        else
        {
            RemoveClipFromQueue(doorClipId);
            pendingDoorThanked = false;
        }
    }

    private void SetLowHealthAlert(ref bool warningType, int hpThreshold, ElectronicVoiceAlert alertType)
    {
        if (controller.carHP <= hpThreshold)
        {
            if (!warningType)
            {
                warningType = true;
                SetClipInQueue(alertType);
            }
        }
        else
        {
            if (warningType)
            {
                warningType = false;
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
                SetClipInQueue((ElectronicVoiceAlert)randomOverheatClipToPlay);
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
        if (controller.handbrakeEngaged && controller.drivePedalPressed && !WasClipPlayed(ElectronicVoiceAlert.ParkBrakeOn))
        {
            SetClipInQueue(ElectronicVoiceAlert.ParkBrakeOn);
        }
        else if (WasClipPlayed(ElectronicVoiceAlert.ParkBrakeOn) && (!controller.handbrakeEngaged || !controller.drivePedalPressed))
        {
            ResetAudioClip(ElectronicVoiceAlert.ParkBrakeOn);         
        }
    }

    private void ResetAudioClip(ElectronicVoiceAlert alert)
    {
        int clipId = (int)alert;

        RemoveClipFromQueue(clipId);
        audioClipsJustPlayed[clipId] = false;
    }

    private void RemoveClipFromQueue(int clipId)
    {
        // queues apparently don't have a .remove, so we'll just rebuild it each time without the alert we want to remove
        // thank you unity.
        alertClipQueue = new Queue<(int, bool, bool, bool, int)> (alertClipQueue.Where(x => x.clipId != clipId));
    }

    private void SetClipInQueue(ElectronicVoiceAlert alert, bool isImportantWarning = false, bool isThankYouClip = false, bool isIgnitionChime = false, int importantType = -1)
    {
        int clipId = (int)alert;

        if (IsClipInQueue(clipId))
            return;

        if (isImportantWarning || isThankYouClip)
        {
            CancelVoiceAudioCoroutine();
            alertClipQueue.Clear();
        }

        alertClipQueue.Enqueue((clipId, isThankYouClip, isImportantWarning, isIgnitionChime, importantType));
    }

    // return true if the same fucking clip is in the same fucking queue so we dont enqueue the same fucking audio clip into the fucking queue
    public bool IsClipInQueue(int alertInt)
    {
        if (alertClipQueue.Any(i => i.clipId == alertInt))
            return true;

        return false;
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
            SetClipInQueue((ElectronicVoiceAlert)currentClipId, isImportantWarning: false, isThankYouClip: false);
        }
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
        }
        audioTimedCoroutine = StartCoroutine(PlayAudioClip(2, true, false, false, 0));
        PlayAudioClipRpc(2, true, false, false, -1);
    }

    public void SetImportantAlert(int clipToPlay, int clipType)
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
    public void PlayAudioClipRpc(int clipToPlay, bool isThankYouClip, bool isImportantWarning, bool isIgnitionChime, int importantType)
    {
        if (alertClipQueue.Count >= voiceAudioClips.Length)
            alertClipQueue.Clear();

        if (isImportantWarning || isThankYouClip)
        {
            CancelVoiceAudioCoroutine();
            alertClipQueue.Clear();
        }
        alertClipQueue.Enqueue((clipToPlay, isThankYouClip, isImportantWarning, isIgnitionChime, importantType));
        if (!audioIsPlaying)
            StartCoroutine(TryPlayAudioClipsInQueue());
    }

    public void CancelVoiceAudioCoroutine()
    {
        if (audioTimedCoroutine != null)
        {
            StopCoroutine(audioTimedCoroutine);
            audioTimedCoroutine = null!;
        }
    }

    private IEnumerator TryPlayAudioClipsInQueue()
    {
        audioIsPlaying = true;
        while (alertClipQueue.Count > 0)
        {
            var (clipId, isThankYouClip, isImportantWarning, isIgnitionChime, clipType) = alertClipQueue.Dequeue();
            if (audioTimedCoroutine != null)
                StopCoroutine(audioTimedCoroutine);
            audioTimedCoroutine = StartCoroutine(PlayAudioClip(clipId, isThankYouClip, isImportantWarning, isIgnitionChime, clipType));
            yield return new WaitUntil(() => audioTimedCoroutine == null);
        }
        audioIsPlaying = false;
    }


    public IEnumerator PlayAudioClip(int clipId, bool playThankAudio, bool isImportantWarning, bool isIgnitionChime, int clipType)
    {
        currentClipId = clipId;
        voiceAudio.loop = false;

        if (isIgnitionChime)
        {
            yield return PlayIgnitionChime();
            yield break;
        }

        if (isImportantWarning)
        {
            yield return PlayImportantAlert(clipId, clipType);
            yield break;
        }

        if (playThankAudio)
        {
            yield return PlayThankYouAudio(clipId);
            yield break;
        }

        yield return PlayVoiceAlert(clipId);
        yield break;
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

    private IEnumerator PlayImportantAlert(int clipId, int clipType)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (clipId != -1)
            {
                RemoveClipFromQueue(clipId);
                audioClipsJustPlayed[clipId] = true;
            }
            else if (clipId == -1 && clipType == -1)
            {
                RemoveClipFromQueue((int)ElectronicVoiceAlert.HeadlampsOn);
                RemoveClipFromQueue((int)ElectronicVoiceAlert.KeyInIgnition);

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
            RemoveClipFromQueue(clipId);
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

    public void ResetMemory()
    {
        hasWarnedEngineCritical = false;
        hasWarnedFuelLevelLow = false;
        hasWarnedTransmissionLevelLow = false;
        hasWarnedCoolantLevelLow = false;
        hasWarnedEngineOilLevelLow = false;
        hasAlertedOnEngineStart = false;
        hasWarnedEngineTemperature = false;

        int[] alerts = {0, 10, 14, 11, 12, 15, 16, 3, 4, 17};
        foreach (int i in alerts)
        {
            audioClipsJustPlayed[i] = false;
            RemoveClipFromQueue(i);
        }
    }
}
