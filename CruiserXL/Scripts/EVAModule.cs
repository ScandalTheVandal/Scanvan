using ScanVan.Scripts;
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

    private const string SYSTEM_STATUS_BOOTING = "sys status: [booting]";
    private const string SERVICE_DUE = "service due";

    public CruiserXLController controller = null!;
    public AudioClip[] voiceAudioClips = null!;
    public AudioSource voiceAudio = null!;

    public AudioClip sixBeepAlert = null!;
    public AudioClip singleAlert = null!;
    public AudioClip highToneAlert = null!;
    public AudioClip highToneAlertAlt = null!;

    public AudioClip doorAlertAlt = null!;
    public AudioClip keysAlertAlt = null!;

    public Coroutine playAudioCoroutine = null!;
    public Coroutine audioTimedCoroutine = null!;

    public Light clusterLight = null!;
    public TextMeshPro clusterScreen = null!;

    public VoiceAlertPriority currentPriority;
    public VoiceAlertDefinition[] alerts = null!;
    public List<VoiceStruct> alertClipQueue = new();

    public bool audioIsPlaying;

    public bool useAltAudio;

    private bool off;

    private const float BASE_VOLUME = 0.8f;

    public int randomServiceAlert = -1;
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
                alertType = ElectronicVoiceAlert.WasherFluidLow,
                triggerCondition = () => controller.ignitionOn && ignitionChimeFinished && 
                randomServiceAlert == (int)ElectronicVoiceAlert.WasherFluidLow,
                resetCondition = () => !controller.ignitionOn || !ignitionChimeFinished,
                priority = VoiceAlertPriority.IsService
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.DiskBrakePads,
                triggerCondition = () => controller.ignitionOn && ignitionChimeFinished && 
                randomServiceAlert == (int)ElectronicVoiceAlert.DiskBrakePads,
                resetCondition = () => !controller.ignitionOn || !ignitionChimeFinished,
                priority = VoiceAlertPriority.IsService
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.BrakeFluidLow,
                triggerCondition = () => controller.ignitionOn && ignitionChimeFinished && 
                randomServiceAlert == (int)ElectronicVoiceAlert.BrakeFluidLow,
                resetCondition = () => !controller.ignitionOn || !ignitionChimeFinished,
                priority = VoiceAlertPriority.IsService
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.AllSystemsOk,
                triggerCondition = () => controller.ignitionOn && ignitionChimeFinished && !hasAlertedOnEngineStart && !audioIsPlaying && controller.carHP > 22, // && Mathf.Abs(controller.wheelRPM) > 50f
                resetCondition = () => !controller.ignitionOn,
                priority = VoiceAlertPriority.IsIgnition
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
                alertType = ElectronicVoiceAlert.TransFluidLevel,
                triggerCondition = () => controller.transmissionHP <= 12 && controller.ignitionStarted,
                resetCondition = () => controller.transmissionHP > 12 || !controller.ignitionOn || isImportantAlert,
                priority = VoiceAlertPriority.Medium
            },

            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.FuelLevel,
                triggerCondition = () => controller.carHP <= 38 && controller.ignitionOn,
                resetCondition = () => controller.carHP > 38 || !controller.ignitionOn || isImportantAlert,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineCoolantLevel,
                triggerCondition = () => controller.carHP <= 30  && controller.ignitionOn,
                resetCondition = () => controller.carHP > 30 || !controller.ignitionOn || isImportantAlert,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineOilLevel,
                triggerCondition = () => controller.carHP <= 25 && controller.ignitionOn,
                resetCondition = () => controller.carHP > 25 || !controller.ignitionOn || isImportantAlert,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.ChargeSysMalfunction,
                triggerCondition = () => controller.carHP <= 22 && controller.ignitionOn,
                resetCondition = () => controller.carHP > 22 || !controller.ignitionOn || isImportantAlert,
                priority = VoiceAlertPriority.Medium
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineOilPressure,
                triggerCondition = () => controller.carHP <= 16 && controller.ignitionOn,
                resetCondition = () => controller.carHP > 16 || !controller.ignitionOn || isImportantAlert,
                priority = VoiceAlertPriority.Low
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineTempAboveNormal,
                triggerCondition = () => controller.carHP <= 12 && randomOverheatClipToPlay == 15 && controller.ignitionOn,
                resetCondition = () => controller.carHP > 12 || !controller.ignitionOn || randomOverheatClipToPlay == 0 || isImportantAlert,
                priority = VoiceAlertPriority.Medium
            },
            new VoiceAlertDefinition
            {
                alertType = ElectronicVoiceAlert.EngineOverheating,
                triggerCondition = () => controller.carHP <= 12 && randomOverheatClipToPlay == 16 && controller.ignitionOn,
                resetCondition = () => controller.carHP > 12 || !controller.ignitionOn || randomOverheatClipToPlay == 0 || isImportantAlert,
                priority = VoiceAlertPriority.Medium
            }
        };
    }

    public void DoEVACycle()
    {
        if (controller == null || !controller.IsSpawned ||
            controller.carDestroyed) return;

        if (controller.vanVariant == VanVariant.Creasman)
        {
            if (!off)
            {
                off = true;
                if (audioTimedCoroutine != null)
                {
                    StopCoroutine(audioTimedCoroutine);
                    audioTimedCoroutine = null!;
                }
                if (alertClipQueue.Count != 0)
                {
                    alertClipQueue.Clear();
                }
                if (playAudioCoroutine != null)
                {
                    StopCoroutine(playAudioCoroutine);
                    playAudioCoroutine = null!;
                }
                if (voiceAudio.isPlaying)
                {
                    voiceAudio.Stop();
                    audioIsPlaying = false;
                }
                isPlayingIgnitionChime = false;
                ignitionChimeStarted = false;
                hasAlertedOnEngineStart = false;
                isPlayingOnEngineStart = false;
                isKeysForgotten = false;
                isImportantAlert = false;
            }
            return;
        }

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

        SetIgnitionClips();
        SetRandomOverheatClip();
        SetVoiceAlertClips();
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
        if (!ignitionChimeFinished || isImportantAlert || isKeysForgotten)
            return;

        for (int i = 0; i < alerts.Length; i++)
        {
            var alert = alerts[i];
            if (alert.triggerCondition() && !alert.hasPlayed)
            {
                if (audioTimedCoroutine != null && voiceAudio.isPlaying)
                {
                    if (CanInterrupt(currentPriority, alert.priority))
                    {
                        // if we can interrupt entirely, stop the current audio, clear the queue and set this clip in queue
                        CancelVoiceAudioCoroutine();
                        voiceAudio.Stop();
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

    public void ResetIgnitionOnStart()
    {
        isPlayingIgnitionChime = false;
        ignitionChimeStarted = false;
        hasAlertedOnEngineStart = false;
        isPlayingOnEngineStart = false;
    }

    public void SetIgnitionClips()
    {
        if (isImportantAlert && isKeysForgotten &&
            !controller.headlightsContainer.activeSelf &&
            !controller.keyIsInIgnition)
        {
            isImportantAlert = false;
            isKeysForgotten = false;

            ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);

            SetThankYouClip(setIgnitionOff: !controller.ignitionOn);
            return;
        }
        if (currentClipId != (int)ElectronicVoiceAlert.ThankYou)
        {
            if ((!isImportantAlert || !isKeysForgotten) &&
                controller.driverSideDoor.boolValue && controller.currentDriver == null &&
                controller.headlightsContainer.activeSelf &&
                !controller.ignitionStarted && controller.keyIsInIgnition)
            {
                isImportantAlert = true;
                isKeysForgotten = true;
                SetImportantAlert(-1, -1);
                return;
            }

            if (!isImportantAlert && !controller.ignitionStarted &&
                controller.driverSideDoor.boolValue && controller.currentDriver == null &&
                controller.headlightsContainer.activeSelf)
            {
                isImportantAlert = true;
                SetImportantAlert((int)ElectronicVoiceAlert.HeadlampsOn, 1);
                return;
            }
            if (!isKeysForgotten && controller.keyIsInIgnition &&
                controller.driverSideDoor.boolValue && controller.currentDriver == null &&
                !controller.ignitionStarted)
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

            SetThankYouClip(setIgnitionOff: !controller.ignitionOn);
            return;
        }
        if (isKeysForgotten && !controller.keyIsInIgnition && currentClipId != (int)ElectronicVoiceAlert.ThankYou)
        {
            isKeysForgotten = false;
            isImportantAlert = false;

            ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
            ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);

            SetThankYouClip(setIgnitionOff: !controller.ignitionOn);
            return;
        }
        if ((isKeysForgotten || isImportantAlert) && (!controller.driverSideDoor.boolValue || controller.currentDriver != null))
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
        if (controller.ignitionOn)
        {
            if (!ignitionChimeStarted && currentClipId != (int)ElectronicVoiceAlert.ThankYou &&
                !isImportantAlert && !isKeysForgotten)
            {
                isPlayingOnEngineStart = true;
                ignitionChimeStarted = true;
                hasAlertedOnEngineStart = false;

                ResetAudioClip(ElectronicVoiceAlert.HeadlampsOn);
                ResetAudioClip(ElectronicVoiceAlert.KeyInIgnition);
                ResetAudioClip(ElectronicVoiceAlert.AllSystemsOk);

                SetIgnitionChime();
                return;
            }
            return;
        }
        for (int i = 0; i < alerts.Length; i++)
        {
            var alert = alerts[i];
            if (alert.priority == VoiceAlertPriority.IsImportant)
                continue;
            alert.hasPlayed = false;

        }
        ResetAudioClip(ElectronicVoiceAlert.ThankYou);
        ignitionChimeFinished = false;
        ignitionChimeStarted = false;
        hasAlertedOnEngineStart = false;
        randomOverheatClipToPlay = 0;
    }

    private void TryPlayQueuedClips()
    {
        if (alertClipQueue.Count == 0)
            return;

        if (audioTimedCoroutine != null || playAudioCoroutine != null)
            return;

        bool doesNotMeetConditionsToPlay = isImportantAlert || isKeysForgotten || !ignitionChimeFinished;
        if (doesNotMeetConditionsToPlay)
            return;

        playAudioCoroutine = StartCoroutine(TryPlayAudioClipsInQueue());
    }

    private void StopAudioClipIfPlaying()
    {
        CancelVoiceAudioCoroutine();
        voiceAudio.Stop();
        voiceAudio.clip = null;
        currentClipId = -1;
        clusterScreen.text = null;
        Plugin.LogDebug("Voice alert stop; A");
        clusterLight.enabled = false;
        StopAudioClipRpc(isKeysForgotten, isImportantAlert);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void StopAudioClipRpc(bool setKeys, bool setImportant)
    {
        CancelVoiceAudioCoroutine();
        isKeysForgotten = setKeys;
        isImportantAlert = setImportant;
        voiceAudio.Stop();
        voiceAudio.clip = null;
        currentClipId = -1;
        clusterScreen.text = null;
        Plugin.LogDebug("Voice alert stop; A [RPC]");
        clusterLight.enabled = false;
    }

    private void ResetAudioClip(ElectronicVoiceAlert alert)
    {
        int clipId = (int)alert;
        RemoveClipFromQueue(clipId);
    }

    private void RemoveClipFromQueue(int clipId)
    {
        if (!IsClipInQueue(clipId, out int foundIndex))
            return;
        alertClipQueue.RemoveAt(foundIndex);
    }

    private void SetClipInQueue(ElectronicVoiceAlert alert, bool isThankYouClip = false, bool isImportantWarning = false, bool isIgnitionChime = false, int importantType = -1)
    {
        int alertClipId = (int)alert;
        if (IsClipInQueue(alertClipId, out _))
            return;
        alertClipQueue.Add(new VoiceStruct { clipId = alertClipId, hasThankYou = isThankYouClip, isImportant = isImportantWarning, isIgnition = isIgnitionChime, importantType = importantType });
    }

    public bool IsClipInQueue(int alertInt, out int foundIndex)
    {
        foundIndex = -1;
        for (int i = 0; i < alertClipQueue.Count; i++)
        {
            var alertClip = alertClipQueue[i];
            if (alertClip.clipId == alertInt)
            {
                foundIndex = i;
                return true;
            }
        }
        return false;
    }

    public void SetThankYouClip(bool setIgnitionOff = false)
    {
        if (setIgnitionOff)
        {
            ResetIgnitionOnStart();
        }
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
        voiceAudio.Stop();
        alertClipQueue.Clear();
        PlayAudioClip(new VoiceStruct { clipId = 2, hasThankYou = true, isImportant = false, isIgnition = false, importantType = 0 });
        SetThankYouClipRpc(isImportantAlert, isKeysForgotten, setIgnitionOff);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetThankYouClipRpc(bool isLamps, bool isKeys, bool setIgnitionOff)
    {
        if (setIgnitionOff)
        {
            ResetIgnitionOnStart();
        }
        CancelVoiceAudioCoroutine();
        isKeysForgotten = isKeys;
        isImportantAlert = isLamps;
        voiceAudio.Stop();
        alertClipQueue.Clear();
        PlayAudioClip(new VoiceStruct { clipId = 2, hasThankYou = true, isImportant = false, isIgnition = false, importantType = 0 });
    }

    public void SetImportantAlert(int clipToPlay, int clipType)
    {
        CancelVoiceAudioCoroutine();
        voiceAudio.Stop();
        alertClipQueue.Clear();
        PlayAudioClip(new VoiceStruct { clipId = clipToPlay, hasThankYou = false, isImportant = true, isIgnition = false, importantType = clipType });
        SetImportantAlertRpc(clipToPlay, clipType, isKeysForgotten, isImportantAlert);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetImportantAlertRpc(int clipToPlay, int clipType, bool isKeys, bool isLamps)
    {
        CancelVoiceAudioCoroutine();
        isKeysForgotten = isKeys;
        isImportantAlert = isLamps;
        voiceAudio.Stop();
        alertClipQueue.Clear();
        PlayAudioClip(new VoiceStruct { clipId = clipToPlay, hasThankYou = false, isImportant = true, isIgnition = false, importantType = clipType });
    }

    public void SetIgnitionChime()
    {
        CancelVoiceAudioCoroutine();
        voiceAudio.Stop();
        alertClipQueue.Clear();
        PlayAudioClip(new VoiceStruct { clipId = -1, hasThankYou = false, isImportant = false, isIgnition = true, importantType = -1 });
        SetIgnitionChimeRpc();
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetIgnitionChimeRpc()
    {
        CancelVoiceAudioCoroutine();
        voiceAudio.Stop();
        alertClipQueue.Clear();
        PlayAudioClip(new VoiceStruct { clipId = -1, hasThankYou = false, isImportant = false, isIgnition = true, importantType = -1 });
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void PlayAudioClipRpc(VoiceStruct voice)
    {
        if (alertClipQueue.Count >= voiceAudioClips.Length)
            alertClipQueue.Clear();

        alertClipQueue.Add(voice);
        playAudioCoroutine = StartCoroutine(TryPlayAudioClipsInQueue());
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
            var currentAlertToPlay = alertClipQueue[0];
            alertClipQueue.RemoveAt(0);
            if (NetworkManager.Singleton.IsServer)
            {
                PlayAudioClipRpc(currentAlertToPlay);
            }
            if (currentAlertToPlay.hasThankYou || currentAlertToPlay.isImportant || currentAlertToPlay.isIgnition)
            {
                CancelVoiceAudioCoroutine();
                alertClipQueue.Clear();
            }
            PlayAudioClip(currentAlertToPlay);
            yield return new WaitUntil(() => audioTimedCoroutine == null);
        }
        audioIsPlaying = false;
        playAudioCoroutine = null!;
        yield break;
    }

    // get current priority of the currently playing clip so we can check if it can be interrupted for any clips that get queued
    private VoiceAlertPriority GetPriority(in VoiceStruct voice)
    {
        if (voice.isImportant) return VoiceAlertPriority.IsImportant;
        if (voice.isIgnition) return VoiceAlertPriority.IsIgnition;
        if (voice.hasThankYou) return VoiceAlertPriority.Override;
        for (int i = 0; i < alerts.Length; i++)
        {
            var alert = alerts[i];
            if ((int)alert.alertType == voice.clipId)
            {
                return alert.priority;
            }
        }
        return VoiceAlertPriority.Low;
    }

    // in prevents creating a copy
    public void PlayAudioClip(in VoiceStruct voice)
    {
        if (NetworkManager.Singleton.IsServer)
            currentPriority = GetPriority(voice);
        currentClipId = voice.clipId;
        voiceAudio.loop = false;
        voiceAudio.volume = BASE_VOLUME * UserConfig.VoiceAlertVolume.Value;
        if (voice.isIgnition)
        {
            audioTimedCoroutine = StartCoroutine(PlayIgnitionChime());
            return;
        }
        if (voice.isImportant)
        {
            audioTimedCoroutine = StartCoroutine(PlayImportantAlert(voice.clipId, voice.importantType));
            return;
        }
        if (voice.hasThankYou)
        {
            audioTimedCoroutine = StartCoroutine(PlayThankYouAudio(voice.clipId));
            return;
        }
        audioTimedCoroutine = StartCoroutine(PlayVoiceAlert(voice.clipId));
    }

    private void ClearClusterDisplay()
    {
        clusterScreen.text = null;
        clusterLight.enabled = false;
    }

    private void StopVoiceAlertCoroutine()
    {
        ClearClusterDisplay();
        currentClipId = -1;
        if (NetworkManager.Singleton.IsServer)
            currentPriority = VoiceAlertPriority.None;
        voiceAudio.clip = null;
        audioTimedCoroutine = null!;
    }

    private IEnumerator PlayImportantAlertAudio(int clipId, AudioClip tone)
    {
        voiceAudio.Stop();
        voiceAudio.PlayOneShot(singleAlert);
        yield return new WaitForSeconds(singleAlert.length);
        voiceAudio.clip = voiceAudioClips[clipId];
        voiceAudio.Play();
        yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);
        voiceAudio.Stop();
        voiceAudio.clip = tone;
        voiceAudio.Play();
        yield return new WaitForSeconds(voiceAudio.clip.length + 0.25f);
    }

    private IEnumerator PlayIgnitionChime()
    {
        currentClipId = -1;
        ClearClusterDisplay();
        Plugin.LogDebug("Voice alert stop; B");
        isPlayingIgnitionChime = true;
        yield return new WaitForSeconds(0.25f);
        clusterScreen.text = controller.vanVariant == VanVariant.JCJenson ? SYSTEM_STATUS_BOOTING : SERVICE_DUE;
        clusterLight.enabled = true;
        voiceAudio.Stop();
        voiceAudio.clip = sixBeepAlert;
        voiceAudio.Play();
        yield return new WaitForSeconds(voiceAudio.clip.length + 0.45f);
        ClearClusterDisplay();
        Plugin.LogDebug("Voice alert stop; C");
        currentClipId = -1;
        isPlayingIgnitionChime = false;
        voiceAudio.clip = null;
        if (NetworkManager.Singleton.IsServer)
            currentPriority = VoiceAlertPriority.None;
        ignitionChimeFinished = true;
        audioTimedCoroutine = null!;
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
            while (isImportantAlert || isKeysForgotten)
            {
                yield return SetScreenTextAfterDelay((int)ElectronicVoiceAlert.KeyInIgnition, 0.08f);
                yield return PlayImportantAlertAudio((int)ElectronicVoiceAlert.KeyInIgnition, highToneAlertAlt);
                yield return SetScreenTextAfterDelay((int)ElectronicVoiceAlert.HeadlampsOn, 0.08f);
                yield return PlayImportantAlertAudio((int)ElectronicVoiceAlert.HeadlampsOn, highToneAlert);
            }
        }
        else
        {
            while (isImportantAlert || isKeysForgotten)
            {
                yield return SetScreenTextAfterDelay(clipId, 0.08f);
                yield return PlayImportantAlertAudio(clipId, clipType == 1 ? highToneAlert : highToneAlertAlt);
            }
        }
    }

    private IEnumerator PlayThankYouAudio(int clipId)
    {
        ClearClusterDisplay();
        Plugin.LogDebug("Voice alert stop; D");
        yield return new WaitForSeconds(0.15f);
        clusterScreen.text = clusterTexts[clipId];
        clusterLight.enabled = true;
        voiceAudio.Stop();
        voiceAudio.clip = voiceAudioClips[2];
        voiceAudio.Play();
        yield return new WaitForSeconds(voiceAudio.clip.length + 0.45f);
        StopVoiceAlertCoroutine();
        Plugin.LogDebug("Voice alert stop; E");
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
        StopVoiceAlertCoroutine();
        Plugin.LogDebug("Voice alert stop; F");
        if (clipId == (int)ElectronicVoiceAlert.AllSystemsOk)
        {
            isPlayingOnEngineStart = false;
        }
        yield break;
    }

    private IEnumerator SetScreenTextAfterDelay(int clipId, float delay)
    {
        ClearClusterDisplay();
        yield return new WaitForSeconds(delay);
        clusterScreen.text = clusterTexts[clipId];
        clusterLight.enabled = true;
        yield break;
    }
}
