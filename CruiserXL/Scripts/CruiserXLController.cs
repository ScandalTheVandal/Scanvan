using System;
using System.Collections;
using System.Collections.Generic;
using ScanVan;
using ScanVan.Behaviour;
using ScanVan.Patches;
using ScanVan.Utils;
using GameNetcodeStuff;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;
using ScanVan.Compatibility;
using ScanVan.Networking;
using UnityEngine.InputSystem.XR;
using ScandalsTweaks.Utils;
using ScanVan.Scripts;
using System.ComponentModel;
using ScandalsTweaks.Scripts;

public class CruiserXLController : VehicleController
{
    [Header("Gimmicks/Variety")]

    public EVAModule voiceModule = null!;

    public Material truckMat = null!;

    public Material rareTruckDialsOn = null!;
    public Material rareTruckClusterOn = null!;
    public Material rareTruckRadioOn = null!;
    public Material rareHeaterOn = null!;
    public Material rareWindowOn = null!;

    public Texture2D defaultTruckTex = null!;
    public Texture2D rareTruckTex = null!;

    public Light radioLightCol = null!; // special hex #FAFEAA, default hex #D6FFCE // FILTER COLOR
    public Light heaterLightCol = null!;
    public Light clusterLightCol = null!; // special hex #FAFEDE, default hex #C9FFFA // FILTER COLOR

    public Light leftWindowLightCol = null!; // special hex #FAFEAA, default hex #D6FFCE // FILTER COLOR
    public Light rightWindowLightCol = null!; // special hex #FAFEAA, default hex #D6FFCE // FILTER COLOR

    public float specialChance;
    public bool isSpecial;

    public Light jcJensonLight = null!;
    public SpriteRenderer jcJensonSymbol = null!;
    public GameObject jcJensonSymbolObj = null!;

    [Header("Physics")]

    public List<WheelCollider> wheels = null!;
    public AnimationCurve steeringWheelCurve = null!;
    public CruiserXLCollisionTrigger collisionTrigger = null!;
    public Rigidbody playerPhysicsBody = null!;

    public AnimationCurve engineCurve = null!;
    public AnimationCurve enginePowerCurve = null!;

    public Vector3 lastVelocity;

    private WheelHit[] wheelHits = new WheelHit[4];
    public Vector3 previousVehiclePosition;
    public Quaternion previousVehicleRotation;

    private float timeSinceLastCollision;
    public bool hasDeliveredVehicle;

    public bool frontWheelsGrounded;
    public bool backWheelsGrounded;
    public bool allWheelsAirborne;
    public bool allWheelsGrounded;

    private float timeSinceUntethered;

    public bool usingSwitchIgnition;
    public bool engineStalled = false;

    public bool handbrakeEngaged;
    public bool inReverse;
    public bool inNeutral;

    public bool clutchPedalPressed;
    public bool lastClutchPedalPressed;

    public float brakeInput;
    public float clutchInput;
    public float gasInput;
    public float throttleInput;

    public float clutchSpeed;
    public float minClutchReturnSpeed;
    public float clutchReturnSpeed;

    public float stallTimer;
    public float clutchFriction;
    public float clutchSlip;
    public float clutchEngagement;

    public float throttleReleaseSpeed;
    public float maxThrottleSpeed;
    public float throttleSpeed;

    public float inclineCompensation;
    public float minInclineCompensation = 1.78f;
    public float maxInclineCompensation = 4.1f;
    public float maxInclineCompensationAngle = 32f;

    public float forwardWheelSpeed;
    public float reverseWheelSpeed;

    public float frontWheelRPM;
    public float backWheelRPM;
    public float wheelRPM;

    private float maxAutoCenterSpeed = 8f;
    private float steeringAngle;
    private float steeringDecay = 1f;

    private float forwardsSlip;
    //private float sidewaysSlip;

    public float baseForwardStiffness = 1f;
    public float baseSidewaysStiffness = 0.75f;

    public float currentMotorTorque;
    public float currentBrakeTorque;

    public float maxBrakingPower;
    public float baseSteeringWheelTurnSpeed;

    public float enginePower;
    public float engineReversePower;

    public float timeAtLastGearboxDamage;
    public int baseTransmissionHP = 20;
    public int syncedTransmissionHP;
    public int transmissionHP;

    public float[] gearRatios = null!;
    public float diffRatio;
    public int currentGear;

    //public float extraWeight = 0f;

    [Header("Networking/Player")]

    public VanPhysicsRegion vanZone = null!;
    public VanPlayerZone vanCabinZone = null!;
    public VanPlayerZone vanStorageZone = null!;

    public Collider itemSafetyBounds = null!;
    public Collider vehicleBounds = null!;

    public CapsuleCollider cabinPoint = null!;
    public Collider storageCompartment = null!;

    public PlayerControllerB playerWhoShifted = null!;

    public VanSeatAnimator frontLeftSeat = null!;
    public VanSeatAnimator frontMiddleSeat = null!;
    public VanSeatAnimator frontRightSeat = null!;

    public PlayerControllerB currentMiddlePassenger = null!;
    public InteractTrigger middlePassengerSeatTrigger = null!;

    public InteractTrigger driversSideWindow = null!;
    public InteractTrigger passengersSideWindow = null!;

    public AnimatedObjectTrigger driversSideWindowTrigger = null!;
    public AnimatedObjectTrigger passengersSideWindowTrigger = null!;

    public Vector3 playerPositionOffset;
    public Vector3 seatNodePositionOffset;
    public Vector2 syncedMoveInputVector;

    public bool localPlayerInMiddlePassengerSeat;

    public float syncedSteeringWheelRotation;

    public float syncedFrontWheelRPM;
    public float syncedBackWheelRPM;
    public float syncedWheelRPM;

    public float syncedEngineRPM;

    public float syncedCurrentMotorTorque;
    public float syncedCurrentBrakeTorque;

    public int syncedCarHP;

    public bool syncedDrivePedalPressed;
    public bool syncedBrakePedalPressed;
    public bool syncedClutchPedalPressed;

    public float syncedTyreStress;
    public bool syncedTyreSlipping;

    public float syncEffectsInterval;
    public float syncTorqueInterval;
    public float syncDrivetrainInterval;

    // animations
    private string STEERING_WHEEL_SPEED = "steeringWheelTurnSpeed";
    private string ANIMATION_SPEED = "animationSpeed";
    private string REMOVE_IN_IGNITION = "SA_RemoveInIgnition";
    private string IGNITION_ANIM = "SAIgnition_Anim";
    private string CAR_ANIM = "SA_CarAnim";
    private string JUMP_WHILE_IN_CAR = "SA_JumpInCar";

    private string CAR_HANDBRAKE_ON_ENGAGEMENT = "SA_CarHandbrakeOn";
    private string CAR_HANDBRAKE_OFF_ENGAGEMENT = "SA_CarHandbrakeOff";
    private string HANDBRAKE_ENGAGEMENT = "engagement";

    private string UPPER_BODY_RADIO_ON = "U_Radio_TurnOn_LHD";
    private string LEFT_ARM_RADIO_ON = "L_Radio_TurnOn_LHD";
    private string RIGHT_ARM_RADIO_ON = "R_Radio_TurnOn_LHD";

    private string UPPER_BODY_HANDBRAKE_ON = "U_EngageHandbrake";
    private string LEFT_ARM_HANDBRAKE_ON = "L_EngageHandbrake";
    private string RIGHT_ARM_HANDBRAKE_ON = "R_EngageHandbrake";

    private string UPPER_BODY_HANDBRAKE_OFF = "U_DisengageHandbrake";
    private string LEFT_ARM_HANDBRAKE_OFF = "L_DisengageHandbrake";
    private string RIGHT_ARM_HANDBRAKE_OFF = "R_DisengageHandbrake";

    [Header("VFX")]

    public GameObject heaterDirectionLever = null!;
    public GameObject heaterTempLever = null!;
    public GameObject fanSpeedLever = null!;

    public MeshRenderer heaterBaseMesh = null!;
    public MeshRenderer windshieldMesh = null!;

    public GameObject[] disableOnDestroy = null!;
    public GameObject[] enableOnDestroy = null!;
    public GameObject windshieldObject = null!;
    public InteractTrigger pushTruckTrigger = null!;
    public MeshRenderer leftBrakeMesh = null!;
    public MeshRenderer rightBrakeMesh = null!;
    public MeshRenderer backLeftBrakeMesh = null!;
    public MeshRenderer backRightBrakeMesh = null!;
    public Collider[] weatherEffectBlockers = null!;

    public Collider ignitionCollider = null!;

    public Animator handbrakeAnimator = null!;

    public ScanNodeProperties scanNode = null!;

    public GameObject blinkerLightsContainer = null!;

    public GameObject sideDoorContainer = null!;

    public GameObject leftElectricMirror = null!;
    public GameObject rightElectricMirror = null!;

    public Coroutine dashboardSymbolPreStartup = null!;

    public Light hazardWarningLight = null!;
    public Light leftSignalLight = null!;
    public Light rightSignalLight = null!;

    public SpriteRenderer hazardWarningSymbol = null!;
    public SpriteRenderer leftSignalSymbol = null!;
    public SpriteRenderer rightSignalSymbol = null!;

    public Light vehicleDisplayLight = null!;
    public SpriteRenderer vehicleDisplay = null!;

    public Light parkingBrakeLight = null!;
    public Light checkEngineLight = null!;
    public Light alertLight = null!;
    public Light seatBeltLight = null!;
    public Light oilLevelLight = null!;
    public Light batteryLight = null!;
    public Light coolantLevelLight = null!;
    public Light dippedBeamLight = null!;
    public Light highBeamLight = null!;
    public Light immobiliserLight = null!;

    public SpriteRenderer parkingBrakeSymbol = null!;
    public SpriteRenderer checkEngineLightSymbol = null!;
    public SpriteRenderer alertLightSymbol = null!;
    public SpriteRenderer seatbeltLightSymbol = null!;
    public SpriteRenderer oilLevelLightSymbol = null!;
    public SpriteRenderer batteryLightSymbol = null!;
    public SpriteRenderer coolantLevelLightSymbol = null!;
    public SpriteRenderer dippedBeamLightSymbol = null!;
    public SpriteRenderer highBeamLightSymbol = null!;
    public SpriteRenderer immobiliserSymbol = null!;

    public Animator ignitionAnimator = null!;
    public GameObject carKeyContainer = null!;
    public GameObject carKeyInHand = null!;
    public Transform ignitionKeyPosition = null!;

    public GameObject headlightSwitch = null!;
    public MeshRenderer lowBeamMesh = null!;
    public MeshRenderer highBeamMesh = null!;
    public GameObject highBeamContainer = null!;
    public GameObject clusterLightsContainer = null!;

    public MeshRenderer leftDoorMesh = null!;
    public MeshRenderer rightDoorMesh = null!;

    public MeshRenderer radioMesh = null!;
    public MeshRenderer radioPowerDial = null!;
    public MeshRenderer radioVolumeDial = null!;
    public GameObject radioLight = null!;
    public GameObject heaterLight = null!;

    public GameObject leftWindowLight = null!;
    public GameObject rightWindowLight = null!;

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

    public MeshRenderer lowBeamMeshLod = null!;
    public MeshRenderer highBeamMeshLod = null!;

    public MeshRenderer backLightsMeshLod = null!;
    public MeshRenderer sideTopLightsMeshLod = null!;

    public MeshRenderer leftBlinkerMeshLod = null!;
    public MeshRenderer rightBlinkerMeshLod = null!;

    public MeshRenderer leftBlinkerMesh = null!;
    public MeshRenderer rightBlinkerMesh = null!;

    public Transform speedometerTransform = null!;
    public Transform tachometerTransform = null!;
    public Transform oilPressureTransform = null!;

    private Coroutine blinkersCoroutine = null!;

    public InteractTrigger windscreenWipersTrigger = null!;

    public InteractTrigger startIgnitionTrigger = null!;
    public InteractTrigger stopIgnitionTrigger = null!;

    public GameObject reverseLightsContainer = null!;
    public MeshRenderer reverseLightsMesh = null!;

    public Animator manualShiftAnimator = null!;

    public bool treatingPlayerInfection;

    public string[] upShiftString = null!;
    public string[] downShiftString = null!;

    public float mirrorAngleFloat;
    public float lightSwitchFloat;
    public float heatSpeedFloat;
    public float heatDirectionFloat;
    public float heatPositionFloat;

    public bool smoothRotation;

    public bool heaterOn;
    public float heaterTemp;
    public float heaterSpeed;

    public float steeringWheelAnimValue;
    public float steeringSpeed;

    //public bool windshieldShattered;

    public bool inIgnitionAnimation;

    public bool isHeaterCold;
    public bool isHeaterWarm;

    private float handbrakePullSpeed = 68f;
    private float handbrakeAnimValue;
    private float timeAtLastHandbrakePull;

    private Vector3 ignitionKeyScale = Vector3.one;
    private Vector3 LHD_Pos_Local = new Vector3(0.0489f, 0.1371f, -0.1566f);
    private Vector3 LHD_Pos_Server = new Vector3(0.0366f, 0.1023f, -0.1088f);
    private Vector3 LHD_Rot_Local = new Vector3(-3.446f, 3.193f, 172.642f);
    private Vector3 LHD_Rot_Server = new Vector3(-191.643f, 174.051f, -7.768005f);

    public int currentSweepStage;
    public bool hasSweepedDashboard;
    public bool hazardsBlinking;
    public bool hazardsOn;
    public bool reverseLightsOn;

    private float speedometerFloat;
    private float tachometerFloat;

    public bool disableAnimations;

    public bool lowBeamsOn;
    public bool highBeamsOn;

    public bool overrideCabinLightSwitch;
    public bool cabinLightSwitchEnabled = true;

    private float oilPressureFloat;
    private float turboPressureFloat;
    public bool overdriveSwitchEnabled;

    public bool twistingKey;
    public bool accessoryMode;
    public bool isCabLightOn;

    public bool liftGateOpen;
    public bool sideDoorOpen;

    [Header("Audio")]

    public RadioBehaviour liveRadioController = null!;

    public AudioSource[] allVehicleAudios = null!;
    public AudioClip[] streamerRadioClips = null!;

    public AudioSource roofAudio = null!;
    public AudioSource cabinLightSwitchAudio = null!;
    public AudioClip cabinLightSwitchToggle = null!;

    public AudioSource handbrakeAudio = null!;
    public AudioClip handbrakeOn = null!;
    public AudioClip handbrakeOff = null!;

    public AudioClip[] clutchInClips = null!;
    public AudioClip[] clutchOutClips = null!;

    public AudioSource engineAudio4 = null!;
    public AudioSource engineAudio3 = null!;
    public AudioClip stallEngine = null!;

    public AudioClip blinkOn = null!;
    public AudioClip blinkOff = null!;

    public AudioClip[] gearCrunchSounds = null!;
    public AudioClip[] shiftSounds = null!;

    public AudioSource roofRainAudio = null!;
    public AudioSource carKeySounds = null!;
    public AudioSource wiperAudio = null!;

    public AudioSource backUpBeeperAudio = null!;

    private Coroutine truckAlarmCoroutine = null!;
    public AudioSource alarmAudio = null!;
    public AudioSource heaterAudio = null!;

    public bool roofRainAudioActive;

    public bool engineKnockingAudioActive;
    public float engineKnockSpeed;
    private float timeSinceTogglingRadio;
    public bool alarmDebounce;
    private float timeAtLastAlarmPing;
    private float timeAtLastEVAPing;

    private float lastSongTime;
    public float minFrequency = 75.55f;
    public float maxFrequency = 255.50f;

    public bool isFmRadio = false;

    private float syncedSongTime;
    private float timeLastSyncedRadio;
    private float radioPingTimestamp;

    [Header("Materials")]

    public Material blinkerOnMat = null!;
    public Material clusterDialsOffMat = null!;
    public Material clusterDialsOnMat = null!; // special hex #FCFFB6, default hex #A0FFDC (EMISSIVE MAP)
    public Material heaterOffMat = null!;
    public Material heaterOnMat = null!; // special hex #FAFE7D, default hex #61FF75  (EMISSIVE MAP)
    public Material greyLightOffMat = null!;
    public Material redLightOffMat = null!;
    public Material clusterOffMaterial = null!;
    public Material clusterOnMaterial = null!; // special hex #FAFF92, default hex #88FFD3  (EMISSIVE MAP)
    public Material radioOffMaterial = null!;
    public Material radioOnMaterial = null!; // special hex #FAFE7D, default hex #61FF75  (EMISSIVE MAP)
    public Material windowOffMaterial = null!;
    public Material windowOnMaterial = null!; // special hex #FAFE7D, default hex #61FF75  (EMISSIVE MAP)
    public Material windshieldMat = null!;


    // --- INIT ---
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (StartOfRound.Instance.inShipPhase ||
            !IsServer)
            return;

