using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net;
using CruiserXL;
using CruiserXL.Behaviour;
using CruiserXL.Patches;
using CruiserXL.Utils;
using GameNetcodeStuff;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;
using UnityEngine.InputSystem.XR;

public class CruiserXLController : VehicleController
{
    [Header("Modules")]
    public EVAModule voiceModule = null!;
    public EngineModule engineModule = null!;
    public DrivetrainModule drivetrainModule = null!;

    [Header("Variation")]
    public int[] frontFaciaVariant = null!;
    public int[] frontBumperVariant = null!;

    public GameObject[] frontFaciaVariants = null!;
    public GameObject[] frontBumperVariants = null!;

    public bool hasFogLights;
    public bool hasEnclosedRoof = true;

    [Header("Vehicle Physics")]
    public List<WheelCollider> wheels = null!;
    public AnimationCurve steeringWheelCurve = null!;
    public NavMeshObstacle navObstacle = null!;
    public CapsuleCollider cabinPoint = null!;

    private WheelHit[] wheelHits = new WheelHit[4];
    public float clampedLimitTruckVelocity;
    public float sidewaysSlip;
    public float forwardsSlip;
    public float baseForwardStiffness = 1f;
    public float baseSidewaysStiffness = 0.75f;
    public float wheelTorque;
    public float wheelBrakeTorque;
    public Vector3 lastVelocity;
    public bool hasDeliveredVehicle;
    public float maxBrakingPower;
    public float baseSteeringWheelTurnSpeed = 4.5f;
    //public float odoMileage; unused
    public bool centerKeyPressed;

    [Header("Battery System")]

    public float batteryCharge = 1; // 0.25 = fucked 0.6 = barely enough to start 1 = fully charged
    public float dischargedBattery = 0.34f;
    public float batteryDrainMultiplier = 12f;
    public bool electricalShutdown;
    public float batteryCheckInterval; // host only
    public float syncedBatteryCharge;

    public float radioDrain = 0.001f;
    public float lowBeamsDrain = 0.002f;
    public float highBeamsDrain = 0.004f;

    [Header("Multiplayer")]
    public Collider vehicleBounds = null!;
    public Collider storageCompartment = null!;
    public PlayerControllerB currentMiddlePassenger = null!;
    public InteractTrigger middlePassengerSeatTrigger = null!;
    public InteractTrigger driversSideWindow = null!;
    public InteractTrigger passengersSideWindow = null!;
    public AnimatedObjectTrigger driversSideWindowTrigger = null!;
    public AnimatedObjectTrigger passengersSideWindowTrigger = null!;

    public Vector3 playerPositionOffset;
    public Vector3 seatNodePositionOffset;
    public Vector3 syncedMovementSpeed;
    public bool localPlayerInMiddlePassengerSeat;
    public float syncCarEffectsInterval;
    public float syncedWheelRotation;
    public bool syncedDrivePedalPressed;
    public bool syncedBrakePedalPressed;
    public float tyreStress;
    public bool wheelSlipping;

    [Header("Effects")]
    public GameObject[] disableOnDestroy = null!;
    public GameObject[] enableOnDestroy = null!;
    public GameObject windshieldObject = null!;
    public InteractTrigger pushTruckTrigger = null!;
    public MeshRenderer leftBrakeMesh = null!;
    public MeshRenderer rightBrakeMesh = null!;
    public MeshRenderer backLeftBrakeMesh = null!;
    public MeshRenderer backRightBrakeMesh = null!;
    public Collider[] weatherEffectBlockers = null!;

    public AnimatedObjectTrigger backSideDoor = null!;
    public AnimatedObjectTrigger rightSideDoor = null!;

    public Animator leftElectricMirrorAnim = null!;
    public Animator rightElectricMirrorAnim = null!;

    private Coroutine dashboardSymbolPreStartup = null!;
    public GameObject parkingBrakeSymbol = null!;
    public GameObject checkEngineLightSymbol = null!;
    public GameObject alertLightSymbol = null!;
    public GameObject seatbeltLightSymbol = null!;
    public GameObject oilLevelLightSymbol = null!;
    public GameObject batteryLightSymbol = null!;
    public GameObject coolantLevelLightSymbol = null!;
    public GameObject dippedBeamLightSymbol = null!;
    public GameObject highBeamLightSymbol = null!;
    public Animator turnSignalAnimator = null!;

    public GameObject carKeyContainer = null!;
    public GameObject carKeyInHand = null!;
    public GameObject ignitionBarrel = null!;
    public Transform ignitionBarrelNotTurnedPosition = null!;
    public Transform ignitionBarrelTurnedPosition = null!;
    public Transform ignitionBarrelTryingPos = null!;
    public Transform ignitionTryingPosition = null!;

    public Animator headlightSwitchAnimator = null!;
    public MeshRenderer lowBeamMesh = null!;
    public MeshRenderer highBeamMesh = null!;
    public GameObject highBeamContainer = null!;
    public GameObject clusterLightsContainer = null!;

    public MeshRenderer radioMesh = null!;
    public MeshRenderer radioPowerDial = null!;
    public MeshRenderer radioVolumeDial = null!;
    public GameObject radioLight = null!;

    public TextMeshPro radioTime = null!;
    public TextMeshPro radioFrequency = null!;

    public GameObject sideLightsContainer = null!;
    public MeshRenderer sideTopLightsMesh = null!;

    public AnimationCurve oilPressureNeedleCurve = null!;
    public AnimationCurve turboPressureNeedleCurve = null!;

    public MeshRenderer interiorMesh = null!;
    public MeshRenderer speedometerMesh = null!;
    public MeshRenderer tachometerMesh = null!;
    public MeshRenderer oilPressureMesh = null!;

    public Transform speedometerTransform = null!;
    public Transform tachometerTransform = null!;
    public Transform oilPressureTransform = null!;

    public GameObject driverCameraNode = null!;
    public GameObject passengerCameraNode = null!;
    public GameObject driverCameraPositionNode = null!;
    public GameObject passengerCameraPositionNode = null!;

    public InteractTrigger startIgnitionTrigger = null!;
    public InteractTrigger stopIgnitionTrigger = null!;

    public GameObject reverseLightsContainer = null!;
    public MeshRenderer reverseLightsMesh = null!;

    public GameObject cabinFan = null!;
    public float cabFanSpeed;
    public bool cabFanOn;

    // ignition key stuff
    private Vector3 LHD_Pos_Local = new Vector3(0.0489f, 0.1371f, -0.1566f);
    private Vector3 LHD_Pos_Server = new Vector3(0.0366f, 0.1023f, -0.1088f);
    private Vector3 LHD_Rot_Local = new Vector3(-3.446f, 3.193f, 172.642f);
    private Vector3 LHD_Rot_Server = new Vector3(-191.643f, 174.051f, -7.768005f);

    private Vector3 keyContainerScale = new(0.06f, 0.06f, 0.06f);

    public float keyIsInTryIgnitionFirstSpeed = 0.028f;
    public float keyIsInTryIgnitionSecondSpeed = 0.1467f;
    public float keyIsNotInTryIgnitionFirstSpeed = 0.66f;
    public float keyIsNotInTryIgnitionSecondSpeed = 0.2f;
    public float ignitionRotSpeed = 45f;

    public int currentSweepStage;
    public bool hasSweepedDashboard;
    public bool hazardsOn;
    public bool leftSignalOn;
    public bool rightSignalOn;
    public bool reverseLightsOn;

    public float speedometerFloat;
    public float tachometerFloat;

    public bool lowBeamsOn;
    public bool lowBeamsOnBefore;
    public bool highBeamsOn;
    public bool highBeamsOnBefore;
    public bool fogLightsOn;
    public bool fogLightsOnBefore;
    public bool cabinLightSwitchEnabled;

    public float oilPressureFloat;
    public float turboPressureFloat;
    public bool overdriveSwitchEnabled;

    public bool liftGateOpen;
    public bool sideDoorOpen;

    [Header("Audio")]
    public AudioSource[] audiosToMute = null!;

    public AudioSource engineAudio3 = null!;
    public AudioSource cabinFanAudio = null!;
    public AudioSource CabinLightSwitchAudio = null!;
    public AudioClip CabinLightSwitchToggle = null!;
    public AudioSource carKeySounds = null!;
    public AudioSource wiperAudio = null!;

    private Coroutine truckAlarmCoroutine = null!;
    public AudioSource alarmSource = null!;
    public AudioClip alarmAudio = null!;

    public float timeSinceTogglingRadio;
    public bool alarmDebounce;
    public float timeAtLastAlarmPing;
    public bool carEngine3AudioActive;

    [Header("Radio")]
    public GameObject liveRadioScript = null!;
    public RadioBehaviour liveRadioController = null!;

    public float minFrequency = 75.55f;
    public float maxFrequency = 255.50f;

    public float timeLastSyncedRadio;
    public float radioPingTimestamp;

    [Header("Spring Seat")]
    public Animator passengerSeatSpringAnimator = null!;
    public AudioSource springSecondaryAudio = null!;
    public Animator ejectorButtonAnimator = null!;
    public AudioSource ejectorButtonAudio = null!;

    [Header("Materials")]
    public Material clusterDialsOffMat = null!;
    public Material clusterDialsOnMat = null!;
    public Material greyLightOffMat = null!;
    public Material redLightOffMat = null!;
    public Material clusterOffMaterial = null!;
    public Material clusterOnMaterial = null!;
    public Material radioOffMaterial = null!;
    public Material radioOnMaterial = null!;
    public Material windshieldMat = null!;


    // DOORS
    public new void SetBackDoorOpen(bool open)
    {
        liftGateOpen = open;
    }

    public void SideDoorOpen(bool open)
    {
        sideDoorOpen = open;
    }

    private new void SetFrontCabinLightOn(bool setOn)
    {
        if (cabinLightSwitchEnabled && !electricalShutdown)
        {
            frontCabinLightContainer.SetActive(setOn);
            frontCabinLightMesh.material = setOn ? headlightsOnMat : greyLightOffMat;
            return;
        }
        frontCabinLightContainer.SetActive(false);
        frontCabinLightMesh.material = greyLightOffMat;
    }

    public void SetCabinLightSwitchLocalClient()
    {
        cabinLightSwitchEnabled = !cabinLightSwitchEnabled;
        CabinLightSwitchAudio.PlayOneShot(CabinLightSwitchToggle);
        SetFrontCabinLightOn(keyIsInIgnition);
        SetCabinLightSwitchServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, cabinLightSwitchEnabled, keyIsInIgnition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCabinLightSwitchServerRpc(int playerWhoSent, bool switchState, bool isKeyInSlot)
    {
        SetCabinLightSwitchClientRpc(playerWhoSent, switchState, isKeyInSlot);
    }

    [ClientRpc]
    private void SetCabinLightSwitchClientRpc(int playerWhoSent, bool switchState, bool isKeyInSlot)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        cabinLightSwitchEnabled = switchState;
        CabinLightSwitchAudio.PlayOneShot(CabinLightSwitchToggle);
        SetFrontCabinLightOn(isKeyInSlot);
    }

    private new void SetWheelFriction()
    {
        WheelFrictionCurve forwardFrictionCurve = new WheelFrictionCurve
        {
            extremumSlip = 1f,
            extremumValue = 0.6f,
            asymptoteSlip = 0.8f,
            asymptoteValue = 0.5f,
            stiffness = baseForwardStiffness,
        };
        FrontRightWheel.forwardFriction = forwardFrictionCurve;
        FrontLeftWheel.forwardFriction = forwardFrictionCurve;
        BackRightWheel.forwardFriction = forwardFrictionCurve;
        BackLeftWheel.forwardFriction = forwardFrictionCurve;
        WheelFrictionCurve sidewaysFrictionCurve = new WheelFrictionCurve
        {
            extremumSlip = 0.7f,
            extremumValue = 1f,
            asymptoteSlip = 0.8f,
            asymptoteValue = 1f,
            stiffness = baseSidewaysStiffness,
        };
        FrontRightWheel.sidewaysFriction = sidewaysFrictionCurve;
        FrontLeftWheel.sidewaysFriction = sidewaysFrictionCurve;
        BackRightWheel.sidewaysFriction = sidewaysFrictionCurve;
        BackLeftWheel.sidewaysFriction = sidewaysFrictionCurve;
    }

    private void SetTruckStats()
    {
        idleSpeed = 80f; //18f && 90f
        pushForceMultiplier = 162f;

        turboBoostForce = 13500f; //3000f
        turboBoostUpwardForce = 32400f; //7200f

        steeringWheelTurnSpeed = baseSteeringWheelTurnSpeed;

        jumpForce = 3600f; //600f

        brakeSpeed = 10000f;
        maxBrakingPower = 12000f;

        speed = 60;
        stability = 0.4f;

        torqueForce = 2.5f;
        carMaxSpeed = 60f;
        pushVerticalOffsetAmount = 1.25f;

        baseCarHP = 45;

        if (!StartOfRound.Instance.inShipPhase)
        {
            carHP = baseCarHP;
        }

        //MinEngineRPM = 1000f;
        //MaxEngineRPM = 3000f;
        MinEngineRPM = 1000f;
        MaxEngineRPM = 5000f;
        engineIntensityPercentage = MaxEngineRPM;

        carAcceleration = 350f; //1900f //375f
        EngineTorque = 100f; //6100f
        engineModule.engineReversePower = 6100f;

        drivetrainModule.autoGear = TruckGearShift.Park;

        SetWheelFriction();

        mainRigidbody.maxLinearVelocity = carMaxSpeed;
        mainRigidbody.maxAngularVelocity = 4f;
        chanceToStartIgnition = (float)UnityEngine.Random.Range(12, 45);

        FrontLeftWheel.mass = 150f;
        FrontRightWheel.mass = 150f;
        BackLeftWheel.mass = 150f;
        BackRightWheel.mass = 150f;

        mainRigidbody.automaticCenterOfMass = false;
        mainRigidbody.centerOfMass = new Vector3(0f, -0.32f, 0.25f);
        mainRigidbody.automaticInertiaTensor = false;
        mainRigidbody.maxDepenetrationVelocity = 1f;

        JointSpring suspensionSpring = new JointSpring
        {
            spring = 32000f, //32000x2 = 64000
            damper = 2000f, //6000f
            targetPosition = 0.7f,
        };

        FrontRightWheel.suspensionSpring = suspensionSpring;
        FrontLeftWheel.suspensionSpring = suspensionSpring;
        BackRightWheel.suspensionSpring = suspensionSpring;
        BackLeftWheel.suspensionSpring = suspensionSpring;

        FrontRightWheel.wheelDampingRate = 8f;
        FrontLeftWheel.wheelDampingRate = 8f;
        BackRightWheel.wheelDampingRate = 8f;
        BackLeftWheel.wheelDampingRate = 8f;

        FrontRightWheel.suspensionDistance = 0.4f;
        FrontLeftWheel.suspensionDistance = 0.4f;
        BackRightWheel.suspensionDistance = 0.4f;
        BackLeftWheel.suspensionDistance = 0.4f;
    }

    public new void Awake()
    {
        base.Awake();

        References.truckController = this;
        wheels = new List<WheelCollider> {
            FrontLeftWheel,
            FrontRightWheel,
            BackLeftWheel,
            BackRightWheel };

        physicsRegion.priority = 1;
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;

        // set interior offsets
        driverSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;
        middlePassengerSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;
        passengerSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;

        backDoorOpen = true; // hacky shit
        SetTruckStats();
    }

    public new void Start()
    {
        SetupCarRainCollisions();
        FrontLeftWheel.brakeTorque = maxBrakingPower;
        FrontRightWheel.brakeTorque = maxBrakingPower;
        BackLeftWheel.brakeTorque = maxBrakingPower;
        BackRightWheel.brakeTorque = maxBrakingPower;
        overdriveSwitchEnabled = false;
        cabinLightSwitchEnabled = true;
        decals = new DecalProjector[24];

        if (!StartOfRound.Instance.inShipPhase)
            return;

        magnetedToShip = true;
        loadedVehicleFromSave = true;
        hasDeliveredVehicle = true;
        inDropshipAnimation = false;
        transform.position = StartOfRound.Instance.magnetPoint.position + StartOfRound.Instance.magnetPoint.forward * 7f;
        transform.rotation = Quaternion.Euler(new(0f, 90f, 0f));
        hasBeenSpawned = true;
        StartMagneting();
    }

    public void SetupCarRainCollisions()
    {
        if (References.rainParticles == null ||
            References.rainHitParticles == null ||
            References.stormyRainParticles == null ||
            References.stormyRainHitParticles == null)
        {
            Plugin.Logger.LogWarning($"rain particles are null! is application editor? {Application.isEditor}");
            if (Application.isEditor)
            {
                Plugin.Logger.LogDebug($"application is editor, this is ignorable");
            }
            return;
        }

        var particleTriggers = new[] {
            References.rainParticles.trigger,
            References.rainHitParticles.trigger,
            References.stormyRainParticles.trigger,
            References.stormyRainHitParticles.trigger};

        for (int i = 0; i < particleTriggers.Length; i++)
        {
            for (int j = 0; j < weatherEffectBlockers.Length; j++)
            {
                int index = particleTriggers[i].colliderCount + j;
                particleTriggers[i].SetCollider(index, weatherEffectBlockers[j]);
            }
        }
    }

    // completely broken and unused
    public void SendClientSyncData()
    {
        if (StartOfRound.Instance.attachedVehicle.TryGetComponent<CruiserXLController>(out var vehicle))
        {
            if (vehicle.turboBoosts > 0)
            {
                vehicle.AddTurboBoostClientRpc(0, vehicle.turboBoosts);
            }
            if (vehicle.ignitionStarted)
            {
                vehicle.StartIgnitionClientRpc(0);
            }
            if (vehicle.magnetedToShip)
            {
                Vector3 eulerAngles = vehicle.magnetTargetRotation.eulerAngles;
                vehicle.MagnetCarClientRpc(vehicle.magnetTargetPosition, eulerAngles, 0);

                vehicle.SyncCarEffectsClientRpc(steeringWheelAnimFloat);
                vehicle.SyncScanvanDataClientRpc(carHP);
            }
        }
    }

    [ClientRpc]
    public void SyncScanvanDataClientRpc(int carHealth)
    {
        if (IsHost)
            return;

        carHP = carHealth;
    }

