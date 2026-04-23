using ScanVan.Utils;
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

namespace ScanVan.Behaviour;

public class VoiceAlertDefinition
{
    public ElectronicVoiceAlert alertType;
    public VoiceAlertPriority priority;

    public Func<bool> triggerCondition = null!;
    public Func<bool> resetCondition = null!;
    public Func<bool> setThankYou = null!;

    public bool canThankYou;
    public bool hasPlayed;
}

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

    public Coroutine playAudioCoroutine = null!;
    public Coroutine audioTimedCoroutine = null!;

    public Light clusterLight = null!;
    public TextMeshPro clusterScreen = null!;

    public VoiceAlertPriority currentPriority;
    public VoiceAlertDefinition[] alerts = null!;
    public Queue<(int clipId, bool thank, bool important, bool ignition, int importantType)> alertClipQueue = new();
    public bool audioIsPlaying;

    public int randomServiceAlert = -1;
    public float baseVolume = 0.8f;
    public string[] clusterTexts = null!;
    public bool isPlayingIgnitionChime;
    public bool isPlayingOnEngineStart;
    public int currentClipId;
    public int randomOverheatClipToPlay;
    public bool isImportantAlert;
    public bool isKeysForgotten;
    public float alertSystemInterval;

    public bool setThankYouClip;
    public bool stopThankYouClip;

    public bool hasAlertedOnEngineStart;
    public bool ignitionChimeStarted;
    public bool ignitionChimeFinished;

    public void Start()
    {
        alerts = new VoiceAlertDefinition[]
        {
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.AllSystemsOk,
                triggerCondition = () => controller.electricsOn && ignitionChimeFinished && !hasAlertedOnEngineStart && !audioIsPlaying && controller.carHP > 22 && controller.wheelRPM > 50f,
                resetCondition = () => !controller.electricsOn,
                priority = VoiceAlertPriority.IsIgnition
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.WasherFluidLow,
                triggerCondition = () => controller.electricsOn && ignitionChimeFinished,
                resetCondition = () => !controller.electricsOn || !ignitionChimeFinished,
                priority = VoiceAlertPriority.IsService
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.DiskBrakePads,
                triggerCondition = () => controller.electricsOn && ignitionChimeFinished && randomServiceAlert > 0,
                resetCondition = () => !controller.electricsOn || !ignitionChimeFinished || randomServiceAlert == -1,
                priority = VoiceAlertPriority.IsService
            },
            new VoiceAlertDefinition
            {
                // unused
                alertType = ElectronicVoiceAlert.FastenBelts,
                triggerCondition = () => controller.testingVehicleInEditor,
                resetCondition = () => !controller.testingVehicleInEditor,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.ThankYou,
                triggerCondition = () => setThankYouClip,
                resetCondition = () => stopThankYouClip,
                priority = VoiceAlertPriority.Override
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.ParkBrakeOn,
                triggerCondition = () => audioTimedCoroutine == null && controller.handbrakeEngaged && controller.drivePedalPressed && controller.ignitionStarted,
                resetCondition = () => !controller.handbrakeEngaged || !controller.ignitionStarted,
                canThankYou = true,
                setThankYou = () => !controller.handbrakeEngaged,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.KeyInIgnition,
                triggerCondition = () => controller.keyIsInIgnition && !controller.ignitionStarted && controller.driverSideDoor.boolValue,
                resetCondition = () => !controller.driverSideDoor.boolValue || !controller.keyIsInIgnition || controller.ignitionStarted,
                priority = VoiceAlertPriority.IsImportant,
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.HeadlampsOn,
                triggerCondition = () => controller.headlightsContainer.activeSelf && !controller.ignitionStarted && controller.driverSideDoor.boolValue,
                resetCondition = () => !controller.driverSideDoor.boolValue || !controller.headlightsContainer.activeSelf || controller.ignitionStarted,
                priority = VoiceAlertPriority.IsImportant,
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.DriverDoorAjar,
                triggerCondition = () => audioTimedCoroutine == null && controller.ignitionStarted && controller.driverSideDoor.boolValue && controller.wheelRPM > 80f,
                resetCondition = () => !controller.driverSideDoor.boolValue || !controller.ignitionStarted,
                canThankYou = true,
                setThankYou = () => !controller.driverSideDoor.boolValue && controller.ignitionStarted,
                priority = VoiceAlertPriority.DoorAlert
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.PassengerDoorAjar,
                triggerCondition = () => audioTimedCoroutine == null && controller.ignitionStarted && controller.passengerSideDoor.boolValue && controller.wheelRPM > 80f,
                resetCondition = () => !controller.passengerSideDoor.boolValue || !controller.ignitionStarted,
                canThankYou = true,
                setThankYou = () => !controller.passengerSideDoor.boolValue && controller.ignitionStarted,
                priority = VoiceAlertPriority.DoorAlert
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.RearHatchAjar,
                triggerCondition = () => audioTimedCoroutine == null && controller.ignitionStarted && controller.liftGateOpen && controller.wheelRPM > 80f,
                resetCondition = () => !controller.liftGateOpen || !controller.ignitionStarted,
                canThankYou = true,
                setThankYou = () => !controller.liftGateOpen && controller.ignitionStarted,
                priority = VoiceAlertPriority.DoorAlert
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.BackRightDoorAjar,
                triggerCondition = () => audioTimedCoroutine == null && controller.ignitionStarted && controller.sideDoorOpen && controller.wheelRPM > 80f,
                resetCondition = () => !controller.sideDoorOpen || !controller.ignitionStarted,
                canThankYou = true,
                setThankYou = () => !controller.sideDoorOpen && controller.ignitionStarted,
                priority = VoiceAlertPriority.DoorAlert
            },

            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.FuelLevel,
                triggerCondition = () => audioTimedCoroutine == null && controller.carHP <= 38,
                resetCondition = () => controller.carHP > 38 || !controller.electricsOn || isImportantAlert,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineCoolantLevel,
                triggerCondition = () => audioTimedCoroutine == null && controller.carHP <= 30,
                resetCondition = () => controller.carHP > 30 || !controller.electricsOn || isImportantAlert,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineOilLevel,
                triggerCondition = () => audioTimedCoroutine == null && controller.carHP <= 25,
                resetCondition = () => controller.carHP > 25 || !controller.electricsOn || isImportantAlert,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.ChargeSysMalfunction,
                triggerCondition = () => audioTimedCoroutine == null && controller.carHP <= 22,
                resetCondition = () => controller.carHP > 22 || !controller.electricsOn || isImportantAlert,
                priority = VoiceAlertPriority.Medium
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineOilPressure,
                triggerCondition = () => audioTimedCoroutine == null && controller.carHP <= 16,
                resetCondition = () => controller.carHP > 16 || !controller.electricsOn || isImportantAlert,
                priority = VoiceAlertPriority.Low
            },

            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineTempAboveNormal,
                triggerCondition = () => audioTimedCoroutine == null && controller.carHP <= 12 && randomOverheatClipToPlay == 15,
                resetCondition = () => controller.carHP > 12 || !controller.electricsOn || randomOverheatClipToPlay == 0 || isImportantAlert,
                priority = VoiceAlertPriority.Medium
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineOverheating,
                triggerCondition = () => audioTimedCoroutine == null && controller.carHP <= 12 && randomOverheatClipToPlay == 16,
                resetCondition = () => controller.carHP > 12 || !controller.electricsOn || randomOverheatClipToPlay == 0 || isImportantAlert,
                priority = VoiceAlertPriority.Medium
            }
        };
    }

    public void DoEVACycle()
    {
        if (controller == null || !controller.IsSpawned ||
            controller.carDestroyed) return;

        if (!NetworkManager.Singleton.IsHost)
        {
            return;
        }

        if (alertSystemInterval <= 0.25f)
        {
            alertSystemInterval += Time.deltaTime;
            return;
        }
        alertSystemInterval = 0f;

        SetRandomOverheatClip();
        SetVoiceAlertClips();
        SetIgnitionClips();
        TryPlayQueuedClips();
    }

    public void SetRandomOverheatClip()
    {
        if (!controller.ignitionStarted)
        {
            randomOverheatClipToPlay = 0;
            return;
        }

        if (controller.isHoodOnFire && controller.carHP <= 15)
        {
            if (randomOverheatClipToPlay == 0)
            {
                int clipIndex = UnityEngine.Random.Range(15, 17);
                randomOverheatClipToPlay = clipIndex;
            }
            return;
        }
        randomOverheatClipToPlay = 0;
    }

    public void SetVoiceAlertClips()
    {
        foreach (var alert in alerts)
        {
            if (alert.triggerCondition() && !alert.hasPlayed)
            {
                if (audioTimedCoroutine != null && voiceAudio.isPlaying)
                {
                    if (CanInterrupt(currentPriority, alert.priority))
                    {
                        // if we can interrupt entirely, stop the current audio, clear the queue and set this clip in queue
                        CancelVoiceAudioCoroutine();
                        alertClipQueue.Clear();
                        SetClipInQueue(alert.alertType);
                    }
                    else
                    {
                        // just set a clip in queue to be played later
                        SetClipInQueue(alert.alertType);
                    }
                }
                else
                {
                    // set clip in queue
                    SetClipInQueue(alert.alertType);
                }
                alert.hasPlayed = true;
            }
            else if (alert.hasPlayed && alert.canThankYou && alert.setThankYou())
            {
                if (audioTimedCoroutine != null && voiceAudio.isPlaying)
                {
                    // override is the "thank you" clip, which can only be interrupted by important alerts
                    if (!CanInterrupt(currentPriority, VoiceAlertPriority.Override))
                        continue;
                }
                SetThankYouClip();
                alert.hasPlayed = false;
            }
            else if (alert.resetCondition() && alert.hasPlayed)
            {
                // reset the alert "played" flag
                ResetAudioClip(alert.alertType);
                alert.hasPlayed = false;
            }
        }
    }

    // this doesn't work *entirely* like a priority system, it's more of a "can the clip that wants to play interrupt the currently playing clip?"
    private bool CanInterrupt(VoiceAlertPriority current, VoiceAlertPriority alertPriority)
    {
        // the "highest priority" aka headlamps or keys left in the ignition alert
        if (alertPriority == VoiceAlertPriority.IsImportant)
            return true;

        // do not allow playing a message while the "all systems okay" message is playing
        if (current == VoiceAlertPriority.IsIgnition)
            return false;

        // thank you clip cannot interrupt the ignition clip, but can interrupt anything else
        if (alertPriority == VoiceAlertPriority.Override)
            return current != VoiceAlertPriority.IsIgnition;

        // if the clip to be played is the "all systems okay" message, allow it to interrupt anything playing right now (except 'service' items)
        if (alertPriority == VoiceAlertPriority.IsIgnition)
            return current != VoiceAlertPriority.IsService;

        // otherwise, check if the current priority is more than the currently playing one
        return alertPriority > current;
    }

    public void SetIgnitionClips()
    {
        // please don't shoot me dead zaggy
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
                controller.driverSideDoor.boolValue &&
                controller.headlightsContainer.activeSelf &&
                !controller.ignitionStarted && controller.keyIsInIgnition)
            {
                isImportantAlert = true;
                isKeysForgotten = true;
                SetImportantAlert(-1, -1);
                return;
            }

            if (!isImportantAlert && !controller.ignitionStarted &&
                controller.driverSideDoor.boolValue &&
                controller.headlightsContainer.activeSelf)
            {
                isImportantAlert = true;
                SetImportantAlert((int)ElectronicVoiceAlert.HeadlampsOn, 1);
                return;
            }
            if (!isKeysForgotten && controller.keyIsInIgnition &&
                !controller.ignitionStarted && controller.driverSideDoor.boolValue)
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
            !controller.driverSideDoor.boolValue)
        {
            isKeysForgotten = false;
            isImportantAlert = false;
            if (isPlayingIgnitionChime)
            {
                ignitionChimeStarted = false;
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
            if (!ignitionChimeStarted && currentClipId != (int)ElectronicVoiceAlert.ThankYou &&
                !isImportantAlert && !isKeysForgotten)
            {
                isPlayingOnEngineStart = true;
                ignitionChimeStarted = true;
                hasAlertedOnEngineStart = false;

                int clipIndex = UnityEngine.Random.Range(-5, 4);
                randomServiceAlert = clipIndex;

                ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
                ResetAudioClip(ElectronicVoiceAlert.AllSystemsOk);

                SetIgnitionChime();
                return;
            }
            return;
        }
        ResetAudioClip(ElectronicVoiceAlert.ThankYou);
        ignitionChimeFinished = false;
        ignitionChimeStarted = false;
        hasAlertedOnEngineStart = false;
        randomOverheatClipToPlay = 0;
        randomServiceAlert = -1;
    }

    private void TryPlayQueuedClips()
    {
        if (alertClipQueue.Count == 0)
            return;

        if (audioTimedCoroutine != null || playAudioCoroutine != null)
            return;

        bool meetsConditionsToPlay = !isImportantAlert && !isKeysForgotten && ignitionChimeStarted;
        if (!meetsConditionsToPlay)
            return;

        playAudioCoroutine = StartCoroutine(TryPlayAudioClipsInQueue());
    }

    private void StopAudioClipIfPlaying()
    {
        CancelVoiceAudioCoroutine();
        voiceAudio.clip = null;
        currentClipId = -1;
        clusterScreen.text = null;
        clusterLight.enabled = false;
        StopAudioClipRpc();
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void StopAudioClipRpc()
    {
        CancelVoiceAudioCoroutine();
        voiceAudio.clip = null;
        currentClipId = -1;
        clusterScreen.text = null;
        clusterLight.enabled = false;
    }

    private void ResetAudioClip(ElectronicVoiceAlert alert)
    {
        int clipId = (int)alert;
        RemoveClipFromQueue(clipId);
    }

    private void RemoveClipFromQueue(int clipId)
    {
        // queues apparently don't have a .remove, so we'll just rebuild it each time without the alert we want to remove
        // thank you unity.

        if (!IsClipInQueue(clipId))
            return;
        alertClipQueue = new Queue<(int, bool, bool, bool, int)> (alertClipQueue.Where(i => i.clipId != clipId));
    }

    private void SetClipInQueue(ElectronicVoiceAlert alert, bool isThankYouClip = false, bool isImportantWarning = false, bool isIgnitionChime = false, int importantType = -1)
    {
        int clipId = (int)alert;
        if (IsClipInQueue(clipId))
            return;
        alertClipQueue.Enqueue((clipId, isThankYouClip, isImportantWarning, isIgnitionChime, importantType));
    }

    public bool IsClipInQueue(int alertInt)
    {
        if (alertClipQueue.Any(i => i.clipId == alertInt))
            return true;

        return false;
    }

    public void SetThankYouClip()
    {
        if (voiceAudio.clip != null && voiceAudio.isPlaying && voiceAudio.clip != voiceAudioClips[(int)ElectronicVoiceAlert.ThankYou] &&
            voiceAudio.time < voiceAudio.clip.length / 2f && currentClipId != -1 && voiceAudio.clip != voiceAudioClips[(int)ElectronicVoiceAlert.ParkBrakeOn])
        {
            var alert = alerts.FirstOrDefault(i => (int)i.alertType == currentClipId);
            if (alert != null && alert.triggerCondition())
            {
                SetClipInQueue((ElectronicVoiceAlert)currentClipId, false, false);
            }
        }
        CancelVoiceAudioCoroutine();
        alertClipQueue.Clear();

        PlayAudioClip(2, true, false, false, 0);
        PlayAudioClipRpc(2, true, false, false, -1);
    }

    public void SetImportantAlert(int clipToPlay, int clipType)
    {
        CancelVoiceAudioCoroutine();
        alertClipQueue.Clear();

        PlayAudioClip(clipToPlay, false, true, false, clipType);
        PlayAudioClipRpc(clipToPlay, false, true, false, clipType);
    }

    public void SetIgnitionChime()
    {
        CancelVoiceAudioCoroutine();
        alertClipQueue.Clear();

        PlayAudioClip(-1, false, false, true, -1);
        PlayAudioClipRpc(-1, false, false, true, -1);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void PlayAudioClipRpc(int clipToPlay = 0, bool isThankYouClip = false, bool isImportantWarning = false, bool isIgnitionChime = false, int importantType = -1, bool overridePlayback = true)
    {
        if (overridePlayback)
        {
            CancelVoiceAudioCoroutine();
            alertClipQueue.Clear();
            PlayAudioClip(clipToPlay, isThankYouClip, isImportantWarning, isIgnitionChime, importantType);
        }
        else
        {
            if (alertClipQueue.Count >= voiceAudioClips.Length)
                alertClipQueue.Clear();

            alertClipQueue.Enqueue((clipToPlay, isThankYouClip, isImportantWarning, isIgnitionChime, importantType));
            playAudioCoroutine = StartCoroutine(TryPlayAudioClipsInQueue());
        }
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
            if (NetworkManager.Singleton.IsServer)
            {
                PlayAudioClipRpc(clipId, isThankYouClip, isImportantWarning, isIgnitionChime, clipType, overridePlayback: false);
            }
            if (isThankYouClip || isImportantWarning || isIgnitionChime)
            {
                CancelVoiceAudioCoroutine();
                alertClipQueue.Clear();
            }
            PlayAudioClip(clipId, isThankYouClip, isImportantWarning, isIgnitionChime, clipType);
            yield return new WaitUntil(() => audioTimedCoroutine == null);
        }
        audioIsPlaying = false;
        playAudioCoroutine = null!;
        yield break;
    }

    // get current priority of the currently playing clip so we can check if it can be interrupted for any clips that get queued
    private VoiceAlertPriority GetPriority(int clipId, bool isThankYou, bool isImportant, bool isIgnition)
    {
        if (isImportant) return VoiceAlertPriority.IsImportant;
        if (isIgnition) return VoiceAlertPriority.IsIgnition;
        if (isThankYou) return VoiceAlertPriority.Override;

        var alert = alerts.FirstOrDefault(i => (int)i.alertType == clipId);
        return alert?.priority ?? VoiceAlertPriority.Low;
    }

    public void PlayAudioClip(int clipId, bool playThankAudio, bool isImportantWarning, bool isIgnitionChime, int clipType)
    {
        if (NetworkManager.Singleton.IsServer)
            currentPriority = GetPriority(clipId, playThankAudio, isImportantWarning, isIgnitionChime);

        currentClipId = clipId;
        voiceAudio.loop = false;
        voiceAudio.volume = baseVolume * UserConfig.VoiceAlertVolume.Value;

        if (isIgnitionChime)
        {
            audioTimedCoroutine = StartCoroutine(PlayIgnitionChime());
            return;
        }

        if (isImportantWarning)
        {
            audioTimedCoroutine = StartCoroutine(PlayImportantAlert(clipId, clipType));
            return;
        }

        if (playThankAudio)
        {
            audioTimedCoroutine = StartCoroutine(PlayThankYouAudio(clipId));
            return;
        }

        audioTimedCoroutine = StartCoroutine(PlayVoiceAlert(clipId));
        return;
    }

    private IEnumerator PlayIgnitionChime()
    {
        currentClipId = -1;
        isPlayingIgnitionChime = true;
        yield return new WaitForSeconds(0.25f);

        clusterScreen.fontSize = 2.1f; // controller.isSpecial ? 1.78f : 
        clusterScreen.text = controller.isSpecial ? "sys status: [booting]" : "service due";
        clusterLight.enabled = true;
        voiceAudio.Stop();
        voiceAudio.clip = sixBeepAlert;
        voiceAudio.Play();

        yield return new WaitForSeconds(voiceAudio.clip.length + 0.45f);
        //clusterScreen.fontSize = 2.1f;
        audioTimedCoroutine = null!;
        clusterScreen.text = null;
        clusterLight.enabled = false;
        currentClipId = -1;
        isPlayingIgnitionChime = false;
        voiceAudio.clip = null;
        if (NetworkManager.Singleton.IsServer)
            currentPriority = VoiceAlertPriority.None;
        ignitionChimeFinished = true;
        yield break;
    }

    private IEnumerator PlayImportantAlert(int clipId, int clipType)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (clipId != -1)
            {
                RemoveClipFromQueue(clipId);
            }
            else if (clipId == -1 && clipType == -1)
            {
                RemoveClipFromQueue((int)ElectronicVoiceAlert.HeadlampsOn);
                RemoveClipFromQueue((int)ElectronicVoiceAlert.KeyInIgnition);
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
        voiceAudio.Stop();
        clusterScreen.text = null;
        clusterLight.enabled = false;
        voiceAudio.clip = voiceAudioClips[2];
        yield return new WaitForSeconds(0.15f);

        clusterScreen.text = clusterTexts[clipId];
        clusterLight.enabled = true;
        voiceAudio.Play();

        yield return new WaitForSeconds(voiceAudio.clip.length + 0.45f);

        audioTimedCoroutine = null!;
        clusterScreen.text = null;
        clusterLight.enabled = false;
        currentClipId = -1;
        currentPriority = VoiceAlertPriority.None;
        voiceAudio.clip = null;
        yield break;
    }

    private IEnumerator PlayVoiceAlert(int clipId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            RemoveClipFromQueue(clipId);
        }
        if (clipId == (int)ElectronicVoiceAlert.AllSystemsOk)
        {
            isPlayingOnEngineStart = true;
            hasAlertedOnEngineStart = true;
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
        clusterLight.enabled = false;
        currentClipId = -1;
        if (clipId == (int)ElectronicVoiceAlert.AllSystemsOk)
        {
            isPlayingOnEngineStart = false;
        }
        if (NetworkManager.Singleton.IsServer)
            currentPriority = VoiceAlertPriority.None;
        voiceAudio.clip = null;
        yield break;
    }

    private IEnumerator SetScreenTextAfterDelay(int clipId, float delay)
    {
        clusterScreen.text = null;
        clusterLight.enabled = false;
        yield return new WaitForSeconds(delay);
        clusterScreen.text = clusterTexts[clipId];
        clusterLight.enabled = true;
        yield break;
    }
}