        specialChance = 0.09f;
        System.Random rng = new System.Random(StartOfRound.Instance.randomMapSeed);
        isSpecial = rng.NextDouble() < specialChance;
        SetSpecialVariantRpc(isSpecial);
    }

    [Rpc(SendTo.Everyone)]
    public void SetSpecialVariantRpc(bool special)
    {
        isSpecial = special;
        SetVariant(isSpecial);
    }

    public void SetVariant(bool isJenson)
    {
        truckMat.mainTexture = isJenson ? rareTruckTex : defaultTruckTex;
        if (isJenson)
        {
            radioLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
            leftWindowLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
            rightWindowLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
            heaterLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
            clusterLightCol.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF); // #FAFEDE
            voiceModule.clusterLight.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF); // #FAFEDE

            vehicleDisplay.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF);
            vehicleDisplayLight.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF);

            // mat swaps
            clusterDialsOnMat = rareTruckDialsOn; // #FCFFB6
            clusterOnMaterial = rareTruckClusterOn; // #FAFF92
            radioOnMaterial = rareTruckRadioOn; // #FAFE7D
            heaterOnMat = rareHeaterOn; // #FAFE7D
            windowOnMaterial = rareWindowOn;
            //jcJensonSymbolObj.SetActive(true);

            radioTime.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33
            radioFrequency.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33
            voiceModule.clusterScreen.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33

            voiceModule.clusterTexts[0] = "sys status: [ok]";
            voiceModule.clusterTexts[15] = "high temp!";
            voiceModule.clusterTexts[13] = "error: wd606";
        }
    }

    public void OnEnable()
    {
        References.vanController = this;
    }

    public new void Awake()
    {
        ragdollPhysicsBody.interpolation = RigidbodyInterpolation.Interpolate;
        windwiperPhysicsBody1.interpolation = RigidbodyInterpolation.Interpolate;
        windwiperPhysicsBody2.interpolation = RigidbodyInterpolation.Interpolate;
        playerPhysicsBody.interpolation = RigidbodyInterpolation.None;
        playerPhysicsBody.freezeRotation = true;
        backDoorOpen = true; // hacky shit
        base.Awake();

        physicsRegion.priority = 1;
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;

        driverSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;
        middlePassengerSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;
        passengerSeatTrigger.playerPositionNode.transform.localPosition += seatNodePositionOffset;

        usingSwitchIgnition = false;
        SetTruckStats();
    }

    private void SetTruckStats()
    {
        idleSpeed = 80f;
        pushForceMultiplier = 156f;

        carTooltips = new string[]
        {
            "Gas pedal: [W]",
            "Brake pedal: [S]",
            "Clutch pedal: [L-Shift]",
            "Switch action: [LMB]"
        };

        turboBoostForce = 10000f; // 13500f
        turboBoostUpwardForce = 23000f; // 32400f

        steeringSpeed = 8f;

        baseSteeringWheelTurnSpeed = 4.2f;
        steeringWheelTurnSpeed = baseSteeringWheelTurnSpeed;

        handbrakeEngaged = true;
        inNeutral = true;

        throttleSpeed = 1f;
        maxThrottleSpeed = 2f;
        throttleReleaseSpeed = 3f;

        minimalBumpForce = 15000f;
        mediumBumpForce = 65000f;
        maximumBumpForce = 120000f;

        minInclineCompensation = 1.78f;
        maxInclineCompensation = 4.1f;
        maxInclineCompensationAngle = 32f;

        baseTransmissionHP = 20;
        transmissionHP = baseTransmissionHP;
        syncedTransmissionHP = baseTransmissionHP; 

        clutchSpeed = 4f;
        clutchReturnSpeed = 1.5f;
        minClutchReturnSpeed = 0.5f;

        jumpForce = 4900f; // 3600f

        brakeSpeed = 10000f;
        maxBrakingPower = 12000f;

        speed = 60;
        stability = 0.4f;

        handbrakeAudio.volume = 0.75f;
        carKeySounds.volume = 0.5f;
        backUpBeeperAudio.volume = 0.95f;
        backUpBeeperAudio.dopplerLevel = 0.1f;

        heaterTemp = 1f;
        isHeaterWarm = true;

        torqueForce = 1.6f;
        carMaxSpeed = 60f;
        pushVerticalOffsetAmount = 1.25f;

        baseCarHP = 48;

        if (!StartOfRound.Instance.inShipPhase)
        {
            carHP = baseCarHP;
            syncedCarHP = baseCarHP;
        }

        MinEngineRPM = 800f;
        MaxEngineRPM = 5000f;
        engineIntensityPercentage = MaxEngineRPM;

        carAcceleration = 350f;
        EngineTorque = 100f;
        engineReversePower = 6100f;

        SetWheelFriction();

        mainRigidbody.maxLinearVelocity = carMaxSpeed;
        mainRigidbody.maxAngularVelocity = 4f;
        chanceToStartIgnition = (float)UnityEngine.Random.Range(12, 45);

        FrontLeftWheel.mass = 150f;
        FrontRightWheel.mass = 150f;
        BackLeftWheel.mass = 150f;
        BackRightWheel.mass = 150f;

        mainRigidbody.automaticCenterOfMass = false;
        mainRigidbody.centerOfMass = new Vector3(0f, -0.26f, 0.25f);
        mainRigidbody.automaticInertiaTensor = false;

        JointSpring suspensionSpring = new JointSpring
        {
            spring = 18000f,
            damper = 3800f,
            targetPosition = 0.7f,
        };

        FrontLeftWheel.sprungMass = 228.2732f;
        FrontRightWheel.sprungMass = 228.2732f;
        BackLeftWheel.sprungMass = 271.7272f;
        BackRightWheel.sprungMass = 271.7272f;
        
        FrontRightWheel.forceAppPointDistance = 0.47f;
        FrontLeftWheel.forceAppPointDistance = 0.47f;
        BackRightWheel.forceAppPointDistance = 0.4f;
        BackLeftWheel.forceAppPointDistance = 0.4f;

        FrontRightWheel.suspensionSpring = suspensionSpring;
        FrontLeftWheel.suspensionSpring = suspensionSpring;
        BackRightWheel.suspensionSpring = suspensionSpring;
        BackLeftWheel.suspensionSpring = suspensionSpring;

        FrontRightWheel.wheelDampingRate = 3.1f;
        FrontLeftWheel.wheelDampingRate = 3.1f;
        BackRightWheel.wheelDampingRate = 3.1f;
        BackLeftWheel.wheelDampingRate = 3.1f;

        FrontRightWheel.suspensionDistance = 0.4f;
        FrontLeftWheel.suspensionDistance = 0.4f;
        BackRightWheel.suspensionDistance = 0.4f;
        BackLeftWheel.suspensionDistance = 0.4f;
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

    public new void Start()
    {
        StartCoroutine(SetCarRainCollisions());
        FrontLeftWheel.brakeTorque = maxBrakingPower;
        FrontRightWheel.brakeTorque = maxBrakingPower;
        BackLeftWheel.brakeTorque = maxBrakingPower;
        BackRightWheel.brakeTorque = maxBrakingPower;

        currentRadioClip = 0;
        decals = new DecalProjector[24];

        if (!StartOfRound.Instance.inShipPhase)
            return;

        loadedVehicleFromSave = true;
        mainRigidbody.isKinematic = true;
        transform.position = StartOfRound.Instance.magnetPoint.position + StartOfRound.Instance.magnetPoint.forward * 7f;
        magnetedToShip = true;
        inDropshipAnimation = false;
        hasDeliveredVehicle = true;
        hasBeenSpawned = true;
        StartMagneting();
    }

    public IEnumerator SetCarRainCollisions()
    {
        yield return new WaitForSeconds(4f);

        var particleTriggers = new[]
        {
            GlobalReferences.rainParticles,
            GlobalReferences.rainHitParticles,
            GlobalReferences.stormyRainParticles,
            GlobalReferences.stormyRainHitParticles,
            GlobalReferences.wesleyHurricaneRainParticles,
            GlobalReferences.wesleyHurricaneRainHitParticles,
            GlobalReferences.wesleyHurricaneSandParticles,
            GlobalReferences.wesleyForsakenRainParticles,
            GlobalReferences.wesleyForsakenRainHitParticles
        };

        for (int i = 0; i < particleTriggers.Length; i++)
        {
            if (particleTriggers[i] == null)
            {
                Plugin.Logger.LogDebug("ScanVan: Weather particle or Trigger is null!");
                continue;
            }

            var trigger = particleTriggers[i]!.trigger;
            for (int j = 0; j < weatherEffectBlockers.Length; j++)
            {
                int index = trigger.colliderCount + j;
                trigger.SetCollider(index, weatherEffectBlockers[j]);
            }
        }
        yield break;
    }

    // --- SYNC DATA ---
    public void SendClientSyncData()
    {
        if (magnetedToShip)
        {
            Vector3 eulerAngles = magnetTargetRotation.eulerAngles;
            MagnetCarRpc(magnetTargetPosition, eulerAngles, magnetStartPosition, magnetStartRotation, RoundManager.Instance.tempTransform.eulerAngles, averageVelocityAtMagnetStart);
        }

        if (turboBoosts > 0)
            AddTurboBoostRpc(turboBoosts);

        SyncClientDataRpc(carHP, steeringWheelAnimFloat, ignitionStarted, isSpecial);
    }

    [Rpc(SendTo.NotServer)]
    public void SyncClientDataRpc(int carHealth, float wheelRot, bool ignOn, bool isJenson)
    {
        if (IsHost)
            return;

        isSpecial = isJenson;
        SetVariant(isSpecial);

        carHP = carHealth;
        syncedCarHP = carHealth;
        syncedSteeringWheelRotation = wheelRot;
        steeringWheelAnimFloat = wheelRot;

        disableAnimations = !ignOn;
        inIgnitionAnimation = !ignOn;
        accessoryMode = ignOn;

        accessoryMode = ignOn;
        keyIsInIgnition = ignOn;

        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: ignOn);
        SetIgnition(ignOn, ignOn);
        SetFrontCabinLightOn(setOn: ignOn);
        TrySetCarIgnitionTriggers();

        if (ignOn) dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
        SetSymbolActive(vehicleDisplay, vehicleDisplayLight, ignOn);
        driversSideWindow.interactable = ignOn;
        passengersSideWindow.interactable = ignOn;
        ignitionCollider.enabled = ignOn;
        ignitionAnimator.SetInteger(IGNITION_ANIM, ignOn ? 1 : 0);
    }


    // --- STORAGE DOORS ---
    public new void SetBackDoorOpen(bool open)
    {
        liftGateOpen = open;
    }

    public void SetSideDoorOpen(bool open)
    {
        sideDoorOpen = open;
    }


    // --- CAB LIGHTING ---
    public void SetCabinLightSwitchLocalClient()
    {
        if (!overrideCabinLightSwitch && !cabinLightSwitchEnabled)
        {
            overrideCabinLightSwitch = true;
            cabinLightSwitchEnabled = true;
            roofAudio.PlayOneShot(cabinLightSwitchToggle);
            frontCabinLightContainer.SetActive(true);
            frontCabinLightMesh.material = headlightsOnMat;
            SetCabinLightSwitchRpc(true, true, true);
            return;
        }
        if (overrideCabinLightSwitch)
        {
            overrideCabinLightSwitch = false;
            cabinLightSwitchEnabled = true;
            roofAudio.PlayOneShot(cabinLightSwitchToggle);
            bool isCabinLightOn = isCabLightOn && keyIsInIgnition && accessoryMode && cabinLightSwitchEnabled;
            frontCabinLightContainer.SetActive(isCabinLightOn);
            frontCabinLightMesh.material = isCabinLightOn ? headlightsOnMat : greyLightOffMat;
            SetCabinLightSwitchRpc(true, isCabinLightOn, false);
            return;
        }
        cabinLightSwitchEnabled = false;
        roofAudio.PlayOneShot(cabinLightSwitchToggle);
        frontCabinLightContainer.SetActive(false);
        frontCabinLightMesh.material = greyLightOffMat;
        SetCabinLightSwitchRpc(false, false, false);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetCabinLightSwitchRpc(bool switchState, bool cabLightOn, bool setOverride)
    {
        overrideCabinLightSwitch = setOverride;
        cabinLightSwitchEnabled = switchState;
        roofAudio.PlayOneShot(cabinLightSwitchToggle);
        frontCabinLightContainer.SetActive(cabLightOn);
        frontCabinLightMesh.material = cabLightOn ? headlightsOnMat : greyLightOffMat;
    }

    public new void SetFrontCabinLightOn(bool setOn)
    {
        isCabLightOn = setOn;
        if (overrideCabinLightSwitch)
        {
            if (!frontCabinLightContainer.activeSelf)
            {
                frontCabinLightContainer.SetActive(true);
                frontCabinLightMesh.material = headlightsOnMat;
            }
            return;
        }
        if (cabinLightSwitchEnabled)
        {
            frontCabinLightContainer.SetActive(setOn);
            frontCabinLightMesh.material = setOn ? headlightsOnMat : greyLightOffMat;
            return;
        }
        frontCabinLightContainer.SetActive(false);
        frontCabinLightMesh.material = greyLightOffMat;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetFrontCabinLightRpc(bool setOn)
    {
        SetFrontCabinLightOn(setOn);
    }


    // --- HANDBRAKE METHODS ---
    public void ToggleHandbrake()
    {
        if (currentDriver != null && !localPlayerInControl)
            return;

        if (localPlayerInControl && Time.realtimeSinceStartup - timeAtLastHandbrakePull < 0.78f)
            return;

        timeAtLastHandbrakePull = Time.realtimeSinceStartup;
        handbrakeEngaged = !handbrakeEngaged;
        if (ignitionStarted)
        {
            if (handbrakeEngaged) PlayCombinedOccupantAnimation(currentDriver!, UPPER_BODY_HANDBRAKE_ON, LEFT_ARM_HANDBRAKE_ON, RIGHT_ARM_HANDBRAKE_ON, 0.075f);
            else PlayCombinedOccupantAnimation(currentDriver!, UPPER_BODY_HANDBRAKE_OFF, LEFT_ARM_HANDBRAKE_OFF, RIGHT_ARM_HANDBRAKE_OFF, 0.075f);
        }
        if (handbrakeEngaged)
        {
            handbrakeAudio.PlayOneShot(handbrakeOn);
        }
        else
        {
            handbrakeAudio.PlayOneShot(handbrakeOff);
        }
        SetHandbrakeRpc(handbrakeEngaged);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHandbrakeRpc(bool setHandbrake)
    {
        if (handbrakeEngaged == setHandbrake)
            return;

        timeAtLastHandbrakePull = Time.realtimeSinceStartup;
        handbrakeEngaged = setHandbrake;
        if (ignitionStarted)
        {
            if (handbrakeEngaged) PlayCombinedOccupantAnimation(currentDriver!, UPPER_BODY_HANDBRAKE_ON, LEFT_ARM_HANDBRAKE_ON, RIGHT_ARM_HANDBRAKE_ON, 0.075f);
            else PlayCombinedOccupantAnimation(currentDriver!, UPPER_BODY_HANDBRAKE_OFF, LEFT_ARM_HANDBRAKE_OFF, RIGHT_ARM_HANDBRAKE_OFF, 0.075f);
        }
        if (handbrakeEngaged)
        {
            handbrakeAudio.PlayOneShot(handbrakeOn);
        }
        else
        {
            handbrakeAudio.PlayOneShot(handbrakeOff);
        }
    }


    // --- TRY SET IGNITION METHOD ---
    public void TrySetCarIgnitionTriggers()
    {
        if (!localPlayerInControl && currentDriver != null)
            return;

        if (ignitionStarted)
        {
            startIgnitionTrigger.hoverTip = "Untwist key : [LMB]";
            startIgnitionTrigger.holdTip = "[ Untwisting key ]";

            startIgnitionTrigger.timeToHold = 0.75f;
            startIgnitionTrigger.timeToHoldSpeedMultiplier = 1f;
            return;
        }
        if (keyIsInIgnition)
        {
            bool switchIgnition = usingSwitchIgnition || currentDriver == null;
            startIgnitionTrigger.hoverTip = switchIgnition ? "Remove key : [LMB]" : "Try ignition : [LMB] (Hold)";
            startIgnitionTrigger.holdTip = switchIgnition ? "[ Removing Key ]" : "[ Trying ignition ]";

            startIgnitionTrigger.timeToHold = switchIgnition ? 0.75f : 1f;
            startIgnitionTrigger.timeToHoldSpeedMultiplier = switchIgnition ? 1f : 0f;
            return;
        }
        startIgnitionTrigger.hoverTip = "Try ignition : [LMB] (Hold)";
        startIgnitionTrigger.holdTip = "[ Trying ignition ]";

        startIgnitionTrigger.timeToHold = 1f;
        startIgnitionTrigger.timeToHoldSpeedMultiplier = 0f;
    }


    // --- TRY IGNITION METHOD ---
    public new void StartTryCarIgnition()
    {
        if (usingSwitchIgnition)
            return;

        if (!localPlayerInControl ||
            ignitionStarted)
            return;

        CancelIgnitionCoroutine();
        disableAnimations = true;
        inIgnitionAnimation = true;
        TryIgnitionRpc(keyIsInIgnition, isCabLightOn, engineStalled);
        TrySetCarIgnitionTriggers();
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: true));
    }

    private new IEnumerator TryIgnition(bool isLocalDriver)
    {
        if (currentDriver == null)
        {
            keyIgnitionCoroutine = null;
            yield break;
        }
        if (keyIsInIgnition)
        {
            if (engineStalled)
            {
                currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 9);
            }
            else
            {
                if (currentDriver?.playerBodyAnimator.GetInteger(CAR_ANIM) == 3)
                    currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 2);
                else
                    currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 12);
            }
            int animIndex = currentDriver!.playerBodyAnimator.GetInteger(CAR_ANIM);
            if (animIndex == 9) animIndex = 12;
            ignitionAnimator.SetInteger(IGNITION_ANIM, animIndex);
            TrySetCarIgnitionTriggers();
            engineStalled = false;
            SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(0.02f);
            carKeySounds.PlayOneShot(twistKey);
            SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(0.1467f);
        }
        else
        {
            currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 2);
            ignitionAnimator.SetInteger(IGNITION_ANIM, 2);
            TrySetCarIgnitionTriggers();
            SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: false);
            yield return new WaitForSeconds(0.6f);
            carKeySounds.PlayOneShot(insertKey);
            SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: true);
            TrySetCarIgnitionTriggers();
            yield return new WaitForSeconds(0.2f);
            carKeySounds.PlayOneShot(twistKey);
            SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(0.185f);
        }
        if (!isLocalDriver) yield break;
        bool clutchInterlock = clutchPedalPressed && clutchEngagement <= 0.31f;
        TryStartIgnitionRpc(clutchInterlock);
        SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
        accessoryMode = true;
        SetFrontCabinLightOn(setOn: accessoryMode);
        TrySetCarIgnitionTriggers();
        driversSideWindow.interactable = accessoryMode;
        passengersSideWindow.interactable = accessoryMode;
        if (dashboardSymbolPreStartup == null && !hasSweepedDashboard)
        {
            dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
        }
        SetSymbolActive(vehicleDisplay, vehicleDisplayLight, true);
        if (clutchInterlock) 
        {
            PlayIgnitionAudio();
        }
        else
        {
            yield break;
        }
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.8f, 2f)); //0.4, 1.1f
        if ((float)UnityEngine.Random.Range(0, 100) < chanceToStartIgnition && clutchInterlock)
        {
            CancelIgnitionAnimation(ignitionOn: true, setIgnitionAnim: true);
            disableAnimations = false;
            inIgnitionAnimation = false;
            accessoryMode = true;
            currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 1);
            SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: true);
            SetIgnition(started: true, cabLightOn: true);
            SetFrontCabinLightOn(setOn: keyIsInIgnition);
            startIgnitionTrigger.StopInteraction();
            TrySetCarIgnitionTriggers();
            startIgnitionTrigger.StopInteraction();
            startIgnitionTrigger.currentCooldownValue = 1f;
            StartIgnitionRpc(false);
        }
        else
        {
            chanceToStartIgnition += 22f;
            chanceToStartIgnition = Mathf.Clamp(chanceToStartIgnition, 0f, 99f);
        }
        yield break;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void TryIgnitionRpc(bool setKeyInSlot, bool cabLightActive, bool hasStalled)
    {
        CancelIgnitionCoroutine();
        disableAnimations = true;
        inIgnitionAnimation = true;
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: setKeyInSlot);
        if (!isCabLightOn && cabLightActive) SetFrontCabinLightOn(cabLightActive);
        engineStalled = hasStalled;
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: false));
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void TryStartIgnitionRpc(bool shiftInterlock)
    {
        SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
        accessoryMode = true;
        SetFrontCabinLightOn(setOn: accessoryMode);
        driversSideWindow.interactable = accessoryMode;
        passengersSideWindow.interactable = accessoryMode;
        if (dashboardSymbolPreStartup == null && !hasSweepedDashboard)
        {
            dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
        }
        SetSymbolActive(vehicleDisplay, vehicleDisplayLight, true);
        if (shiftInterlock) PlayIgnitionAudio();
    }

    public void PlayIgnitionAudio()
    {
        engineAudio1.Stop();
        engineAudio1.clip = revEngineStart;
        engineAudio1.volume = 0.7f;
        engineAudio1.PlayOneShot(engineRev);
        engineAudio1.pitch = 1f;
        carEngine1AudioActive = true;
    }


    // --- CANCEL IGNITION METHOD ---
    public new void CancelTryCarIgnition()
    {
        if (usingSwitchIgnition)
            return;

        if (!localPlayerInControl ||
            ignitionStarted ||
            keyIgnitionCoroutine == null ||
            (keyIgnitionCoroutine == null && startIgnitionTrigger.isBeingHeldByPlayer))
            return;

        // hopefully fix a bug where the wrong animation can play?
        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (localPlayer.playerBodyAnimator.GetInteger(CAR_ANIM) == 2 && keyIsInIgnition)
            localPlayer.playerBodyAnimator.SetInteger(CAR_ANIM, 3);
        else if (localPlayer.playerBodyAnimator.GetInteger(CAR_ANIM) == 12 && keyIsInIgnition)
            localPlayer.playerBodyAnimator.SetInteger(CAR_ANIM, 3);
        else if (localPlayer.playerBodyAnimator.GetInteger(CAR_ANIM) == 9 && keyIsInIgnition)
            localPlayer.playerBodyAnimator.SetInteger(CAR_ANIM, 3);
        else
            localPlayer.playerBodyAnimator.SetInteger(CAR_ANIM, 0);

        CancelIgnitionAnimation(ignitionOn: false, setIgnitionAnim: false);
        disableAnimations = true;
        inIgnitionAnimation = false;
        TrySetCarIgnitionTriggers();

        int playerAnimIndex = localPlayer.playerBodyAnimator.GetInteger(CAR_ANIM);
        int ignitionAnimIndex = playerAnimIndex;
        if (playerAnimIndex == 13) ignitionAnimIndex = 3;
        ignitionAnimator.SetInteger(IGNITION_ANIM, ignitionAnimIndex);

        CancelTryIgnitionRpc(keyIsInIgnition, isCabLightOn, accessoryMode, playerAnimIndex, ignitionAnimIndex);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CancelTryIgnitionRpc(bool setKeyInSlot, bool cabinLightOn, bool accessoriesActive, int playerAnimIndex, int ignitionAnimIndex)
    {
        CancelIgnitionAnimation(ignitionOn: false, setIgnitionAnim: false);
        disableAnimations = true;
        inIgnitionAnimation = false;

        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, playerAnimIndex);
        ignitionAnimator.SetInteger(IGNITION_ANIM, ignitionAnimIndex);

        // account for netlag when the key is first inserted
        if (setKeyInSlot == true && !keyIsInIgnition)
        {
            carKeySounds.PlayOneShot(insertKey);
        }
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: setKeyInSlot);
        if (keyIsInIgnition == true)
        {
            if (cabinLightOn && !isCabLightOn) 
                SetFrontCabinLightOn(setOn: cabinLightOn);
            if (accessoriesActive && !accessoryMode)
            {
                accessoryMode = true;
                driversSideWindow.interactable = accessoryMode;
                passengersSideWindow.interactable = accessoryMode;
                if (dashboardSymbolPreStartup == null && !hasSweepedDashboard)
                {
                    dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
                }
            }
        }
    }


    // --- START IGNITION METHOD ---
    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void StartIgnitionRpc(bool setTriggers)
    {
        CancelIgnitionAnimation(ignitionOn: true, setIgnitionAnim: true);
        disableAnimations = false;
        inIgnitionAnimation = false;
        accessoryMode = true;
        currentDriver?.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: true);
        SetIgnition(started: true, cabLightOn: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        if (setTriggers) TrySetCarIgnitionTriggers();
    }

    public void SetIgnition(bool started, bool cabLightOn)
    {
        SetFrontCabinLightOn(cabLightOn);
        carEngine1AudioActive = started;
        if (started)
        {
            disableAnimations = false;
            inIgnitionAnimation = false;
            accessoryMode = true;

            if (started == ignitionStarted)
                return;

            ignitionStarted = true;
            carExhaustParticle.Play();
            engineAudio1.Stop();
            engineAudio1.PlayOneShot(engineStartSuccessful);
            engineAudio1.clip = engineRun;
            return;
        }
        ignitionStarted = false;
        carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }


    // --- UNTWIST KEY IN IGNITION METHOD ---
    public void UntwistKeyInIgnition()
    {
        if (keyIsInIgnition && !ignitionStarted && !usingSwitchIgnition)
            return;

        if (currentDriver != null && !localPlayerInControl)
            return;

        if (!ignitionStarted || inIgnitionAnimation)
            return;

        CancelIgnitionCoroutine();
        keyIgnitionCoroutine = StartCoroutine(UntwistKey());
        TrySetCarIgnitionTriggers();
        UntwistKeyInIgnitionRpc();
    }

    private IEnumerator UntwistKey()
    {
        disableAnimations = true;
        inIgnitionAnimation = false;
        // untwist key while engine running, but keep key in
        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 7);
        ignitionAnimator.SetInteger(IGNITION_ANIM, 7);
        yield return new WaitForSeconds(0.08f);
        if (dashboardSymbolPreStartup != null)
        {
            StopCoroutine(dashboardSymbolPreStartup);
            dashboardSymbolPreStartup = null!;
            StopPreIgnitionChecks();
        }
        carKeySounds.PlayOneShot(twistKey);
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: true);
        SetIgnition(started: false, cabLightOn: true);
        engineAudio3.volume = 0.775f;
        engineAudio3.PlayOneShot(stallEngine);
        TrySetCarIgnitionTriggers();
        yield return new WaitForSeconds(0.08f);
        keyIgnitionCoroutine = null;
        yield break;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void UntwistKeyInIgnitionRpc()
    {
        CancelIgnitionCoroutine();
        keyIgnitionCoroutine = StartCoroutine(UntwistKey());
    }


    // --- REMOVE IGNITION METHOD ---
    public new void RemoveKeyFromIgnition()
    {
        if (keyIsInIgnition && !usingSwitchIgnition && localPlayerInControl)
            return;

        if (currentDriver != null && !localPlayerInControl)
            return;

        if (ignitionStarted || !keyIsInIgnition || inIgnitionAnimation)
            return;

        CancelIgnitionCoroutine();
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
        TrySetCarIgnitionTriggers();
        usingSwitchIgnition = false;
        chanceToStartIgnition = 20f;
        RemoveKeyFromIgnitionRpc();
    }

    private new IEnumerator RemoveKey()
    {
        disableAnimations = true;
        inIgnitionAnimation = false;
        if (currentDriver?.playerBodyAnimator.GetInteger(CAR_ANIM) == 0)
            currentDriver?.playerBodyAnimator.SetTrigger(REMOVE_IN_IGNITION);
        else 
            currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 8);
        ignitionAnimator.SetInteger(IGNITION_ANIM, 0);
        engineStalled = false;
        yield return new WaitForSeconds(ignitionStarted ? 0.18f : 0.26f);
        if (dashboardSymbolPreStartup != null)
        {
            StopCoroutine(dashboardSymbolPreStartup);
            dashboardSymbolPreStartup = null!;
            StopPreIgnitionChecks();
        }
        carKeySounds.PlayOneShot(removeKey);
        SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: false);
        SetIgnition(started: false, cabLightOn: false);
        accessoryMode = false;
        SetSymbolActive(vehicleDisplay, vehicleDisplayLight, false);
        driversSideWindow.interactable = false;
        passengersSideWindow.interactable = false;
        TrySetCarIgnitionTriggers();
        yield return new WaitForSeconds(0.73f);
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: false);
        keyIgnitionCoroutine = null;
        yield break;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void RemoveKeyFromIgnitionRpc()
    {
        CancelIgnitionCoroutine();
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
    }

    // --- DASHBOARD LIGHTING METHODS ---
    // there will be a better way to go about this
    // but i'm lazy right now
    public IEnumerator PreIgnitionSymbolCheck()
    {
        //if (jcJensonSymbolObj.activeSelf) SetSymbolActive(jcJensonSymbol, jcJensonLight, true);
        SetSymbolActive(dippedBeamLightSymbol, dippedBeamLight, true);
        SetSymbolActive(highBeamLightSymbol, highBeamLight, true);
        SetSymbolActive(parkingBrakeSymbol, parkingBrakeLight, true);
        SetSymbolActive(seatbeltLightSymbol, seatBeltLight, true);
        SetSymbolActive(oilLevelLightSymbol, oilLevelLight, true);
        SetSymbolActive(batteryLightSymbol, batteryLight, true);
        SetSymbolActive(coolantLevelLightSymbol, coolantLevelLight, true);
        SetSymbolActive(alertLightSymbol, alertLight, true);
        SetSymbolActive(checkEngineLightSymbol, checkEngineLight, true);
        currentSweepStage = 1;
        yield return new WaitForSeconds(1.0f);

        SetSymbolActive(dippedBeamLightSymbol, dippedBeamLight, lowBeamsOn);
        SetSymbolActive(highBeamLightSymbol, highBeamLight, highBeamsOn);
        currentSweepStage = 2;
        yield return new WaitForSeconds(1.0f);

        SetSymbolActive(seatbeltLightSymbol, seatBeltLight, false);
        SetSymbolActive(parkingBrakeSymbol, parkingBrakeLight, handbrakeEngaged);
        currentSweepStage = 3;
        yield return new WaitForSeconds(1.0f);

        SetSymbolActive(oilLevelLightSymbol, oilLevelLight, carHP <= 25);
        SetSymbolActive(batteryLightSymbol, batteryLight, !ignitionStarted || carHP <= 22);
        SetSymbolActive(coolantLevelLightSymbol, coolantLevelLight, carHP <= 30);
        SetSymbolActive(alertLightSymbol, alertLight, carHP <= 16);
        SetSymbolActive(checkEngineLightSymbol, checkEngineLight, carHP <= 38);

        currentSweepStage = 4;
        hasSweepedDashboard = true;
    }

    private void StopPreIgnitionChecks()
    {
        hasSweepedDashboard = false;
        currentSweepStage = 0;
        //if (jcJensonSymbolObj.activeSelf) SetSymbolActive(jcJensonSymbol, jcJensonLight, false);
        SetSymbolActive(dippedBeamLightSymbol, dippedBeamLight, false);
        SetSymbolActive(highBeamLightSymbol, highBeamLight, false);
        SetSymbolActive(parkingBrakeSymbol, parkingBrakeLight, false);
        SetSymbolActive(seatbeltLightSymbol, seatBeltLight, false);
        SetSymbolActive(oilLevelLightSymbol, oilLevelLight, false);
        SetSymbolActive(batteryLightSymbol, batteryLight, false);
        SetSymbolActive(coolantLevelLightSymbol, coolantLevelLight, false);
        SetSymbolActive(alertLightSymbol, alertLight, false);
        SetSymbolActive(checkEngineLightSymbol, checkEngineLight, false);
    }


    // --- MISC IGNITION STUFF ---
    public void CancelIgnitionAnimation(bool ignitionOn, bool setIgnitionAnim)
    {
        CancelIgnitionCoroutine();
        keyIsInDriverHand = false;
        twistingKey = false;
        carEngine1AudioActive = ignitionOn;
        if (setIgnitionAnim) ignitionAnimator.SetInteger("SAIgnition_Anim", ignitionOn ? 1 : 0);
    }

    private void CancelIgnitionCoroutine()
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
            keyIgnitionCoroutine = null;
        }
    }

    public void SetKeyIgnitionValues(bool trying, bool keyInHand, bool keyInSlot)
    {
        twistingKey = trying;
        keyIsInDriverHand = keyInHand;
        keyIsInIgnition = keyInSlot;
    }


    // --- GENERAL REPEAT METHODS ---
    public void SetTriggerHoverTip(InteractTrigger trigger, string tip)
    {
        trigger.hoverTip = tip;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void CancelSetPlayerInSeatRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (GameNetworkManager.Instance.localPlayerController != playerController)
            return;
        GlobalUtilities.CancelVehicleSeatInteraction();
    }


    // --- DRIVER OCCUPANT METHODS ---
    public void SetDriverInCar()
    {
        SetDriverInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetDriverInCarServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentDriver != null)
        {
            CancelSetPlayerInSeatRpc(playerId);
            return;
        }
        currentDriver = StartOfRound.Instance.allPlayerScripts[playerId];
        NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[playerId].actualClientId);
        SetDriverInCarOwnerRpc();
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    public void SetDriverInCarOwnerRpc()
    {
        PlayerUtils.disableAnimationSync = true;
        frontLeftSeat.SetLocalPlayerIntoSeat();
        ActivateControl();
        SetTriggerHoverTip(driverSideDoorTrigger, "Exit : [LMB]");
        ignitionCollider.enabled = true;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        engineStalled = false;
        TrySetCarIgnitionTriggers();
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, 0.5f);
        if (keyIsInIgnition) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        if (ignitionStarted) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 1);
        if (driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        SetDriverInCarClientsRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, keyIsInIgnition, ignitionStarted);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetDriverInCarClientsRpc(int playerId, bool setKeyInSlot, bool setStarted)
    {
        currentDriver = StartOfRound.Instance.allPlayerScripts[playerId];
        ignitionCollider.enabled = false;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        engineStalled = false;
        frontLeftSeat.SetPlayerAnimations(StartOfRound.Instance.allPlayerScripts[playerId], false);
        keyIsInIgnition = setKeyInSlot;
        ignitionStarted = setStarted;
        if (setKeyInSlot) currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        if (setStarted) currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, 1);
    }

    public new void ExitDriverSideSeat()
    {
        if (!localPlayerInControl)
            return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        int exitPoint = CanExitCar(passengerSide: false);
        if (exitPoint == 0) if (!driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(driverSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(driverSideExitPoints[1].position);
    }

    public void OnDriverExit()
    {
        if (!IsSpawned || 
            NetworkManager == null || 
            !NetworkManager.IsListening)
        {
            return;
        }
        PlayerUtils.disableAnimationSync = false;
        localPlayerInControl = false;
        SetTriggerHoverTip(driverSideDoorTrigger, "Use door : [LMB]");
        disableAnimations = !ignitionStarted;
        inIgnitionAnimation = false;
        usingSwitchIgnition = false;
        ignitionCollider.enabled = true;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        GlobalUtilities.ResetHUDToolTips(GameNetworkManager.Instance.localPlayerController);
        if (currentDriver != GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.DisplayTip("Err?",
                "This state should not occur! aborting!");
            return;
        }
        DisableControl();
        CancelIgnitionAnimation(ignitionOn: ignitionStarted, setIgnitionAnim: true);
        SetIgnition(started: ignitionStarted, cabLightOn: isCabLightOn);
        chanceToStartIgnition = 20f;
        TrySetCarIgnitionTriggers();
        engineStalled = false;
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
        OnDriverExitRpc(
            GameNetworkManager.Instance.localPlayerController, 
            syncedPosition, 
            syncedRotation, 
            ignitionStarted, 
            keyIsInIgnition,
            isCabLightOn,
            accessoryMode);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnDriverExitRpc(NetworkBehaviourReference playerNetObjRef, Vector3 carLocation, Quaternion carRotation, bool setIgnitionState, bool setKeyInSlot, bool cabinLightOn, bool preIgnition)
    {
        if (IsServer) NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerObj))
        {
            Plugin.Logger.LogError("OnDriverExitRpc failed to find player object reference from network behaviour!");
            return;
        }
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        drivePedalPressed = false;
        brakePedalPressed = false;
        currentDriver = null;
        frontLeftSeat.ReturnPlayerAnimations(playerObj, false);
        accessoryMode = preIgnition;
        keyIsInIgnition = setKeyInSlot;
        ignitionStarted = setIgnitionState;
        if (ignitionStarted && !carExhaustParticle.isEmitting) carExhaustParticle.Play();
        else if (!ignitionStarted && carExhaustParticle.isEmitting) carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        disableAnimations = !ignitionStarted;
        inIgnitionAnimation = false;
        usingSwitchIgnition = false;
        ignitionCollider.enabled = true;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        CancelIgnitionAnimation(ignitionOn: ignitionStarted, setIgnitionAnim: true);
        SetIgnition(started: ignitionStarted, cabLightOn: cabinLightOn);
        TrySetCarIgnitionTriggers();
        engineStalled = false;
    }


    // --- MIDDLE PASSENGER OCCUPANT METHODS ---
    public void SetMiddlePassengerInCar()
    {
        SetMiddlePassengerInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetMiddlePassengerInCarServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentMiddlePassenger != null)
        {
            CancelSetPlayerInSeatRpc(playerId);
            return;
        }
        currentMiddlePassenger = playerController;
        SetMiddlePassengerInCarRpc(playerId);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetMiddlePassengerInCarRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            Plugin.Logger.LogError($"ScanVan: No middle passenger found! clientId? {playerId}");
            return;
        }
        if (GameNetworkManager.Instance.localPlayerController == playerController)
            SetPassengerIntoMiddleSeat();
        else
            frontMiddleSeat.SetPlayerAnimations(playerController, false);
        currentMiddlePassenger = playerController;
        currentMiddlePassenger.playerBodyAnimator.SetFloat(ANIMATION_SPEED, 0.5f);
    }

    public void SetPassengerIntoMiddleSeat()
    {
        PlayerUtils.disableAnimationSync = true;
        frontMiddleSeat.SetLocalPlayerIntoSeat();
        localPlayerInMiddlePassengerSeat = true;
        SetTriggerHoverTip(driverSideDoorTrigger, "Exit : [LMB]");
        SetTriggerHoverTip(passengerSideDoorTrigger, "Exit : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, 0.5f);
        if (driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        if (passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
    }

    public void ExitMiddlePassengerSideSeat(bool isPassengerSide)
    {
        if (!localPlayerInMiddlePassengerSeat)
            return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        int exitPoint = CanExitCar(passengerSide: isPassengerSide);
        if (isPassengerSide)
        {
            if (exitPoint == 0) if (!passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
            if (exitPoint != -1)
            {
                GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[exitPoint].position);
                return;
            }
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[1].position);
            return;
        }
        if (exitPoint == 0) if (!driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(driverSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(driverSideExitPoints[1].position);
    }

    public void OnMiddlePassengerExit()
    {
        if (!IsSpawned || 
            NetworkManager == null || 
            !NetworkManager.IsListening)
        {
            return;
        }
        PlayerUtils.disableAnimationSync = false;
        localPlayerInMiddlePassengerSeat = false;
        SetTriggerHoverTip(driverSideDoorTrigger, "Use door : [LMB]");
        SetTriggerHoverTip(passengerSideDoorTrigger, "Use door : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        GlobalUtilities.ResetHUDToolTips(GameNetworkManager.Instance.localPlayerController);
        if (currentMiddlePassenger != GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.DisplayTip("Err?",
                "This state should not occur! aborting!");
            return;
        }
        currentMiddlePassenger = null!;
        OnMiddlePassengerExitRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            GameNetworkManager.Instance.localPlayerController.transform.position);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnMiddlePassengerExitRpc(int playerId, Vector3 exitPoint)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            !playerController.isPlayerControlled)
        {
            return;
        }
        if (playerController == GameNetworkManager.Instance.localPlayerController)
            return;
        frontMiddleSeat.ReturnPlayerAnimations(StartOfRound.Instance.allPlayerScripts[playerId], false);
        playerController.TeleportPlayer(exitPoint, false, 0f, false, true);
        currentMiddlePassenger = null!;
    }


    // --- PASSENGER OCCUPANT METHODS ---
    public void SetPassengerInCar()
    {
        SetPassengerInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetPassengerInCarServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentPassenger != null)
        {
            CancelSetPlayerInSeatRpc(playerId);
            return;
        }
        currentPassenger = playerController;
        SetPassengerInCarRpc(playerId);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetPassengerInCarRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            Plugin.Logger.LogError($"ScanVan: No passenger found! clientId? {playerId}");
            return;
        }
        if (GameNetworkManager.Instance.localPlayerController == playerController) 
            SetPassengerIntoPassengerSeat();
        else 
            frontRightSeat.SetPlayerAnimations(playerController, false);
        currentPassenger = playerController;
        currentPassenger.playerBodyAnimator.SetFloat(ANIMATION_SPEED, 0.5f);
    }

    public void SetPassengerIntoPassengerSeat()
    {
        PlayerUtils.disableAnimationSync = true;
        frontRightSeat.SetLocalPlayerIntoSeat();
        localPlayerInPassengerSeat = true;
        SetTriggerHoverTip(passengerSideDoorTrigger, "Exit : [LMB]");
        if (passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
    }

    public new void ExitPassengerSideSeat()
    {
        if (!localPlayerInPassengerSeat || localPlayerInMiddlePassengerSeat)
            return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        int exitPoint = CanExitCar(passengerSide: true);
        if (exitPoint == 0) if (!passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        if (exitPoint != -1)
        {
            GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[exitPoint].position);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.TeleportPlayer(passengerSideExitPoints[1].position);
    }

    public new void OnPassengerExit()
    {
        if (!IsSpawned ||
            NetworkManager == null ||
            !NetworkManager.IsListening)
        {
            return;
        }
        PlayerUtils.disableAnimationSync = false;
        localPlayerInPassengerSeat = false;
        SetTriggerHoverTip(passengerSideDoorTrigger, "Use door : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger(CAR_ANIM, 0);
        GlobalUtilities.ResetHUDToolTips(GameNetworkManager.Instance.localPlayerController);
        if (currentPassenger != GameNetworkManager.Instance.localPlayerController)
        {
            HUDManager.Instance.DisplayTip("Err?",
                "This state should not occur! aborting!");
            return;
        }
        currentPassenger = null!;
        OnPassengerExitRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            GameNetworkManager.Instance.localPlayerController.transform.position);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnPassengerExitRpc(int playerId, Vector3 exitPoint)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null ||
            !playerController.isPlayerControlled)
        {
            return;
        }
        if (playerController == GameNetworkManager.Instance.localPlayerController)
            return;
        frontRightSeat.ReturnPlayerAnimations(StartOfRound.Instance.allPlayerScripts[playerId], false);
        playerController.TeleportPlayer(exitPoint, false, 0f, false, true);
        currentPassenger = null!;
    }


    // --- LEAVE OCCUPANT MID-GAME METHODS ---
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void OnDriverLeaveGameServerRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            return;
        }
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
        OnDriverLeave(playerController, ignitionStarted, keyIsInIgnition, isCabLightOn, accessoryMode);
        OnDriverLeaveGameRpc(playerId, syncedPosition, syncedRotation, ignitionStarted, keyIsInIgnition, isCabLightOn, accessoryMode);
    }

    public void OnDriverLeave(PlayerControllerB playerController, bool setIgnitionState, bool setKeyInSlot, bool cabinLightOn, bool preIgnition)
    {
        drivePedalPressed = false;
        brakePedalPressed = false;
        currentDriver = null;

        accessoryMode = preIgnition;
        keyIsInIgnition = setKeyInSlot;
        ignitionStarted = setIgnitionState;

        if (ignitionStarted && !carExhaustParticle.isEmitting) carExhaustParticle.Play();
        else if (!ignitionStarted && carExhaustParticle.isEmitting) carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        disableAnimations = !ignitionStarted;
        inIgnitionAnimation = false;

        ignitionCollider.enabled = setKeyInSlot;
        startIgnitionTrigger.isBeingHeldByPlayer = false;

        frontLeftSeat.ReturnPlayerAnimations(playerController, false);
        playerController.TeleportPlayer(Vector3.zero, false, 0f, false, true);

        CancelIgnitionAnimation(ignitionOn: ignitionStarted, setIgnitionAnim: true);
        SetIgnition(started: ignitionStarted, cabLightOn: accessoryMode);
        engineStalled = false;
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void OnDriverLeaveGameRpc(int playerId, Vector3 carLocation, Quaternion carRotation, bool setIgnitionState, bool setKeyInSlot, bool cabinLightOn, bool preIgnition)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            return;
        }
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        OnDriverLeave(playerController, setIgnitionState, setKeyInSlot, cabinLightOn, preIgnition);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void OnMiddlePassengerLeaveGameRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            return;
        }
        frontMiddleSeat.ReturnPlayerAnimations(playerController, false);
        playerController.TeleportPlayer(Vector3.zero, false, 0f, false, true);
        currentMiddlePassenger = null!;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void OnPassengerLeaveGameRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        if (playerController == null)
        {
            return;
        }
        frontRightSeat.ReturnPlayerAnimations(playerController, false);
        playerController.TeleportPlayer(Vector3.zero, false, 0f, false, true);
        currentPassenger = null!;
    }


    // --- OCCUPANT EXITING METHODS ---
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


    // --- PLAYER-VEHICLE COLLISION --- 
    // None of this is needed, with the way we have collisions for players-vehicles setup, we don't need to toggle layers anymore, and 
    // we don't have to worry about buggy host-interactions anymore with our setup.
    public new void EnableVehicleCollisionForAllPlayers()
    {
        Plugin.Logger.LogDebug("ScanVan: Attempted to enable collision, but this is unnecessary!");
        return;
    }

    public new void DisableVehicleCollisionForAllPlayers()
    {
        Plugin.Logger.LogDebug("ScanVan: Attempted to disable collision, but this is unnecessary!");
        return;
    }

    public new void SetVehicleCollisionForPlayer(bool setEnabled, PlayerControllerB player)
    {
        Plugin.Logger.LogDebug("ScanVan: Attempted to set collision, but this is unnecessary!");
        return;
    }


    // --- PLAYER INPUT TO VEHICLE INPUT & VEHICLE CONTROL METHODS ---
    private new void GetVehicleInput()
    {
        PlayerControllerB localDriver = GameNetworkManager.Instance.localPlayerController;
        if (localDriver == null)
            return;

        if (localDriver.isTypingChat ||
            localDriver.quickMenuManager.isMenuOpen)
            return;

        SyncPlayerInputs();

        moveInputVector = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();
        moveInputVector.x = StartOfRound.Instance.localPlayerUsingController ? moveInputVector.x : Mathf.Round(moveInputVector.x);
        moveInputVector.y = Mathf.Round(moveInputVector.y);

        brakePedalPressed = Plugin.VehicleControlsInstance.BrakePedalKey.IsPressed();
        brakeInput = Mathf.MoveTowards(brakeInput, brakePedalPressed ? 1f : 0f, 10f * Time.deltaTime);

        clutchPedalPressed = Plugin.VehicleControlsInstance.ClutchPedalKey.IsPressed();
        clutchInput = Mathf.MoveTowards(clutchInput, clutchPedalPressed ? 1f : 0f, 10f * Time.deltaTime);

        smoothRotation = UserConfig.SmoothWheel.Value;
        if (!ignitionStarted)
        {
            steeringAnimValue = 0f;
            steeringWheelAnimValue = 0f;
            drivePedalPressed = false;
            throttleInput = 0f;
            brakeInput = 0f;
            return;
        }

        /*
        if (Plugin.VehicleControlsInstance.SteerLeftKey.IsPressed()) steeringAnimValue = -1f;
        else if (Plugin.VehicleControlsInstance.SteerRightKey.IsPressed()) steeringAnimValue = 1f;
        else steeringAnimValue = 0f;
        */

        steeringAnimValue = moveInputVector.x;

        if (UserConfig.SmoothWheel.Value)
        {
            if (steeringAnimValue == 0)
                steeringWheelAnimValue = NormaliseFloat(Mathf.Lerp(steeringWheelAnimValue, 0f, steeringSpeed * Time.deltaTime));
            else
                steeringWheelAnimValue = NormaliseFloat(Mathf.Lerp(steeringWheelAnimValue, steeringAnimValue, steeringSpeed * Time.deltaTime));
        }
        else
        {
            steeringWheelAnimValue = steeringAnimValue;
        }

        drivePedalPressed = Plugin.VehicleControlsInstance.GasPedalKey.IsPressed() || (inReverse && Plugin.VehicleControlsInstance.ReversePedalKey.IsPressed());
        gasInput = Mathf.MoveTowards(gasInput, drivePedalPressed ? 1f : 0f, 10f * Time.deltaTime);

        float avgSpeed = averageVelocity.magnitude;
        if (avgSpeed < 0.5f)
        {
            return;
        }

        float autoCenterSpeed = Mathf.Clamp01(avgSpeed / maxAutoCenterSpeed);
        if (moveInputVector.x == 0f && UserConfig.RecenterWheel.Value)
            steeringWheelAnimFloat = NormaliseFloat(Mathf.MoveTowards(steeringWheelAnimFloat, 0f, steeringWheelTurnSpeed * autoCenterSpeed * Time.deltaTime / 6f));
    }

    private void SyncPlayerInputs()
    {
        if (syncedMoveInputVector != moveInputVector || 
            (syncedDrivePedalPressed != drivePedalPressed || syncedBrakePedalPressed != brakePedalPressed || syncedClutchPedalPressed != clutchPedalPressed))
        {
            syncedMoveInputVector = moveInputVector;
            syncedDrivePedalPressed = drivePedalPressed;
            syncedBrakePedalPressed = brakePedalPressed;
            syncedClutchPedalPressed = clutchPedalPressed;
            SyncPlayerInputsRpc(moveInputVector, drivePedalPressed, brakePedalPressed, clutchPedalPressed);
        }
    }

    private new void ActivateControl()
    {
        Plugin.VehicleControlsInstance.JumpKey.performed += DoTurboBoost;
        Plugin.VehicleControlsInstance.GearShiftForwardKey.performed += ShiftGearForwardInput;
        Plugin.VehicleControlsInstance.GearShiftBackwardKey.performed += ShiftGearBackInput;
        Plugin.VehicleControlsInstance.ToggleMagnetKey.performed += ActivateMagnet;
        Plugin.VehicleControlsInstance.ToggleHeadlightsKey.performed += ActivateHeadlights;
        Plugin.VehicleControlsInstance.ToggleWipersKey.performed += ActivateWipers;
        Plugin.VehicleControlsInstance.ActivateHornKey.performed += ActivateHorn;
        Plugin.VehicleControlsInstance.ActivateHornKey.canceled += ActivateHorn;

        Plugin.VehicleControlsInstance.SwitchIgnitionKey.performed += SetUsingSwitchIgnition;
        Plugin.VehicleControlsInstance.SwitchIgnitionKey.canceled += SetUsingSwitchIgnition;

        currentDriver = GameNetworkManager.Instance.localPlayerController;
        localPlayerInControl = true;
        steeringAnimValue = 0f;
        steeringWheelAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
        clutchPedalPressed = false;
        throttleInput = 0f;
        brakeInput = 0f;
        clutchEngagement = 1f;
        clutchSlip = 0f;
    }

    private new void DisableControl()
    {
        Plugin.VehicleControlsInstance.JumpKey.performed -= DoTurboBoost;
        Plugin.VehicleControlsInstance.GearShiftForwardKey.performed -= ShiftGearForwardInput;
        Plugin.VehicleControlsInstance.GearShiftBackwardKey.performed -= ShiftGearBackInput;
        Plugin.VehicleControlsInstance.ToggleMagnetKey.performed -= ActivateMagnet;
        Plugin.VehicleControlsInstance.ToggleHeadlightsKey.performed -= ActivateHeadlights;
        Plugin.VehicleControlsInstance.ToggleWipersKey.performed -= ActivateWipers;
        Plugin.VehicleControlsInstance.ActivateHornKey.performed -= ActivateHorn;
        Plugin.VehicleControlsInstance.ActivateHornKey.canceled -= ActivateHorn;

        Plugin.VehicleControlsInstance.SwitchIgnitionKey.performed -= SetUsingSwitchIgnition;
        Plugin.VehicleControlsInstance.SwitchIgnitionKey.canceled -= SetUsingSwitchIgnition;

        currentDriver = null;
        localPlayerInControl = false;
        steeringAnimValue = 0f;
        steeringWheelAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
        clutchPedalPressed = false;
        throttleInput = 0f;
        brakeInput = 0f;
        clutchEngagement = 1f;
        clutchSlip = 0f;
    }

    /// <summary>
    ///  Available from BetterVehicleControls, licensed under MIT License.
    ///  Source: https://github.com/1A3Dev/LC-BetterVehicleControls/blob/master/source/Patches/VehicleController_Patch.cs#L217
    /// </summary>
    public void ActivateMagnet(InputAction.CallbackContext context)
    {
        if (!context.performed) 
            return;

        float magnetDistance = Vector3.Distance(transform.position, StartOfRound.Instance.magnetPoint.position);
        if (magnetDistance >= 20f)
        {
            Plugin.Logger.LogDebug($"ScanVan: Van is too far away from the magnet to toggle it. Distance: {magnetDistance}");
            return;
        }

        GlobalReferences.magnetTrigger!.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        string newState = GlobalReferences.magnetTrigger.boolValue ? "Activated" : "Deactivated";
        HUDManager.Instance.DisplayGlobalNotification($"Ship Magnet {newState}");
    }

    public void SetUsingSwitchIgnition(InputAction.CallbackContext context)
    {
        if (!localPlayerInControl)
            return;

        if (keyIgnitionCoroutine != null)
            return;

        if ((context.performed && !usingSwitchIgnition) || 
            (context.canceled && usingSwitchIgnition))
        {
            usingSwitchIgnition = !usingSwitchIgnition;
            TrySetCarIgnitionTriggers();
            HUDManager.Instance.holdFillAmount = 0f;
            startIgnitionTrigger.isBeingHeldByPlayer = false;
        }
    }


    // --- SHIFTING GEARS METHODS ---
    public void SetGearAnimation(string setGear)
    {
        manualShiftAnimator.CrossFade(setGear, 0.01f);
        if (ignitionStarted) currentDriver?.playerBodyAnimator.CrossFade(setGear, 0.01f);
        gearStickAudio.PlayOneShot(shiftSounds[UnityEngine.Random.Range(0, shiftSounds.Length)]);
    }

    public new void ShiftGearForwardInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!localPlayerInControl ||
            Time.realtimeSinceStartup - timeSinceTogglingRadio < 0.5f ||
            Time.realtimeSinceStartup - timeAtLastGearShift < 0.15f)
            return;

        if (keyIgnitionCoroutine != null)
            return;

        AttemptToShiftGearForward();
    }

    public void AttemptToShiftGearForward()
    {
        if (currentGear >= gearRatios.Length - 1)
            return;

        int crunchId = -1;
        if (!clutchPedalPressed && ignitionStarted)
        {
            crunchId = UnityEngine.Random.Range(0, gearCrunchSounds.Length);
            engineAudio3.volume = 1f;
            engineAudio3.PlayOneShot(gearCrunchSounds[crunchId]);
            if ((float)UnityEngine.Random.Range(0, 40) < 20)
            {
                DealPermanentGearboxDamage(1);
            }
        }

        timeAtLastGearShift = Time.realtimeSinceStartup;
        playerWhoShifted = GameNetworkManager.Instance.localPlayerController;
        int animIndex = 0;

        if (currentGear == 0 && !inNeutral)
        {
            animIndex = 0;
            SetGearAnimation(upShiftString[animIndex]);
            inReverse = false;
            inNeutral = true;
            currentGear = 1;
        }
        else if (inNeutral)
        {
            animIndex = 1;
            SetGearAnimation(upShiftString[animIndex]);
            inNeutral = false;
            inReverse = false;
            currentGear = 1;
        }
        else if (currentGear < gearRatios.Length - 1)
        {
            if (currentGear == 0) currentGear = 1;
            else currentGear++;

            animIndex = currentGear;
            SetGearAnimation(upShiftString[animIndex]);
            inReverse = false;
            inNeutral = false;
        }
        ShiftToGearRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, currentGear, animIndex, true, inNeutral, inReverse, crunchId);
    }


    public new void ShiftGearBackInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!localPlayerInControl ||
            Time.realtimeSinceStartup - timeSinceTogglingRadio < 0.5f ||
            Time.realtimeSinceStartup - timeAtLastGearShift < 0.15f)
            return;

        if (keyIgnitionCoroutine != null)
            return;

        AttemptToShiftGearBackwards();
    }

    public void AttemptToShiftGearBackwards()
    {
        if (currentGear == 0 && inReverse)
            return;

        int crunchId = -1;
        if (!clutchPedalPressed && ignitionStarted)
        {
            crunchId = UnityEngine.Random.Range(0, gearCrunchSounds.Length);
            engineAudio3.volume = 1f;
            engineAudio3.PlayOneShot(gearCrunchSounds[crunchId]);
            if ((float)UnityEngine.Random.Range(0, 40) < 20)
            {
                DealPermanentGearboxDamage(1);
            }
        }

        timeAtLastGearShift = Time.realtimeSinceStartup;
        playerWhoShifted = GameNetworkManager.Instance.localPlayerController;
        int animIndex = 0;

        if (currentGear == 1 && inNeutral)
        {
            animIndex = 0;
            SetGearAnimation(downShiftString[animIndex]);
            inReverse = true;
            inNeutral = false;
            currentGear = 0;
        }
        else if (currentGear > 1)
        {
            currentGear--;
            animIndex = currentGear + 1;
            SetGearAnimation(downShiftString[animIndex]);
            inNeutral = false;
            inReverse = false;
        }
        else if (currentGear == 1)
        {
            animIndex = 1;
            SetGearAnimation(downShiftString[animIndex]);
            inNeutral = true;
            inReverse = false;
        }
        ShiftToGearRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId, currentGear, animIndex, false, inNeutral, inReverse, crunchId);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ShiftToGearRpc(int playerId, int setGear, int gearAnimation, bool upShifting, bool isNeutral, bool isReverse, int crunchId)
    {
        timeAtLastGearShift = Time.realtimeSinceStartup;
        playerWhoShifted = StartOfRound.Instance.allPlayerScripts[playerId];

        currentGear = setGear;
        inNeutral = isNeutral;
        inReverse = isReverse;

        if (crunchId != -1)
        {
            engineAudio3.volume = 1f;
            engineAudio3.PlayOneShot(gearCrunchSounds[crunchId]);
        }

        if (upShifting)
        {
            SetGearAnimation(upShiftString[gearAnimation]);
        }
        else
        {
            SetGearAnimation(downShiftString[gearAnimation]);
        }
    }


    // --- TRANSMISSION DAMAGE ---
    public void DealPermanentGearboxDamage(int damageAmount)
    {
        if (StartOfRound.Instance.inShipPhase || magnetedToShip ||
            carDestroyed || !IsOwner)
            return;

        if (transmissionHP <= 0)
            return;

        timeAtLastGearboxDamage = Time.realtimeSinceStartup;
        transmissionHP -= damageAmount;
        transmissionHP = Mathf.Clamp(transmissionHP, 0, baseTransmissionHP);
        syncedTransmissionHP = transmissionHP;
        DealGearboxDamageRpc(transmissionHP);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void DealGearboxDamageRpc(int gearHealth)
    {
        timeAtLastGearboxDamage = Time.realtimeSinceStartup;
        transmissionHP = gearHealth;
        syncedTransmissionHP = transmissionHP;
    }


    // --- AUTOPILOT MAGNET ---
    public new void StartMagneting()
    {
        if (!IsOwner)
            return;

        SetVehicleKinematic(setKinematic: true);
        magnetTime = 0f;
        magnetRotationTime = 0f;
        StartOfRound.Instance.isObjectAttachedToMagnet = true;
        StartOfRound.Instance.attachedVehicle = this;
        magnetedToShip = true;
        averageVelocityAtMagnetStart = averageVelocity;
        RoundManager.Instance.tempTransform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
        Vector3 tempRotation = RoundManager.Instance.tempTransform.eulerAngles;

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

        if (StartOfRound.Instance.inShipPhase) return;
        if (GameNetworkManager.Instance.localPlayerController == null) return;
        MagnetCarRpc(magnetTargetPosition, eulerAngles, magnetStartPosition, magnetStartRotation, tempRotation, averageVelocityAtMagnetStart);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void MagnetCarRpc(Vector3 targetPosition, Vector3 targetRotation, Vector3 startPosition, Quaternion startRotation, Vector3 tempRotation, Vector3 avgVel)
    {
        SetVehicleKinematic(setKinematic: true);

        magnetedToShip = true;
        magnetTime = 0f;
        magnetRotationTime = 0f;
        averageVelocityAtMagnetStart = avgVel;
        RoundManager.Instance.tempTransform.eulerAngles = tempRotation;

        StartOfRound.Instance.isObjectAttachedToMagnet = true;
        StartOfRound.Instance.attachedVehicle = this;

        magnetStartPosition = startPosition;
        magnetStartRotation = startRotation;

        magnetTargetPosition = targetPosition;
        magnetTargetRotation = Quaternion.Euler(targetRotation);

        CollectItemsInTruck();
    }

    public new void CollectItemsInTruck()
    {   
        foreach (GrabbableObject component in References.itemsInTruck)
        {
            if (component != null &&
                !component.isHeld &&
                !component.isHeldByEnemy &&
                component.transform.parent == transform)
            {
                if (References.lastDriver != null)
                {
                    References.lastDriver.SetItemInElevator(magnetedToShip, magnetedToShip, component);
                }
                else if (GameNetworkManager.Instance.localPlayerController != null)
                {
                    GameNetworkManager.Instance.localPlayerController.SetItemInElevator(magnetedToShip, magnetedToShip, component);
                }
            }
        }
    }


    // --- WEEDKILLER FUNCTIONALITY ---
    public void SetOverdriveSwitchLocalClient()
    {
        overdriveSwitchEnabled = !overdriveSwitchEnabled;
        cabinLightSwitchAudio.PlayOneShot(cabinLightSwitchToggle);
        SetOverdriveSwitchRpc(overdriveSwitchEnabled);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetOverdriveSwitchRpc(bool switchState)
    {
        overdriveSwitchEnabled = switchState;
        cabinLightSwitchAudio.PlayOneShot(cabinLightSwitchToggle);
    }


    public new void AddEngineOil()
    {
        if (transmissionHP < baseTransmissionHP)
        {
            int setGearHealth = Mathf.Min(transmissionHP + 2, baseTransmissionHP);
            AddTransmissionOilOnLocalClient(setGearHealth);
            AddTransmissionOilRpc(setGearHealth);
            return;
        }
        int setEngineHealth = Mathf.Min(carHP + 2, baseCarHP);
        AddEngineOilOnLocalClient(setEngineHealth);
        AddEngineOilRpc(setEngineHealth);
    }

    public void AddTransmissionOilOnLocalClient(int setGearHealth)
    {
        hoodAudio.PlayOneShot(pourOil);
        transmissionHP = setGearHealth;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void AddTransmissionOilRpc(int setGearHP)
    {
        AddTransmissionOilOnLocalClient(setGearHP);
    }

    public new void AddEngineOilOnLocalClient(int setCarHP)
    {
        hoodAudio.PlayOneShot(pourOil);
        carHP = setCarHP;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void AddEngineOilRpc(int setHP)
    {
        AddEngineOilOnLocalClient(setHP);
    }

    public new void AddTurboBoost()
    {
        if (!overdriveSwitchEnabled && transmissionHP < baseTransmissionHP)
        {
            int setGearHealth = Mathf.Min(transmissionHP + 2, baseTransmissionHP);
            AddTransmissionOilOnLocalClient(setGearHealth);
            AddTransmissionOilRpc(setGearHealth);
            return;
        }
        int setTurboBoosts = Mathf.Min(turboBoosts + 1, 4);
        AddTurboBoostOnLocalClient(setTurboBoosts);
        AddTurboBoostRpc(setTurboBoosts);
    }

    public new void AddTurboBoostOnLocalClient(int setTurboBoosts)
    {
        hoodAudio.PlayOneShot(pourTurbo);
        turboBoosts = setTurboBoosts;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void AddTurboBoostRpc(int setTurboBoosts)
    {
        AddTurboBoostOnLocalClient(setTurboBoosts);
    }


    // --- CLIMATE CONTROL METHODS ---
    public void SetHeaterOnLocalClient(bool on)
    {
        if (heaterOn == on) return;
        heaterOn = on;
        SetHeaterRpc(heaterOn);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterRpc(bool heatOn)
    {
        heaterOn = heatOn;
    }


    public void SetHeaterFanSpeedOnLocalClient(float fanSpeed)
    {
        if (heaterSpeed == fanSpeed) return;
        heaterSpeed = fanSpeed;
        SetHeaterFanSpeedRpc(heaterSpeed);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterFanSpeedRpc(float fanSpeed)
    {
        heaterSpeed = fanSpeed;
    }


    public void SetHeaterDirectionOnLocalClient(float heatTemp)
    {
        if (heaterTemp == heatTemp) return;
        heaterTemp = heatTemp;
        isHeaterCold = heatTemp == 0;
        isHeaterWarm = heatTemp == 1;
        SetHeaterDirectionRpc(heaterTemp);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterDirectionRpc(float temp)
    {
        heaterTemp = temp;
        isHeaterCold = heaterTemp == 0;
        isHeaterWarm = heaterTemp == 1;
    }


    // --- TURBO BOOST AND JUMP ABILITY ---
    private new void DoTurboBoost(InputAction.CallbackContext context)
    {
        if (!context.performed) 
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (playerController == null) return;
        if (playerController.isPlayerDead) return;
        if (!playerController.isPlayerControlled) return;
        if (playerController.isTypingChat ||
            playerController.quickMenuManager.isMenuOpen) return;

        if (!localPlayerInControl || !ignitionStarted ||
            jumpingInCar || keyIsInDriverHand) return;

        Vector2 dir = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();
        UseTurboBoostLocalClient(dir);
        UseTurboBoostRpc();
    }

    public new void UseTurboBoostLocalClient(Vector2 dir = default(Vector2))
    {
        bool canTurboBoost = turboBoosts > 0 && overdriveSwitchEnabled;
        currentDriver?.playerBodyAnimator.SetTrigger(JUMP_WHILE_IN_CAR);
        currentDriver?.movementAudio.PlayOneShot(jumpInCarSFX);
        if (IsOwner)
        {
            if (turboBoosts == 0 || !overdriveSwitchEnabled)
            {
                jumpingInCar = true;
                StartCoroutine(jerkCarUpward(dir));
                return;
            }
            else if (canTurboBoost)
            {
                Vector3 boostForce = transform.TransformDirection(new Vector3(dir.x, 0f, dir.y));
                mainRigidbody.AddForce(boostForce * turboBoostForce + Vector3.up * turboBoostUpwardForce * 0.6f, ForceMode.Impulse);
            }
        }
        if (canTurboBoost)
        {
            turboBoosts = Mathf.Max(0, turboBoosts - 1);
            turboBoostAudio.PlayOneShot(turboBoostSFX);
            engineAudio1.PlayOneShot(turboBoostSFX2);
            turboBoostParticle.Play(true);
            if (Vector3.Distance(StartOfRound.Instance.audioListener.transform.position, turboBoostAudio.transform.position) < 10f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                return;
            }
        }
    }

    private new IEnumerator jerkCarUpward(Vector3 dir)
    {
        if (!IsOwner)
        {
            jumpingInCar = false;
            yield break;
        }
        yield return new WaitForSeconds(0.16f);
        Vector3 jerkForce = transform.TransformDirection(new Vector3(dir.x, 0f, dir.y));
        mainRigidbody.AddForce(jerkForce * turboBoostForce * 0.22f + Vector3.up * turboBoostUpwardForce * 0.1f, ForceMode.Impulse);
        mainRigidbody.AddForceAtPosition(Vector3.up * jumpForce, hoodFireAudio.transform.position - Vector3.up * 2f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.15f);
        jumpingInCar = false;
        yield break;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void UseTurboBoostRpc()
    {
        UseTurboBoostLocalClient(default(Vector2));
    }


    // --- KEYBINDS ---
    public void ActivateHeadlights(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!localPlayerInControl)
            return;

        ToggleHeadlightsLocalClient();
    }

    public void ActivateWipers(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!localPlayerInControl)
            return;

        windscreenWipersTrigger.onInteract.Invoke(StartOfRound.Instance.localPlayerController);
    }

    public void ActivateHorn(InputAction.CallbackContext context)
    {
        if (!localPlayerInControl || truckAlarmCoroutine != null)
            return;

        if (((context.performed && !honkingHorn) ||
            (context.canceled && honkingHorn)))
        {
            SetHonkingLocalClient(!honkingHorn);
        }
    }


    // --- HORN ---
    public new void SetHonkingLocalClient(bool honk)
    {
        honkingHorn = honk;
        SetHonkRpc(honk);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHonkRpc(bool honk)
    {
        honkingHorn = honk;
    }


    // --- VEHICLE REMOVAL ---
    public new void OnDisable()
    {
        RemoveCarRainCollision();
        DisableControl();
        vanZone.disablePhysicsRegion = true;
        if (StartOfRound.Instance.CurrentPlayerPhysicsRegions.Contains(vanZone))
        {
            StartOfRound.Instance.CurrentPlayerPhysicsRegions.Remove(vanZone);
        }
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[i];
            if (playerController.transform.parent == vanZone.physicsTransform)
            {
                Transform playerTransform = playerController.isInElevator ? playerController.playersManager.elevatorTransform : playerController.playersManager.playersContainer;
                playerController.transform.SetParent(playerTransform);
                Plugin.Logger.LogWarning($"Hauler: Player {i} setting parent since vehicle was disabled");
            }
        }
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
        truckMat.mainTexture = defaultTruckTex;
        References.vanController = null!;
    }


    // --- UPDATE ---
    public new void Update()
    {
        if (destroyNextFrame)
        {
            if (IsOwner)
            {
                UnityEngine.Object.Destroy(base.windwiperPhysicsBody1.gameObject);
                UnityEngine.Object.Destroy(base.windwiperPhysicsBody2.gameObject);
                UnityEngine.Object.Destroy(base.ragdollPhysicsBody.gameObject);
                UnityEngine.Object.Destroy(this.playerPhysicsBody.gameObject);
                UnityEngine.Object.Destroy(base.gameObject);
            }
            return;
        }
        if (NetworkObject != null && !NetworkObject.IsSpawned)
        {
            RemoveCarRainCollision();
            vanZone.disablePhysicsRegion = true;
            if (StartOfRound.Instance.CurrentPlayerPhysicsRegions.Contains(vanZone))
            {
                StartOfRound.Instance.CurrentPlayerPhysicsRegions.Remove(vanZone);
            }
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[i];
                if (playerController.transform.parent == vanZone.physicsTransform)
                {
                    Transform playerTransform = playerController.isInElevator ? playerController.playersManager.elevatorTransform : playerController.playersManager.playersContainer;
                    playerController.transform.SetParent(playerTransform);
                    Plugin.Logger.LogWarning($"ScanVan: Player {i} setting parent since vehicle was removed");
                }
            }
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
        ReactToDamage();
        if (magnetedToShip)
        {
            if (!StartOfRound.Instance.magnetOn)
            {
                magnetedToShip = false;
                StartOfRound.Instance.isObjectAttachedToMagnet = false;
                CollectItemsInTruck();
                return;
            }
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
        }

        //HUDManager.Instance.enableConsoleLogging = true;
        //HUDManager.Instance.SetDebugText($"clutch? {clutchEngagement}\ngear? {currentGear}\nneutral? {inNeutral}\nreverse? {inReverse}\nslip? {clutchSlip}");
        //HUDManager.Instance.SetDebugText($"item count? {References.itemsInTruck.Count}");
        //HUDManager.Instance.SetDebugText($"onVan? {vanZone.playerInZone}\ninCab? {vanCabinZone.playerInZone}\ninStorage? {vanStorageZone.playerInZone}");

        SetCarEffects(steeringWheelAnimValue);
        UpdateOccupantAnimations();

        if (localPlayerInControl)
        {
            GetVehicleInput();
            return;
        }

        moveInputVector = syncedMoveInputVector;
        steeringAnimValue = moveInputVector.x;
        steeringWheelAnimValue = 0f;
        drivePedalPressed = syncedDrivePedalPressed;
        brakePedalPressed = syncedBrakePedalPressed;
        clutchPedalPressed = syncedClutchPedalPressed;
    }

    private void UpdateOccupantAnimations()
    {
        if (currentDriver == null || currentDriver.playerBodyAnimator == null)
            return;

        if (disableAnimations ||
            keyIgnitionCoroutine != null ||
            !ignitionStarted)
            return;

        float playerSteer = steeringWheelAnimFloat + 0.5f;
        currentDriver.playerBodyAnimator.SetFloat(ANIMATION_SPEED, -playerSteer); // player steering animation
        currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, 1);
        currentDriver.playerBodyAnimator.SetFloat(CAR_HANDBRAKE_ON_ENGAGEMENT, handbrakeAnimValue);
        currentDriver.playerBodyAnimator.SetFloat(CAR_HANDBRAKE_OFF_ENGAGEMENT, 1f * handbrakeAnimValue);
    }


    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SyncPlayerVehicleAnimationsRpc(int playerId, int animIndex, bool isTrigger, string upperStringName, string leftStringName, string rightStringName)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerId];
        if (player == null)
        {
            return;
        }
        if (player.isPlayerDead || 
            !player.isPlayerControlled)
            return;

        if (isTrigger)
        {
            PlayCombinedOccupantAnimation(player, upperStringName, leftStringName, rightStringName, 0.05f);
            return;
        }
        player.playerBodyAnimator.SetInteger("SA_CarAnim", animIndex);
    }


    // --- RADIO TIME SYNC ---
    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SyncRadioTimeRpc(float songTime, float syncedTime)
    {
        currentSongTime = songTime;
        syncedSongTime = syncedTime;
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        SetRadioTime();
    }

    public void SetRadioTime()
    {
        if (isFmRadio || 
            radioAudio.clip == null || 
            !radioOn) return;
        float setRadioTime = (syncedSongTime + (Time.realtimeSinceStartup - timeLastSyncedRadio)) % radioAudio.clip.length;
        if (Mathf.Abs(setRadioTime - radioAudio.time) > 1f)
        {
            radioAudio.time = setRadioTime;
        }
    }


    // --- RADIO TYPE (FM/CD) ---
    public void ChangeRadioType()
    {
        if (ScanVanNetworker.Instance!.NoMusic.Value)
        {
            HUDManager.Instance.DisplayTip("Live radio disabled", 
                "The host has disabled this feature!");
            return;
        }

        if (!radioOn) return;
        isFmRadio = !isFmRadio;
        if (!isFmRadio)
        {
            liveRadioController.TurnRadioOnOff(false);
            radioInterference.Stop();
            SetCurrentRadioClip();
            if (radioAudio.loop) radioAudio.loop = false;
            if (radioAudio.clip != null) radioAudio.Play();
            currentSongTime = lastSongTime;
            syncedSongTime = currentSongTime;
            timeLastSyncedRadio = Time.realtimeSinceStartup;
            radioAudio.time = currentSongTime;
            radioOn = true;
        }
        else
        {
            liveRadioController.TogglePowerLocalClient(false);
            if (!radioAudio.loop) radioAudio.loop = true;
            lastSongTime = currentSongTime;
            syncedSongTime = lastSongTime;
            timeLastSyncedRadio = Time.realtimeSinceStartup;
            radioAudio.time = lastSongTime;
            radioOn = true;
        }
        ChangeRadioTypeRpc(isFmRadio, currentRadioClip, lastSongTime, currentSongTime);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ChangeRadioTypeRpc(bool isFmStation, int radioStation, float lastTime, float currentTime)
    {
        isFmRadio = isFmStation;
        currentRadioClip = radioStation;
        if (!isFmRadio)
        {
            liveRadioController.TurnRadioOnOff(false);
            radioInterference.Stop();
            SetCurrentRadioClip();
            if (radioAudio.loop) radioAudio.loop = false;
            if (radioAudio.clip != null) radioAudio.Play();
            currentSongTime = currentTime;
            syncedSongTime = currentTime;
            timeLastSyncedRadio = Time.realtimeSinceStartup;
            radioAudio.time = currentTime;
            radioOn = true;
            return;
        }
        if (!radioAudio.loop) radioAudio.loop = true;
        lastSongTime = lastTime;
        syncedSongTime = lastTime;
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        radioAudio.time = lastTime;
        radioOn = true;
    }


    // --- RADIO SEEK (CHANGE CHANNEL/TRACK) ---
    public void ChangeRadioStation(bool seekForward)
    {
        if (ScanVanNetworker.Instance!.NoMusic.Value)
        {
            HUDManager.Instance.DisplayTip("DMCA-Free Radio disabled", 
                "This feature has temporarily been pulled due to outrageous copyright laws prohibiting the use of most vintage music");
            return;
        }
        if (isFmRadio)
        {
            TrySetFMRadioRpc(lastSongTime);
            if (!radioAudio.loop) radioAudio.loop = true;
            liveRadioController.ToggleStationLocalClient();
            return;
        }

        if (!radioOn)
        {
            return;
        }

        if (!ScanVanNetworker.Instance!.NoMusic.Value)
        {
            if (radioClips.Length > 0)
            {
                if (seekForward) currentRadioClip = (currentRadioClip + 1) % radioClips.Length; // seek forwards
                else currentRadioClip = (currentRadioClip - 1 + radioClips.Length) % radioClips.Length; // seek backwards
            }
            else
            {
                currentRadioClip = 0;
            }
        }
        else
        {
            if (seekForward) currentRadioClip = (currentRadioClip + 1) % streamerRadioClips.Length;
            else currentRadioClip = (currentRadioClip - 1 + streamerRadioClips.Length) % streamerRadioClips.Length;
        }
        SetCurrentRadioClip();
        if (radioAudio.loop) radioAudio.loop = false;
        if (radioAudio.clip != null) radioAudio.Play();
        currentSongTime = 0f;
        lastSongTime = 0f;
        syncedSongTime = 0f;
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        radioAudio.time = 0f;
        SetRadioStationRpc(currentRadioClip);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetRadioStationRpc(int radioStation)
    {
        if (!radioOn) radioOn = true;
        currentRadioClip = radioStation;
        SetCurrentRadioClip();
        if (radioAudio.loop) radioAudio.loop = false;
        if (radioAudio.clip != null) radioAudio.Play();
        currentSongTime = 0f;
        lastSongTime = 0f;
        syncedSongTime = 0f;
        timeLastSyncedRadio = Time.realtimeSinceStartup;
        radioAudio.time = 0f;
    }


    // --- HELPER FUNCTION ---
    private void PlayCombinedOccupantAnimation(PlayerControllerB player, string upperString, string leftString, string rightString, float time)
    {
        if (player == null)
            return;

        player.playerBodyAnimator.CrossFade(upperString, time);
        player.playerBodyAnimator.CrossFade(leftString, time);
        player.playerBodyAnimator.CrossFade(rightString, time);
    }


    // --- RADIO TOGGLE ---
    public new void SwitchRadio()
    {
        if (ScanVanNetworker.Instance!.NoMusic.Value)
        {
            HUDManager.Instance.DisplayTip("DMCA-Free Radio disabled",
                "This feature has temporarily been pulled due to outrageous copyright laws prohibiting the use of most vintage music");
            return;
        }
        if (localPlayerInControl && keyIgnitionCoroutine == null && ignitionStarted)
        {
            timeSinceTogglingRadio = Time.realtimeSinceStartup;
            PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
            PlayCombinedOccupantAnimation(localPlayer, UPPER_BODY_RADIO_ON, LEFT_ARM_RADIO_ON, RIGHT_ARM_RADIO_ON, 0.05f);
            SyncPlayerVehicleAnimationsRpc((int)localPlayer.playerClientId, 0, true, UPPER_BODY_RADIO_ON, LEFT_ARM_RADIO_ON, RIGHT_ARM_RADIO_ON);
        }
        if (isFmRadio)
        {
            TrySetFMRadioRpc(lastSongTime);
            if (!radioAudio.loop) radioAudio.loop = true;
            liveRadioController.TogglePowerLocalClient(true);
            return;
        }
        if (radioAudio.loop) radioAudio.loop = false;
        radioOn = !radioOn;
        if (radioOn)
        {
            SetCurrentRadioClip();
            if (radioAudio.clip != null) radioAudio.Play();
            currentSongTime = lastSongTime;
            syncedSongTime = lastSongTime;
            timeLastSyncedRadio = Time.realtimeSinceStartup;
            radioAudio.time = lastSongTime;
        }
        else
        {
            lastSongTime = currentSongTime;
            syncedSongTime = currentSongTime;
            timeLastSyncedRadio = Time.realtimeSinceStartup;
            if (radioAudio.clip != null) radioAudio.Stop();
        }
        SetRadioRpc(radioOn, currentRadioClip, currentSongTime, lastSongTime);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void TrySetFMRadioRpc(float lastTime)
    {
        isFmRadio = true;
        if (!radioAudio.loop) radioAudio.loop = true;
        lastSongTime = lastTime;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetRadioRpc(bool on, int currentClip, float radioTime, float lastRadioTime)
    {
        if (radioOn == on) return;
        if (radioAudio.loop) radioAudio.loop = false;
        radioOn = on;
        currentRadioClip = currentClip;
        if (on)
        {
            SetCurrentRadioClip();
            if (radioAudio.clip != null) radioAudio.Play();
            currentSongTime = radioTime;
            syncedSongTime = radioTime;
            timeLastSyncedRadio = Time.realtimeSinceStartup;
            radioAudio.time = radioTime;
        }
        else
        {
            lastSongTime = lastRadioTime;
            syncedSongTime = lastRadioTime;
            timeLastSyncedRadio = Time.realtimeSinceStartup;
            if (radioAudio.clip != null) radioAudio.Stop();
        }
    }

    private void SetCurrentRadioClip()
    {
        if (!ScanVanNetworker.Instance!.NoMusic.Value)
        {
            if (radioClips == null || radioClips.Length == 0)
            {
                radioAudio.clip = null;
                currentRadioClip = 0;
                return;
            }

            currentRadioClip %= radioClips.Length;

            if (currentRadioClip < 0)
                currentRadioClip += radioClips.Length;

            radioAudio.clip = radioClips[currentRadioClip];
        }
        else
        {
            currentRadioClip %= streamerRadioClips.Length;

            if (currentRadioClip < 0)
                currentRadioClip += streamerRadioClips.Length;

            radioAudio.clip = streamerRadioClips[currentRadioClip];
        }
    }


    // --- RADIO VALUES ---
    public new void SetRadioValues()
    {
        if (!radioOn)
            return;
        if (radioAudio.clip == null)
            return;
        if (IsServer && radioAudio.isPlaying && 
            Time.realtimeSinceStartup > radioPingTimestamp)
        {
            radioPingTimestamp = (Time.realtimeSinceStartup + 1f);
            RoundManager.Instance.PlayAudibleNoise(radioAudio.transform.position, 16f, Mathf.Min((radioAudio.volume + radioInterference.volume) * 0.5f, 0.9f), 0, false, 2692);
        }
        if (isFmRadio) 
            return;
        if (IsServer)
        {
            if (!radioAudio.isPlaying)
            {
                ChangeRadioStation(true);
                return;
            }
            currentSongTime = radioAudio.time;
            if (Time.realtimeSinceStartup - timeLastSyncedRadio > 1f)
            {
                timeLastSyncedRadio = Time.realtimeSinceStartup;
                syncedSongTime = radioAudio.time;
                SyncRadioTimeRpc(currentSongTime, syncedSongTime);
            }
        }
    }


    // --- WHEEL VISUALS ---
    private void MatchWheelMeshToCollider(MeshRenderer wheelMesh, MeshRenderer brakeMesh, WheelCollider wheelCollider)
    {
        wheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = rotation;
        brakeMesh.transform.position = wheelMesh.transform.position;
    }


    // --- VEHICLE ALARM ---
    public void TryBeginAlarm()
    {
        if (truckAlarmCoroutine != null)
            return;

        BeginAlarmRpc();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void BeginAlarmRpc()
    {
        truckAlarmCoroutine = StartCoroutine(BeginAlarmSound());
    }

    private IEnumerator BeginAlarmSound()
    {
        alarmAudio.loop = true;
        alarmDebounce = true;
        hornAudio.Stop();
        honkingHorn = false;
        alarmAudio.Play();
        if (blinkersCoroutine != null)
        {
            StopCoroutine(blinkersCoroutine);
            blinkersCoroutine = null!;
        }
        SetSymbolActive(immobiliserSymbol, immobiliserLight, true);
        blinkersCoroutine = StartCoroutine(FlashBlinkerLights());
        yield return new WaitForSeconds(17.296f);
        alarmAudio.loop = false;
        SetSymbolActive(immobiliserSymbol, immobiliserLight, false);
        if (!hazardsOn)
        {
            if (blinkersCoroutine != null)
            {
                StopCoroutine(blinkersCoroutine);
                blinkersCoroutine = null!;
            }
            hazardsBlinking = false;
            leftBlinkerMesh.material = headlightsOffMat;
            leftBlinkerMeshLod.material = headlightsOffMat;
            rightBlinkerMesh.material = headlightsOffMat;
            rightBlinkerMeshLod.material = headlightsOffMat;

            SetSymbolActive(leftSignalSymbol, leftSignalLight, false);
            SetSymbolActive(hazardWarningSymbol, hazardWarningLight, false);
            SetSymbolActive(rightSignalSymbol, rightSignalLight, false);

            blinkerLightsContainer.SetActive(false);
        }
        truckAlarmCoroutine = null!;
        alarmDebounce = false;
    }


    // --- VISUAL EFFECTS ---
    public new void SetCarEffects(float setSteering)
    {
        // steering
        setSteering = IsOwner ? setSteering : 0f;
        steeringWheelAnimFloat = Mathf.Clamp(steeringWheelAnimFloat + setSteering * (steeringWheelTurnSpeed * Time.deltaTime / 6f), -1f, 1f);
        steeringWheelAnimator.SetFloat(STEERING_WHEEL_SPEED, Mathf.Clamp((steeringWheelAnimFloat + 1f) / 2f, 0f, 1f));

        // handbrake
        handbrakeAnimValue = Mathf.MoveTowards(handbrakeAnimValue, handbrakeEngaged ? 0f : 1f, handbrakePullSpeed * Time.deltaTime * (Time.realtimeSinceStartup - timeAtLastHandbrakePull));
        handbrakeAnimator.SetFloat(HANDBRAKE_ENGAGEMENT, handbrakeAnimValue);

        // cluster
        speedometerTransform.localRotation = Quaternion.Euler(0f, -225f * speedometerFloat, 0f);
        tachometerTransform.localRotation = Quaternion.Euler(0f, -135f * tachometerFloat, 0f);
        oilPressureTransform.localRotation = Quaternion.Euler(0f, -135f * oilPressureFloat, 0f);

        if (ignitionStarted)
        {
            float vehicleHPfloat = (float)carHP / baseCarHP;
            float turboPressureInt = turboPressureNeedleCurve.Evaluate(vehicleHPfloat);
            float oilPressureCurve = oilPressureNeedleCurve.Evaluate(
                (float)((carHP / 2) + ((turboBoosts * 3.5) * (turboPressureFloat * (overdriveSwitchEnabled ? 1 : 0)))) / (baseCarHP + 4));
            float speedometerRot = Mathf.Abs(backWheelRPM) / 850f;
            float tachometerRot = EngineRPM / MaxEngineRPM;

            speedometerFloat = Mathf.Lerp(speedometerFloat, speedometerRot, 6f * Time.deltaTime);
            tachometerFloat = Mathf.Lerp(tachometerFloat, tachometerRot, 6f * Time.deltaTime);
            turboPressureFloat = Mathf.Lerp(turboPressureFloat, turboPressureInt, 6f * Time.deltaTime);
            oilPressureFloat = Mathf.Lerp(oilPressureFloat, oilPressureCurve, 4f * Time.deltaTime);
        }
        else
        {
            bool tryIgnition = engineAudio1.volume > 0.1f && twistingKey;
            speedometerFloat = Mathf.Lerp(speedometerFloat, 0f, 6f * Time.deltaTime);
            tachometerFloat = Mathf.Lerp(tachometerFloat, (tryIgnition ? 0.065f : 0f), 4.5f * Time.deltaTime);
            turboPressureFloat = Mathf.Lerp(turboPressureFloat, 0f, 6f * Time.deltaTime);
            oilPressureFloat = Mathf.Lerp(oilPressureFloat, 0f, 6f * Time.deltaTime);
        }

        mirrorAngleFloat = Mathf.MoveTowards(mirrorAngleFloat, !accessoryMode ? 77.5f : 0f, 50f * Time.deltaTime);
        leftElectricMirror.transform.localRotation = Quaternion.Euler(0f, -mirrorAngleFloat, 0f);
        rightElectricMirror.transform.localRotation = Quaternion.Euler(0f, mirrorAngleFloat, 0f);

        SetCarRadio();
        SetCarInteriorAnimations();
        SetCarHeater();
        SetCarLightingEffects();
        SetCarAudioEffects();
        SetCarTyreSlipEffects();
        SetCarKeyEffects();

        if (IsOwner)
        {
            SyncCarEffectsToOtherClients();
            if (!syncedExtremeStress && underExtremeStress && extremeStressAudio.volume > 0.35f)
            {
                syncedExtremeStress = true;
                SyncExtremeStressRpc(underExtremeStress);
            }
            else if (syncedExtremeStress && !underExtremeStress && extremeStressAudio.volume < 0.5f)
            {
                syncedExtremeStress = false;
                SyncExtremeStressRpc(underExtremeStress);
            }
            return;
        }
        if (smoothRotation) steeringWheelAnimFloat = Mathf.Lerp(steeringWheelAnimFloat, syncedSteeringWheelRotation, 6f * Time.deltaTime);
        else steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, syncedSteeringWheelRotation, steeringWheelTurnSpeed * Time.deltaTime / 6f);
    }

    public void SetCarInteriorAnimations()
    {
        float lDirection = !lowBeamsOn ? 0f : !highBeamsOn ? 82.5f : 165f;

        lightSwitchFloat = Mathf.MoveTowards(lightSwitchFloat, lDirection, 800f * Time.deltaTime);
        headlightSwitch.transform.localRotation = Quaternion.Euler(0f, lightSwitchFloat, 0f);
    }

    public void SetCarHeater()
    {
        // fan speed
        switch (heaterSpeed)
        {
            case 1:
                heatSpeedFloat = Mathf.Lerp(heatSpeedFloat, -12.75f, 50f * Time.deltaTime);
                break;
            case 2:
                heatSpeedFloat = Mathf.Lerp(heatSpeedFloat, -4.2f, 50f * Time.deltaTime);
                break;
            case 3:
                heatSpeedFloat = Mathf.Lerp(heatSpeedFloat, 4.2f, 50f * Time.deltaTime);
                break;
            case 4:
                heatSpeedFloat = Mathf.Lerp(heatSpeedFloat, 12.75f, 50f * Time.deltaTime);
                break;
        }
        fanSpeedLever.transform.localRotation = Quaternion.Euler(heatSpeedFloat, 0f, 0f);

        // fan direction
        heatDirectionFloat = Mathf.Lerp(heatDirectionFloat, heaterOn ? -0.5f : 20f, 50f * Time.deltaTime);
        heaterDirectionLever.transform.localRotation = Quaternion.Euler(0f, heatDirectionFloat, 0f);

        // blower temperature
        heatPositionFloat = Mathf.Lerp(heatPositionFloat, isHeaterCold ? 20f : -20f, 50f * Time.deltaTime);
        heaterTempLever.transform.localRotation = Quaternion.Euler(0f, heatPositionFloat, 0f);
    }

    // helper function
    public void SetObjectRotation(GameObject obj, Quaternion target, float deltaSpeed, bool useRotate)
    {
        if (useRotate)
        {
            obj.transform.localRotation = Quaternion.RotateTowards(obj.transform.localRotation, target, deltaSpeed * Time.deltaTime);
            return;
        }
        obj.transform.localRotation = Quaternion.Lerp(obj.transform.localRotation, target, deltaSpeed * Time.deltaTime);
    }

    // in-car radio screen
    public void SetCarRadio()
    {
        // time & radio frequency on dash
        if (!accessoryMode && !radioOn)
        {
            radioTime.text = null;
            radioFrequency.text = null;
            return;
        }
        if (radioOn)
        {
            // display the current time on the dashboard
            radioTime.text = GetClockTime(HUDManager.Instance.clockNumber.text);
            if (Time.realtimeSinceStartup - liveRadioController._timeSinceChangingVol < 2f)
            {
                radioFrequency.text = SetRadioVolume(liveRadioController._volume);
            }
            else
            {
                if (isFmRadio)
                {
                    radioFrequency.text =
                        (liveRadioController._stream == null || string.IsNullOrEmpty(liveRadioController._stream.buffer_info))
                        ? "PI SEEK"
                        : liveRadioController._currentFrequency;
                }
                else
                {
                    radioFrequency.text = SetRadioCDTrack(currentRadioClip);
                }
            }
            return;
        }
        radioTime.text = null;
        radioFrequency.text = "RADIO OFF";
    }

    // return the current radio volume
    private string SetRadioVolume(float vol)
    {
        if (vol <= 0f)
            return "RADIO MUTE";
        if (vol == 1f)
            return "VOL MAX";

        int display = Mathf.RoundToInt(vol * 10f);
        return $"VOL {display:00}";
    }

    // return the current CD track
    private string SetRadioCDTrack(int currentTrack)
    {
        if ((!ScanVanNetworker.Instance!.NoMusic.Value && radioClips.Length == 0) || 
            ScanVanNetworker.Instance!.NoMusic.Value)
        {
            return "NO CD";
        }
        int displayTrack = currentTrack + 1;
        return $"CD PLAY {displayTrack:00}";
    }

    // get the current clock time to display, display the users computer time if on a company building with no time progression
    private string GetClockTime(string clockText)
    {
        if (RoundManager.Instance.currentLevel != null && 
            !RoundManager.Instance.currentLevel.planetHasTime)
        {
            return DateTime.Now.ToString("h:mm"); // users computer time, 12 hour like the ingame clock
        }
        return clockText
            .Trim()
            .Replace("\n", "")
            .Replace("AM", "")
            .Replace("PM", "")
            .Trim();
    }

    // brake-lights and reverse-lights, any additional
    // lights can be added here i guess
    public void SetCarLightingEffects()
    {
        bool brakeLightsOn = brakePedalPressed && ignitionStarted && !handbrakeEngaged;
        bool backingUpLightsOn = inReverse && ignitionStarted;
        if (backLightsOn != brakeLightsOn)
        {
            backLightsOn = brakeLightsOn;
            backLightsMesh.material = brakeLightsOn ? backLightOnMat : greyLightOffMat;
            backLightsMeshLod.material = brakeLightsOn ? backLightOnMat : greyLightOffMat;
            backLightsContainer.SetActive(brakeLightsOn);
        }
        if (reverseLightsOn != backingUpLightsOn)
        {
            reverseLightsOn = backingUpLightsOn;
            reverseLightsMesh.enabled = backingUpLightsOn;
            reverseLightsContainer.SetActive(backingUpLightsOn);
        }
    }


    // --- VEHICLE AUDIO METHODS ---
    /// <summary>
    ///  Available from EnemySoundFixes, licensed under GNU General Public License.
    ///  Source: https://github.com/ButteryStancakes/EnemySoundFixes/blob/master/Patches/CruiserPatches.cs
    /// </summary>
    private new void SetVehicleAudioProperties(AudioSource audio, bool audioActive, float lowest, float highest, float lerpSpeed, bool useVolumeInsteadOfPitch = false, float onVolume = 1f)
    {
        if (audioActive && 
            (audio == extremeStressAudio && magnetedToShip) || 
            ((audio == rollingAudio || audio == skiddingAudio) && 
            (magnetedToShip || allWheelsAirborne)))
            audioActive = false;

        if (audioActive)
        {
            if (!audio.isPlaying)
            {
                audio.Play();
            }
            if (useVolumeInsteadOfPitch)
            {
                audio.volume = Mathf.Max(Mathf.Lerp(audio.volume, highest, lerpSpeed * Time.deltaTime), lowest);
                return;
            }
            audio.volume = Mathf.Lerp(audio.volume, onVolume, 20f * Time.deltaTime);
            audio.pitch = Mathf.Lerp(audio.pitch, highest, lerpSpeed * Time.deltaTime);
            return;
        }
        if (useVolumeInsteadOfPitch)
        {
            audio.volume = Mathf.Lerp(audio.volume, 0f, lerpSpeed * Time.deltaTime);
        }
        else
        {
            audio.volume = Mathf.Lerp(audio.volume, 0f, 4f * Time.deltaTime);
            audio.pitch = Mathf.Lerp(audio.pitch, lowest, 4f * Time.deltaTime);
        }
        if (audio.isPlaying && audio.volume <= 0.001f)
        {
            audio.Stop();
        }
    }

    public void SetCarAudioEffects()
    {
        float engineAudioAnimCurve = engineCurve.Evaluate(EngineRPM / engineIntensityPercentage);
        //float engineKnockingAnimCurve = engineCurve.Evaluate(EngineRPM / engineIntensityPercentage);
        float ignNum = 1f;
        if (ignitionStarted) ignNum = 1.35f;
        float highestAudio1 = ignitionStarted ? Mathf.Clamp(engineAudioAnimCurve, 0.65f, 1.15f) * 1.35f : 1f; 
        float highestAudio2 = Mathf.Clamp(engineAudioAnimCurve, 0.7f, 1.5f);
        float wheelSpeed = Mathf.Abs(wheelRPM);
        float highestTyre = Mathf.Clamp(wheelSpeed / (180f * 0.35f), 0f, 1f);
        float heatSpeed = heaterSpeed/4f;
        carEngine2AudioActive = ignitionStarted;
        carRollingAudioActive = !allWheelsAirborne && wheelSpeed > 10f;
        //engineKnockingAudioActive = ignitionStarted && carHP < 17;
        //engineKnockSpeed = Mathf.Clamp(engineKnockingAnimCurve, 0.9f, 1.5f);
        SetVehicleAudioProperties(engineAudio1, carEngine1AudioActive, 0.65f * ignNum, highestAudio1, 2f, useVolumeInsteadOfPitch: false, 0.7f);
        SetVehicleAudioProperties(engineAudio2, carEngine2AudioActive, 0.7f, highestAudio2, 3f, useVolumeInsteadOfPitch: false, 0.5f);
        //SetVehicleAudioProperties(engineAudio4, engineKnockingAudioActive, 0.9f, engineKnockSpeed, 2f, false, 0.62f);
        SetVehicleAudioProperties(rollingAudio, carRollingAudioActive, 0f, highestTyre, 5f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(extremeStressAudio, underExtremeStress, 0.2f, 1f, 3f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(heaterAudio, heaterOn && ignitionStarted, 0f, heatSpeed, 3f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(backUpBeeperAudio, ignitionStarted && inReverse, 0f, 1f, 4f, useVolumeInsteadOfPitch: true);
        SetRadioValues();
        if (engineAudio1.volume > 0.3f && engineAudio1.isPlaying && 
            Time.realtimeSinceStartup - timeAtLastEngineAudioPing > 2f)
        {
            timeAtLastEngineAudioPing = Time.realtimeSinceStartup;
            if (!ignitionStarted)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 15f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            else if (EngineRPM < 980f)
            {
                if (!magnetedToShip) RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 11f, 0.5f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            else if (EngineRPM >= 980f && EngineRPM < 3700f)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 25f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            else if (EngineRPM >= 3700f)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 32f, 0.75f, 0, noiseIsInsideClosedShip: false, 2692);
            }
        }

        steeringWheelAudio.volume = 0.5f;
        //float currentXInput = Mathf.Abs(moveInputVector.x);
        //SetVehicleAudioProperties(steeringWheelAudio, currentXInput > 0.1f, 0f, currentXInput, 5f, true);

        SetVehicleAudioProperties(roofRainAudio, roofRainAudioActive, 0, 1f, 3f, useVolumeInsteadOfPitch: true);
        roofRainAudio.spatialBlend = Mathf.MoveTowards(roofRainAudio.spatialBlend, roofRainAudioActive ? 0f : 1f, 4f * Time.deltaTime);

        if (lastClutchPedalPressed != clutchPedalPressed)
        {
            lastClutchPedalPressed = clutchPedalPressed;
            if (lastClutchPedalPressed)
            {
                int engagedID = UnityEngine.Random.Range(0, clutchInClips.Length);
                currentDriver?.movementAudio.PlayOneShot(clutchInClips[engagedID]);
            }
            else
            {
                int disengagedID = UnityEngine.Random.Range(0, clutchOutClips.Length);
                currentDriver?.movementAudio.PlayOneShot(clutchOutClips[disengagedID]);
            }
        }

        if (voiceModule.voiceAudio.isPlaying && 
            Time.realtimeSinceStartup - timeAtLastEVAPing > 2f)
        {
            timeAtLastEVAPing = Time.realtimeSinceStartup;
            RoundManager.Instance.PlayAudibleNoise(voiceModule.voiceAudio.transform.position, 30f, 0.91f, 0, noiseIsInsideClosedShip: false, 106217);
        }

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
            if (Time.realtimeSinceStartup - timeAtLastAlarmPing > 2f)
            {
                timeAtLastAlarmPing = Time.realtimeSinceStartup;
                RoundManager.Instance.PlayAudibleNoise(hornAudio.transform.position, 30f, 0.91f, 0, noiseIsInsideClosedShip: false, 106217);
            }
            return;
        }
        if (honkingHorn)
        {
            hornAudio.pitch = 1f;

            if (!hornAudio.isPlaying)
                hornAudio.Play();

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


    // --- MISC EFFECTS ---
    // tyre skid effects
    public void SetCarTyreSlipEffects()
    {
        if (IsOwner)
        {
            float vehicleSpeed = Vector3.Dot(Vector3.Normalize(mainRigidbody.velocity * 1000f), transform.forward);
            float wheelSpeed = Mathf.Abs(backWheelRPM);
            bool audioActive = false;

            if (backWheelsGrounded)
            {
                bool forwardSlipping = currentMotorTorque > 600f && Mathf.Abs(forwardsSlip) > 0.245f;
                if (forwardSlipping && wheelSpeed > 350f)
                {
                    vehicleSpeed = Mathf.Max(vehicleSpeed, 0.8f);
                    audioActive = true;

                    if (averageVelocity.magnitude > 8f && !tireSparks.isPlaying)
                        tireSparks.Play(true);
                }
                else
                {
                    audioActive = false;
                    if (tireSparks.isEmitting)
                        tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            else
            {
                audioActive = false;
                if (tireSparks.isEmitting)
                    tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            SetVehicleAudioProperties(skiddingAudio, audioActive, 0f, vehicleSpeed, 3f, true, 1f);
            if (Mathf.Abs(syncedTyreStress - vehicleSpeed) > 0.02f || syncedTyreSlipping != audioActive)
            {
                syncedTyreStress = vehicleSpeed;
                syncedTyreSlipping = audioActive;
                SetTyreStressRpc(vehicleSpeed, audioActive);
            }
            return;
        }

        if (syncedTyreSlipping && averageVelocity.magnitude > 8f && !tireSparks.isPlaying)
        {
            tireSparks.Play(true);
        }
        else if (!syncedTyreSlipping && tireSparks.isEmitting)
        {
            tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        SetVehicleAudioProperties(skiddingAudio, syncedTyreSlipping, 0f, syncedTyreStress, 3f, true, 1f);
    }

    public void SetCarKeyEffects()
    {
        if (currentDriver == null || keyIsInIgnition)
        {
            if (keyObject.enabled != keyIsInIgnition)
                keyObject.enabled = keyIsInIgnition;

            if (keyObject.transform.parent != carKeyContainer.transform)
                keyObject.transform.SetParent(carKeyContainer.transform);
            keyObject.transform.localScale = ignitionKeyScale;

            if (carKeyInHand.transform.parent != carKeyContainer.transform)
                carKeyInHand.transform.SetParent(carKeyContainer.transform, false);
            carKeyInHand.transform.localScale = Vector3.one;

            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;

            keyObject.transform.position = ignitionKeyPosition.position;
            keyObject.transform.rotation = ignitionKeyPosition.rotation;
            return;
        }

        if (keyIsInDriverHand)
        {
            if (!keyObject.enabled)
                keyObject.enabled = true;

            Transform keyParent;
            Vector3 posOffset, rotOffset;

            keyParent = localPlayerInControl
                ? currentDriver.localItemHolder
                : currentDriver.serverItemHolder;

            posOffset = localPlayerInControl ? LHD_Pos_Local : LHD_Pos_Server;
            rotOffset = localPlayerInControl ? LHD_Rot_Local : LHD_Rot_Server;

            if (carKeyInHand.transform.parent != keyParent.parent)
                carKeyInHand.transform.SetParent(keyParent.parent, false);
            carKeyInHand.transform.localScale = Vector3.one;

            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;

            if (keyObject.transform.parent != carKeyInHand.transform)
                keyObject.transform.SetParent(carKeyInHand.transform);

            keyObject.transform.localPosition = posOffset;
            keyObject.transform.localRotation = Quaternion.Euler(rotOffset);
        }
        else
        {
            if (keyObject.enabled)
                keyObject.enabled = false;

            if (keyObject.transform.parent != carKeyContainer.transform)
                keyObject.transform.SetParent(carKeyContainer.transform);
            keyObject.transform.localScale = ignitionKeyScale;

            if (carKeyInHand.transform.parent != carKeyContainer.transform)
                carKeyInHand.transform.SetParent(carKeyContainer.transform, false);
            carKeyInHand.transform.localScale = Vector3.one;

            carKeyInHand.transform.localPosition = Vector3.zero;
            carKeyInHand.transform.localRotation = Quaternion.identity;

            keyObject.transform.position = ignitionKeyPosition.position;
            keyObject.transform.rotation = ignitionKeyPosition.rotation;
        }
    }


    // --- PHYSICS UPDATE ---
    public new void FixedUpdate()
    {
        SetVehicleToDropship();
        SetVehicleToFixedPosition();
        TryAttachToShipMagnet();

        MovePhysicsBodies();
        CalculateVehicleVelocity();
        SyncCarPhysicsToOtherClients();

        if (carDestroyed)
        {
            SetPreviousVehiclePosition();
            return;
        }

        ApplySteering();
        ApplyWheelForces();

        // wheel visuals
        MatchWheelMeshToCollider(leftWheelMesh, leftBrakeMesh, FrontLeftWheel);
        MatchWheelMeshToCollider(rightWheelMesh, rightBrakeMesh, FrontRightWheel);
        MatchWheelMeshToCollider(backLeftWheelMesh, backLeftBrakeMesh, BackLeftWheel);
        MatchWheelMeshToCollider(backRightWheelMesh, backRightBrakeMesh, BackRightWheel);

        // brake visuals
        leftBrakeMesh.transform.localEulerAngles = new Vector3(leftBrakeMesh.transform.localEulerAngles.x, FrontLeftWheel.steerAngle, 0f);
        rightBrakeMesh.transform.localEulerAngles = new Vector3(rightBrakeMesh.transform.localEulerAngles.x, FrontRightWheel.steerAngle, 0f);
        backLeftBrakeMesh.transform.localEulerAngles = new Vector3(backLeftBrakeMesh.transform.localEulerAngles.x, 0f, 0f);
        backRightBrakeMesh.transform.localEulerAngles = new Vector3(backRightBrakeMesh.transform.localEulerAngles.x, 0f, 0f);

        allWheelsAirborne = !FrontLeftWheel.isGrounded &&
                            !FrontRightWheel.isGrounded &&
                            !BackLeftWheel.isGrounded &&
                            !BackRightWheel.isGrounded;

        allWheelsGrounded = FrontLeftWheel.isGrounded &&
                            FrontRightWheel.isGrounded &&
                            BackLeftWheel.isGrounded &&
                            BackRightWheel.isGrounded;

        backWheelsGrounded = BackLeftWheel.isGrounded &&
                             BackRightWheel.isGrounded;

        frontWheelsGrounded = FrontLeftWheel.isGrounded &&
                              FrontRightWheel.isGrounded;

        if (!IsOwner)
        {
            SetCarPhysicsValuesOnClient();
            CalculateWheelSlip(calculatePhysics: false);
            SetWheelStiffness(FrontLeftWheel, false);
            SetWheelStiffness(FrontRightWheel, false);
            SetWheelStiffness(BackLeftWheel, false);
            SetWheelStiffness(BackRightWheel, false);
            SetPreviousVehiclePosition();
            return;
        }

        UpdateCarDrivetrain();
        UpdateCarEngine();

        SyncDrivetrain();
        SyncWheelTorque();

        SetSteeringDecay();

        if (mainRigidbody.IsSleeping() || magnetedToShip || allWheelsAirborne)
        {
            CalculateWheelSlip(calculatePhysics: false);
            SetPreviousVehiclePosition();
            return;
        }

        ApplyAntiSlipForce();
        CalculateWheelSlip(calculatePhysics: true);
        SetPreviousVehiclePosition();
    }

    private void SetCarPhysicsValuesOnClient()
    {
        currentMotorTorque = syncedCurrentMotorTorque;
        currentBrakeTorque = syncedCurrentBrakeTorque;

        float targetRpm = ignitionStarted ? syncedEngineRPM : 0f;
        EngineRPM = Mathf.Lerp(EngineRPM, targetRpm, 3f * Time.fixedDeltaTime);

        steeringDecay = 1f;
        enginePower = 0f;

        inclineCompensation = 1f;
        throttleInput = 0f;
        brakeInput = 0f;
        clutchEngagement = 1f;
        clutchSlip = 0f;

        frontWheelRPM = syncedFrontWheelRPM;
        backWheelRPM = syncedBackWheelRPM;
        wheelRPM = syncedWheelRPM;

        forwardWheelSpeed = 8000f;
        reverseWheelSpeed = -8000f;
    }

    private void SetPreviousVehiclePosition()
    {
        previousVehiclePosition = mainRigidbody.position;
        previousVehicleRotation = mainRigidbody.rotation;
        lastVelocity = mainRigidbody.velocity;
    }

    private void SetVehicleToDropship()
    {
        if (StartOfRound.Instance.inShipPhase ||
            loadedVehicleFromSave ||
            hasDeliveredVehicle)
            return;

        if (itemShip == null && References.itemShip != null)
            itemShip = References.itemShip;

        if (itemShip == null)
        {
            inDropshipAnimation = false;
            SetVehicleKinematic(setKinematic: true);
            mainRigidbody.MovePosition(StartOfRound.Instance.notSpawnedPosition.position + Vector3.forward * 30f);
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
            return;
        }
        if (itemShip.untetheredVehicle)
        {
            inDropshipAnimation = false;
            mainRigidbody.MovePosition(itemShip.deliverVehiclePoint.position);
            mainRigidbody.MoveRotation(itemShip.deliverVehiclePoint.rotation);
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
            hasBeenSpawned = true;
            hasDeliveredVehicle = true;
        }
        else if (itemShip.deliveringVehicle)
        {
            inDropshipAnimation = true;
            SetVehicleKinematic(setKinematic: true);
            mainRigidbody.MovePosition(itemShip.deliverVehiclePoint.position);
            mainRigidbody.MoveRotation(itemShip.deliverVehiclePoint.rotation);
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
        }
    }

    private void SetVehicleKinematic(bool setKinematic)
    {
        if (mainRigidbody.isKinematic == setKinematic)
            return;

        mainRigidbody.isKinematic = setKinematic;
        Plugin.Logger.LogDebug($"ScanVan: Set 'mainRigidbody' kinematic to: {setKinematic}");
    }

    private void SetVehicleToFixedPosition()
    {
        // magnet/client sync
        if (magnetedToShip)
        {
            SetVehicleKinematic(setKinematic: true);
            syncedPosition = mainRigidbody.position;
            syncedRotation = mainRigidbody.rotation;
            mainRigidbody.MovePosition(Vector3.Lerp(magnetStartPosition, StartOfRound.Instance.elevatorTransform.position + magnetTargetPosition, magnetPositionCurve.Evaluate(magnetTime)));
            mainRigidbody.MoveRotation(Quaternion.Lerp(magnetStartRotation, magnetTargetRotation, magnetRotationCurve.Evaluate(magnetRotationTime)));
            averageVelocityAtMagnetStart = Vector3.Lerp(averageVelocityAtMagnetStart, Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 4f), 4f * Time.fixedDeltaTime);
            if (!finishedMagneting) magnetStartPosition += Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 5f) * Time.fixedDeltaTime;
            return;
        }

        if (IsOwner || inDropshipAnimation)
            return;

        SetVehicleKinematic(setKinematic: true);
        Vector3 syncVel = syncedPosition + (averageVelocity * Time.fixedDeltaTime);
        //Mathf.Clamp(syncSpeedMultiplier * Vector3.Distance(mainRigidbody.position, syncVel), 1.3f, 300f);
        Vector3 position = Vector3.Lerp(mainRigidbody.position, syncVel, Time.fixedDeltaTime * syncSpeedMultiplier);
        mainRigidbody.MovePosition(position);
        mainRigidbody.MoveRotation(Quaternion.Lerp(mainRigidbody.rotation, syncedRotation, syncRotationSpeed));
        truckVelocityLastFrame = mainRigidbody.velocity;
    }

    private void TryAttachToShipMagnet()
    {
        if (magnetedToShip)
            return;

        if (!IsOwner || carDestroyed ||
            StartOfRound.Instance.isObjectAttachedToMagnet ||
            StartOfRound.Instance.attachedVehicle != null ||
            !StartOfRound.Instance.magnetOn ||
            Vector3.Distance(transform.position, StartOfRound.Instance.magnetPoint.position) >= 10f)
            return;

        if (!Physics.Linecast(transform.position, StartOfRound.Instance.magnetPoint.position, 256, QueryTriggerInteraction.Ignore))
        {
            StartMagneting();
            return;
        }
    }

    private void MovePhysicsBodies()
    {
        ragdollPhysicsBody.Move(
          transform.position,
          transform.rotation);
        windwiperPhysicsBody1.Move(
          windwiper1.position,
          windwiper1.rotation);
        windwiperPhysicsBody2.Move(
          windwiper2.position,
          windwiper2.rotation);
        playerPhysicsBody.transform.localPosition = Vector3.zero;
        playerPhysicsBody.transform.localRotation = Quaternion.identity;
    }

    private void CalculateVehicleVelocity()
    {
        if (averageCount > movingAverageLength)
        {
            averageVelocity += (mainRigidbody.velocity - averageVelocity) / (float)(movingAverageLength + 1);
        }
        else
        {
            averageCount++;
            averageVelocity += mainRigidbody.velocity;
            if (averageCount == movingAverageLength)
            {
                averageVelocity /= (float)averageCount;
            }
        }
    }

    private void ApplySteering()
    {
        steeringAngle = steeringWheelCurve.Evaluate(Mathf.Abs(steeringWheelAnimFloat)) * 50f * Mathf.Sign(steeringWheelAnimFloat) * steeringDecay;
        FrontLeftWheel.steerAngle = steeringAngle;
        FrontRightWheel.steerAngle = steeringAngle;
    }

    private void SetSteeringDecay()
    {
        if (averageVelocity.magnitude < 28f || !frontWheelsGrounded)
        {
            steeringDecay = Mathf.MoveTowards(steeringDecay, 1f, 4f * Time.fixedDeltaTime);
        }
        else if (averageVelocity.magnitude > 28f && frontWheelsGrounded)
        {
            steeringDecay = Mathf.Lerp(1f, 0.65f, (averageVelocity.magnitude - 28f) / 50f);
            steeringDecay = Mathf.Max(steeringDecay, 0.65f);
        }
    }

    private void ApplyWheelForces()
    {
        // front wheels
        SetTorqueToWheelCollider(FrontLeftWheel, currentMotorTorque, currentBrakeTorque);
        SetTorqueToWheelCollider(FrontRightWheel, currentMotorTorque, currentBrakeTorque);

        // back wheels
        SetTorqueToWheelCollider(BackLeftWheel, currentMotorTorque, currentBrakeTorque);
        SetTorqueToWheelCollider(BackRightWheel, currentMotorTorque, currentBrakeTorque);

        SetWheelRotationVelocity();
    }

    private void SetTorqueToWheelCollider(WheelCollider wheelCollider, float motorForce, float brakeForce)
    {
        wheelCollider.motorTorque = motorForce;
        wheelCollider.brakeTorque = brakeForce;
    }

    private void SetWheelRotationVelocity()
    {
        // rotation speed-limiter
        FrontLeftWheel.rotationSpeed = Mathf.Clamp(FrontLeftWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
        FrontRightWheel.rotationSpeed = Mathf.Clamp(FrontRightWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
        BackLeftWheel.rotationSpeed = Mathf.Clamp(BackLeftWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
        BackRightWheel.rotationSpeed = Mathf.Clamp(BackRightWheel.rotationSpeed, reverseWheelSpeed, forwardWheelSpeed);
    }

    private void CalculateWheelSlip(bool calculatePhysics)
    {
        if (!calculatePhysics)
        {
            forwardsSlip = 0f;
            return;
        }
        for (int i = 0; i < wheels.Count; i++)
        {
            if (wheels[i].GetGroundHit(out var hit))
            {
                wheelHits[i] = hit;
                SetWheelStiffness(wheels[i], hit.collider.CompareTag("Snow"));
            }
            else
            {
                wheelHits[i] = default;
            }
        }
        forwardsSlip = (wheelHits[2].forwardSlip + wheelHits[3].forwardSlip) * 0.5f;
    }

    private void UpdateCarDrivetrain()
    {
        bool gasPressed = drivePedalPressed && ignitionStarted;
        bool atIdle = !drivePedalPressed && ignitionStarted;

        float selectedGear = Mathf.Abs(gearRatios[currentGear]);
        float clutchGrip = clutchEngagement * clutchFriction;
        float wheelSpeed = Mathf.Abs(wheelRPM) * selectedGear * diffRatio * clutchGrip;

        // hill assist
        float slopeAngle = Vector3.Angle(transform.forward, Vector3.ProjectOnPlane(transform.forward, Vector3.up)); // get the slope angle
        float dot = Vector3.Dot(transform.forward, Vector3.up);

        float load = Mathf.Clamp01(Mathf.Abs(EngineRPM - wheelSpeed) / 800f); // rpm difference between the wheels and the engine
        float assistValue = Mathf.InverseLerp(500f, 0f, Mathf.Abs(wheelRPM)); // rpm the assist is active up until
        float assist = Mathf.Clamp01(dot) * assistValue * load; // assist rate

        float throttleAssist = Mathf.Lerp(1.25f, 0.5f, throttleInput);
        if (clutchPedalPressed)
            clutchEngagement = NormaliseFloat(Mathf.MoveTowards(clutchEngagement, 0f, clutchSpeed * Time.fixedDeltaTime));
        else
            clutchEngagement = NormaliseFloat(Mathf.MoveTowards(clutchEngagement, 1f, Mathf.Lerp(clutchReturnSpeed, minClutchReturnSpeed, assist) * throttleAssist * Time.fixedDeltaTime));

        if (drivePedalPressed)
            throttleInput = NormaliseFloat(Mathf.MoveTowards(throttleInput, 1f, Mathf.Lerp(throttleSpeed, maxThrottleSpeed, assist) * Time.fixedDeltaTime));
        else
            throttleInput = NormaliseFloat(Mathf.MoveTowards(throttleInput, 0f, throttleReleaseSpeed * Time.fixedDeltaTime));

        float slopeValue = 0f;
        if (dot > 0f) // only apply uphill
        {
            slopeValue = Mathf.Clamp01(slopeAngle / maxInclineCompensationAngle);
        }
        //inclineCompensation = (clutchEngagement * Mathf.Lerp(1.78f, 2.4f, slopeValue * assistValue));
        inclineCompensation = Mathf.Lerp(minInclineCompensation, maxInclineCompensation, slopeValue * assistValue);

        frontWheelRPM = NormaliseFloat((FrontLeftWheel.rpm + FrontRightWheel.rpm) / 2f);
        backWheelRPM = NormaliseFloat((BackLeftWheel.rpm + BackRightWheel.rpm) / 2f);
        wheelRPM = NormaliseFloat((frontWheelRPM + backWheelRPM) / 2f);

        if (handbrakeEngaged) currentBrakeTorque = Mathf.MoveTowards(currentBrakeTorque, maxBrakingPower, brakeSpeed * Time.fixedDeltaTime);
        else
        {
            if (brakePedalPressed) currentBrakeTorque = Mathf.MoveTowards(currentBrakeTorque, maxBrakingPower * brakeInput, brakeSpeed * Time.fixedDeltaTime);
            else if (!brakePedalPressed && brakeInput == 0f) currentBrakeTorque = 0f;
        }

        if (!ignitionStarted || inNeutral || clutchEngagement == 0f)
        {
            currentMotorTorque = 0f;
            if (!inNeutral && !ignitionStarted) currentBrakeTorque = maxBrakingPower;

            forwardWheelSpeed = 8000f;
            reverseWheelSpeed = -8000f;
            return;
        }
        else if (inReverse)
        {
            if (gasPressed) currentMotorTorque = -engineReversePower * clutchGrip;
            else if (atIdle) currentMotorTorque = idleSpeed * -1f * clutchGrip;

            // this has to be inverted for reverse
            forwardWheelSpeed = MaxEngineRPM / (gearRatios[Mathf.Clamp(0, gearRatios.Length - 1, 1)] * diffRatio) * (360f / 60f);
            reverseWheelSpeed = MaxEngineRPM / (gearRatios[0] * diffRatio) * (360f / 60f);
            return;
        }
        if (gasPressed) currentMotorTorque = enginePower * inclineCompensation * clutchGrip;
        else if (atIdle) currentMotorTorque = idleSpeed * clutchGrip;

        if (currentGear < 1) // do not let the current gear drop below its minimum
            currentGear = 1;

        forwardWheelSpeed = MaxEngineRPM / (gearRatios[Mathf.Clamp(currentGear, 1, gearRatios.Length - 1)] * diffRatio) * (360f / 60f); // ensure we don't set a reverse speed on the forward speed
        reverseWheelSpeed = MaxEngineRPM / (gearRatios[0] * diffRatio) * (360f / 60f); // 0 in our array is always reverse, so use zero for the backwards speed
    }

    private void ApplyAntiSlipForce()
    {
        Vector3 groundNormal = Vector3.zero;
        for (int i = 0; i < wheelHits.Length; i++)
        {
            groundNormal += wheelHits[i].normal;
        }
        groundNormal = groundNormal.normalized;

        if (!allWheelsGrounded || Vector3.Angle(-groundNormal, Physics.gravity) > 30f)
            return;

        Vector3 carFrontHillDirection = Vector3.ProjectOnPlane(transform.forward, groundNormal).normalized;
        Vector3 hillGravity = -groundNormal * Physics.gravity.magnitude;

        Vector3 force = hillGravity - Physics.gravity; //apply the difference between real gravity and the 'hill' downward gravity

        if (!handbrakeEngaged)
        {
            force = Vector3.ProjectOnPlane(force, carFrontHillDirection);
        }
        mainRigidbody.AddForce(force, ForceMode.Acceleration);
    }

    // --- HELPER FUNCTION ---
    public float NormaliseFloat(float num)
    {
        if (float.IsNaN(num) || float.IsInfinity(num) || 
            float.IsNegativeInfinity(num) || float.IsPositiveInfinity(num))
            return 0f;
        return num;
    }


    // --- TYRE SURFACE SLIP ---
    private void SetWheelStiffness(WheelCollider wheel, bool isSnow)
    {
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

        forwardFriction.stiffness = isSnow ? 0.56f : baseForwardStiffness;
        sidewaysFriction.stiffness = isSnow ? 0.58f : baseSidewaysStiffness;

        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }


    // --- DRIVETRAIN SYNC ---
    public void SyncWheelTorque()
    {
        if (syncTorqueInterval >= 0.14f)
        {
            float fWheelSyncRPM = Mathf.Round(currentMotorTorque);
            float bWheelSyncRPM = Mathf.Round(currentBrakeTorque);

            if (syncedCurrentMotorTorque != fWheelSyncRPM ||
                syncedCurrentBrakeTorque != bWheelSyncRPM)
            {
                syncTorqueInterval = 0f;
                syncedCurrentMotorTorque = currentMotorTorque;
                syncedCurrentBrakeTorque = currentBrakeTorque;
                SyncWheelTorqueRpc(currentMotorTorque, currentBrakeTorque);
                return;
            }
        }
        else
        {
            syncTorqueInterval += Time.fixedDeltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncWheelTorqueRpc(float motorTorque, float brakeTorque)
    {
        syncedCurrentMotorTorque = motorTorque;
        syncedCurrentBrakeTorque = brakeTorque;
    }


    public void SyncDrivetrain()
    {
        float syncThreshold = 0.15f * averageVelocity.magnitude;
        syncThreshold = Mathf.Clamp(syncThreshold, 0.15f, 0.21f);
        if (syncDrivetrainInterval >= syncThreshold)
        {
            float engineSpeed = NormaliseFloat(Mathf.Round(EngineRPM));

            float wheelSyncRPM = NormaliseFloat(Mathf.Round(wheelRPM));
            float fWheelSyncRPM = NormaliseFloat(Mathf.Round(frontWheelRPM));
            float bWheelSyncRPM = NormaliseFloat(Mathf.Round(backWheelRPM));

            if (syncedWheelRPM != wheelSyncRPM ||
                syncedEngineRPM != engineSpeed)
            {
                syncDrivetrainInterval = 0f;

                syncedFrontWheelRPM = fWheelSyncRPM;
                syncedBackWheelRPM = bWheelSyncRPM;

                syncedWheelRPM = wheelSyncRPM;
                syncedEngineRPM = engineSpeed;

                SyncDrivetrainRpc(frontWheelRPM, backWheelRPM, wheelRPM, syncedEngineRPM);
                return;
            }
        }
        else
        {
            syncDrivetrainInterval += Time.fixedDeltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncDrivetrainRpc(float frontWheelSpeed, float backWheelSpeed, float wheelSpeed, float engineSpeed)
    {
        syncedFrontWheelRPM = frontWheelSpeed;
        syncedBackWheelRPM = backWheelSpeed;
        syncedWheelRPM = wheelSpeed;
        syncedEngineRPM = engineSpeed;
    }


    // --- ENGINE ---
    private void UpdateCarEngine()
    {
        if (carDestroyed) return;
        if (!ignitionStarted)
        {
            enginePower = 0f;
            stallTimer = 0f;
            return;
        }

        float selectedGear = Mathf.Abs(gearRatios[currentGear]);
        float wheelSpeed = Mathf.Abs(backWheelRPM) * selectedGear * diffRatio;

        enginePower = enginePowerCurve.Evaluate(EngineRPM / MaxEngineRPM) *
          EngineTorque * (selectedGear * diffRatio);

        if (inNeutral || clutchEngagement == 0)
        {
            stallTimer = 0f;
            EngineRPM = Mathf.Lerp(EngineRPM, drivePedalPressed ? MaxEngineRPM : MinEngineRPM,
                drivePedalPressed ? 2f * Time.fixedDeltaTime : 1.8f * Time.fixedDeltaTime);
            return;
        }
        float clutchValue = Mathf.Pow(clutchEngagement, 2f);
        float freeRPM = Mathf.Lerp(MinEngineRPM, MaxEngineRPM, throttleInput);
        float targetRPM = Mathf.Lerp(freeRPM, wheelSpeed, clutchValue);
        float engineAcceleration = Mathf.Lerp(2f, 3.5f, clutchValue);

        EngineRPM = Mathf.Lerp(EngineRPM, targetRPM, engineAcceleration * Time.fixedDeltaTime);
        EngineRPM = Mathf.Clamp(EngineRPM, 0f, MaxEngineRPM);
        clutchSlip = Mathf.Abs(EngineRPM - wheelSpeed);

        bool engineLugging = EngineRPM <= MinEngineRPM - 150f;
        bool engineOverloaded = !clutchPedalPressed && clutchEngagement >= 0.25f && 
                                drivePedalPressed && EngineRPM <= MinEngineRPM + 300f && 
                                !engineLugging;

        if (engineLugging || engineOverloaded) // || EngineRPM - wheelSpeed >= 300f
        {
            stallTimer += Time.fixedDeltaTime;
            if (stallTimer > (engineLugging ? 0.01f : 0.25f) && !engineStalled)
            {
                StallEngineRpc();
                engineStalled = true;
                stallTimer = 0f;
                StallEngine();
                return;
            }
        }
        else
        {
            stallTimer = 0f;
        }
    }

    public void StallEngine()
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
            keyIgnitionCoroutine = null!;
        }
        disableAnimations = true;
        engineAudio3.volume = 0.775f;
        engineAudio3.PlayOneShot(stallEngine);
        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, 1);
        ignitionAnimator.SetInteger(IGNITION_ANIM, 7);
        chanceToStartIgnition = 101f;
        SetFrontCabinLightOn(true);
        carEngine1AudioActive = false;
        ignitionStarted = false;
        usingSwitchIgnition = false;
        carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        TrySetCarIgnitionTriggers();
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void StallEngineRpc()
    {
        engineStalled = true;
        stallTimer = 0f;
        StallEngine();
    }


    // --- MISC SYNC METHODS ---
    public void SyncCarEffectsToOtherClients()
    {
        if (syncEffectsInterval > 0.045f)
        {
            if (syncedSteeringWheelRotation != steeringWheelAnimFloat)
            {
                syncEffectsInterval = 0f;
                syncedSteeringWheelRotation = steeringWheelAnimFloat;
                SyncCarEffectsRpc(steeringWheelAnimFloat, smoothRotation);
                return;
            }
        }
        else
        {
            syncEffectsInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarEffectsRpc(float wheelRotation, bool smoothRot)
    {
        syncedSteeringWheelRotation = wheelRotation;
        smoothRotation = smoothRot;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncPlayerInputsRpc(Vector2 playerInput, bool gasPressed, bool brakePressed, bool clutchPressed)
    {
        syncedMoveInputVector = playerInput;
        syncedDrivePedalPressed = gasPressed;
        syncedBrakePedalPressed = brakePressed;
        syncedClutchPedalPressed = clutchPressed;
    }

    private new void SyncCarPhysicsToOtherClients()
    {
        if (!IsOwner || magnetedToShip || inDropshipAnimation)
            return;

        SetVehicleKinematic(setKinematic: false);
        float syncThreshold = 0.12f * (averageVelocity.magnitude / 200f);
        syncThreshold = Mathf.Clamp(syncThreshold, 0.01f, 0.2f);
        if (syncCarPositionInterval >= syncThreshold)
        {
            if (Vector3.Distance(syncedPosition, transform.position) > 0.014f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
                SyncCarPositionRpc(transform.position, transform.eulerAngles);
                return;
            }
            if (Vector3.Angle(transform.forward, syncedRotation * Vector3.forward) > 2f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
                SyncCarPositionRpc(transform.position, transform.eulerAngles);
                return;
            }
        }
        else
        {
            syncCarPositionInterval += Time.fixedDeltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarPositionRpc(Vector3 carPosition, Vector3 carRotation)
    {
        syncedPosition = carPosition;
        syncedRotation = Quaternion.Euler(carRotation);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetTyreStressRpc(float stress, bool wheelSkidding)
    {
        syncedTyreStress = stress;
        syncedTyreSlipping = wheelSkidding;
    }


    // --- SCAN NODE MISC ---
    public void UpdateScanNodeText()
    {
        if (carDestroyed)
        {
            scanNode.headerText = "Destroyed Truck";
            scanNode.subText = "Your once trusty mule..";
            return;
        }

        if (GameNetworkManager.Instance.connectedPlayers == 1) scanNode.subText = "Your only trusty mule..";
        else scanNode.subText = "Your trusty mule!";
    }


    // --- LATE UPDATE METHOD ---
    public new void LateUpdate()
    {
        UpdateScanNodeText();
        if (carDestroyed)
            return;
        SetDashboardSymbols();

        if (localPlayerInControl && !setControlTips)
        {
            setControlTips = true;
            HUDManager.Instance.ChangeControlTipMultiple(carTooltips, false, null);
        }
        else if (!localPlayerInControl && setControlTips)
        {
            setControlTips = false;
        }

        bool muteAudio = magnetedToShip && (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipDoorsEnabled);
        if (StartOfRound.Instance.inShipPhase && windshieldBroken)
        {
            RegenerateWindshield();
        }

        hornAudio.mute = muteAudio;
        engineAudio1.mute = muteAudio;
        engineAudio2.mute = muteAudio;
        carKeySounds.mute = muteAudio;
        cabinLightSwitchAudio.mute = muteAudio;
        heaterAudio.mute = muteAudio;
        voiceModule.voiceAudio.mute = muteAudio;
        wiperAudio.mute = muteAudio;
        rollingAudio.mute = muteAudio;
        skiddingAudio.mute = muteAudio;
        turbulenceAudio.mute = muteAudio;
        hoodFireAudio.mute = muteAudio;
        extremeStressAudio.mute = muteAudio;
        pushAudio.mute = muteAudio;
        bool isAlertActive = voiceModule.audioIsPlaying || voiceModule.voiceAudio.isPlaying || voiceModule.voiceAudio.clip != null ||
            (voiceModule.isPlayingIgnitionChime && !voiceModule.hasAlertedOnEngineStart);
        radioAudio.mute = muteAudio || isAlertActive;
        radioInterference.mute = muteAudio || isAlertActive;

        if (currentDriver != null && References.lastDriver != currentDriver && !magnetedToShip)
            References.lastDriver = currentDriver;

        if (honkingHorn && hornAudio.isPlaying && hornAudio.pitch < 1f)
            hornAudio.Stop();

        voiceModule.DoEVACycle();
    }

    /*
    public void AddExtraWeightToCar()
    {
        if (References.itemsInTruck == null ||
            References.itemsInTruck.Count <= 10f)
        {
            extraWeight = 0f;
            return;
        }
        extraWeight = 0f;
    }
    */


    // --- DASHBOARD SYMBOLS ---
    public void SetDashboardSymbols()
    {
        if (!keyIsInIgnition)
        {
            SetSymbolActive(dippedBeamLightSymbol, dippedBeamLight, false);
            SetSymbolActive(highBeamLightSymbol, highBeamLight, false);
            SetSymbolActive(parkingBrakeSymbol, parkingBrakeLight, false);
            SetSymbolActive(oilLevelLightSymbol, oilLevelLight, false);
            SetSymbolActive(batteryLightSymbol, batteryLight, false);
            SetSymbolActive(coolantLevelLightSymbol, coolantLevelLight, false);
            SetSymbolActive(alertLightSymbol, alertLight, false);
            SetSymbolActive(checkEngineLightSymbol, checkEngineLight, false);
            return;
        }

        if (!hasSweepedDashboard)
            return;

        SetSymbolActive(dippedBeamLightSymbol, dippedBeamLight, currentSweepStage > 1 && lowBeamsOn);
        SetSymbolActive(highBeamLightSymbol, highBeamLight, currentSweepStage > 1 && highBeamsOn);
        SetSymbolActive(parkingBrakeSymbol, parkingBrakeLight, currentSweepStage > 2 && handbrakeEngaged);
        SetSymbolActive(oilLevelLightSymbol, oilLevelLight, currentSweepStage > 3 && carHP <= 25);
        SetSymbolActive(batteryLightSymbol, batteryLight, currentSweepStage > 3 && (!ignitionStarted || carHP <= 22));
        SetSymbolActive(coolantLevelLightSymbol, coolantLevelLight, currentSweepStage > 3 && carHP <= 30);
        SetSymbolActive(alertLightSymbol, alertLight, currentSweepStage > 3 && carHP <= 16);
        SetSymbolActive(checkEngineLightSymbol, checkEngineLight, currentSweepStage > 3 && carHP <= 38);
    }

    public void SetSymbolActive(SpriteRenderer symbol, Light symbolLight, bool active)
    {
        if (symbol == null || symbol.enabled == active)
            return;

        symbol.enabled = active;
        if (symbolLight != null) symbolLight.enabled = active;
    }


    // --- COLLISION ---
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
                CarBumpLocalClient(averageVelocity * 0.7f);
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
                        enemyHitSpeed = 1f;
                        _ = carReactToPlayerHitMultiplier;
                    }
                    else if (obstacleSize <= 2f)
                    {
                        enemyHitSpeed = 9f;
                        _ = carReactToPlayerHitMultiplier;
                    }
                    else
                    {
                        enemyHitSpeed = 15f;
                        _ = carReactToPlayerHitMultiplier;
                    }
                    vel = Vector3.Scale(vel, new Vector3(1f, 0f, 1f));
                    mainRigidbody.AddForceAtPosition(Vector3.up * torqueForce, position, ForceMode.VelocityChange);
                    bool result = false;
                    if (vel.magnitude < enemyHitSpeed)
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
                            CarBumpLocalClient(averageVelocity);
                            mainRigidbody.velocity = Vector3.Normalize(-impulse * 100000000f) * 9f;
                            PlayerControllerB playerControllerB;
                            if (currentDriver != null)
                            {
                                playerControllerB = currentDriver;
                            }
                            else
                            {
                                if (currentMiddlePassenger != null)
                                    playerControllerB = currentMiddlePassenger;
                                else if (currentPassenger != null)
                                    playerControllerB = currentPassenger;
                                else
                                    playerControllerB = null!;
                            }

                            if (vel.magnitude > 2f && dealDamage)
                            {
                                enemyScript.HitEnemyOnLocalClient(2, Vector3.zero, playerControllerB, playHitSFX: true, 331);
                            }
                            result = true;
                            if ((isSpecial && obstacleSize > 1f) || (!isSpecial && obstacleSize > 2f))
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
                            DealPermanentDamage(isSpecial ? 1 : 2, position);
                        }
                        PlayerControllerB playerWhoHit;
                        if (currentDriver != null)
                        {
                            playerWhoHit = currentDriver;
                        }
                        else
                        {
                            if (currentMiddlePassenger != null)
                                playerWhoHit = currentMiddlePassenger;
                            else if (currentPassenger != null)
                                playerWhoHit = currentPassenger;
                            else
                                playerWhoHit = null!;
                        }
                        enemyScript.HitEnemyOnLocalClient(20, Vector3.zero, playerWhoHit, false, 331);
                    }
                    PlayCollisionAudio(position, 5, 1f);
                    return result;
                }
            default:
                return false;
        }
    }

    public new void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner)
            return;

        if (magnetedToShip || !hasBeenSpawned)
            return;

        if (collision.collider.gameObject.layer != 8)
            return;

        if (Time.realtimeSinceStartup - timeSinceLastCollision < 0.05f ||
            Time.realtimeSinceStartup - timeSinceUntethered < 4f)
            return;

        timeSinceLastCollision = Time.realtimeSinceStartup;

        float differenceInVelocity = Mathf.Abs(lastVelocity.magnitude - mainRigidbody.velocity.magnitude);
        float collisionImpulse = 0f;
        int contactCount = collision.GetContacts(contacts);
        Vector3 setPosition = Vector3.zero;

        for (int i = 0; i < contactCount; i++)
        {
            if (contacts[i].impulse.magnitude > collisionImpulse)
            {
                collisionImpulse = contacts[i].impulse.magnitude;
            }
            setPosition += contacts[i].point;
        }

        setPosition /= (float)contactCount;
        collisionImpulse /= Time.fixedDeltaTime;

        if (collisionImpulse < minimalBumpForce || averageVelocity.magnitude < 6f)
        {
            if (collisionImpulse > 3 && averageVelocity.magnitude > 2.5f)
            {
                SetInternalStress(0.15f);
                lastStressType = "Scraping";
            }
            return;
        }

        /*
        HUDManager.Instance.enableConsoleLogging = true;
        HUDManager.Instance.SetDebugText($"diff? {differenceInVelocity}\nlastVel? {lastVelocity.magnitude}\nbumpForce? {collisionImpulse}");
        */

        
        float collisionVolume = 0.5f;
        int audioType = -1;

        /*
        if (differenceInVelocity >= 9f && lastVelocity.magnitude > 34f)
        {
            if (carHP < 3)
            {
                DestroyCarRpc();
                DestroyCar();
                return;
            }
            if (!windshieldBroken)
            {
                BreakWindshield();
                BreakWindshieldRpc();
            }
            DamageVehicle((float)UnityEngine.Random.Range(20, 30), collision.relativeVelocity, carHP - 10);
            audioType = 2;
            collisionVolume = Mathf.Clamp((collisionImpulse - maximumBumpForce) / 20000f, 0.8f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);
            PlayCollisionAudio(setPosition, audioType, collisionVolume);
            return;
        }

        if (collisionImpulse > maximumBumpForce && lastVelocity.magnitude >= 20f)
        {
            audioType = 2;
            collisionVolume = Mathf.Clamp((collisionImpulse - maximumBumpForce) / 20000f, 0.8f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);
            DamageVehicle(differenceInVelocity, collision.relativeVelocity, 1);
        }
        else if (collisionImpulse > mediumBumpForce && lastVelocity.magnitude >= 10f)
        {
            audioType = 1;
            collisionVolume = Mathf.Clamp((collisionImpulse - mediumBumpForce) / (maximumBumpForce - mediumBumpForce), 0.67f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.5f, 1f);
        }
        else if (lastVelocity.magnitude > 3f)
        {
            audioType = 1;
            collisionVolume = Mathf.Clamp((collisionImpulse - mediumBumpForce) / (maximumBumpForce - mediumBumpForce), 0.25f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.25f, 1f);
        }

        if (audioType != -1)
        {
            PlayCollisionAudio(setPosition, audioType, collisionVolume);
            if (collisionImpulse > maximumBumpForce + 10000f && lastVelocity.magnitude > 20f && differenceInVelocity >= 9f)
            {
                DamageVehicle((float)UnityEngine.Random.Range(10, 20), Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), (int)(differenceInVelocity/2f));
                if (!windshieldBroken)
                {
                    BreakWindshield();
                    BreakWindshieldRpc();
                }
                CarCollisionRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                DealPermanentDamage(2);
                return;
            }
            CarBumpLocalClient(Vector3.ClampMagnitude(-collision.relativeVelocity, 40f));
        }
        */

        
        if (differenceInVelocity >= 11f && lastVelocity.magnitude > 38f)
        {
            if (carHP < 3)
            {
                DestroyCarRpc();
                DestroyCar();
                return;
            }

            DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
            if (!windshieldBroken)
            {
                BreakWindshield();
                BreakWindshieldRpc();
            }
            CarCollisionRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
            DealPermanentDamage(UnityEngine.Random.Range(10, 20));

            audioType = 2;
            collisionVolume = Mathf.Clamp((collisionImpulse - maximumBumpForce) / 20000f, 0.8f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);

            PlayCollisionAudio(setPosition, audioType, collisionVolume);
            return;
        }

        if (collisionImpulse >= minimalBumpForce && collisionImpulse < mediumBumpForce && differenceInVelocity > 4f)
        {
            audioType = 0;
            collisionVolume = Mathf.Clamp((collisionImpulse - minimalBumpForce) / (mediumBumpForce - minimalBumpForce), 0.25f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.25f, 1f);
        }
        else if (collisionImpulse >= mediumBumpForce && collisionImpulse < maximumBumpForce && differenceInVelocity > 9f)
        {
            audioType = 1;
            collisionVolume = Mathf.Clamp((collisionImpulse - mediumBumpForce) / (maximumBumpForce - mediumBumpForce), 0.67f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.5f, 1f);
        }
        else if (collisionImpulse >= maximumBumpForce && differenceInVelocity > 9.5f)
        {
            audioType = 2;
            collisionVolume = Mathf.Clamp((collisionImpulse - maximumBumpForce) / 20000f, 0.8f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);
            DamageVehicle(differenceInVelocity, collision.relativeVelocity, 1);
        }

        PlayCollisionAudio(setPosition, audioType, collisionVolume);
        if (collisionImpulse > maximumBumpForce + 10000f && lastVelocity.magnitude >= 28f)
        {
            audioType = 2;
            collisionVolume = Mathf.Clamp((collisionImpulse - maximumBumpForce) / 20000f, 0.8f, 1f);
            collisionVolume = Mathf.Clamp(collisionVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);
            if (differenceInVelocity >= 14f)
            {
                DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                if (!windshieldBroken)
                {
                    BreakWindshield();
                    BreakWindshieldRpc();
                }
                CarCollisionRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                DealPermanentDamage(2);
                return;
            }
        }
        CarBumpLocalClient(Vector3.ClampMagnitude(-collision.relativeVelocity, 40f));
        
    }

    public void DamageVehicle(float diff, Vector3 collision, int damageAmount = 1)
    {
        DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision, 60f), diff / 1.5f);
        CarCollisionRpc(Vector3.ClampMagnitude(-collision, 60f), diff / 1.5f);
        if (damageAmount != 0) DealPermanentDamage(damageAmount);        
        //if (diff >= 11f)
        //{
        //    DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision, 60f), diff);
        //    CarCollisionRpc(Vector3.ClampMagnitude(-collision, 60f), diff);
        //    if (damageAmount != 0) DealPermanentDamage(damageAmount);
        //}
        //else if (diff > 4f && diff < 11f)
        //{
        //    DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision, 60f), diff/1.5f);
        //    CarCollisionRpc(Vector3.ClampMagnitude(-collision, 60f), diff/1.5f);
        //    if (damageAmount != 0) DealPermanentDamage(damageAmount);
        //}
    }

    public void CarBumpLocalClient(Vector3 vel)
    {
        if (localPlayerInControl ||
            localPlayerInMiddlePassengerSeat ||
            localPlayerInPassengerSeat)
            return;
        if (!VehicleUtils.IsPlayerInVanBounds(this))
            return;
        if (VehicleUtils.IsPlayerInVanCabin(this)) vel = Vector3.ClampMagnitude(vel, 5);
        else if (VehicleUtils.IsPlayerInVanStorage(this)) vel = Vector3.ClampMagnitude(vel, 30);
        GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;

        CarBumpRpc(vel);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CarBumpRpc(Vector3 vel)
    {
        if (localPlayerInControl ||
            localPlayerInMiddlePassengerSeat ||
            localPlayerInPassengerSeat)
            return;
        if (!VehicleUtils.IsPlayerInVanBounds(this))
            return;
        if (VehicleUtils.IsPlayerInVanCabin(this)) vel = Vector3.ClampMagnitude(vel, 5);
        else if (VehicleUtils.IsPlayerInVanStorage(this)) vel = Vector3.ClampMagnitude(vel, 30);
        GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CarCollisionRpc(Vector3 vel, float magn)
    {
        DamagePlayerInVehicle(vel, magn);
    }

    private new void DamagePlayerInVehicle(Vector3 vel, float magnitude)
    {
        if (!localPlayerInPassengerSeat &&
            !localPlayerInMiddlePassengerSeat &&
            !localPlayerInControl)
        {
            if (!VehicleUtils.IsPlayerInVanBounds(this))
                return;
            if (GameNetworkManager.Instance.localPlayerController.health < 15)
            {
                GameNetworkManager.Instance.localPlayerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f, false);
                return;
            }
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            GameNetworkManager.Instance.localPlayerController.DamagePlayer(5, true, true, CauseOfDeath.Inertia, 0, false, vel);
            GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
            return;
        }
        if (magnitude > 27f)
        {
            if (GameNetworkManager.Instance.localPlayerController.health < 10)
            {
                GameNetworkManager.Instance.localPlayerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f, false);
                return;
            }
            GameNetworkManager.Instance.localPlayerController.DamagePlayer((int)vel.magnitude, true, true, CauseOfDeath.Inertia, 0, false, vel);
            return;
        }
        if (magnitude <= 16f && magnitude >= 4f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            GameNetworkManager.Instance.localPlayerController.DamagePlayer(5, true, true, CauseOfDeath.Inertia, 0, false, vel);
            return;
        }
        HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
        if (GameNetworkManager.Instance.localPlayerController.health < 10)
        {
            GameNetworkManager.Instance.localPlayerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f, false);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.DamagePlayer(10, true, true, CauseOfDeath.Inertia, 0, false, vel);
    }

    //[Rpc(SendTo.NotMe, RequireOwnership = false)]
    //public void ShatterWindshieldRpc()
    //{
    //    ShatterWindshield();
    //}

    //public void ShatterWindshield()
    //{
    //    if (windshieldShattered)
    //        return;

    //    windshieldBroken = false;
    //    windshieldShattered = true;
    //    Material[] array = windshieldMesh.sharedMaterials;
    //    array[0] = windshieldBrokenMat;
    //    windshieldMesh.sharedMaterials = array;
    //    miscAudio.transform.localPosition = windshieldObject.transform.localPosition;
    //    miscAudio.PlayOneShot(windshieldBreak);
    //}

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void BreakWindshieldRpc()
    {
        BreakWindshield();
    }

    public new void BreakWindshield()
    {
        if (windshieldBroken)
            return;

        windshieldBroken = true;
        //windshieldShattered = true;
        windshieldObject.SetActive(false);
        glassParticle.Play();
        miscAudio.transform.localPosition = windshieldObject.transform.localPosition;
        miscAudio.PlayOneShot(windshieldBreak);
    }

    private void RegenerateWindshield()
    {
        windshieldBroken = false;
        //windshieldShattered = false;
        windshieldObject.SetActive(true);
        Material[] array = windshieldMesh.sharedMaterials;
        array[0] = windshieldMat;
        windshieldMesh.sharedMaterials = array;
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
                CarCollisionSFXRpc(collisionAudio1.transform.localPosition, 0, audioType, setVolume);
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
                CarCollisionSFXRpc(collisionAudio2.transform.localPosition, 1, audioType, setVolume);
            }
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CarCollisionSFXRpc(Vector3 audioPosition, int audio, int audioType, float vol)
    {
        AudioSource audioSource = ((audio != 0) ? collisionAudio2 : collisionAudio1);
        bool audioFinished = audioSource.clip.length - audioSource.time < 0.2f;
        audioSource.transform.localPosition = audioPosition;
        PlayRandomClipAndPropertiesFromAudio(audioSource, vol, audioFinished, audioType);
    }

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
        if (ignitionStarted)
        {
            if (audioType >= 2)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 18f + setVolume / 1f * 7f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
            }
            else if (audioType >= 1)
            {
                RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 12f + setVolume / 1f * 7f, 0.6f, 0, noiseIsInsideClosedShip: false, 2692);
            }
        }
        if (audioType == -1)
        {
            array = minCollisions;
            audioClip = array[UnityEngine.Random.Range(0, array.Length)];
            audio.PlayOneShot(audioClip);
        }
    }

    private new void SetInternalStress(float carStressIncrease = 0f)
    {
        if (!IsOwner || magnetedToShip || carDestroyed)
        {
            return;
        }

        if (carStressIncrease <= 0f) carStressChange = Mathf.Clamp(carStressChange - Time.deltaTime, -0.25f, 0.5f);
        else carStressChange = Mathf.Clamp(carStressChange + Time.deltaTime * carStressIncrease, 0f, 10f);

        underExtremeStress = (carStressIncrease >= 1f);
        carStress = Mathf.Clamp(carStress + carStressChange, 0f, 100f);

        if (carStress > 7f)
        {
            carStress = 0f;
            DealPermanentDamage(2, default(Vector3));
            lastDamageType = "Stress";
        }
    }

    public new void DealPermanentDamage(int damageAmount, Vector3 damagePosition = default(Vector3))
    {
        if (!IsOwner || magnetedToShip || carDestroyed)
        {
            return;
        }
        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP -= damageAmount;
        syncedCarHP = carHP;
        if (carHP <= 0)
        {
            DealDamageRpc(carHP);
            DestroyCarRpc();
            DestroyCar();
            return;
        }
        DealDamageRpc(carHP);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void DealDamageRpc(int carHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP = carHealth;
        syncedCarHP = carHP;
    }


    // --- DESTRUCTION ---
    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void DestroyCarRpc()
    {
        DestroyCar();
    }

    public new void DestroyCar()
    {
        if (carDestroyed)
            return;

        carDestroyed = true;
        UnMagnetCar();
        StopAudiosPlayback();
        RemoveCarRainCollision();
        CollectItemsInTruck();
        StopParticleVFX();
        SetMaterials();
        SetLightsOff();
        BreakWindshield();


        RoundManager.Instance.PlayAudibleNoise(engineAudio1.transform.position, 20f, 0.8f, 0, noiseIsInsideClosedShip: false, 2692);

        DisableWheelCollider(FrontLeftWheel, leftWheelMesh, leftBrakeMesh);
        DisableWheelCollider(FrontRightWheel, rightWheelMesh, rightBrakeMesh);
        DisableWheelCollider(BackLeftWheel, backLeftWheelMesh, backLeftBrakeMesh);
        DisableWheelCollider(BackRightWheel, backRightWheelMesh, backRightBrakeMesh);

        DisableObjectsOnDestroy();
        EnableObjectsOnDestroy();
        destroyedTruckMesh.SetActive(true);

        SetExplosionForce(forceMultiplier: 1560f, explosionPos: hoodFireAudio.transform.position);

        DisableIgnition();
        DisableDrivetrain();

        ResetControl();
        KillOccupants();

        SetInteractions();

        ResetOccupants();

        Landmine.SpawnExplosion(transform.position + transform.forward + Vector3.up * 1.5f, spawnExplosionEffect: true, 6f, 10f, 30, 1000f, truckDestroyedExplosion, goThroughCar: true);
    }

    private void UnMagnetCar()
    {
        if (!magnetedToShip || StartOfRound.Instance.attachedVehicle != this)
            return;

        magnetedToShip = false;
        StartOfRound.Instance.attachedVehicle = null;
        StartOfRound.Instance.isObjectAttachedToMagnet = false;
        CollectItemsInTruck();
    }

    private void StopAudiosPlayback()
    {
        underExtremeStress = false;
        engineAudio1.Stop();
        engineAudio2.Stop();
        engineAudio3.Stop();
        engineAudio4.Stop();
        turbulenceAudio.Stop();
        pushAudio.Stop();
        miscAudio.Stop();
        steeringWheelAudio.Stop();
        gearStickAudio.Stop();
        rollingAudio.Stop();
        radioAudio.Stop();
        radioInterference.Stop();
        extremeStressAudio.Stop();
        carKeySounds.Stop();
        honkingHorn = false;
        hornAudio.Stop();
        skiddingAudio.Stop();
        turboBoostAudio.Stop();
        if (radioOn) liveRadioController.TurnRadioOnOff(false);
        heaterOn = false;
        radioOn = false;
        extremeStressAudio.Stop();
        cabinLightSwitchAudio.Stop();
        if (voiceModule.audioTimedCoroutine != null)
        {
            StopCoroutine(voiceModule.audioTimedCoroutine);
            voiceModule.audioTimedCoroutine = null!;
        }
        voiceModule.voiceAudio.Stop();
        voiceModule.voiceAudio.mute = true;
        if (blinkersCoroutine != null)
        {
            StopCoroutine(blinkersCoroutine);
            blinkersCoroutine = null!;
        }
        hazardsBlinking = false;
    }

    private void StopParticleVFX()
    {
        tireSparks.Stop();
        turboBoostParticle.Stop();
    }

    private void SetMaterials()
    {
        leftBlinkerMesh.material = headlightsOffMat;
        leftBlinkerMeshLod.material = headlightsOffMat;
        rightBlinkerMesh.material = headlightsOffMat;
        rightBlinkerMeshLod.material = headlightsOffMat;
    }

    private void SetLightsOff()
    {
        SetSymbolActive(leftSignalSymbol, leftSignalLight, false);
        SetSymbolActive(hazardWarningSymbol, hazardWarningLight, false);
        SetSymbolActive(rightSignalSymbol, rightSignalLight, false);
        blinkerLightsContainer.SetActive(false);
    }

    private void DisableWheelCollider(WheelCollider wheelCollider, MeshRenderer wheelMesh, MeshRenderer brakeMesh)
    {
        if (wheelCollider == null || brakeMesh == null || !wheelCollider.enabled)
            return;

        wheelCollider.motorTorque = 0f;
        wheelCollider.brakeTorque = 0f;
        wheelCollider.enabled = false;
        wheelMesh.enabled = false;
        brakeMesh.enabled = false;
    }

    private void DisableObjectsOnDestroy()
    {
        for (int obj = 0; obj < disableOnDestroy.Length; obj++)
        {
            if (!disableOnDestroy[obj].activeSelf)
                continue;
            disableOnDestroy[obj].SetActive(false);
        }
        mainBodyMesh.gameObject.SetActive(false);

        carHoodAnimator.gameObject.SetActive(false);
        backDoorContainer.SetActive(false);
        headlightsContainer.SetActive(false);
        sideLightsContainer.SetActive(false);
        backLightsContainer.SetActive(false);
    }

    private void EnableObjectsOnDestroy()
    {
        for (int obj = 0; obj < enableOnDestroy.Length; obj++)
        {
            if (enableOnDestroy[obj].activeSelf)
                continue;
            enableOnDestroy[obj].SetActive(true);
        }
    }

    private void SetExplosionForce(float forceMultiplier, Vector3 explosionPos)
    {
        mainRigidbody.ResetCenterOfMass();
        mainRigidbody.AddForceAtPosition(Vector3.up * forceMultiplier, explosionPos - Vector3.up, ForceMode.Impulse);
    }

    private void DisableIgnition()
    {
        CancelIgnitionCoroutine();
        SetFrontCabinLightOn(setOn: false);
        ignitionStarted = false;
        if (carExhaustParticle.isEmitting) carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        keyIsInIgnition = false;
        keyIsInDriverHand = false;
        twistingKey = false;
    }

    private void DisableDrivetrain()
    {
        EngineRPM = 0f;
        frontWheelRPM = 0f;
        backWheelRPM = 0f;
        wheelRPM = 0f;
        currentGear = 1;
    }

    private void ResetControl()
    {
        steeringAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
        moveInputVector = Vector2.zero;
        syncedMoveInputVector = Vector2.zero;
    }

    private void KillOccupants()
    {
        if (!VehicleUtils.IsPlayerSeatedInVan())
            return;
        GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.up * 27f + 20f * UnityEngine.Random.insideUnitSphere, spawnBody: true, CauseOfDeath.Blast, 6, Vector3.up * 1.5f, false);
    }

    private void SetInteractions()
    {
        InteractTrigger[] interactTriggers = this.gameObject.GetComponentsInChildren<InteractTrigger>();
        for (int i = 0; i < interactTriggers.Length; i++)
        {
            interactTriggers[i].interactable = false;
            interactTriggers[i].CancelAnimationExternally();
        }
        pushTruckTrigger.interactable = true;
    }

    private void ResetOccupants()
    {
        currentDriver = null!;
        currentMiddlePassenger = null!;
        currentPassenger = null!;
    }


    // --- REMOVAL MISC ---
    public void RemoveCarRainCollision()
    {
        var particleTriggers = new[]
        {
            GlobalReferences.rainParticles,
            GlobalReferences.rainHitParticles,
            GlobalReferences.stormyRainParticles,
            GlobalReferences.stormyRainHitParticles,
            GlobalReferences.wesleyHurricaneRainParticles,
            GlobalReferences.wesleyHurricaneRainHitParticles,
            GlobalReferences.wesleyHurricaneSandParticles,
            GlobalReferences.wesleyForsakenRainParticles,
            GlobalReferences.wesleyForsakenRainHitParticles
        };

        foreach (var particle in particleTriggers)
        {
            if (particle == null)
            {
                Plugin.Logger.LogDebug("ScanVan: Weather particle or Trigger is null!");
                continue;
            }

            var trigger = particle.trigger;
            for (int j = trigger.colliderCount - 1; j >= 0; j--)
            {
                var collider = (Collider)trigger.GetCollider(j);
                if (weatherEffectBlockers.Contains(collider))
                {
                    trigger.RemoveCollider(j);
                }
            }
        }
    }


    // --- IDK ---
    private new void ReactToDamage()
    {
        healthMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            healthMeter.localScale.z,
            Mathf.Clamp(((float)carHP + (float)transmissionHP) / ((float)baseCarHP + (float)baseTransmissionHP), 0.01f, 1f),
            6f * Time.deltaTime));
        turboMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            turboMeter.localScale.z,
            Mathf.Clamp((float)turboBoosts / 4f, 0.01f, 1f),
            6f * Time.deltaTime));

        if (transmissionHP <= 12)
        {
            clutchFriction = (float)transmissionHP / 15f;
            clutchFriction = Mathf.Clamp(clutchFriction, 0.4f, 1f);
        }
        else
        {
            clutchFriction = 1f;
        }

        if (!IsOwner) return;
        if (carHP < 9 && Time.realtimeSinceStartup - timeAtLastDamage > 8f)
        {
            timeAtLastDamage = Time.realtimeSinceStartup;
            carHP++;
            syncedCarHP = carHP;
            SyncCarHealthRpc(carHP);
        }
        if (carHP < 6)
        {
            if (!isHoodOnFire)
                SetHoodFireAndSync(setOnFire: true);
        }
        else if (isHoodOnFire)
        {
            SetHoodFireAndSync(setOnFire: false);
        }
    }


    // --- DAMAGE/HEALTH SYNC ---
    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    private void SyncCarHealthRpc(int carHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        syncedCarHP = carHealth;
        carHP = syncedCarHP;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncExtremeStressRpc(bool underStress)
    {
        if (carDestroyed)
        {
            underExtremeStress = false;
        }
        else
        {
            underExtremeStress = underStress;
        }
    }


    // --- HOOD FIRE VFX ---
    private void SetHoodFireAndSync(bool setOnFire)
    {
        SetHoodOnFireLocalClient(setOnFire);
        SetHoodOnFireRpc(setOnFire);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetHoodOnFireRpc(bool onFire)
    {
        SetHoodOnFireLocalClient(onFire);
    }

    private void SetHoodOnFireLocalClient(bool setOnFire)
    {
        isHoodOnFire = setOnFire;
        if (setOnFire)
        {
            hoodFireAudio.Play();
            hoodFireParticle.Play();
            if (!carHoodOpen && !carDestroyed) SetHoodOpenLocalClient(setOpen: true);
            return;
        }
        hoodFireAudio.Stop();
        hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
    }


    // --- HOOD INTERACTION ---
    public new void ToggleHoodOpenLocalClient()
    {
        carHoodOpen = !carHoodOpen;
        carHoodAnimator.SetBool("hoodOpen", carHoodOpen);
        SetHoodOpenRpc(open: carHoodOpen);
    }

    // used for when the hood is 'on fire'
    public new void SetHoodOpenLocalClient(bool setOpen)
    {
        if (carHoodOpen && carHoodOpen == setOpen)
            return;

        carHoodOpen = setOpen;
        carHoodAnimator.SetBool("hoodOpen", setOpen);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHoodOpenRpc(bool open)
    {
        carHoodOpen = open;
        carHoodAnimator.SetBool("hoodOpen", carHoodOpen);
    }


    // --- PUSH METHODS ---
    public new void PushTruckWithArms()
    {
        if (magnetedToShip)
            return;

        if (!Physics.Raycast(
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position,
            GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward,
            out hit,
            10f,
            1073742656,
            QueryTriggerInteraction.Ignore))
            return;

        if (GameNetworkManager.Instance.localPlayerController.overridePhysicsParent != null)
            return;

        if (GameNetworkManager.Instance.localPlayerController.physicsParent == physicsRegion.physicsTransform)
            return;

        Vector3 point = hit.point;
        Vector3 forward = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward;
        int clip = UnityEngine.Random.Range(0, minCollisions.Length);

        if (IsOwner)
        {
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(forward * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, point - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
            PushTruckFromOwnerRpc(point, clip);
            return;
        }
        PushTruckRpc(point, forward, clip);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PushTruckRpc(Vector3 pushPosition, Vector3 dir, int clip)
    {
        pushAudio.clip = minCollisions[clip];
        pushAudio.transform.position = pushPosition;
        pushAudio.Play();
        turbulenceAmount = Mathf.Min(turbulenceAmount + 0.5f, 2f);
        if (IsOwner)
        {
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(dir * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, pushPosition - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
        }
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PushTruckFromOwnerRpc(Vector3 pos, int clip)
    {
        pushAudio.clip = minCollisions[clip];
        pushAudio.transform.position = pos;
        pushAudio.Play();
        turbulenceAmount = Mathf.Min(turbulenceAmount + 0.5f, 2f);
    }


    // --- HAZARD WARNING LAMPS ---
    private IEnumerator FlashBlinkerLights()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.4f);
            hazardsBlinking = true;
            steeringWheelAudio.PlayOneShot(blinkOn);

            leftBlinkerMesh.material = blinkerOnMat;
            leftBlinkerMeshLod.material = blinkerOnMat;
            rightBlinkerMesh.material = blinkerOnMat;
            rightBlinkerMeshLod.material = blinkerOnMat;

            SetSymbolActive(leftSignalSymbol, leftSignalLight, true);
            SetSymbolActive(hazardWarningSymbol, hazardWarningLight, true);
            SetSymbolActive(rightSignalSymbol, rightSignalLight, true);

            blinkerLightsContainer.SetActive(true);
            yield return new WaitForSeconds(0.4f);
            hazardsBlinking = false;
            steeringWheelAudio.PlayOneShot(blinkOff);

            leftBlinkerMesh.material = headlightsOffMat;
            leftBlinkerMeshLod.material = headlightsOffMat;
            rightBlinkerMesh.material = headlightsOffMat;
            rightBlinkerMeshLod.material = headlightsOffMat;

            SetSymbolActive(leftSignalSymbol, leftSignalLight, false);
            SetSymbolActive(hazardWarningSymbol, hazardWarningLight, false);
            SetSymbolActive(rightSignalSymbol, rightSignalLight, false);

            blinkerLightsContainer.SetActive(false);
        }
    }

    public void SetHazardLightsOnLocalClient()
    {
        hazardsOn = !hazardsOn;
        if (truckAlarmCoroutine == null)
        {
            if (blinkersCoroutine != null)
            {
                StopCoroutine(blinkersCoroutine);
                blinkersCoroutine = null!;
            }
            if (hazardsOn)
            {
                blinkersCoroutine = StartCoroutine(FlashBlinkerLights());
            }
            else
            {
                hazardsBlinking = false;

                leftBlinkerMesh.material = headlightsOffMat;
                leftBlinkerMeshLod.material = headlightsOffMat;
                rightBlinkerMesh.material = headlightsOffMat;
                rightBlinkerMeshLod.material = headlightsOffMat;

                SetSymbolActive(leftSignalSymbol, leftSignalLight, false);
                SetSymbolActive(hazardWarningSymbol, hazardWarningLight, false);
                SetSymbolActive(rightSignalSymbol, rightSignalLight, false);

                blinkerLightsContainer.SetActive(false);
            }
        }
        SetHazardLightsRpc(hazardsOn);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHazardLightsRpc(bool warningsOn)
    {
        hazardsOn = warningsOn;
        if (truckAlarmCoroutine == null)
        {
            if (blinkersCoroutine != null)
            {
                StopCoroutine(blinkersCoroutine);
                blinkersCoroutine = null!;
            }
            if (hazardsOn)
            {
                blinkersCoroutine = StartCoroutine(FlashBlinkerLights());
            }
            else
            {
                hazardsBlinking = false;

                leftBlinkerMesh.material = headlightsOffMat;
                leftBlinkerMeshLod.material = headlightsOffMat;
                rightBlinkerMesh.material = headlightsOffMat;
                rightBlinkerMeshLod.material = headlightsOffMat;

                SetSymbolActive(leftSignalSymbol, leftSignalLight, false);
                SetSymbolActive(hazardWarningSymbol, hazardWarningLight, false);
                SetSymbolActive(rightSignalSymbol, rightSignalLight, false);

                blinkerLightsContainer.SetActive(false);
            }
        }
    }


    // --- HEADLAMPS ---
    public new void ToggleHeadlightsLocalClient()
    {
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

        cabinLightSwitchAudio.PlayOneShot(headlightsToggleSFX);
        headlightsContainer.SetActive(lowBeamsOn);
        highBeamContainer.SetActive(highBeamsOn);
        radioLight.SetActive(lowBeamsOn);
        heaterLight.SetActive(lowBeamsOn);
        leftWindowLight.SetActive(lowBeamsOn);
        rightWindowLight.SetActive(lowBeamsOn);
        sideLightsContainer.SetActive(lowBeamsOn);
        clusterLightsContainer.SetActive(lowBeamsOn);
        SetHeadlightMaterial(lowBeamsOn, highBeamsOn);
        ToggleHeadlightsRpc(lowBeamsOn, highBeamsOn);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ToggleHeadlightsRpc(bool setLowBeamsOn, bool setHighBeamsOn)
    {
        lowBeamsOn = setLowBeamsOn;
        highBeamsOn = setHighBeamsOn;

        cabinLightSwitchAudio.PlayOneShot(headlightsToggleSFX);
        headlightsContainer.SetActive(lowBeamsOn);
        highBeamContainer.SetActive(highBeamsOn);
        radioLight.SetActive(lowBeamsOn);
        heaterLight.SetActive(lowBeamsOn);
        leftWindowLight.SetActive(lowBeamsOn);
        rightWindowLight.SetActive(lowBeamsOn);
        sideLightsContainer.SetActive(lowBeamsOn);
        clusterLightsContainer.SetActive(lowBeamsOn);
        SetHeadlightMaterial(lowBeamsOn, highBeamsOn);
    }

    private void SetHeadlightMaterial(bool lowOn, bool highOn)
    {
        Material[] sharedMaterials = interiorMesh.sharedMaterials;
        sharedMaterials[2] = lowOn ? clusterOnMaterial : clusterOffMaterial;
        sharedMaterials[3] = lowOn ? clusterDialsOnMat : clusterDialsOffMat;
        interiorMesh.sharedMaterials = sharedMaterials;

        speedometerMesh.material = lowOn ? clusterOnMaterial : clusterOffMaterial;
        tachometerMesh.material = lowOn ? clusterOnMaterial : clusterOffMaterial;
        oilPressureMesh.material = lowOn ? clusterOnMaterial : clusterOffMaterial;

        heaterBaseMesh.material = lowOn ? heaterOnMat : heaterOffMat;

        Material[] leftWindowMats = leftDoorMesh.sharedMaterials;
        Material[] rightWindowMats = rightDoorMesh.sharedMaterials;

        leftWindowMats[2] = lowOn ? windowOnMaterial : windowOffMaterial;
        rightWindowMats[2] = lowOn ? windowOnMaterial : windowOffMaterial;

        leftDoorMesh.sharedMaterials = leftWindowMats;
        rightDoorMesh.sharedMaterials = rightWindowMats;

        radioMesh.material = lowOn ? radioOnMaterial : radioOffMaterial;
        radioPowerDial.material = lowOn ? radioOnMaterial : radioOffMaterial;
        radioVolumeDial.material = lowOn ? radioOnMaterial : radioOffMaterial;

        lowBeamMesh.material = lowOn ? headlightsOnMat : headlightsOffMat;
        lowBeamMeshLod.material = lowOn ? headlightsOnMat : headlightsOffMat;
        highBeamMesh.material = highOn ? headlightsOnMat : headlightsOffMat;
        highBeamMeshLod.material = highOn ? headlightsOnMat : headlightsOffMat;
        sideTopLightsMesh.material = lowOn ? backLightOnMat : redLightOffMat;
        sideTopLightsMeshLod.material = lowOn ? backLightOnMat : redLightOffMat;
    }
}