    public new void StartTryCarIgnition()
    {
        if (!localPlayerInControl)
            return;

        if (ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        bool discharged = batteryCharge <= dischargedBattery;
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: true, discharged));
        TryIgnitionServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, keyIsInIgnition, discharged);
    }

    private IEnumerator TryIgnition(bool isLocalDriver, bool discharged)
    {
        StopCoroutine(jerkCarUpward(Vector3.zero));
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.ResetTrigger("SA_JumpInCar");
        jumpingInCar = false;
        if (keyIsInIgnition)
        {
            if (isLocalDriver)
            {
                if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 3)
                {
                    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 2);
                }
                else
                {
                    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 12);
                }
            }
            yield return new WaitForSeconds(0.02f);
            carKeySounds.PlayOneShot(twistKey);
            SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(keyIsInTryIgnitionSecondSpeed);
        }
        else
        {
            if (isLocalDriver)
            {
                GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 2);
            }
            SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: false);
            yield return new WaitForSeconds(0.6f);
            carKeySounds.PlayOneShot(insertKey);
            SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: true);
            if (!discharged)
            {
                SetFrontCabinLightOn(setOn: keyIsInIgnition);
                leftElectricMirrorAnim.SetBool("mirrorsFolded", !keyIsInIgnition);
                rightElectricMirrorAnim.SetBool("mirrorsFolded", !keyIsInIgnition);
                driversSideWindow.interactable = keyIsInIgnition;
                passengersSideWindow.interactable = keyIsInIgnition;
                if (dashboardSymbolPreStartup == null && !hasSweepedDashboard)
                {
                    dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
                }
            }
            yield return new WaitForSeconds(keyIsNotInTryIgnitionSecondSpeed);
            carKeySounds.PlayOneShot(twistKey);
            SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
        }
        SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);

        if (!isLocalDriver) yield break;
        if (discharged) yield break;
        if (!brakePedalPressed) yield break;
        if (truckAlarmCoroutine != null) yield break;

        engineAudio1.Stop();
        engineAudio1.clip = revEngineStart;
        engineAudio1.volume = 0.7f;
        engineAudio1.PlayOneShot(engineRev);
        carEngine1AudioActive = true;
        TryStartVehicleServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
        if (drivetrainModule.autoGear == TruckGearShift.Park)
        {
            float healthPercent = (float)carHP / baseCarHP;
            float baseChance = UnityEngine.Random.Range(68f, 78f);
            chanceToStartIgnition = baseChance * healthPercent;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.8f, 2f)); //0.4, 1.1f
            if ((float)UnityEngine.Random.Range(0, 100) < chanceToStartIgnition &&
                drivetrainModule.autoGear == TruckGearShift.Park) // no shifting out of park, cheeky
            {
                GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
                SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: true);
                SetIgnition(started: true);
                SetFrontCabinLightOn(setOn: keyIsInIgnition);
                CancelIgnitionAnimation(ignitionOn: true);
                StartIgnitionServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
            }
            else
            {
                chanceToStartIgnition = (float)UnityEngine.Random.Range(12, 22);
            }
        }
        else
        {
            chanceToStartIgnition = (float)UnityEngine.Random.Range(12, 22);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryIgnitionServerRpc(int driverId, bool setKeyInSlot, bool discharged)
    {
        TryIgnitionClientRpc(driverId, setKeyInSlot, discharged);
    }

    [ClientRpc]
    public void TryIgnitionClientRpc(int driverId, bool setKeyInSlot, bool discharged)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == driverId)
            return;

        if (ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: setKeyInSlot);
        if (!discharged)
            SetFrontCabinLightOn(setKeyInSlot);
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: false, discharged));
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryStartVehicleServerRpc(int driverId)
    {
        TryStartVehicleClientRpc(driverId);
    }

    [ClientRpc]
    public void TryStartVehicleClientRpc(int driverId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == driverId)
            return;

        engineAudio1.Stop();
        engineAudio1.clip = revEngineStart;
        engineAudio1.volume = 0.7f;
        engineAudio1.PlayOneShot(engineRev);
        carEngine1AudioActive = true;
    }

    public new void CancelTryCarIgnition()
    {
        if (!localPlayerInControl)
            return;

        if (ignitionStarted)
            return;

        // hopefully fix a bug where the wrong animation can play?
        if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 2 &&
            keyIsInIgnition)
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 3);
        else if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 12)
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 3);
        else
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);

        bool discharged = batteryCharge <= dischargedBattery;
        CancelIgnitionAnimation(ignitionOn: false);
        if (!discharged) SetFrontCabinLightOn(setOn: keyIsInIgnition);
        CancelTryIgnitionServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, keyIsInIgnition, discharged);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CancelTryIgnitionServerRpc(int driverId, bool setKeyInSlot, bool discharged)
    {
        CancelTryIgnitionClientRpc(driverId, setKeyInSlot, discharged);
    }

    [ClientRpc]
    public void CancelTryIgnitionClientRpc(int driverId, bool setKeyInSlot, bool discharged)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == driverId)
            return;

        // account for netlag when the key is first inserted
        if (setKeyInSlot == true && (keyIsInIgnition != setKeyInSlot))
        {
            carKeySounds.PlayOneShot(insertKey);
            if (!discharged)
            {
                if (dashboardSymbolPreStartup == null && !hasSweepedDashboard)
                {
                    dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
                }
                leftElectricMirrorAnim.SetBool("mirrorsFolded", !setKeyInSlot);
                rightElectricMirrorAnim.SetBool("mirrorsFolded", !setKeyInSlot);
            }
        }
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: setKeyInSlot);
        if (!discharged)
        {
            driversSideWindow.interactable = keyIsInIgnition;
            passengersSideWindow.interactable = keyIsInIgnition;
            SetFrontCabinLightOn(setOn: keyIsInIgnition);
        }
        CancelIgnitionAnimation(ignitionOn: false);
    }

    [ServerRpc(RequireOwnership = false)]
    public new void StartIgnitionServerRpc(int driverId)
    {
        StartIgnitionClientRpc(driverId);

    }

    [ClientRpc]
    public new void StartIgnitionClientRpc(int driverId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == driverId)
            return;

        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: true);
        SetIgnition(started: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        CancelIgnitionAnimation(ignitionOn: true);
    }

    public new void SetIgnition(bool started)
    {
        SetFrontCabinLightOn(keyIsInIgnition);
        if (started == ignitionStarted)
        {
            return;
        }
        ignitionStarted = started;
        carEngine1AudioActive = started;
        if (started)
        {
            startKeyIgnitionTrigger.SetActive(false);
            removeKeyIgnitionTrigger.SetActive(true);
            cabFanOn = true;
            carExhaustParticle.Play();
            engineAudio1.Stop();
            engineAudio1.PlayOneShot(engineStartSuccessful);
            engineAudio1.clip = engineRun;
            return;
        }
        startKeyIgnitionTrigger.SetActive(true);
        removeKeyIgnitionTrigger.SetActive(false);
        cabFanOn = false;
        voiceModule.hasPlayedIgnitionChime = false;
        carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public new void RemoveKeyFromIgnition()
    {
        if (currentDriver != null && !localPlayerInControl)
            return;

        if (!ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(RemoveKey((int)GameNetworkManager.Instance.localPlayerController.playerClientId));
        RemoveKeyFromIgnitionServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);

        if (!localPlayerInControl)
            return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 6);
        if (!UserConfig.AutoSwitchToParked.Value)
            return;

        int expectedGear = (int)TruckGearShift.Park;
        if ((int)drivetrainModule.autoGear != expectedGear)
        {
            ShiftToGearAndSync(expectedGear);
        }
    }

    private IEnumerator RemoveKey(int playerWhoSent)
    {
        yield return new WaitForSeconds(0.26f);
        if (dashboardSymbolPreStartup != null)
        {
            StopCoroutine(dashboardSymbolPreStartup);
            dashboardSymbolPreStartup = null!;
            StopPreIgnitionChecks();
        }
        carKeySounds.PlayOneShot(removeKey);
        SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: false);
        SetIgnition(started: false);
        leftElectricMirrorAnim.SetBool("mirrorsFolded", true);
        rightElectricMirrorAnim.SetBool("mirrorsFolded", true);
        driversSideWindow.interactable = false;
        passengersSideWindow.interactable = false;
        yield return new WaitForSeconds(0.73f);
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: false);
        keyIgnitionCoroutine = null;
    }

    [ServerRpc(RequireOwnership = false)]
    public new void RemoveKeyFromIgnitionServerRpc(int driverId)
    {
        RemoveKeyFromIgnitionClientRpc(driverId);
    }

    [ClientRpc]
    public new void RemoveKeyFromIgnitionClientRpc(int driverId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == driverId)
            return;

        if (!ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(RemoveKey(driverId));
    }

    // there will be a better way to go about this
    private IEnumerator PreIgnitionSymbolCheck()
    {
        parkingBrakeSymbol.SetActive(true);
        checkEngineLightSymbol.SetActive(true);
        alertLightSymbol.SetActive(true);
        seatbeltLightSymbol.SetActive(true);
        dippedBeamLightSymbol.SetActive(true);
        highBeamLightSymbol.SetActive(true);
        oilLevelLightSymbol.SetActive(true);
        batteryLightSymbol.SetActive(true);
        coolantLevelLightSymbol.SetActive(true);
        currentSweepStage = 1;
        yield return new WaitForSeconds(1.0f);

        dippedBeamLightSymbol.SetActive(lowBeamsOn);
        highBeamLightSymbol.SetActive(highBeamsOn);
        currentSweepStage = 2;
        yield return new WaitForSeconds(1.0f);

        seatbeltLightSymbol.SetActive(false);
        parkingBrakeSymbol.SetActive(drivetrainModule.autoGear == TruckGearShift.Park);
        currentSweepStage = 3;
        yield return new WaitForSeconds(1.0f);

        oilLevelLightSymbol.SetActive(carHP <= 15);
        batteryLightSymbol.SetActive(batteryCharge < 0.62);
        coolantLevelLightSymbol.SetActive(carHP <= 19);
        alertLightSymbol.SetActive(carHP <= 12);
        checkEngineLightSymbol.SetActive(carHP <= 21);
        currentSweepStage = 4;
        hasSweepedDashboard = true;
    }

    private void StopPreIgnitionChecks()
    {
        currentSweepStage = 0;
        hasSweepedDashboard = false;
        parkingBrakeSymbol.SetActive(false);
        checkEngineLightSymbol.SetActive(false);
        alertLightSymbol.SetActive(false);
        seatbeltLightSymbol.SetActive(false);
        dippedBeamLightSymbol.SetActive(false);
        highBeamLightSymbol.SetActive(false);
        oilLevelLightSymbol.SetActive(false);
        batteryLightSymbol.SetActive(false);
        coolantLevelLightSymbol.SetActive(false);
    }

    public void CancelIgnitionAnimation(bool ignitionOn)
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
            keyIgnitionCoroutine = null;
        }
        keyIsInDriverHand = false;
        engineModule.tryingIgnition = false;
        carEngine1AudioActive = ignitionOn;
    }

    public void SetKeyIgnitionValues(bool trying, bool keyInHand, bool keyInSlot)
    {
        engineModule.tryingIgnition = trying;
        keyIsInDriverHand = keyInHand;
        keyIsInIgnition = keyInSlot;
    }




    public void ResetTruckVelocityTimer()
    {
        if (averageVelocity.magnitude < 3f) limitTruckVelocityTimer = 0.7f;
    }

    public void SetTriggerHoverTip(InteractTrigger trigger, string tip)
    {
        trigger.hoverTip = tip;
    }

    public void SetDriverInCar()
    {
        if (currentDriver != null)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            HUDManager.Instance.DisplayTip("Seat occupied",
                "You cannot enter an occupied seat! aborting!");
            return;
        }
        SetDriverInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDriverInCarServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentDriver != null ||
            (currentDriver != null && currentDriver != playerController))
        {
            CancelSetPlayerInVehicleClientRpc(playerId);
            return;
        }
        ResetTruckVelocityTimer();
        currentDriver = StartOfRound.Instance.allPlayerScripts[playerId];
        NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[playerId].actualClientId);
        SetDriverInCarClientRpc(playerId, batteryCharge);
    }

    [ClientRpc]
    public void SetDriverInCarClientRpc(int playerId, float charge)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId)
        {
            SetVehicleCollisionForPlayer(setEnabled: false, GameNetworkManager.Instance.localPlayerController);
            InteractTriggerPatches.specialInteractCoroutine =
                StartCoroutine(InteractTriggerPatches.SpecialTruckInteractAnimation(
                    trigger: driverSeatTrigger,
                    playerController: GameNetworkManager.Instance.localPlayerController,
                    controller: this,
                    isPassenger: false));

            ResetTruckVelocityTimer();
            ActivateControl();
            SetTriggerHoverTip(driverSideDoorTrigger, "Exit : [LMB]");
            batteryCharge = charge;
            startIgnitionTrigger.isBeingHeldByPlayer = false;
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
            if (keyIsInIgnition) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
            if (ignitionStarted) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
            if (driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
            return;
        }
        ResetTruckVelocityTimer();
        currentDriver = StartOfRound.Instance.allPlayerScripts[playerId];
        SetVehicleCollisionForPlayer(setEnabled: false, currentDriver);
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        PlayerUtils.ReplaceClientPlayerAnimator(playerId);
    }

    public new void ExitDriverSideSeat()
    {
        if (!localPlayerInControl)
            return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        if (!driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        int exitPoint = CanExitCar(passengerSide: false);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(driverSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(driverSideExitPoints[1].position);
    }

    public void OnDriverExit()
    {
        //SetVehicleCollisionForPlayer(setEnabled: true, GameNetworkManager.Instance.localPlayerController);
        ResetTruckVelocityTimer();
        localPlayerInControl = false;
        DisableVehicleCollisionForAllPlayers();
        SetTriggerHoverTip(driverSideDoorTrigger, "Use door : [LMB]");
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        PlayerUtils.ResetHUDToolTips(GameNetworkManager.Instance.localPlayerController);
        if (currentDriver != GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.DisplayTip("Err?",
                "This state should not occur! aborting!");
            return;
        }
        DisableControl();
        CancelIgnitionAnimation(ignitionOn: ignitionStarted);
        SetIgnition(started: ignitionStarted);
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
        OnDriverExitServerRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            transform.position,
            transform.rotation,
            ignitionStarted,
            keyIsInIgnition,
            false,
            false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnDriverExitServerRpc(int playerId, Vector3 carLocation, Quaternion carRotation, bool setIgnitionState, bool setKeyInSlot, bool gasFloored, bool brakeFloored)
    {
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        drivePedalPressed = gasFloored;
        brakePedalPressed = brakeFloored;
        currentDriver = null;
        NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
        OnDriverExitClientRpc(playerId, carLocation, carRotation, setIgnitionState, setKeyInSlot, gasFloored, brakeFloored);
    }

    [ClientRpc]
    public void OnDriverExitClientRpc(int playerId, Vector3 carLocation, Quaternion carRotation, bool setIgnitionState, bool setKeyInSlot, bool gasFloored, bool brakeFloored)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId)
            return;

        ResetTruckVelocityTimer();
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        drivePedalPressed = gasFloored;
        brakePedalPressed = brakeFloored;
        currentDriver = null;
        keyIsInIgnition = setKeyInSlot;
        ignitionStarted = setIgnitionState;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        PlayerUtils.ReturnClientPlayerAnimator(playerId);
        CancelIgnitionAnimation(ignitionOn: ignitionStarted);
        SetIgnition(started: ignitionStarted);
        if (localPlayerInPassengerSeat || localPlayerInMiddlePassengerSeat)
            SetVehicleCollisionForPlayer(setEnabled: false, GameNetworkManager.Instance.localPlayerController);
    }




    public void SetMiddlePassengerInCar()
    {
        if (currentMiddlePassenger != null)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            HUDManager.Instance.DisplayTip("Seat occupied",
                "You cannot enter an occupied seat! aborting!");
            return;
        }
        SetMiddlePassengerInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetMiddlePassengerInCarServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentMiddlePassenger != null ||
            (currentMiddlePassenger != null && currentMiddlePassenger != playerController))
        {
            CancelSetPlayerInVehicleClientRpc(playerId);
            return;
        }
        currentMiddlePassenger = StartOfRound.Instance.allPlayerScripts[playerId];
        SetMiddlePassengerInCarClientRpc(playerId);
    }

    [ClientRpc]
    public void SetMiddlePassengerInCarClientRpc(int playerId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId)
        {
            SetVehicleCollisionForPlayer(setEnabled: false, GameNetworkManager.Instance.localPlayerController);
            InteractTriggerPatches.specialInteractCoroutine =
                StartCoroutine(InteractTriggerPatches.SpecialTruckInteractAnimation(
                    trigger: middlePassengerSeatTrigger,
                    playerController: GameNetworkManager.Instance.localPlayerController,
                    controller: this,
                    isPassenger: true));

            currentMiddlePassenger = GameNetworkManager.Instance.localPlayerController;
            localPlayerInMiddlePassengerSeat = true;
            SetTriggerHoverTip(passengerSideDoorTrigger, "Exit : [LMB]");
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
            if (passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
            return;
        }
        currentMiddlePassenger = StartOfRound.Instance.allPlayerScripts[playerId];
        SetVehicleCollisionForPlayer(setEnabled: false, currentMiddlePassenger);
    }

    public void ExitMiddlePassengerSideSeat()
    {
        if (!localPlayerInMiddlePassengerSeat)
            return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        if (!passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        int exitPoint = CanExitCar(passengerSide: true);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[1].position);
    }

    public void OnMiddlePassengerExit()
    {
        SetVehicleCollisionForPlayer(setEnabled: true, GameNetworkManager.Instance.localPlayerController);
        localPlayerInMiddlePassengerSeat = false;
        SetTriggerHoverTip(passengerSideDoorTrigger, "Use door : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        PlayerUtils.ResetHUDToolTips(GameNetworkManager.Instance.localPlayerController);
        if (currentMiddlePassenger != GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.DisplayTip("Err?",
                "This state should not occur! aborting!");
            return;
        }
        currentMiddlePassenger = null!;
        OnMiddlePassengerExitServerRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            GameNetworkManager.Instance.localPlayerController.transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnMiddlePassengerExitServerRpc(int playerId, Vector3 exitPoint)
    {
        OnMiddlePassengerExitClientRpc(playerId, exitPoint);
    }

    [ClientRpc]
    public void OnMiddlePassengerExitClientRpc(int playerId, Vector3 exitPoint)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == GameNetworkManager.Instance.localPlayerController)
            return;
        playerController.TeleportPlayer(exitPoint, false, 0f, false, true);
        currentMiddlePassenger = null!;
        if (!base.IsOwner)
        {
            SetVehicleCollisionForPlayer(setEnabled: true, GameNetworkManager.Instance.localPlayerController);
        }
    }




    public void SetPassengerInCar()
    {
        if (currentPassenger != null)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            HUDManager.Instance.DisplayTip("Seat occupied",
                "You cannot enter an occupied seat! aborting!");
            return;
        }
        SetPassengerInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPassengerInCarServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentPassenger != null ||
            (currentPassenger != null && currentPassenger != playerController))
        {
            CancelSetPlayerInVehicleClientRpc(playerId);
            return;
        }
        currentPassenger = StartOfRound.Instance.allPlayerScripts[playerId];
        SetPassengerInCarClientRpc(playerId);
    }

    [ClientRpc]
    public void SetPassengerInCarClientRpc(int playerId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId)
        {
            SetVehicleCollisionForPlayer(setEnabled: false, GameNetworkManager.Instance.localPlayerController);
            InteractTriggerPatches.specialInteractCoroutine =
                StartCoroutine(InteractTriggerPatches.SpecialTruckInteractAnimation(
                    trigger: passengerSeatTrigger,
                    playerController: GameNetworkManager.Instance.localPlayerController,
                    controller: this,
                    isPassenger: true));

            currentPassenger = GameNetworkManager.Instance.localPlayerController;
            localPlayerInPassengerSeat = true;
            SetTriggerHoverTip(passengerSideDoorTrigger, "Exit : [LMB]");
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
            if (passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
            return;
        }
        currentPassenger = StartOfRound.Instance.allPlayerScripts[playerId];
        SetVehicleCollisionForPlayer(setEnabled: false, currentPassenger);
    }

    public new void ExitPassengerSideSeat()
    {
        if (!localPlayerInPassengerSeat)
            return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        if (!passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        int exitPoint = CanExitCar(passengerSide: true);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[1].position);
    }

    public new void OnPassengerExit()
    {
        SetVehicleCollisionForPlayer(setEnabled: true, GameNetworkManager.Instance.localPlayerController);
        localPlayerInPassengerSeat = false;
        SetTriggerHoverTip(passengerSideDoorTrigger, "Use door : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        PlayerUtils.ResetHUDToolTips(GameNetworkManager.Instance.localPlayerController);
        if (currentPassenger != GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.DisplayTip("Err?",
                "This state should not occur! aborting!");
            return;
        }
        currentPassenger = null!;
        OnPassengerExitServerRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            GameNetworkManager.Instance.localPlayerController.transform.position);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnPassengerExitServerRpc(int playerId, Vector3 exitPoint)
    {
        OnPassengerExitClientRpc(playerId, exitPoint);
    }

    [ClientRpc]
    public void OnPassengerExitClientRpc(int playerId, Vector3 exitPoint)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == GameNetworkManager.Instance.localPlayerController)
            return;
        playerController.TeleportPlayer(exitPoint, false, 0f, false, true);
        currentPassenger = null!;
        if (!base.IsOwner)
        {
            SetVehicleCollisionForPlayer(setEnabled: true, GameNetworkManager.Instance.localPlayerController);
        }
    }



    [ClientRpc]
    public void CancelSetPlayerInVehicleClientRpc(int playerId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId != playerId)
            return;

        HUDManager.Instance.DisplayTip("Kicked from vehicle",
            "You have been forcefully kicked to prevent a softlock!");
    }

    private new int CanExitCar(bool passengerSide)
    {
        if (!passengerSide)
        {
            for (int i = 0; i < driverSideExitPoints.Length; i++)
            {
                if (!CheckExitPointInvalid(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, driverSideExitPoints[i].position, exitCarLayerMask, QueryTriggerInteraction.Ignore))
                {
                    return i;
                }
            }
            return -1;
        }
        for (int j = 0; j < passengerSideExitPoints.Length; j++)
        {
            if (!CheckExitPointInvalid(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, passengerSideExitPoints[j].position, exitCarLayerMask, QueryTriggerInteraction.Ignore))
            {
                return j;
            }
        }
        return -1;
    }

    public bool CheckExitPointInvalid(Vector3 playerPos, Vector3 exitPoint, int layerMask, QueryTriggerInteraction interaction)
    {
        if (Physics.Linecast(playerPos, exitPoint, layerMask, interaction))
        {
            return true;
        }

        if (Physics.CheckCapsule(exitPoint, exitPoint + Vector3.up, 0.5f, layerMask, interaction))
        {
            return true;
        }

        LayerMask maskAndVehicle = layerMask | LayerMask.GetMask("Vehicle");

        if (!Physics.Linecast(exitPoint, exitPoint + Vector3.down * 4f, maskAndVehicle, interaction))
        {
            return true;
        }

        return false;
    }




    public new void EnableVehicleCollisionForAllPlayers()
    {
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            if (StartOfRound.Instance.allPlayerScripts[i] != currentPassenger && StartOfRound.Instance.allPlayerScripts[i] != currentMiddlePassenger)
            {
                StartOfRound.Instance.allPlayerScripts[i].GetComponent<CharacterController>().excludeLayers = 0;
            }
        }
    }

    public new void DisableVehicleCollisionForAllPlayers()
    {
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            if (!localPlayerInControl &&
                !localPlayerInMiddlePassengerSeat &&
                !localPlayerInPassengerSeat &&
                StartOfRound.Instance.allPlayerScripts[i] == GameNetworkManager.Instance.localPlayerController)
            {
                StartOfRound.Instance.allPlayerScripts[i].GetComponent<CharacterController>().excludeLayers = 0;
            }
            else
            {
                StartOfRound.Instance.allPlayerScripts[i].GetComponent<CharacterController>().excludeLayers = 1073741824;
            }
        }
    }

    public new void SetVehicleCollisionForPlayer(bool setEnabled, PlayerControllerB player)
    {
        if (!setEnabled)
        {
            player.GetComponent<CharacterController>().excludeLayers = 1073741824;
            return;
        }
        player.GetComponent<CharacterController>().excludeLayers = 0;
    }




    private new void ActivateControl()
    {
        InputActionAsset inputActionAsset = IngamePlayerSettings.Instance.playerInput.actions;
        inputActionAsset.FindAction("Jump").performed += DoTurboBoost;

        Plugin.VehicleControlsInstance.JumpKey.performed += DoJump;
        Plugin.VehicleControlsInstance.GearShiftForwardKey.performed += ShiftGearForwardInput;
        Plugin.VehicleControlsInstance.GearShiftBackwardKey.performed += ShiftGearBackInput;
        Plugin.VehicleControlsInstance.ToggleHeadlightsKey.performed += ActivateHeadlights;
        Plugin.VehicleControlsInstance.ToggleWipersKey.performed += ActivateWipers;
        Plugin.VehicleControlsInstance.ActivateHornKey.performed += ActivateHorn;
        Plugin.VehicleControlsInstance.ActivateHornKey.canceled += ActivateHorn;
        //Plugin.VehicleControlsInstance.ToggleMagnetKey.performed += ActivateMagnet;

        currentDriver = GameNetworkManager.Instance.localPlayerController;
        localPlayerInControl = true;
        centerKeyPressed = false;
    }

    private new void DisableControl()
    {
        InputActionAsset inputActionAsset = IngamePlayerSettings.Instance.playerInput.actions;
        inputActionAsset.FindAction("Jump").performed -= DoTurboBoost;

        Plugin.VehicleControlsInstance.JumpKey.performed -= DoJump;
        Plugin.VehicleControlsInstance.GearShiftForwardKey.performed -= ShiftGearForwardInput;
        Plugin.VehicleControlsInstance.GearShiftBackwardKey.performed -= ShiftGearBackInput;
        Plugin.VehicleControlsInstance.ToggleHeadlightsKey.performed -= ActivateHeadlights;
        Plugin.VehicleControlsInstance.ToggleWipersKey.performed -= ActivateWipers;
        Plugin.VehicleControlsInstance.ActivateHornKey.performed -= ActivateHorn;
        Plugin.VehicleControlsInstance.ActivateHornKey.canceled -= ActivateHorn;
        //Plugin.VehicleControlsInstance.ToggleMagnetKey.performed -= ActivateMagnet;

        currentDriver = null;
        localPlayerInControl = false;
        steeringAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
        centerKeyPressed = false;
    }




    public new void ShiftGearForwardInput(InputAction.CallbackContext context)
    {
        if (!localPlayerInControl)
            return;

        if (!context.performed)
            return;

        if (Time.realtimeSinceStartup - timeSinceTogglingRadio < 0.4f)
            return;

        if (Time.realtimeSinceStartup - timeAtLastGearShift < 0.15f)
            return;

        ShiftGearForward();
    }

    public new void ShiftGearBackInput(InputAction.CallbackContext context)
    {
        if (!localPlayerInControl)
            return;

        if (!context.performed)
            return;

        if (Time.realtimeSinceStartup - timeSinceTogglingRadio < 0.4f)
            return;

        if (Time.realtimeSinceStartup - timeAtLastGearShift < 0.15f)
            return;

        ShiftGearBack();
    }

    public new void ShiftGearForward()
    {
        if (drivetrainModule.autoGear != TruckGearShift.Park)
        {
            if (drivetrainModule.autoGear == TruckGearShift.Reverse)
            {
                ShiftToGearAndSync(4);
            }
            else if (drivetrainModule.autoGear == TruckGearShift.Neutral)
            {
                ShiftToGearAndSync(3);
            }
            else if (drivetrainModule.autoGear == TruckGearShift.Drive)
            {
                ShiftToGearAndSync(2);
            }
        }
    }

    private new void ShiftGearBack()
    {
        if (drivetrainModule.autoGear != TruckGearShift.Drive)
        {
            if (drivetrainModule.autoGear == TruckGearShift.Park)
            {
                ShiftToGearAndSync(3);
            }
            else if (drivetrainModule.autoGear == TruckGearShift.Reverse)
            {
                ShiftToGearAndSync(2);
            }
            else if (drivetrainModule.autoGear == TruckGearShift.Neutral)
            {
                ShiftToGearAndSync(1);
            }
        }
    }

    public new void ShiftToGearAndSync(int setGear)
    {
        if (Time.realtimeSinceStartup - timeSinceTogglingRadio < 0.4f)
            return;

        if (drivetrainModule.autoGear == TruckGearShift.Park && setGear != 4 && 
            !brakePedalPressed && !UserConfig.AutoSwitchFromParked.Value)
            return;

        if (drivetrainModule.autoGear == (TruckGearShift)setGear)
            return;

        if (drivetrainModule.autoGear == TruckGearShift.Park &&
            electricalShutdown) return;

        timeAtLastGearShift = Time.realtimeSinceStartup;
        drivetrainModule.autoGear = (TruckGearShift)setGear;
        gearStickAudio.PlayOneShot(gearStickAudios[setGear - 1]);
        ShiftToGearServerRpc(setGear, (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public new void ShiftToGearServerRpc(int setGear, int playerId)
    {
        ShiftToGearClientRpc(setGear, playerId);
    }

    [ClientRpc]
    public new void ShiftToGearClientRpc(int setGear, int playerId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId)
            return;

        timeAtLastGearShift = Time.realtimeSinceStartup;
        drivetrainModule.autoGear = (TruckGearShift)setGear;
        gearStickAudio.PlayOneShot(gearStickAudios[setGear - 1]);
    }




    private new void GetVehicleInput()
    {
        if (currentDriver == null) return;
        if (currentDriver.isTypingChat || currentDriver.quickMenuManager.isMenuOpen) return;

        // figure out a better solution for this
        if (syncedDrivePedalPressed != drivePedalPressed ||
            syncedBrakePedalPressed != brakePedalPressed)
        {
            syncedDrivePedalPressed = drivePedalPressed;
            syncedBrakePedalPressed = brakePedalPressed;
            SyncDriverPedalInputsServerRpc(drivePedalPressed, brakePedalPressed);
        }

        moveInputVector = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        moveInputVector.x = Mathf.Round(moveInputVector.x);
        moveInputVector.y = Mathf.Round(moveInputVector.y);
        steeringAnimValue = ignitionStarted ? moveInputVector.x : 0f;
        brakePedalPressed = Plugin.VehicleControlsInstance.BrakePedalKey.IsPressed();

        if (!ignitionStarted)
            return;

        // needs reworking and reconsideration
        engineModule.throttleInput = drivePedalPressed ? Mathf.MoveTowards(engineModule.throttleInput, 1f, 8f * Time.deltaTime) : Mathf.MoveTowards(engineModule.throttleInput, 0f, 8f * Time.deltaTime);
        int targetDirection = 0;
        if (Plugin.VehicleControlsInstance.GasPedalKey.IsPressed())
        {
            drivePedalPressed = true;
        }
        else if (Plugin.VehicleControlsInstance.MoveForwardsKey.IsPressed())
        {
            targetDirection = 1;
            drivePedalPressed = true;
        }
        else if (Plugin.VehicleControlsInstance.MoveBackwardsKey.IsPressed())
        {
            targetDirection = 2;
            drivePedalPressed = true;
        }
        else
        {
            drivePedalPressed = false;
        }

        if (drivePedalPressed && (
            (UserConfig.AutoSwitchFromParked.Value && drivetrainModule.autoGear == TruckGearShift.Park) ||
            (UserConfig.AutoSwitchDriveReverse.Value && drivetrainModule.autoGear != TruckGearShift.Park && targetDirection != 0)
        ))
        {
            int expectedGear = targetDirection != 2 ? (int)TruckGearShift.Drive : (int)TruckGearShift.Reverse;
            if ((int)drivetrainModule.autoGear != expectedGear)
            {
                ShiftToGearAndSync(expectedGear);
            }
        }

        if (moveInputVector.x == 0f && UserConfig.RecenterWheel.Value)
        {
            if (UserConfig.RecenterWheelSpeed.Value < 0f)
            {
                steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, 0, steeringWheelTurnSpeed * Time.deltaTime / 6f);
            }
            else if (UserConfig.RecenterWheelSpeed.Value > 0f)
            {
                steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, 0, UserConfig.RecenterWheelSpeed.Value * Time.deltaTime / 6f);
            }
            else
            {
                steeringAnimValue = moveInputVector.x;
                steeringWheelAnimFloat = steeringAnimValue;
            }
        }
    }

    public new void StartMagneting()
    {
        mainRigidbody.isKinematic = true;
        magnetTime = 0f;
        magnetRotationTime = 0f;
        StartOfRound.Instance.isObjectAttachedToMagnet = true;
        StartOfRound.Instance.attachedVehicle = this;
        magnetedToShip = true;
        averageVelocityAtMagnetStart = averageVelocity;
        RoundManager.Instance.tempTransform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);

        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.y = Mathf.Round((eulerAngles.y + 90f) / 180f) * 180f - 90f;
        eulerAngles.z = Mathf.Round(eulerAngles.z / 90f) * 90f;
        float x = Mathf.Repeat(eulerAngles.x + UnityEngine.Random.Range(-5f, 5f) + 180, 360) - 180;
        eulerAngles.x = Mathf.Clamp(x, -20f, 20f);
        magnetTargetRotation = Quaternion.Euler(eulerAngles);

        Vector3 offset = new(0f, -0.5f, -boundsCollider.size.x * 0.5f * boundsCollider.transform.lossyScale.x);
        Vector3 localPos = StartOfRound.Instance.magnetPoint.position + offset;
        magnetTargetPosition = StartOfRound.Instance.elevatorTransform.InverseTransformPoint(localPos);

        magnetStartPosition = transform.position;
        magnetStartRotation = transform.rotation;

        Quaternion rotation = magnetTargetRotation;
        transform.rotation = rotation;

        CollectItemsInTruck();

        if (StartOfRound.Instance.inShipPhase)
        {
            return;
        }
        if (GameNetworkManager.Instance.localPlayerController == null)
        {
            return;
        }
        if (!base.IsOwner)
        {
            return;
        }
        MagnetCarServerRpc(magnetTargetPosition, eulerAngles, (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public new void MagnetCarServerRpc(Vector3 targetPosition, Vector3 targetRotation, int playerWhoSent)
    {
        MagnetCarClientRpc(targetPosition, targetRotation, playerWhoSent);
    }

    [ClientRpc]
    public new void MagnetCarClientRpc(Vector3 targetPosition, Vector3 targetRotation, int playerWhoSent)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        magnetedToShip = true;
        magnetTime = 0f;
        magnetRotationTime = 0f;
        StartOfRound.Instance.isObjectAttachedToMagnet = true;
        StartOfRound.Instance.attachedVehicle = this;
        magnetStartPosition = transform.position;
        magnetStartRotation = transform.rotation;
        magnetTargetPosition = targetPosition;
        magnetTargetRotation = Quaternion.Euler(targetRotation);
        CollectItemsInTruck();
    }

    public new void CollectItemsInTruck()
    {
        Collider[] array = Physics.OverlapSphere(transform.position, 25f, 64, QueryTriggerInteraction.Collide);
        for (int i = 0; i < array.Length; i++)
        {
            GrabbableObject component = array[i].GetComponent<GrabbableObject>();
            if (component != null &&
                !component.isHeld &&
                !component.isHeldByEnemy &&
                array[i].transform.parent == transform)
            {
                // only credit the last driver (credit to buttery for figuring this out!)
                if (References.lastDriver != null)
                {
                    References.lastDriver.SetItemInElevator(magnetedToShip, magnetedToShip, component);
                }
                else if (References.lastDriver == null && GameNetworkManager.Instance.localPlayerController != null)
                {
                    GameNetworkManager.Instance.localPlayerController?.SetItemInElevator(magnetedToShip, magnetedToShip, component);
                }
            }
        }
    }

    public new void AddEngineOil()
    {
        int num = Mathf.Min(carHP + 4, baseCarHP);
        AddEngineOilOnLocalClient(num);
        AddEngineOilServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, num);
    }

    public new void AddEngineOilOnLocalClient(int setCarHP)
    {
        hoodAudio.PlayOneShot(pourOil);
        carHP = setCarHP;
    }

    [ServerRpc(RequireOwnership = false)]
    public new void AddEngineOilServerRpc(int playerId, int setHP)
    {
        AddEngineOilClientRpc(playerId, setHP);
    }

    [ClientRpc]
    public new void AddEngineOilClientRpc(int playerId, int setHP)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId != playerId)
        {
            AddEngineOilOnLocalClient(setHP);
        }
    }

    public new void AddTurboBoost()
    {
        int setTurboBoosts = Mathf.Min(turboBoosts + 1, 5);
        AddTurboBoostOnLocalClient(setTurboBoosts);
        AddTurboBoostServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, setTurboBoosts);
    }

    public new void AddTurboBoostOnLocalClient(int setTurboBoosts)
    {
        hoodAudio.PlayOneShot(pourTurbo);
        turboBoosts = setTurboBoosts;
    }

    [ServerRpc(RequireOwnership = false)]
    public new void AddTurboBoostServerRpc(int playerId, int setTurboBoosts)
    {
        AddTurboBoostClientRpc(playerId, setTurboBoosts);
    }

    [ClientRpc]
    public new void AddTurboBoostClientRpc(int playerId, int setTurboBoosts)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId != playerId)
        {
            AddTurboBoostOnLocalClient(setTurboBoosts);
        }
    }

    public void SetOverdriveSwitchLocalClient()
    {
        overdriveSwitchEnabled = !overdriveSwitchEnabled;
        CabinLightSwitchAudio.PlayOneShot(CabinLightSwitchToggle);
        SetOverdriveSwitchServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, overdriveSwitchEnabled);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetOverdriveSwitchServerRpc(int playerWhoSent, bool switchState)
    {
        SetOverdriveSwitchClientRpc(playerWhoSent, switchState);
    }

    [ClientRpc]
    private void SetOverdriveSwitchClientRpc(int playerWhoSent, bool switchState)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        overdriveSwitchEnabled = switchState;
        CabinLightSwitchAudio.PlayOneShot(CabinLightSwitchToggle);
    }

    // i need to re-write all this boost ability stuff
    public void DoJump(InputAction.CallbackContext context)
    {
        if (!localPlayerInControl)
            return;

        if (!context.performed)
            return;

        if (!ignitionStarted)
            return;

        if (jumpingInCar || keyIsInDriverHand)
            return;

        if (turboBoosts == 0)
        {
            DoTurboBoost(context);
        }
        else
        {
            if (turboBoostParticle.isPlaying)
                return;

            Vector2 dir = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();
            if (base.IsOwner)
            {
                jumpingInCar = true;
                GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetTrigger("SA_JumpInCar");
                StartCoroutine(jerkCarUpward(dir));
            }
            springAudio.PlayOneShot(jumpInCarSFX);
        }
    }

    private new void DoTurboBoost(InputAction.CallbackContext context)
    {
        if (context.performed && localPlayerInControl && currentDriver && !currentDriver.isTypingChat && !currentDriver.quickMenuManager.isMenuOpen && !jumpingInCar && !keyIsInDriverHand)
        {
            Vector2 dir = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
            UseTurboBoostLocalClient(dir);
            UseTurboBoostServerRpc();
        }
    }

    public new void UseTurboBoostLocalClient(Vector2 dir = default(Vector2))
    {
        if (!ignitionStarted)
            return;

        if (turboBoosts > 0 && ignitionStarted && overdriveSwitchEnabled)
        {
            turboBoosts = Mathf.Max(0, turboBoosts - 1);
            turboBoostAudio.PlayOneShot(turboBoostSFX);
            engineAudio1.PlayOneShot(turboBoostSFX2);
            turboBoostParticle.Play(withChildren: true);

            if (Vector3.Distance(StartOfRound.Instance.audioListener.transform.position, turboBoostAudio.transform.position) > 10f)
                return;

            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        }
        else
        {
            springAudio.PlayOneShot(jumpInCarSFX);
        }

        if (!base.IsOwner)
            return;

        if (keyIgnitionCoroutine != null)
            return;

        if (turboBoosts <= 0 || !overdriveSwitchEnabled)
        {
            jumpingInCar = true;
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetTrigger("SA_JumpInCar");
            StartCoroutine(jerkCarUpward(dir));
            return;
        }

        Vector3 vector = transform.TransformDirection(new Vector3(dir.x, 0f, dir.y));
        mainRigidbody.AddForce(vector * turboBoostForce + Vector3.up * turboBoostUpwardForce * 0.6f, ForceMode.Impulse);
    }

    private new IEnumerator jerkCarUpward(Vector3 dir)
    {
        yield return new WaitForSeconds(0.16f);
        if (!base.IsOwner)
        {
            jumpingInCar = false;
            yield break;
        }
        Vector3 vector = transform.TransformDirection(new Vector3(dir.x, 0f, dir.y));
        mainRigidbody.AddForce(vector * turboBoostForce * 0.22f + Vector3.up * turboBoostUpwardForce * 0.1f, ForceMode.Impulse);
        mainRigidbody.AddForceAtPosition(Vector3.up * jumpForce, hoodFireAudio.transform.position - Vector3.up * 2f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.15f);
        jumpingInCar = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public new void UseTurboBoostServerRpc()
    {
        UseTurboBoostClientRpc();
    }

    [ClientRpc]
    public new void UseTurboBoostClientRpc()
    {
        if (base.IsOwner)
            return;

        UseTurboBoostLocalClient();
    }

    public void ActivateHeadlights(InputAction.CallbackContext context)
    {
        if (!localPlayerInControl)
            return;

        if (!context.performed)
            return;

        ToggleHeadlightsLocalClient();
    }

    public void ActivateWipers(InputAction.CallbackContext context)
    {
        if (!localPlayerInControl)
            return;

        if (!context.performed)
            return;

        // this needs to be cached, but i'm too lazy right now
        InteractTrigger? windscreenWipers = transform.Find("Meshes/BodyContainer/Interior_XL/XLColumn/RightStockAnimContainer/RightStock/ToggleWiper")?.GetComponent<InteractTrigger>();
        windscreenWipers?.onInteract.Invoke(StartOfRound.Instance.localPlayerController);
    }

    public void ActivateHorn(InputAction.CallbackContext context)
    {
        if (!localPlayerInControl)
            return;

        if (truckAlarmCoroutine != null)
            return;

        if (((context.performed && !honkingHorn) ||
            (context.canceled && honkingHorn)))
        {
            SetHonkingLocalClient(!honkingHorn);
        }
    }

    public new void SetHonkingLocalClient(bool honk)
    {
        if (electricalShutdown) return;
        base.SetHonkingLocalClient(honk);
    }

    public new void OnDisable()
    {
        RemoveCarRainCollision();
        DisableControl();
        if (localPlayerInControl || localPlayerInPassengerSeat || localPlayerInMiddlePassengerSeat)
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
        GrabbableObject[] componentsInChildren = physicsRegion.physicsTransform.GetComponentsInChildren<GrabbableObject>();
        for (int i = 0; i < componentsInChildren.Length; i++)
        {
            if (RoundManager.Instance.mapPropsContainer != null)
            {
                componentsInChildren[i].transform.SetParent(RoundManager.Instance.mapPropsContainer.transform, worldPositionStays: true);
            }
            else
            {
                componentsInChildren[i].transform.SetParent(null, worldPositionStays: true);
            }
            if (!componentsInChildren[i].isHeld)
            {
                componentsInChildren[i].FallToGround(false, false, default(Vector3));
            }
        }
        physicsRegion.disablePhysicsRegion = true;
        if (StartOfRound.Instance.CurrentPlayerPhysicsRegions.Contains(physicsRegion))
        {
            StartOfRound.Instance.CurrentPlayerPhysicsRegions.Remove(physicsRegion);
        }
    }

    // drivetrain misc stuff
    private void UpdateDrivetrainState()
    {
        float vehicleStress = 0f;
        wheelBrakeTorque = 0f;

        if (brakePedalPressed && drivetrainModule.autoGear != TruckGearShift.Park)
            wheelBrakeTorque = Mathf.MoveTowards(FrontLeftWheel.brakeTorque, maxBrakingPower, brakeSpeed * Time.deltaTime);

        if (ignitionStarted)
        {
            if (drivePedalPressed)
            {
                switch (drivetrainModule.autoGear)
                {
                    case TruckGearShift.Park:
                        vehicleStress += 1.2f;
                        lastStressType += "; Accelerating while in park";
                        break;

                    case TruckGearShift.Reverse:
                        wheelTorque = -engineModule.engineReversePower;
                        break;
                    case TruckGearShift.Drive:
                        wheelTorque = drivetrainModule.automaticTransmissionCoroutine == null ?
                            Mathf.Clamp(Mathf.MoveTowards(FrontLeftWheel.motorTorque, engineModule.enginePower * 2f, carAcceleration * Time.deltaTime), 2000f, engineModule.enginePower * 2f) : 0f;
                        break;
                }
            }
            else if (drivetrainModule.autoGear != TruckGearShift.Neutral)
            {
                float idleDirection = (drivetrainModule.autoGear == TruckGearShift.Reverse) ? -1f : 1f;
                wheelTorque = idleSpeed * idleDirection;
            }
            if (drivetrainModule.autoGear == TruckGearShift.Park)
            {
                if (BackLeftWheel.isGrounded && BackRightWheel.isGrounded &&
                    averageVelocity.magnitude > 14f)
                {
                    vehicleStress += Mathf.Clamp(((averageVelocity.magnitude * 165f) - 200f) / 150f, 0f, 4f);
                    lastStressType += "; In park while at a high speed";
                }
                wheelBrakeTorque = 12000f;
            }
            else if (drivetrainModule.autoGear == TruckGearShift.Neutral)
                wheelTorque = 0f;
        }
        else
        {
            drivetrainModule.forwardWheelSpeed = 3000f;
            drivetrainModule.reverseWheelSpeed = -3000f;

            switch (drivetrainModule.autoGear)
            {
                case TruckGearShift.Park:
                case TruckGearShift.Reverse:
                case TruckGearShift.Drive:
                    wheelTorque = 0f;
                    wheelBrakeTorque = 12000f;
                    break;

                case TruckGearShift.Neutral:
                    wheelTorque = 0f;
                    wheelBrakeTorque = 0f;
                    break;
            }

            if (engineModule.tryingIgnition && drivetrainModule.autoGear != TruckGearShift.Park &&
                batteryCharge >= dischargedBattery) 
            {
                vehicleStress += 5.25f;
                lastStressType += "; Trying ignition while not in park";
            }
        }
        SetInternalStress(vehicleStress);
        stressPerSecond = vehicleStress;
    }

    public new void Update()
    {
        if (destroyNextFrame)
        {
            if (base.IsOwner)
            {
                UnityEngine.Object.Destroy(base.windwiperPhysicsBody1.gameObject);
                UnityEngine.Object.Destroy(base.windwiperPhysicsBody2.gameObject);
                UnityEngine.Object.Destroy(base.ragdollPhysicsBody.gameObject);
                UnityEngine.Object.Destroy(base.gameObject);
            }
            return;
        }
        if (base.NetworkObject != null && !base.NetworkObject.IsSpawned)
        {
            RemoveCarRainCollision();
            physicsRegion.disablePhysicsRegion = true;
            if (StartOfRound.Instance.CurrentPlayerPhysicsRegions.Contains(physicsRegion))
                StartOfRound.Instance.CurrentPlayerPhysicsRegions.Remove(physicsRegion);

            if (localPlayerInControl || localPlayerInPassengerSeat || localPlayerInMiddlePassengerSeat)
                GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();

            GrabbableObject[] componentsInChildren = physicsRegion.physicsTransform.GetComponentsInChildren<GrabbableObject>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                if (RoundManager.Instance.mapPropsContainer != null)
                {
                    componentsInChildren[i].transform.SetParent(RoundManager.Instance.mapPropsContainer.transform, worldPositionStays: true);
                }
                else
                {
                    componentsInChildren[i].transform.SetParent(null, worldPositionStays: true);
                }
                if (!componentsInChildren[i].isHeld)
                {
                    componentsInChildren[i].FallToGround(false, false, default(Vector3));
                }
            }
            destroyNextFrame = true;
            return;
        }

        if (!hasBeenSpawned)
            return;

        if (magnetedToShip)
        {
            if (!StartOfRound.Instance.magnetOn)
            {
                magnetedToShip = false;
                StartOfRound.Instance.isObjectAttachedToMagnet = false;
                CollectItemsInTruck();
                return;
            }

            limitTruckVelocityTimer = 1.3f;
            magnetTime = Mathf.Min(magnetTime + Time.deltaTime, 1f);
            magnetRotationTime = Mathf.Min(magnetTime + Time.deltaTime * 0.75f, 1f);

            if (!finishedMagneting && magnetTime > 0.7f)
            {
                finishedMagneting = true;
                turbulenceAmount = 2f;
                turbulenceAudio.volume = 0.6f;
                turbulenceAudio.PlayOneShot(maxCollisions[UnityEngine.Random.Range(0, maxCollisions.Length)]);
            }
        }
        else
        {
            finishedMagneting = false;
            if (StartOfRound.Instance.attachedVehicle == this)
            {
                StartOfRound.Instance.attachedVehicle = null;
            }
            if (base.IsOwner)
            {
                if (enabledCollisionForAllPlayers)
                {
                    enabledCollisionForAllPlayers = false;
                    DisableVehicleCollisionForAllPlayers();
                }
                if (!inDropshipAnimation) SyncCarPositionToOtherClients();
            }
            else
            {
                if (!enabledCollisionForAllPlayers)
                {
                    enabledCollisionForAllPlayers = true;
                    EnableVehicleCollisionForAllPlayers();
                }
            }
            if (base.IsOwner && !carDestroyed &&
                !StartOfRound.Instance.isObjectAttachedToMagnet &&
                StartOfRound.Instance.attachedVehicle == null &&
                StartOfRound.Instance.magnetOn && Vector3.Distance(transform.position, StartOfRound.Instance.magnetPoint.position) < 12f)
            {
                if (!Physics.Linecast(transform.position, StartOfRound.Instance.magnetPoint.position, 256, QueryTriggerInteraction.Ignore))
                    StartMagneting();
                return;
            }
        }

        if (carDestroyed)
            return;
        ReactToDamage();

        if (currentDriver != null)
        {
            if (Time.realtimeSinceStartup - timeAtLastGearShift < 1.7f)
            {
                currentDriver.playerBodyAnimator.SetFloat("SA_CarMotionTime", gearStickAnimValue);
            }
            if (localPlayerInControl && ignitionStarted && keyIgnitionCoroutine == null)
            {
                currentDriver.playerBodyAnimator.SetFloat("animationSpeed", steeringWheelAnimFloat + 0.5f);
                if (currentDriver.ladderCameraHorizontal > -35f)
                {
                    if (Time.realtimeSinceStartup - timeAtLastGearShift < 1.7f)
                    {
                        currentDriver.playerBodyAnimator.SetInteger("SA_CarAnim", 5);
                    }
                    else
                    {
                        currentDriver.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
                    }
                }
                else
                {
                    currentDriver.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
                }
                //currentDriver.playerBodyAnimator.SetInteger("SA_CarHonk", (int)((honkingHorn && currentDriver.ladderCameraHorizontal > -25f) ? 1 : 0)); old animation
            }
        }

        SetCarEffects(steeringAnimValue);
        if (IsHost) DoBatteryCycle(); //batteryCharge >=0.23f
        if (localPlayerInControl && currentDriver != null)
        {
            GetVehicleInput();
            return;
        }
        moveInputVector = Vector2.zero;
    }

    public void KillDashboardSymbols()
    {
        parkingBrakeSymbol.SetActive(false);
        checkEngineLightSymbol.SetActive(false);
        alertLightSymbol.SetActive(false);
        seatbeltLightSymbol.SetActive(false);
        dippedBeamLightSymbol.SetActive(false);
        highBeamLightSymbol.SetActive(false);
        oilLevelLightSymbol.SetActive(false);
        batteryLightSymbol.SetActive(false);
        coolantLevelLightSymbol.SetActive(false);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void ShutdownElectricalSystemRpc()
    {
        ShutdownElectricalSystem();
    }

    public void ShutdownElectricalSystem()
    {
        electricalShutdown = true;
        driversSideWindow.interactable = false;
        passengersSideWindow.interactable = false;
        lowBeamsOnBefore = lowBeamsOn;
        highBeamsOnBefore = highBeamsOn;
        if (keyIsInIgnition) KillDashboardSymbols();
        if (frontCabinLightContainer.activeSelf) SetFrontCabinLightOn(setOn: false);
        if (liveRadioController._radioOn)
        {
            radioTurnedOnBefore = true;
            liveRadioController.TurnRadioOnOff(false);
        }
        if (lowBeamsOn)
        {
            headlightsContainer.SetActive(false);
            highBeamContainer.SetActive(false);
            radioLight.SetActive(false);
            sideLightsContainer.SetActive(false);
            clusterLightsContainer.SetActive(false);
            SetHeadlightMaterial(false, false);
        }
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void RestartElectricalSystemRpc()
    {
        RestartElectricalSystem();
    }

    public void RestartElectricalSystem()
    {
        electricalShutdown = false;
        driversSideWindow.interactable = keyIsInIgnition;
        passengersSideWindow.interactable = keyIsInIgnition;
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        if (radioTurnedOnBefore && !liveRadioController._radioOn)
        {
            radioTurnedOnBefore = false;
            liveRadioController.TurnOnRadioRpc();
        }
        headlightsContainer.SetActive(lowBeamsOn);
        highBeamContainer.SetActive(highBeamsOn);
        radioLight.SetActive(lowBeamsOn);
        sideLightsContainer.SetActive(lowBeamsOn);
        clusterLightsContainer.SetActive(lowBeamsOn);
        SetHeadlightMaterial(lowBeamsOn, highBeamsOn);
    }

    public void DoBatteryCycle()
    {
        if (batteryCharge <= 0f) return;

        if (batteryCheckInterval < 0.15f)
        {
            batteryCheckInterval += Time.deltaTime;
            return;
        }
        batteryCheckInterval = 0f;

        float chargeToSync = Mathf.Round(batteryCharge * 100f) / 100f;
        if (syncedBatteryCharge != chargeToSync)
        {
            syncedBatteryCharge = chargeToSync;
            SyncCarBatteryServerRpc(chargeToSync);
        }

        //HUDManager.Instance.SetDebugText($"charge?: {roundedCharge}");

        if (batteryCharge <= dischargedBattery && !electricalShutdown)
        {
            ShutdownElectricalSystem();
            ShutdownElectricalSystemRpc();
        }
        else if (batteryCharge >= dischargedBattery && electricalShutdown)
        {
            RestartElectricalSystem();
            RestartElectricalSystemRpc();
        }

        if (ignitionStarted)
        {
            batteryCharge += 0.15f * Time.deltaTime;
            batteryCharge = Mathf.Clamp01(batteryCharge);
            return;
        }

        if (electricalShutdown) return;
        float drainMultiplier = 0f;
        if (liveRadioController != null && liveRadioController._radioOn)
            drainMultiplier += (radioDrain * batteryDrainMultiplier);

        if (lowBeamsOn)
            drainMultiplier += (lowBeamsDrain * batteryDrainMultiplier);

        if (highBeamsOn)
            drainMultiplier += (highBeamsDrain * batteryDrainMultiplier);

        batteryCharge -= drainMultiplier * Time.deltaTime;
        batteryCharge = Mathf.Clamp01(batteryCharge);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncCarBatteryServerRpc(float charge)
    {
        SyncCarBatteryClientRpc(charge);
    }

    [ClientRpc]
    public void SyncCarBatteryClientRpc(float charge)
    {
        if (!IsOwner)
            return;

        syncedBatteryCharge = charge;
        batteryCharge = charge;
    }

    public new void ChangeRadioStation()
    {
        liveRadioController.ToggleStationLocalClient();
        return;

        // completely unused, needs rewriting and reworking
        currentSongTime = 0f;
        if (!radioOn)
        {
            SetRadioOnLocalClient(true, false);
        }
        currentRadioClip = (currentRadioClip + 1) % radioClips.Length;
        radioAudio.clip = radioClips[currentRadioClip];
        SetRadioTime();
        radioAudio.Play();
        int num = (int)Mathf.Round(radioSignalQuality);
        switch (num)
        {
            case 3:
                radioSignalQuality = 1f;
                radioSignalDecreaseThreshold = 10f;
                break;
            case 0:
                radioSignalQuality = 3f;
                radioSignalDecreaseThreshold = 90f;
                break;
            case 1:
                radioSignalQuality = 2f;
                radioSignalDecreaseThreshold = 70f;
                break;
            case 2:
                radioSignalQuality = 1f;
                radioSignalDecreaseThreshold = 30f;
                break;
        }

        SetRadioStationServerRpc(currentRadioClip, num);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncRadioTimeServerRpc(float songTime)
    {
        SyncRadioTimeClientRpc(songTime);
    }

    [ClientRpc]
    public void SyncRadioTimeClientRpc(float songTime)
    {
        if (IsHost)
            return;

        currentSongTime = songTime;
        SetRadioTime();
    }

    public void SetRadioTime()
    {
        if (radioAudio.clip == null)
            return;

        radioAudio.time = Mathf.Clamp(currentSongTime % radioAudio.clip.length, 0.01f, radioAudio.clip.length - 0.1f);
    }

    [ServerRpc(RequireOwnership = false)]
    public new void SetRadioStationServerRpc(int radioStation, int signalQuality)
    {
        currentSongTime = 0f;
        SetRadioStationClientRpc(radioStation, signalQuality);
    }

    [ClientRpc]
    public new void SetRadioStationClientRpc(int radioStation, int signalQuality)
    {
        currentSongTime = 0f;
        currentRadioClip = radioStation;
        radioSignalQuality = signalQuality;
        radioAudio.clip = radioClips[currentRadioClip];
        SetRadioTime();
        radioAudio.Play();
        SetRadioOnLocalClient(true, true);
    }

    private new void SetRadioOnLocalClient(bool on, bool setClip = true)
    {
        radioOn = on;
        if (on)
        {
            if (setClip)
            {
                radioAudio.clip = radioClips[currentRadioClip];
                SetRadioTime();
            }
            radioAudio.Play();
            radioInterference.Play();
            return;
        }
        radioAudio.Stop();
        radioInterference.Stop();
    }

    public new void SwitchRadio()
    {
        liveRadioController.TogglePowerLocalClient();
        timeSinceTogglingRadio = Time.realtimeSinceStartup;
        if (localPlayerInControl && ignitionStarted)
        {
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetTrigger("SA_RadioTurnOn");
        }
        return;

        // completely unused, needs rewriting and reworking
        radioOn = !radioOn;
        if (radioOn)
        {
            radioAudio.clip = radioClips[currentRadioClip];
            SetRadioTime();
            radioAudio.Play();
            radioInterference.Play();

            SetRadioStationServerRpc(currentRadioClip, (int)Mathf.Round(radioSignalQuality));
        }
        else
        {
            radioAudio.Stop();
            radioInterference.Stop();
        }
        SetRadioOnServerRpc(radioOn);
    }

    [ServerRpc(RequireOwnership = false)]
    public new void SetRadioOnServerRpc(bool on)
    {
        SetRadioOnClientRpc(on);
    }

    [ClientRpc]
    public new void SetRadioOnClientRpc(bool on)
    {
        if (radioOn == on)
            return;

        SetRadioOnLocalClient(on);
    }

    public new void SetRadioValues()
    {
        if (radioAudio.isPlaying && radioAudio.volume >= 0.2f && Time.realtimeSinceStartup > radioPingTimestamp)
        {
            radioPingTimestamp = Time.realtimeSinceStartup + 1f;
            RoundManager.Instance.PlayAudibleNoise(radioAudio.transform.position, 16f, Mathf.Min((radioAudio.volume + radioInterference.volume) * 0.5f, 0.9f), 0, false, 2692);
        }
        return;

        // completely unused, needs rewriting and reworking
        if (!radioOn)
        {
            return;
        }
        if (IsHost)
        {
            currentSongTime += Time.deltaTime;
        }
        // sync radio time once per second
        if (IsHost && (Time.realtimeSinceStartup - timeLastSyncedRadio > 1f))
        {
            timeLastSyncedRadio = Time.realtimeSinceStartup;

            SyncRadioTimeServerRpc(currentSongTime);
        }
        if (!radioAudio.isPlaying)
        {
            radioAudio.Play();
        }
        if (IsServer && radioAudio.isPlaying && Time.realtimeSinceStartup > radioPingTimestamp)
        {
            radioPingTimestamp = Time.realtimeSinceStartup + 1f;
            RoundManager.Instance.PlayAudibleNoise(radioAudio.transform.position, 16f, Mathf.Min((radioAudio.volume + radioInterference.volume) * 0.5f, 0.9f), 0, false, 2692);
        }
        if (base.IsOwner)
        {
            float num = UnityEngine.Random.Range(0, 100);
            float num2 = (3f - radioSignalQuality - 1.5f) * radioSignalTurbulence;
            radioSignalDecreaseThreshold = Mathf.Clamp(radioSignalDecreaseThreshold + Time.deltaTime * num2, 0f, 100f);
            if (num > radioSignalDecreaseThreshold)
            {
                radioSignalQuality = Mathf.Clamp(radioSignalQuality - Time.deltaTime, 0f, 3f);
            }
            else
            {
                radioSignalQuality = Mathf.Clamp(radioSignalQuality + Time.deltaTime, 0f, 3f);
            }
            if (Time.realtimeSinceStartup - changeRadioSignalTime > 0.3f)
            {
                changeRadioSignalTime = Time.realtimeSinceStartup;
                if (radioSignalQuality < 1.2f && UnityEngine.Random.Range(0, 100) < 6)
                {
                    radioSignalQuality = Mathf.Min(radioSignalQuality + 1.5f, 3f);
                    radioSignalDecreaseThreshold = Mathf.Min(radioSignalDecreaseThreshold + 30f, 100f);
                }
                SetRadioSignalQualityServerRpc((int)Mathf.Round(radioSignalQuality));
            }
        }
        switch ((int)Mathf.Round(radioSignalQuality))
        {
            case 3:
                radioAudio.volume = Mathf.Lerp(radioAudio.volume, 1f, 2f * Time.deltaTime);
                radioInterference.volume = Mathf.Lerp(radioInterference.volume, 0f, 2f * Time.deltaTime);
                break;
            case 2:
                radioAudio.volume = Mathf.Lerp(radioAudio.volume, 0.85f, 2f * Time.deltaTime);
                radioInterference.volume = Mathf.Lerp(radioInterference.volume, 0.4f, 2f * Time.deltaTime);
                break;
            case 1:
                radioAudio.volume = Mathf.Lerp(radioAudio.volume, 0.6f, 2f * Time.deltaTime);
                radioInterference.volume = Mathf.Lerp(radioInterference.volume, 0.8f, 2f * Time.deltaTime);
                break;
            case 0:
                radioAudio.volume = Mathf.Lerp(radioAudio.volume, 0.4f, 2f * Time.deltaTime);
                radioInterference.volume = Mathf.Lerp(radioInterference.volume, 1f, 2f * Time.deltaTime);
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public new void SetRadioSignalQualityServerRpc(int signalQuality)
    {
        SetRadioSignalQualityClientRpc(signalQuality);
    }

    [ClientRpc]
    public new void SetRadioSignalQualityClientRpc(int signalQuality)
    {
        if (base.IsOwner)
            return;

        radioSignalQuality = signalQuality;
    }

    // improved wheel mesh --> collider
    private void MatchWheelMeshToCollider(MeshRenderer wheelMesh, MeshRenderer brakeMesh, WheelCollider wheelCollider)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);

        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = rotation;
        brakeMesh.transform.position = wheelMesh.transform.position;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetTyreStressOnServerRpc(float stress, bool wheelSkidding)
    {
        {
            SetTyreStressOnLocalClientRpc(stress, wheelSkidding);
        }
    }

    [ClientRpc]
    public void SetTyreStressOnLocalClientRpc(float stress, bool wheelSkidding)
    {
        if (base.IsOwner)
            return;

        tyreStress = stress;
        wheelSlipping = wheelSkidding;
    }

    // alarm :3
    public void TryBeginAlarm()
    {
        if (truckAlarmCoroutine != null)
            return;

        BeginAlarmOnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void BeginAlarmOnServerRpc()
    {
        BeginAlarmOnClientRpc();
    }

    [ClientRpc]
    public void BeginAlarmOnClientRpc()
    {
        truckAlarmCoroutine = StartCoroutine(BeginAlarmSound());
    }

    private IEnumerator BeginAlarmSound()
    {
        alarmDebounce = true;
        hornAudio.Stop();
        honkingHorn = false;
        alarmSource.Play();
        turnSignalAnimator.SetBool("hazardsOn", true);
        yield return new WaitForSeconds(alarmAudio.length - 0.01f);
        turnSignalAnimator.SetBool("hazardsOn", hazardsOn);
        truckAlarmCoroutine = null!;
        alarmDebounce = false;
    }

    // set vehicle effects (i.e. wheels, steering, etc)
    public new void SetCarEffects(float setSteering)
    {
        setSteering = IsOwner ? setSteering : 0f;
        steeringWheelAnimFloat = Mathf.Clamp(steeringWheelAnimFloat + setSteering * (steeringWheelTurnSpeed * Time.deltaTime / 6f), -1f, 1f);
        steeringWheelAnimator.SetFloat("steeringWheelTurnSpeed", Mathf.Clamp((steeringWheelAnimFloat + 1f) / 2f, 0f, 1f));

        SetCarGauges();
        SetCarCabinFan();
        SetCarRadio();
        SetCarAutomaticShifter();
        SetCarLightingEffects();
        SetCarAudioEffects();
        CalculateTyreSlip();
        SetCarKeyEffects();

        MatchWheelMeshToCollider(leftWheelMesh, leftBrakeMesh, FrontLeftWheel);
        MatchWheelMeshToCollider(rightWheelMesh, rightBrakeMesh, FrontRightWheel);
        MatchWheelMeshToCollider(backLeftWheelMesh, backLeftBrakeMesh, BackLeftWheel);
        MatchWheelMeshToCollider(backRightWheelMesh, backRightBrakeMesh, BackRightWheel);

        leftBrakeMesh.transform.localEulerAngles = new Vector3(leftBrakeMesh.transform.localEulerAngles.x, FrontLeftWheel.steerAngle, 0f);
        rightBrakeMesh.transform.localEulerAngles = new Vector3(rightBrakeMesh.transform.localEulerAngles.x, FrontRightWheel.steerAngle, 0f);
        backLeftBrakeMesh.transform.localEulerAngles = new Vector3(backLeftBrakeMesh.transform.localEulerAngles.x, 0f, 0f);
        backRightBrakeMesh.transform.localEulerAngles = new Vector3(backRightBrakeMesh.transform.localEulerAngles.x, 0f, 0f);

        if (base.IsOwner)
        {
            SyncCarEffectsToOtherClients();
            if (!syncedExtremeStress && underExtremeStress && extremeStressAudio.volume > 0.35f)
            {
                syncedExtremeStress = true;
                SyncExtremeStressServerRpc(underExtremeStress);
            }
            else if (syncedExtremeStress && !underExtremeStress && extremeStressAudio.volume < 0.5f)
            {
                syncedExtremeStress = false;
                SyncExtremeStressServerRpc(underExtremeStress);
            }
            return;
        }
        // synced client effects
        steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, syncedWheelRotation, steeringWheelTurnSpeed * Time.deltaTime / 6f);
    }

    // vehicle gauge stuff
    public void SetCarGauges()
    {
        speedometerTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -225f, speedometerFloat));
        tachometerTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -135f, tachometerFloat));
        oilPressureTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -135f, oilPressureFloat));
        if (ignitionStarted)
        {
            float vehicleHPfloat = (float)carHP / baseCarHP;
            float turboPressureInt = turboPressureNeedleCurve.Evaluate(vehicleHPfloat);
            float oilPressureCurve = oilPressureNeedleCurve.Evaluate(
                (float)((carHP / 2) + ((turboBoosts * 3.5) * (turboPressureFloat * (overdriveSwitchEnabled ? 1 : 0)))) / (baseCarHP + 5));

            speedometerFloat = IsOwner
                ? Mathf.Lerp(speedometerFloat, (drivetrainModule.wheelRPM / 850f), Time.deltaTime * 6f)
                : (drivetrainModule.wheelRPM / 850f);

            tachometerFloat = IsOwner
                ? Mathf.Lerp(tachometerFloat, (EngineRPM / MaxEngineRPM), Time.deltaTime * 6f)
                : (EngineRPM / MaxEngineRPM);

            turboPressureFloat = Mathf.Lerp(turboPressureFloat, turboPressureInt, 6f * Time.deltaTime);
            oilPressureFloat = Mathf.Lerp(oilPressureFloat, oilPressureCurve, 4f * Time.deltaTime);
            return;
        }
        //else
        //{
        oilPressureFloat = Mathf.Lerp(oilPressureFloat, 0f, 6f * Time.deltaTime);
        speedometerFloat = Mathf.Lerp(speedometerFloat, 0f, 6f * Time.deltaTime);
        tachometerFloat = Mathf.Lerp(
            tachometerFloat,
            ((engineAudio1.volume > 0.1f && engineModule.tryingIgnition && batteryCharge >= 0.23f)
            ? 0.065f : 0f),
            4.5f * Time.deltaTime);
        //}
        //speedometerTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -225f, speedometerFloat));
        //tachometerTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -135f, tachometerFloat));
        //oilPressureTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -135f, oilPressureFloat));
    }

    // cabin fan stuff
    public void SetCarCabinFan()
    {
        cabFanSpeed = Mathf.MoveTowards(cabFanSpeed, ((ignitionStarted && !electricalShutdown) ? -16f : 0f), 6f * Time.deltaTime);
        cabinFan.transform.Rotate(Vector3.down, cabFanSpeed, Space.Self);
    }

    // radio text stuff
    public void SetCarRadio()
    {
        // time & radio Frequency on dash
        if ((!keyIsInIgnition && !liveRadioController._radioOn) || 
            batteryCharge <= dischargedBattery)
        {
            radioTime.text = null;
            radioFrequency.text = null;
            return;
        }
        if (liveRadioController._radioOn)
        {
            radioTime.text = SetRadioClock(HUDManager.Instance.clockNumber.text);
            if (Time.realtimeSinceStartup - liveRadioController._timeSinceChangingVol < 2f)
            {
                radioFrequency.text = SetRadioVolume(liveRadioController._volume);
            }
            else
            {
                radioFrequency.text =
                    (liveRadioController._stream == null || string.IsNullOrEmpty(liveRadioController._stream.buffer_info))
                    ? "PI SEEK"
                    : liveRadioController._currentFrequency;
            }
            return;
        }
        radioTime.text = null;
        radioFrequency.text = "RADIO OFF";
    }

    // display the current radio volume
    private string SetRadioVolume(float vol)
    {
        if (vol <= 0f)
            return "RADIO MUTE";
        if (vol >= 1f)
            return "VOL MAX";

        int display = Mathf.RoundToInt(vol * 10f);
        return $"VOL {display:00}";
    }

    // trim the radio time text to not display 
    // "PM" "AM" or a space
    private string SetRadioClock(string clockText)
    {
        return clockText
            .Trim()
            .Replace("\n", " ")
            .Replace("AM", "")
            .Replace("PM", "")
            .Trim();
    }

    // automatic shifter position
    public void SetCarAutomaticShifter()
    {
        switch (drivetrainModule.autoGear)
        {
            case TruckGearShift.Park:
                {
                    gearStickAnimValue = Mathf.MoveTowards(gearStickAnimValue, 1f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - timeAtLastGearShift));
                    break;
                }
            case TruckGearShift.Reverse:
                {
                    gearStickAnimValue = Mathf.MoveTowards(gearStickAnimValue, 0.597f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - timeAtLastGearShift));
                    break;
                }
            case TruckGearShift.Neutral:
                {
                    gearStickAnimValue = Mathf.MoveTowards(gearStickAnimValue, 0.32f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - timeAtLastGearShift));
                    break;
                }
            case TruckGearShift.Drive:
                {
                    gearStickAnimValue = Mathf.MoveTowards(gearStickAnimValue, 0f, 15f * Time.deltaTime * (Time.realtimeSinceStartup - timeAtLastGearShift));
                    break;
                }
        }
        gearStickAnimator.SetFloat("gear", Mathf.Clamp(gearStickAnimValue, 0.01f, 0.99f));
    }

    // unused
    public void SetCarManualShifter()
    {
        //TBD
    }

    // brake-lights and reverse-lights, any additional
    // lights can be added here i guess
    public void SetCarLightingEffects()
    {
        bool brakeLightsOn = drivetrainModule.autoGear != TruckGearShift.Park && wheelBrakeTorque > 100f && ignitionStarted && batteryCharge >= dischargedBattery;
        bool backingUpLightsOn = drivetrainModule.autoGear == TruckGearShift.Reverse && ignitionStarted && batteryCharge >= dischargedBattery;
        if (backLightsOn != brakeLightsOn)
        {
            backLightsOn = brakeLightsOn;
            backLightsMesh.material = brakeLightsOn ? backLightOnMat : greyLightOffMat;
            backLightsContainer.SetActive(brakeLightsOn);
        }
        if (reverseLightsOn != backingUpLightsOn)
        {
            reverseLightsOn = backingUpLightsOn;
            reverseLightsMesh.enabled = backingUpLightsOn;
            reverseLightsContainer.SetActive(backingUpLightsOn);
        }
    }

    public void SetCarAudioEffects()
    {
        float engineAudioAnimCurve = engineModule.engineCurve.Evaluate(EngineRPM / engineIntensityPercentage);
        float batteryStrength = Mathf.Lerp(0.6f, 1f, Mathf.Pow(batteryCharge, 0.5f));
        float highestAudio1 = ignitionStarted
            ? Mathf.Clamp(engineAudioAnimCurve, 0.65f, 1.15f)
            : batteryStrength;
        float highestAudio2 = Mathf.Clamp(engineAudioAnimCurve, 0.7f, 1.5f);
        float highestTyre = Mathf.Clamp(drivetrainModule.wheelRPM / (180f * 0.35f), 0f, 1f);
        if (!cabinFanAudio.loop) cabinFanAudio.loop = true;
        carEngine2AudioActive = ignitionStarted;
        carRollingAudioActive = (FrontLeftWheel.isGrounded || FrontRightWheel.isGrounded || BackLeftWheel.isGrounded || BackRightWheel.isGrounded) && drivetrainModule.wheelRPM > 10f;
        SetVehicleAudioProperties(engineAudio1, carEngine1AudioActive, 0.7f, highestAudio1, 2f, useVolumeInsteadOfPitch: false, 0.7f);
        SetVehicleAudioProperties(engineAudio2, carEngine2AudioActive, 0.7f, highestAudio2, 3f, useVolumeInsteadOfPitch: false, 0.5f);
        SetVehicleAudioProperties(rollingAudio, carRollingAudioActive, 0f, highestTyre, 5f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(extremeStressAudio, underExtremeStress, 0.2f, 1f, 3f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(cabinFanAudio, cabFanOn, 0f, 1f, 3f, useVolumeInsteadOfPitch: true, 0.9f);
        SetRadioValues();
        if (engineAudio1.volume > 0.3f && engineAudio1.isPlaying && Time.realtimeSinceStartup - timeAtLastEngineAudioPing > 2f)
        {
            timeAtLastEngineAudioPing = Time.realtimeSinceStartup;
            if (EngineRPM > 2100f)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 32f, 0.75f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            if (EngineRPM > 600f && EngineRPM < 2100f)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 25f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            else if (!ignitionStarted)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 15f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            else
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 11f, 0.5f, 0, noiseIsInsideClosedShip: false, 2692);
            }
        }
        if (cabFanOn && !cabinFanAudio.isPlaying)
            cabinFanAudio.Play();
        else if (!cabFanOn && cabinFanAudio.isPlaying)
            cabinFanAudio.Stop();

        turbulenceAudio.volume = Mathf.Lerp(turbulenceAudio.volume, Mathf.Min(1f, turbulenceAmount), 10f * Time.deltaTime);
        turbulenceAmount = Mathf.Max(turbulenceAmount - Time.deltaTime, 0f);
        if (turbulenceAudio.volume > 0.02f)
        {
            if (!turbulenceAudio.isPlaying)
                turbulenceAudio.Play();
        }
        else if (turbulenceAudio.isPlaying)
            turbulenceAudio.Stop();

        if (truckAlarmCoroutine != null)
        {
            // if the alarm is blaring, play audible noises so dogs can hear it
            if (Time.realtimeSinceStartup - timeAtLastAlarmPing > 2f)
            {
                timeAtLastAlarmPing = Time.realtimeSinceStartup;
                RoundManager.Instance.PlayAudibleNoise(hornAudio.transform.position, 30f, 0.91f, 0, noiseIsInsideClosedShip: false, 106217);
            }
            return;
        }
        if (honkingHorn)
        {
            // if the alarm isn't blaring, and the player is blaring the horn, play the horn audio
            hornAudio.pitch = 1f;

            if (!hornAudio.isPlaying)
                hornAudio.Play();

            // audible noise stuff for dogs to hear
            if (Time.realtimeSinceStartup - timeAtLastHornPing > 2f)
            {
                timeAtLastHornPing = Time.realtimeSinceStartup;
                RoundManager.Instance.PlayAudibleNoise(hornAudio.transform.position, 28f, 0.85f, 0, noiseIsInsideClosedShip: false, 106217);
            }
        }
        else
        {
            hornAudio.pitch = Mathf.Max(hornAudio.pitch - Time.deltaTime * 6f, 0.01f);

            if (hornAudio.pitch < 0.02f && hornAudio.isPlaying)
                hornAudio.Stop();
        }
    }

    // tyre skid effects
    public void CalculateTyreSlip()
    {
        if (base.IsOwner)
        {
            float vehicleSpeed = Vector3.Dot(
                Vector3.Normalize(mainRigidbody.velocity * 1000f),
                transform.forward
            );

            bool wheelsGrounded = BackLeftWheel.isGrounded &&
                BackRightWheel.isGrounded;

            bool audioActive = vehicleSpeed > -0.6f && vehicleSpeed < 0.4f &&
                               (averageVelocity.magnitude > 4f || drivetrainModule.wheelRPM > 85f);

            if (wheelsGrounded)
            {
                bool sidewaySlipping = (wheelTorque > 900f) &&
                    Mathf.Abs(sidewaysSlip) > 0.35f;

                bool forwardSlipping = (wheelTorque > 4800f) &&
                    Mathf.Abs(forwardsSlip) > 0.25f;

                if (sidewaySlipping || forwardSlipping)
                {
                    vehicleSpeed = Mathf.Max(vehicleSpeed, 0.8f);
                    audioActive = true;

                    if (averageVelocity.magnitude > 8f && !tireSparks.isPlaying)
                        tireSparks.Play(true);
                }
                else
                {
                    audioActive = false;
                    tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            else
            {
                audioActive = false;
                tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            SetVehicleAudioProperties(skiddingAudio, audioActive, 0f, vehicleSpeed, 3f, true, 1f);

            if (Mathf.Abs(tyreStress - vehicleSpeed) > 0.04f || wheelSlipping != audioActive)
                SetTyreStressOnServerRpc(vehicleSpeed, audioActive);

            return;
        }

        if (wheelSlipping && averageVelocity.magnitude > 8f && !tireSparks.isPlaying)
        {
            tireSparks.Play(true);
        }
        else if (!wheelSlipping && tireSparks.isEmitting)
        {
            tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        SetVehicleAudioProperties(skiddingAudio, wheelSlipping, 0f, tyreStress, 3f, true, 1f);
    }

    // what a mess, but it works, and it's
    // better than whatever i had before
    public void SetCarKeyEffects()
    {
        Transform ignBarrelRot = (ignitionStarted || engineModule.tryingIgnition)
            ? ignitionBarrelTryingPos
            : ignitionBarrelNotTurnedPosition;

        ignitionBarrel.transform.localPosition = ignBarrelRot.localPosition;
        ignitionBarrel.transform.localRotation = Quaternion.Lerp(
            ignitionBarrel.transform.localRotation,
            ignBarrelRot.localRotation,
            Time.deltaTime * ignitionRotSpeed
        );

        if (keyIsInIgnition)
        {
            if (!keyObject.enabled)
                keyObject.enabled = true;

            if (keyObject.transform.parent != carKeyContainer.transform)
                keyObject.transform.SetParent(carKeyContainer.transform);
            keyObject.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
            if (carKeyInHand.transform.parent != carKeyContainer.transform)
                carKeyInHand.transform.SetParent(carKeyContainer.transform, false);
            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;

            Transform ignKeyRot = (ignitionStarted || engineModule.tryingIgnition)
                ? ignitionTryingPosition
                : ignitionNotTurnedPosition;

            keyObject.transform.localPosition = ignKeyRot.localPosition;
            keyObject.transform.localRotation = Quaternion.Lerp(
                keyObject.transform.localRotation,
                ignKeyRot.localRotation,
                Time.deltaTime * ignitionRotSpeed
            );
        }
        else
        {
            if (keyObject.enabled)
                keyObject.enabled = false;
        }
        if (currentDriver == null) return;
        if (keyIsInDriverHand && !keyIsInIgnition)
        {
            if (!keyObject.enabled)
                keyObject.enabled = true;

            Transform handParent;
            Vector3 posOffset, rotOffset;

            handParent = localPlayerInControl
                ? currentDriver.localItemHolder.parent
                : currentDriver.serverItemHolder.parent;

            posOffset = localPlayerInControl ? LHD_Pos_Local : LHD_Pos_Server;
            rotOffset = localPlayerInControl ? LHD_Rot_Local : LHD_Rot_Server;

            if (carKeyInHand.transform.parent != handParent.transform)
                carKeyInHand.transform.SetParent(handParent, false);

            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;
            carKeyInHand.transform.localScale = Vector3.one;

            if (keyObject.transform.parent != carKeyInHand.transform)
                keyObject.transform.SetParent(carKeyInHand.transform);

            keyObject.transform.localPosition = posOffset;
            keyObject.transform.localRotation = Quaternion.Euler(rotOffset);
        }
    }

    public new void FixedUpdate()
    {
        // legacy code
        //foreach (WheelCollider wheel in wheels)
        //{
        //    if (wheel.GetGroundHit(out var hit))
        //    {
        //        groundNormal += hit.normal;
        //        groundedWheelCount++;
        //        SetWheelStiffness(wheel, hit.collider.CompareTag("Snow"));
        //    }
        //}
        //groundNormal.Normalize();
        //if (groundedWheelCount < 3 || Vector3.Angle(-groundNormal, Physics.gravity) > 30f) return;
        //Vector3 carFrontHillDirection = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
        //Vector3 hillGravity = -groundNormal * Physics.gravity.magnitude;
        //Vector3 force = hillGravity - Physics.gravity; // Difference between real gravity and hill gravity
        //if (drivetrainModule.autoGear != TruckGearShift.Park)
        //{
        //    force = Vector3.ProjectOnPlane(force, carFrontHillDirection);
        //}
        //mainRigidbody.AddForce(force, ForceMode.Acceleration);

        lastVelocity = IsOwner ? mainRigidbody.velocity : syncedMovementSpeed; // Track the last velocity (used for experimental collisions)

        Vector3 groundNormal = Vector3.zero;
        int groundedWheelCount = 0;
        if (FrontLeftWheel.enabled &&
            FrontRightWheel.enabled &&
            BackLeftWheel.enabled &&
            BackRightWheel.enabled)
        {
            for (int i = 0; i < wheels.Count; i++)
            {
                if (wheels[i].GetGroundHit(out var hit))
                {
                    wheelHits[i] = hit;
                    groundNormal += hit.normal;
                    groundedWheelCount++;
                    SetWheelStiffness(wheels[i], hit.collider.CompareTag("Snow"));
                }
                else
                {
                    wheelHits[i] = default;
                }
            }
            groundNormal.Normalize();
        }

        if (!StartOfRound.Instance.inShipPhase &&
            !loadedVehicleFromSave &&
            !hasDeliveredVehicle)
        {
            // optimisation
            if (itemShip == null && References.itemShip != null)
                itemShip = References.itemShip;

            if (itemShip != null &&
                !hasBeenSpawned)
            {
                if (itemShip.untetheredVehicle)
                {
                    // release the truck
                    inDropshipAnimation = false;
                    mainRigidbody.MovePosition(itemShip.deliverVehiclePoint.position);
                    mainRigidbody.MoveRotation(itemShip.deliverVehiclePoint.rotation);
                    syncedPosition = transform.position;
                    syncedRotation = transform.rotation;
                    hasBeenSpawned = true;
                    hasDeliveredVehicle = true;
                }
                else if (itemShip.deliveringVehicle)
                {
                    // in delivery animation
                    inDropshipAnimation = true;
                    mainRigidbody.isKinematic = true;
                    mainRigidbody.MovePosition(itemShip.deliverVehiclePoint.position);
                    mainRigidbody.MoveRotation(itemShip.deliverVehiclePoint.rotation);
                    syncedPosition = transform.position;
                    syncedRotation = transform.rotation;
                }
            }
            else if (itemShip == null)
            {
                // failsafe idk
                inDropshipAnimation = false;
                mainRigidbody.isKinematic = true;
                mainRigidbody.MovePosition(StartOfRound.Instance.notSpawnedPosition.position + Vector3.forward * 30f);
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
            }
        }
        if (magnetedToShip)
        {
            // move the truck to its magnetised point
            syncedPosition = transform.position;
            syncedRotation = transform.rotation;
            mainRigidbody.MovePosition(Vector3.Lerp(magnetStartPosition, StartOfRound.Instance.elevatorTransform.position + magnetTargetPosition, magnetPositionCurve.Evaluate(magnetTime)));
            mainRigidbody.MoveRotation(Quaternion.Lerp(magnetStartRotation, magnetTargetRotation, magnetRotationCurve.Evaluate(magnetRotationTime)));
            averageVelocityAtMagnetStart = Vector3.Lerp(averageVelocityAtMagnetStart, Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 4f), 4f * Time.deltaTime);

            if (!finishedMagneting)
                magnetStartPosition += Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 5f) * Time.fixedDeltaTime;
        }
        else
        {
            if (!base.IsOwner && !inDropshipAnimation)
            {
                // move the truck for non-owner clients
                mainRigidbody.isKinematic = true;

                float syncMultiplier = Mathf.Clamp(syncSpeedMultiplier * Vector3.Distance(transform.position, syncedPosition), 1.3f, 300f);
                Vector3 position = syncedPosition + (syncedMovementSpeed * Time.fixedDeltaTime);

                mainRigidbody.MovePosition(Vector3.Lerp(transform.position, position, syncMultiplier * Time.fixedDeltaTime));
                mainRigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, syncedRotation, syncRotationSpeed));
            }
        }

        averageVelocity += (mainRigidbody.velocity - averageVelocity) / (movingAverageLength + 1);
        ragdollPhysicsBody.Move(transform.position, transform.rotation);
        windwiperPhysicsBody1.Move(windwiper1.position, windwiper1.rotation);
        windwiperPhysicsBody2.Move(windwiper2.position, windwiper2.rotation);
        if (carDestroyed) return;

        if (!FrontLeftWheel.enabled ||
            !FrontRightWheel.enabled ||
            !BackLeftWheel.enabled ||
            !BackRightWheel.enabled)
            return;

        // apply angle to the front wheels dependant
        // on the rotation of the steering wheel
        float steeringAngle = steeringWheelCurve.Evaluate(Mathf.Abs(steeringWheelAnimFloat)) * 50f;
        float steeringSign = Mathf.Sign(steeringWheelAnimFloat);
        FrontLeftWheel.steerAngle = steeringAngle * steeringSign;
        FrontRightWheel.steerAngle = steeringAngle * steeringSign;

        // loop over each powered wheel, and apply motor,
        // brake torque and clamp how fast they can spin
        // (for our gear system)
        foreach (WheelCollider drivenWheel in wheels)
        {
            drivenWheel.motorTorque = wheelTorque;
            drivenWheel.brakeTorque = wheelBrakeTorque;
            drivenWheel.rotationSpeed = Mathf.Clamp(drivenWheel.rotationSpeed,
                drivetrainModule.reverseWheelSpeed, drivetrainModule.forwardWheelSpeed);
        }

        if (!base.IsOwner)
        {
            EngineRPM = Mathf.Lerp(EngineRPM, ignitionStarted ?
                engineModule.syncedEngineRPM : 0f, ignitionStarted ?
                Time.deltaTime * 6f : 3f * Time.deltaTime);

            drivetrainModule.wheelRPM = Mathf.Lerp(drivetrainModule.wheelRPM, drivetrainModule.syncedWheelRPM,
                Time.deltaTime * 6f);

            wheelTorque = drivetrainModule.syncedMotorTorque;
            wheelBrakeTorque = drivetrainModule.syncedBrakeTorque;
            engineModule.enginePower = 0f;
            drivetrainModule.currentGear = 1;
            return;
        }

        UpdateDrivetrainState();
        if (mainRigidbody.IsSleeping() || magnetedToShip) return; // minor optimisation stuff
        forwardsSlip = (wheelHits[2].forwardSlip + wheelHits[3].forwardSlip) * 0.5f;
        sidewaysSlip = (wheelHits[2].sidewaysSlip + wheelHits[3].sidewaysSlip) * 0.5f;

        // limit the trucks rigidbody velocity, so we don't exceed lightspeed
        if (limitTruckVelocityTimer <= 0f)
        {
            mainRigidbody.maxAngularVelocity = 4f;
            mainRigidbody.maxLinearVelocity = carMaxSpeed;
            mainRigidbody.maxDepenetrationVelocity = 7f;
        }
        else
        {
            limitTruckVelocityTimer -= Time.deltaTime * 0.5f;
            clampedLimitTruckVelocity = Mathf.Clamp(limitTruckVelocityTimer, 0f, 1f);
            mainRigidbody.maxDepenetrationVelocity = Mathf.Lerp(0.3f, 7f, clampedLimitTruckVelocity);
            mainRigidbody.maxAngularVelocity = Mathf.Lerp(0.1f, 4f, clampedLimitTruckVelocity);
            mainRigidbody.maxLinearVelocity = Mathf.Lerp(0.1f, 60f, clampedLimitTruckVelocity);
        }

        // anti-slip slide
        if (groundedWheelCount < 3 || Vector3.Angle(-groundNormal, Physics.gravity) > 30f) return;
        Vector3 carFrontHillDirection = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
        Vector3 hillGravity = -groundNormal * Physics.gravity.magnitude;
        Vector3 force = hillGravity - Physics.gravity; // difference between real gravity & hill gravity
        if (drivetrainModule.autoGear != TruckGearShift.Park)
        {
            force = Vector3.ProjectOnPlane(force, carFrontHillDirection);
        }
        mainRigidbody.AddForce(force, ForceMode.Acceleration);
    }

    private void SetWheelStiffness(WheelCollider wheel, bool isSnow)
    {
        // set the tyres to be slippy, should the
        // truck be on a snowy surface
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

        forwardFriction.stiffness = isSnow ? 0.25f : baseForwardStiffness;
        sidewaysFriction.stiffness = isSnow ? 0.225f : baseSidewaysStiffness;

        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    // sync the position of the steering wheel to
    // non-owner clients
    public void SyncCarEffectsToOtherClients()
    {
        if (syncCarEffectsInterval > 0.045f)
        {
            if (syncedWheelRotation != steeringWheelAnimFloat)
            {
                syncCarEffectsInterval = 0f;
                syncedWheelRotation = steeringWheelAnimFloat;
                SyncCarEffectsServerRpc(steeringWheelAnimFloat);
                return;
            }
        }
        else
        {
            syncCarEffectsInterval += Time.deltaTime;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncCarEffectsServerRpc(float wheelRotation)
    {
        SyncCarEffectsClientRpc(wheelRotation);
    }

    [ClientRpc]
    public void SyncCarEffectsClientRpc(float wheelRotation)
    {
        if (base.IsOwner)
            return;

        syncedWheelRotation = wheelRotation;
    }

    // sync the state of the pedals to
    // non-owner clients (also used for the
    // EVA module)
    [ServerRpc(RequireOwnership = false)]
    public void SyncDriverPedalInputsServerRpc(bool gasPressed, bool brakePressed)
    {
        SyncDriverPedalInputsClientRpc(gasPressed, brakePressed);
    }

    [ClientRpc]
    public void SyncDriverPedalInputsClientRpc(bool gasPressed, bool brakePressed)
    {
        if (base.IsOwner)
            return;

        drivePedalPressed = gasPressed;
        brakePedalPressed = brakePressed;
    }

    // sync the position of the truck to 
    // non-owner clients
    private void SyncCarPositionToOtherClients()
    {
        mainRigidbody.isKinematic = false;
        if (syncCarPositionInterval >= (0.12f * (averageVelocity.magnitude / 200f)))
        {
            if (Vector3.Distance(syncedPosition, transform.position) > 0.01f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
                syncedMovementSpeed = averageVelocity;
                SyncCarPositionServerRpc(transform.position, transform.eulerAngles, averageVelocity);
                return;
            }
            if (Vector3.Angle(transform.forward, syncedRotation * Vector3.forward) > 2f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
                syncedMovementSpeed = averageVelocity;
                SyncCarPositionServerRpc(transform.position, transform.eulerAngles, averageVelocity);
                return;
            }
        }
        else
        {
            syncCarPositionInterval += Time.deltaTime;
        }
        syncCarPositionInterval = Mathf.Clamp(syncCarPositionInterval, 0.002f, 0.2f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncCarPositionServerRpc(Vector3 carPosition, Vector3 carRotation, Vector3 averageSpeed)
    {
        SyncCarPositionClientRpc(carPosition, carRotation, averageSpeed);
    }

    [ClientRpc]
    public void SyncCarPositionClientRpc(Vector3 carPosition, Vector3 carRotation, Vector3 averageSpeed)
    {
        if (base.IsOwner)
            return;

        syncedPosition = carPosition;
        syncedRotation = Quaternion.Euler(carRotation);
        syncedMovementSpeed = averageSpeed;
    }

    // car reaction to damage (enemies, objs, players, etc)
    public new bool CarReactToObstacle(Vector3 vel, Vector3 position, Vector3 impulse, CarObstacleType type, float obstacleSize = 1f, EnemyAI enemyScript = null!, bool dealDamage = true)
    {
        switch (type)
        {
            case CarObstacleType.Object:
                if (carHP < 10)
                {
                    mainRigidbody.AddForceAtPosition(Vector3.up * torqueForce + vel, position, ForceMode.Impulse);
                }
                else
                {
                    mainRigidbody.AddForceAtPosition((Vector3.up * torqueForce + vel) * 0.5f, position, ForceMode.Impulse);
                }
                CarBumpServerRpc(averageVelocity * 0.7f);
                if (dealDamage)
                {
                    DealPermanentDamage(1, position);
                }
                return true;
            case CarObstacleType.Player:
                PlayCollisionAudio(position, 5, Mathf.Clamp(vel.magnitude / 7f, 0.65f, 1f));
                if (vel.magnitude < 4.25f)
                {
                    mainRigidbody.velocity = Vector3.Normalize(-impulse * 100000000f) * 9f;
                    DealPermanentDamage(1);
                    return true;
                }
                mainRigidbody.AddForceAtPosition(Vector3.up * torqueForce, position, ForceMode.VelocityChange);
                return false;
            case CarObstacleType.Enemy:
                {
                    float enemyHitSpeed;
                    if (obstacleSize <= 1f)
                    {
                        enemyHitSpeed = 9f; // 1f
                        _ = carReactToPlayerHitMultiplier;
                    }
                    else if (obstacleSize <= 2f)
                    {
                        enemyHitSpeed = 16f; // 9f
                        _ = carReactToPlayerHitMultiplier;
                    }
                    else
                    {
                        enemyHitSpeed = 21f; // 15f
                        _ = carReactToPlayerHitMultiplier;
                    }
                    vel = Vector3.Scale(vel, new Vector3(1f, 0f, 1f));
                    mainRigidbody.AddForceAtPosition(Vector3.up * torqueForce, position, ForceMode.VelocityChange);
                    bool result = false;
                    if (vel.magnitude < (enemyHitSpeed * 2f))
                    {
                        if (obstacleSize <= 1f)
                        {
                            mainRigidbody.AddForce(Vector3.Normalize(-impulse * 1E+09f) * 4f, ForceMode.Impulse);
                            if (vel.magnitude > 1f)
                            {
                                enemyScript.KillEnemyOnOwnerClient();
                            }
                        }
                        else
                        {
                            CarBumpServerRpc(averageVelocity);
                            mainRigidbody.velocity = Vector3.Normalize(-impulse * 100000000f) * 9f;
                            PlayerControllerB playerControllerB;
                            if (currentDriver != null)
                                playerControllerB = currentDriver;
                            else
                                playerControllerB = null!;

                            if (vel.magnitude > 2f && dealDamage)
                            {
                                enemyScript.HitEnemyOnLocalClient(2, Vector3.zero, playerControllerB, playHitSFX: true, 331);
                            }
                            result = true;
                            if (obstacleSize > 2f)
                            {
                                DealPermanentDamage(1, position);
                            }
                        }
                    }
                    else
                    {
                        mainRigidbody.AddForce(Vector3.Normalize(-impulse * 1E+09f) * (carReactToPlayerHitMultiplier - 220f), ForceMode.Impulse);
                        if (dealDamage)
                        {
                            DealPermanentDamage(1, position);
                        }
                        PlayerControllerB playerWhoHit;
                        if (currentDriver != null)
                            playerWhoHit = currentDriver;
                        else
                            playerWhoHit = null!;

                        enemyScript.HitEnemyOnLocalClient(12, Vector3.zero, playerWhoHit, false, -1);
                    }
                    PlayCollisionAudio(position, 5, 1f);
                    return result;
                }
            default:
                return false;
        }
    }

    public new void LateUpdate()
    {
        if (carDestroyed)
            return;

        SetDashboardSymbols();
        bool inOrbit = magnetedToShip &&
            (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipDoorsEnabled);
        hornAudio.mute = inOrbit;
        engineAudio1.mute = inOrbit;
        engineAudio2.mute = inOrbit;
        carKeySounds.mute = inOrbit;
        cabinFanAudio.mute = inOrbit;
        CabinLightSwitchAudio.mute = inOrbit;
        voiceModule.voiceAudio.mute = inOrbit;
        wiperAudio.mute = inOrbit;
        rollingAudio.mute = inOrbit;
        skiddingAudio.mute = inOrbit;
        turbulenceAudio.mute = inOrbit;
        hoodFireAudio.mute = inOrbit;
        extremeStressAudio.mute = inOrbit;
        pushAudio.mute = inOrbit;
        radioAudio.mute = inOrbit;
        radioInterference.mute = inOrbit;

        if (windshieldBroken && StartOfRound.Instance.inShipPhase)
            RegenerateWindshield();

        if (currentDriver != null && References.lastDriver != currentDriver &&
            !magnetedToShip)
            References.lastDriver = currentDriver;
    }

    // there's probably a better way to go about doing this
    public void SetDashboardSymbols()
    {
        if (!hasSweepedDashboard)
            return;

        if (!keyIsInIgnition || batteryCharge <= dischargedBattery)
        {
            if (parkingBrakeSymbol.activeSelf) parkingBrakeSymbol.SetActive(false);
            if (checkEngineLightSymbol.activeSelf) checkEngineLightSymbol.SetActive(false);
            if (alertLightSymbol.activeSelf) alertLightSymbol.SetActive(false);
            if (seatbeltLightSymbol.activeSelf) seatbeltLightSymbol.SetActive(false);
            if (oilLevelLightSymbol.activeSelf) oilLevelLightSymbol.SetActive(false);
            if (batteryLightSymbol.activeSelf) batteryLightSymbol.SetActive(false);
            if (coolantLevelLightSymbol.activeSelf) coolantLevelLightSymbol.SetActive(false);
            if (dippedBeamLightSymbol.activeSelf) dippedBeamLightSymbol.SetActive(false);
            if (highBeamLightSymbol.activeSelf) highBeamLightSymbol.SetActive(false);
            return;
        }
        SetSymbolActive(dippedBeamLightSymbol, currentSweepStage > 1 && lowBeamsOn);
        SetSymbolActive(highBeamLightSymbol, currentSweepStage > 1 && highBeamsOn);
        SetSymbolActive(parkingBrakeSymbol, currentSweepStage > 2 && drivetrainModule.autoGear == TruckGearShift.Park);
        SetSymbolActive(oilLevelLightSymbol, currentSweepStage > 3 && carHP <= 15);
        SetSymbolActive(batteryLightSymbol, currentSweepStage > 3 && batteryCharge < 0.62);
        SetSymbolActive(coolantLevelLightSymbol, currentSweepStage > 3 && carHP <= 19);
        SetSymbolActive(alertLightSymbol, currentSweepStage > 3 && carHP <= 12);
        SetSymbolActive(checkEngineLightSymbol, currentSweepStage > 3 && carHP <= 21);
    }

    public void SetSymbolActive(GameObject obj, bool active)
    {
        if (obj.activeSelf != active)
            obj.SetActive(active);
    }

    // truck collision stuff
    public new void OnCollisionEnter(Collision collision)
    {
        if (!base.IsOwner)
            return;

        if (magnetedToShip || !hasBeenSpawned)
            return;

        if (collision.collider.gameObject.layer != 8)
            return;

        float currentSpeed = mainRigidbody.velocity.magnitude;
        float previousSpeed = lastVelocity.magnitude;
        float differenceInVelocity = Mathf.Abs(previousSpeed - currentSpeed);

        float carBump = 0f;
        int collisionContact = collision.GetContacts(contacts);
        Vector3 zero = Vector3.zero;

        for (int i = 0; i < collisionContact; i++)
        {
            if (contacts[i].impulse.magnitude > carBump)
            {
                carBump = contacts[i].impulse.magnitude;
            }
            zero += contacts[i].point;
        }

        zero /= (float)collisionContact;
        carBump /= Time.fixedDeltaTime;

        //if (differenceInVelocity > 0.15f)
        //{
        //    Plugin.Logger.LogError($"diff? {differenceInVelocity}, spd? {currentSpeed}, imp? {carBump}");
        //}

        if (carBump < minimalBumpForce || averageVelocity.magnitude < 4f)
        {
            if (carBump > 3 && averageVelocity.magnitude > 2.5f)
            {
                SetInternalStress(0.35f);
                lastStressType = "Scraping?";
            }
        }

        float setVolume = 0.5f;
        int audioType = -1;

        if (differenceInVelocity > 10f)
        {
            if (averageVelocity.magnitude > 31f)
            {
                if (carHP < 3)
                {
                    DealPermanentDamage(Mathf.Max(carHP - 1, 2));
                    return;
                }
                DealPermanentDamage(carHP - 2);
            }
            else if (averageVelocity.magnitude > 27f)
            {
                DealPermanentDamage(2);
            }
        }

        if (carBump > maximumBumpForce &&
            averageVelocity.magnitude > 11f)
        {
            audioType = 2;
            setVolume = Mathf.Clamp((carBump - maximumBumpForce) / 20000f, 0.8f, 1f);
            setVolume = Mathf.Clamp(setVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);
            if (differenceInVelocity > 10f)
            {
                DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                CarCollisionServerRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                DealPermanentDamage(2);
            }
            else if (differenceInVelocity > 6f &&
                differenceInVelocity < 10f)
            {
                DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                CarCollisionServerRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
            }
        }
        else if (carBump > mediumBumpForce && averageVelocity.magnitude > 3f)
        {
            audioType = 1;
            setVolume = Mathf.Clamp((carBump - mediumBumpForce) / (maximumBumpForce - mediumBumpForce), 0.67f, 1f);
            setVolume = Mathf.Clamp(setVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.5f, 1f);
            if (differenceInVelocity > 10f)
            {
                DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                CarCollisionServerRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                DealPermanentDamage(2);
            }
            else if (differenceInVelocity > 6f &&
                differenceInVelocity < 10f)
            {
                DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                CarCollisionServerRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
            }
        }
        else if (averageVelocity.magnitude > 1.5f)
        {
            audioType = 0;
            setVolume = Mathf.Clamp((carBump - minimalBumpForce) / (mediumBumpForce - minimalBumpForce), 0.25f, 1f);
            setVolume = Mathf.Clamp(setVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.25f, 1f);
            if (differenceInVelocity > 10f)
            {
                DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                CarCollisionServerRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                DealPermanentDamage(2);
            }
            else if (differenceInVelocity > 6f &&
                differenceInVelocity < 10f)
            {
                DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                CarCollisionServerRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
            }
        }

        if (audioType != -1)
        {
            PlayCollisionAudio(zero, audioType, setVolume);
            if (carBump > maximumBumpForce + 10000f &&
                averageVelocity.magnitude > 20f)
            {
                if (differenceInVelocity > 16f)
                {
                    DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                    BreakWindshield();
                    BreakWindshieldServerRpc();
                    CarCollisionServerRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                    DealPermanentDamage(2);
                    return;
                }
            }
            CarBumpServerRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 40f));
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public new void CarBumpServerRpc(Vector3 vel)
    {
        CarBumpClientRpc(vel);
    }

    [ClientRpc]
    public new void CarBumpClientRpc(Vector3 vel)
    {
        if (localPlayerInControl ||
            localPlayerInMiddlePassengerSeat ||
            localPlayerInPassengerSeat)
            return;

        if (!VehicleUtils.IsPlayerInVehicleBounds(GameNetworkManager.Instance.localPlayerController, this))
            return;

        if (vel.magnitude >= 50f)
            return;

        GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
    }

    [ServerRpc(RequireOwnership = false)]
    public new void CarCollisionServerRpc(Vector3 vel, float magn)
    {
        CarCollisionClientRpc(vel, magn);
    }

    [ClientRpc]
    public new void CarCollisionClientRpc(Vector3 vel, float magn)
    {
        if (base.IsOwner)
            return;

        DamagePlayerInVehicle(vel, magn);
    }

    // help
    private new void DamagePlayerInVehicle(Vector3 vel, float magnitude)
    {
        if (!localPlayerInPassengerSeat &&
            !localPlayerInMiddlePassengerSeat &&
            !localPlayerInControl)
        {

            if (!VehicleUtils.IsPlayerInVehicleBounds(GameNetworkManager.Instance.localPlayerController, this))
                return;
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            GameNetworkManager.Instance.localPlayerController.DamagePlayer(10, true, true, CauseOfDeath.Inertia, 0, false, vel);
            GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
            return;
        }
        if (magnitude > (28f))
        {
            GameNetworkManager.Instance.localPlayerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f);
            return;
        }
        if (magnitude <= 24f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            GameNetworkManager.Instance.localPlayerController.DamagePlayer(30, true, true, CauseOfDeath.Inertia, 0, false, vel);
            return;
        }
        HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
        if (GameNetworkManager.Instance.localPlayerController.health < 20)
        {
            GameNetworkManager.Instance.localPlayerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.DamagePlayer(40, true, true, CauseOfDeath.Inertia, 0, false, vel);
    }

    // explosion

    [ServerRpc(RequireOwnership = false)]
    public void BreakWindshieldServerRpc()
    {
        BreakWindshieldClientRpc();
    }

    [ClientRpc]
    public void BreakWindshieldClientRpc()
    {
        if (base.IsOwner)
            return;

        BreakWindshield();
    }

    private new void BreakWindshield()
    {
        if (windshieldBroken)
            return;

        windshieldBroken = true;
        windshieldPhysicsCollider.enabled = false;
        windshieldObject.SetActive(false);
        glassParticle.Play();
    }

    private void RegenerateWindshield()
    {
        if (!windshieldBroken)
            return;

        windshieldBroken = false;
        windshieldPhysicsCollider.enabled = true;
        windshieldObject.SetActive(true);
    }




    public new void PlayCollisionAudio(Vector3 setPosition, int audioType, float setVolume)
    {
        if (Time.realtimeSinceStartup - audio1Time > Time.realtimeSinceStartup - audio2Time)
        {
            bool audioTime = Time.realtimeSinceStartup - audio1Time >= collisionAudio1.clip.length * 0.8f;
            if (audio1Type <= audioType || audioTime)
            {
                audio1Time = Time.realtimeSinceStartup;
                audio1Type = audioType;
                collisionAudio1.transform.position = setPosition;
                PlayRandomClipAndPropertiesFromAudio(collisionAudio1, setVolume, audioTime, audioType);
                CarCollisionSFXServerRpc(collisionAudio1.transform.localPosition, 0, audioType, setVolume);
                return;
            }
        }
        else
        {
            bool audioTime = Time.realtimeSinceStartup - audio2Time >= collisionAudio2.clip.length * 0.8f;
            if (audio1Type <= audioType || audioTime)
            {
                audio2Time = Time.realtimeSinceStartup;
                audio2Type = audioType;
                collisionAudio2.transform.position = setPosition;
                PlayRandomClipAndPropertiesFromAudio(collisionAudio2, setVolume, audioTime, audioType);
                CarCollisionSFXServerRpc(collisionAudio2.transform.localPosition, 1, audioType, setVolume);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public new void CarCollisionSFXServerRpc(Vector3 audioPosition, int audio, int audioType, float vol)
    {
        CarCollisionSFXClientRpc(audioPosition, audio, audioType, vol);
    }

    [ClientRpc]
    public new void CarCollisionSFXClientRpc(Vector3 audioPosition, int audio, int audioType, float vol)
    {
        if (base.IsOwner)
            return;

        AudioSource audioSource = ((audio != 0) ? collisionAudio2 : collisionAudio1);
        bool audioFinished = audioSource.clip.length - audioSource.time < 0.2f;
        audioSource.transform.localPosition = audioPosition;
        PlayRandomClipAndPropertiesFromAudio(audioSource, vol, audioFinished, audioType);
    }

    // collision noises and all that fancy shit
    private new void PlayRandomClipAndPropertiesFromAudio(AudioSource audio, float setVolume, bool audioFinished, int audioType)
    {
        if (!audioFinished)
        {
            audio.Stop();
        }
        AudioClip[] array;
        switch (audioType)
        {
            case 0:
                array = minCollisions;
                turbulenceAmount = Mathf.Min(turbulenceAmount + 0.4f, 2f);
                break;
            case 1:
                array = medCollisions;
                turbulenceAmount = Mathf.Min(turbulenceAmount + 0.75f, 2f);
                break;
            case 2:
                array = maxCollisions;
                turbulenceAmount = Mathf.Min(turbulenceAmount + 1.4f, 2f);
                break;
            default:
                array = obstacleCollisions;
                turbulenceAmount = Mathf.Min(turbulenceAmount + 0.75f, 2f);
                break;
        }
        AudioClip audioClip = array[UnityEngine.Random.Range(0, array.Length)];
        if (audioClip == audio.clip && UnityEngine.Random.Range(0, 10) <= 5)
        {
            audioClip = array[UnityEngine.Random.Range(0, array.Length)];
        }
        if (audioFinished)
        {
            audio.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        }
        audio.clip = audioClip;
        audio.PlayOneShot(audioClip, setVolume);
        if (audioType >= 2)
        {
            RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 18f + setVolume / 1f * 7f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
        }
        else if (audioType >= 1)
        {
            RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 12f + setVolume / 1f * 7f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
        }
        if (audioType == -1)
        {
            array = minCollisions;
            audioClip = array[UnityEngine.Random.Range(0, array.Length)];
            audio.PlayOneShot(audioClip);
        }
    }

    // driving in park :fire:
    private new void SetInternalStress(float carStressIncrease = 0f)
    {
        if (StartOfRound.Instance.inShipPhase)
            return;

        if (carStressIncrease <= 0f)
        {
            carStressChange = Mathf.Clamp(carStressChange - Time.deltaTime, -0.25f, 0.5f);
        }
        else
        {
            carStressChange = Mathf.Clamp(carStressChange + Time.deltaTime * carStressIncrease, 0f, 10f);
        }

        underExtremeStress = carStressIncrease >= 1f && ignitionStarted;
        carStress = Mathf.Clamp(carStress + carStressChange, 0f, 100f);

        if (carStress < 7f)
            return;

        carStress = 0f;
        DealPermanentDamage(2);
        lastDamageType = "Stress";
    }

    // deal HP damage to the truck
    private new void DealPermanentDamage(int damageAmount, Vector3 damagePosition = default(Vector3))
    {
        if ((StartOfRound.Instance.inShipPhase) || magnetedToShip || carDestroyed || !base.IsOwner)
            return;

        if (Time.realtimeSinceStartup - timeAtLastDamage < 0.4f)
            return;

        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP -= damageAmount;
        if (carHP <= 0)
        {
            DestroyCar();
            DestroyCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
        }
        else
        {
            DealDamageServerRpc(damageAmount, (int)GameNetworkManager.Instance.localPlayerController.playerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public new void DealDamageServerRpc(int amount, int playerWhoSent)
    {
        DealDamageClientRpc(amount, playerWhoSent);
    }

    [ClientRpc]
    public new void DealDamageClientRpc(int amount, int playerWhoSent)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        carHP -= amount;
        timeAtLastDamage = Time.realtimeSinceStartup;
    }

    // real explosion
    private new void DestroyCar()
    {
        if (carDestroyed)
            return;

        carDestroyed = true;
        magnetedToShip = false;

        RemoveCarRainCollision();
        CollectItemsInTruck();

        underExtremeStress = false;
        hoodPoppedUp = false;

        keyObject.enabled = false;
        engineAudio1.Stop();
        engineAudio2.Stop();
        turbulenceAudio.Stop();
        rollingAudio.Stop();
        radioAudio.Stop();
        radioInterference.Stop();
        extremeStressAudio.Stop();
        carKeySounds.Stop();
        CabinLightSwitchAudio.Stop();
        honkingHorn = false;
        hornAudio.Stop();
        tireSparks.Stop();
        skiddingAudio.Stop();
        turboBoostAudio.Stop();
        turboBoostParticle.Stop();
        if (voiceModule.audioTimedCoroutine != null)
        {
            StopCoroutine(voiceModule.audioTimedCoroutine);
            voiceModule.audioTimedCoroutine = null!;
        }
        voiceModule.voiceAudio.Stop();
        voiceModule.voiceAudio.mute = true;

        EngineRPM = 0f;
        drivetrainModule.wheelRPM = 0f;
        drivetrainModule.currentGear = 1;

        RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 20f, 0.8f, 0, noiseIsInsideClosedShip: false, 2692);

        FrontLeftWheel.motorTorque = 0f;
        FrontRightWheel.motorTorque = 0f;
        BackLeftWheel.motorTorque = 0f;
        BackRightWheel.motorTorque = 0f;

        FrontRightWheel.brakeTorque = 0f;
        FrontLeftWheel.brakeTorque = 0f;
        BackLeftWheel.brakeTorque = 0f;
        BackRightWheel.brakeTorque = 0f;

        FrontLeftWheel.enabled = false;
        FrontRightWheel.enabled = false;
        BackLeftWheel.enabled = false;
        BackRightWheel.enabled = false;

        leftWheelMesh.enabled = false;
        rightWheelMesh.enabled = false;
        backLeftWheelMesh.enabled = false;
        backRightWheelMesh.enabled = false;

        leftBrakeMesh.enabled = false;
        rightBrakeMesh.enabled = false;
        backLeftBrakeMesh.enabled = false;
        backRightBrakeMesh.enabled = false;

        carHoodAnimator.gameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
        backDoorContainer.SetActive(value: false);
        headlightsContainer.SetActive(value: false);
        sideLightsContainer.SetActive(value: false);
        backLightsContainer.SetActive(value: false);

        BreakWindshield();
        destroyedTruckMesh.SetActive(value: true);
        mainBodyMesh.gameObject.SetActive(value: false);

        WheelCollider[] componentsInChildren = gameObject.GetComponentsInChildren<WheelCollider>();
        for (int j = 0; j < componentsInChildren.Length; j++)
        {
            componentsInChildren[j].enabled = false;
        }

        for (int disableDestroy = 0; disableDestroy < disableOnDestroy.Length; disableDestroy++)
        {
            disableOnDestroy[disableDestroy].SetActive(false);
        }

        for (int enableDestroy = 0; enableDestroy < enableOnDestroy.Length; enableDestroy++)
        {
            enableOnDestroy[enableDestroy].SetActive(true);
        }

        mainRigidbody.AddForceAtPosition(Vector3.up * 1560f, hoodFireAudio.transform.position - Vector3.up, ForceMode.Impulse);

        SetIgnition(started: false);
        SetFrontCabinLightOn(setOn: false);
        carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        keyIsInDriverHand = false;
        keyIsInIgnition = false;
        keyIgnitionCoroutine = null;
        backDoorOpen = true;
        sideDoorOpen = true;
        hasEnclosedRoof = false;

        if (localPlayerInControl || localPlayerInMiddlePassengerSeat || localPlayerInPassengerSeat)
        {
            GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.up * 27f + 20f * UnityEngine.Random.insideUnitSphere, spawnBody: true, CauseOfDeath.Blast, 6, Vector3.up * 1.5f);
        }

        InteractTrigger[] componentsInChildren2 = gameObject.GetComponentsInChildren<InteractTrigger>();
        for (int k = 0; k < componentsInChildren2.Length; k++)
        {
            componentsInChildren2[k].interactable = false;
            componentsInChildren2[k].CancelAnimationExternally();
        }

        passengerSeatTrigger.interactable = false;
        middlePassengerSeatTrigger.interactable = false;
        driverSeatTrigger.interactable = false;

        currentDriver = null!;
        currentMiddlePassenger = null!;
        currentPassenger = null!;

        Landmine.SpawnExplosion(transform.position + transform.forward + Vector3.up * 1.5f, spawnExplosionEffect: true, 6f, 10f, 30, 200f, truckDestroyedExplosion, goThroughCar: true);
        mainRigidbody.AddExplosionForce(800f * 50f, transform.position, 12f, 3f * 6f, ForceMode.Impulse);
        pushTruckTrigger.interactable = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public new void DestroyCarServerRpc(int playerWhoSent)
    {
        DestroyCarClientRpc(playerWhoSent);
    }

    [ClientRpc]
    public new void DestroyCarClientRpc(int playerWhoSent)
    {
        if (carDestroyed)
            return;

        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        DestroyCar();
    }

    // erm, don't ask?
    public void RemoveCarRainCollision()
    {
        var particleTriggers = new[]
        {
           References.rainParticles.trigger,
           References.rainHitParticles.trigger,
           References.stormyRainParticles.trigger,
           References.stormyRainHitParticles.trigger
        };

        foreach (var trigger in particleTriggers)
        {
            for (int i = trigger.colliderCount - 1; i >= 0; i--)
            {
                var collider = (Collider)trigger.GetCollider(i);
                if (weatherEffectBlockers.Contains(collider))
                {
                    trigger.RemoveCollider(i);
                }
            }
        }
    }

    private new void ReactToDamage()
    {
        if (carDestroyed)
            return;

        healthMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            healthMeter.localScale.z,
            Mathf.Clamp((float)carHP / (float)baseCarHP, 0.01f, 1f),
            6f * Time.deltaTime));
        turboMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            turboMeter.localScale.z,
            Mathf.Clamp((float)turboBoosts / 5f, 0.01f, 1f),
            6f * Time.deltaTime));

        if (carHP < 7 && Time.realtimeSinceStartup - timeAtLastDamage > 16f)
        {
            timeAtLastDamage = Time.realtimeSinceStartup;
            carHP++;
        }

        if (!base.IsOwner)
            return;

        // sync the hood being on fire
        // for non-owner clients
        if (carHP < 3)
        {
            if (!isHoodOnFire)
            {
                if (!hoodPoppedUp)
                {
                    hoodPoppedUp = true;
                    SetHoodOpenLocalClient(setOpen: hoodPoppedUp);
                }
                isHoodOnFire = true;
                hoodFireAudio.Play();
                hoodFireParticle.Play();
                SetHoodOnFireServerRpc(isHoodOnFire);
            }
        }
        else if (isHoodOnFire)
        {
            hoodPoppedUp = false;
            isHoodOnFire = false;
            hoodFireAudio.Stop();
            hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
            SetHoodOnFireServerRpc(isHoodOnFire);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHoodOnFireServerRpc(bool onFire)
    {
        SetHoodOnFireClientRpc(onFire);
    }

    [ClientRpc]
    public void SetHoodOnFireClientRpc(bool onFire)
    {
        if (base.IsOwner)
            return;

        isHoodOnFire = onFire;
        if (isHoodOnFire)
        {
            hoodFireAudio.Play();
            hoodFireParticle.Play();
            return;
        }
        hoodFireAudio.Stop();
        hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
    }

    // improved checks for whether the player
    // is allowed to push the truck
    public new void PushTruckWithArms()
    {
        if (magnetedToShip)
            return;

        if (UserConfig.PreventPushInPark.Value && drivetrainModule.autoGear == TruckGearShift.Park && !ignitionStarted)
            return;

        if (!Physics.Raycast(
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position,
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward,
            out base.hit,
            10f,
            1073742656,
            QueryTriggerInteraction.Ignore))
            return;

        if (VehicleUtils.IsPlayerInVehicleBounds(GameNetworkManager.Instance.localPlayerController, this))
            return;

        if (GameNetworkManager.Instance.localPlayerController.overridePhysicsParent != null)
            return;

        Vector3 point = hit.point;
        Vector3 forward = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward;

        if (!base.IsOwner)
        {
            PushTruckServerRpc(point, forward);
            return;
        }

        mainRigidbody.AddForceAtPosition(Vector3.Normalize(forward * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, point - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
        PushTruckFromOwnerServerRpc(point);
    }

    [ServerRpc(RequireOwnership = false)]
    public new void PushTruckServerRpc(Vector3 pos, Vector3 dir)
    {
        PushTruckClientRpc(pos, dir);
    }

    [ServerRpc(RequireOwnership = false)]
    public new void PushTruckFromOwnerServerRpc(Vector3 pos)
    {
        PushTruckFromOwnerClientRpc(pos);
    }

    [ClientRpc]
    public new void PushTruckClientRpc(Vector3 pushPosition, Vector3 dir)
    {
        pushAudio.transform.position = pushPosition;
        pushAudio.Play();
        turbulenceAmount = Mathf.Min(turbulenceAmount + 0.5f, 2f);
        if (base.IsOwner)
        {
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(dir * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, pushPosition - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
        }
    }

    [ClientRpc]
    public new void PushTruckFromOwnerClientRpc(Vector3 pos)
    {
        pushAudio.transform.position = pos;
        pushAudio.Play();
        turbulenceAmount = Mathf.Min(turbulenceAmount + 0.5f, 2f);
    }

    public new void ToggleHoodOpenLocalClient()
    {
        carHoodOpen = !carHoodOpen;
        carHoodAnimator.SetBool("hoodOpen", carHoodOpen);
        SetHoodOpenServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, carHoodOpen);
        //if (carHoodOpen)
        //{
        //    hoodAudio.PlayOneShot(carHoodOpenSFX);
        //    return;
        //}
        //hoodAudio.PlayOneShot(carHoodCloseSFX);
    }

    public new void SetHoodOpenLocalClient(bool setOpen)
    {
        if (carHoodOpen == setOpen)
            return;

        SetHoodOpenServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, open: true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHoodOpenServerRpc(int playerWhoSent, bool open)
    {
        SetHoodOpenClientRpc(open);
    }

    [ClientRpc]
    public void SetHoodOpenClientRpc(int playerWhoSent, bool open)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        if (carHoodOpen == open)
            return;

        carHoodOpen = open;
        carHoodAnimator.SetBool("hoodOpen", open);
        //if (open)
        //{
        //    hoodAudio.PlayOneShot(carHoodOpenSFX);
        //    return;
        //}
        //hoodAudio.PlayOneShot(carHoodCloseSFX);
    }

    // unused as of right now, may enable again later
    public void SetHazardLightsOnLocalClient()
    {
        hazardsOn = !hazardsOn;
        if (truckAlarmCoroutine == null) turnSignalAnimator.SetBool("hazardsOn", hazardsOn);
        SetHazardLightsOnServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, hazardsOn);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetHazardLightsOnServerRpc(int playerWhoSent, bool warningsOn)
    {
        SetHazardLightsOnClientRpc(playerWhoSent, warningsOn);
    }

    [ClientRpc]
    public void SetHazardLightsOnClientRpc(int playerWhoSent, bool warningsOn)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        hazardsOn = warningsOn;
        if (truckAlarmCoroutine == null) turnSignalAnimator.SetBool("hazardsOn", hazardsOn);
    }

    // heamdlights with low-high beam functions
    public new void ToggleHeadlightsLocalClient()
    {
        if (electricalShutdown) return;
        if (!lowBeamsOn && !highBeamsOn)
        {
            lowBeamsOn = true;
            highBeamsOn = false;
        }
        else if (lowBeamsOn && !highBeamsOn)
        {
            highBeamsOn = true;
            lowBeamsOn = true;
        }
        else if (lowBeamsOn && highBeamsOn)
        {
            highBeamsOn = false;
            lowBeamsOn = false;
        }

        miscAudio.transform.position = headlightsContainer.transform.position;
        miscAudio.PlayOneShot(headlightsToggleSFX);

        headlightSwitchAnimator.SetBool("lowBeamsToggle", lowBeamsOn);
        headlightSwitchAnimator.SetBool("highBeamsToggle", highBeamsOn);

        headlightsContainer.SetActive(lowBeamsOn);
        highBeamContainer.SetActive(highBeamsOn);
        radioLight.SetActive(lowBeamsOn);
        sideLightsContainer.SetActive(lowBeamsOn);
        clusterLightsContainer.SetActive(lowBeamsOn);
        SetHeadlightMaterial(lowBeamsOn, highBeamsOn);
        ToggleHeadlightsServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, lowBeamsOn, highBeamsOn);
    }

    // additional stuff for light materials
    private void SetHeadlightMaterial(bool lowOn, bool highOn)
    {
        Material[] sharedMaterials = interiorMesh.sharedMaterials;
        sharedMaterials[1] = lowOn ? clusterOnMaterial : clusterOffMaterial;
        sharedMaterials[2] = lowOn ? clusterDialsOnMat : clusterDialsOffMat;
        interiorMesh.sharedMaterials = sharedMaterials;

        speedometerMesh.material = lowOn ? clusterOnMaterial : clusterOffMaterial;
        tachometerMesh.material = lowOn ? clusterOnMaterial : clusterOffMaterial;
        oilPressureMesh.material = lowOn ? clusterOnMaterial : clusterOffMaterial;

        radioMesh.material = lowOn ? radioOnMaterial : radioOffMaterial;
        radioPowerDial.material = lowOn ? radioOnMaterial : radioOffMaterial;
        radioVolumeDial.material = lowOn ? radioOnMaterial : radioOffMaterial;

        lowBeamMesh.material = lowOn ? headlightsOnMat : headlightsOffMat;
        highBeamMesh.material = highOn ? headlightsOnMat : headlightsOffMat;
        sideTopLightsMesh.material = lowOn ? backLightOnMat : redLightOffMat;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleHeadlightsServerRpc(int playerWhoSent, bool setLowBeamsOn, bool setHighBeamsOn)
    {
        ToggleHeadlightsClientRpc(playerWhoSent, setLowBeamsOn, setHighBeamsOn);
    }

    [ClientRpc]
    public void ToggleHeadlightsClientRpc(int playerWhoSent, bool setLowBeamsOn, bool setHighBeamsOn)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerWhoSent)
            return;

        lowBeamsOn = setLowBeamsOn;
        highBeamsOn = setHighBeamsOn;

        miscAudio.transform.position = headlightsContainer.transform.position;
        miscAudio.PlayOneShot(headlightsToggleSFX);

        headlightSwitchAnimator.SetBool("lowBeamsToggle", lowBeamsOn);
        headlightSwitchAnimator.SetBool("highBeamsToggle", highBeamsOn);

        headlightsContainer.SetActive(lowBeamsOn);
        highBeamContainer.SetActive(highBeamsOn);
        radioLight.SetActive(lowBeamsOn);
        sideLightsContainer.SetActive(lowBeamsOn);
        clusterLightsContainer.SetActive(lowBeamsOn);
        SetHeadlightMaterial(lowBeamsOn, highBeamsOn);
    }
}
