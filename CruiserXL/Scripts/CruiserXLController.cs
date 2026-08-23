using GameNetcodeStuff;
using ScandalLib.Patches;
using ScanVan;
using ScanVan.Behaviour;
using ScanVan.Networking;
using ScanVan.Scripts;
using ScanVan.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;

public class CruiserXLController : VehicleController
{
    [Header("Variety")]

    public VanVariant vanVariant = VanVariant.Default;

    private const string A_DOOR_IS_AJAR = "a door is ajar";
    private const string DONT_FORGET_YOUR_KEYS = "dont forget your keys";

    public Material vanMaterial = null!;
    public Texture2D defaultVanTex = null!;

    public VanCameraManager cameraManager = null!;

    public Material steelBoltMaterial = null!;
    public Material genericMaterial = null!;

    public GameObject frontLeftLowBeamLightObj = null!;
    public GameObject frontRightLowBeamLightObj = null!;

    public GameObject frontLeftHighBeamLightObj = null!;
    public GameObject frontRightHighBeamLightObj = null!;

    public GameObject frontLeftLightObj = null!;
    public GameObject frontRightLightObj = null!;

    public GameObject frontLeftBlinkerLightObj = null!;
    public GameObject frontRightBlinkerLightObj = null!;

    public Light frontLeftBlinkerLight = null!;
    public Light frontRightBlinkerLight = null!;

    // default
    //
    public AnimatedObjectTrigger windshieldWipers = null!;
    public GameObject windshieldWipersObj = null!;
    public Animator windshieldWipersAnimator = null!;

    public GameObject roofRailsObj = null!;
    public GameObject roofRailsInteractObj = null!;

    public GameObject leftMirrorObj = null!;
    public GameObject centerMirrorObj = null!;
    public GameObject rightMirrorObj = null!;

    public Camera leftCameraMirror = null!;
    public Camera centerCameraMirror = null!;
    public Camera rightCameraMirror = null!;

    public MeshRenderer leftMirrorMesh = null!;
    public MeshRenderer centerMirrorMesh = null!;
    public MeshRenderer rightMirrorMesh = null!;

    public MeshFilter interiorMeshFilter = null!;

    public MeshFilter mainBodyMeshFilter = null!;
    public MeshFilter mainBodyLodMeshFilter = null!;

    public MeshFilter leftDoorMeshFilter = null!;
    public MeshFilter leftDoorLodMeshFilter = null!;

    public MeshFilter rightDoorMeshFilter = null!;
    public MeshFilter rightDoorLodMeshFilter = null!;

    public MeshFilter lowBeamMeshFilter = null!;
    public MeshFilter lowBeamLodMeshFilter = null!;

    public MeshFilter highBeamMeshFilter = null!;
    public MeshFilter highBeamLodMeshFilter = null!;

    public MeshFilter leftSignalMeshFilter = null!;
    public MeshFilter leftSignalLodMeshFilter = null!;

    public MeshFilter rightSignalMeshFilter = null!;
    public MeshFilter rightSignalLodMeshFilter = null!;

    public MeshFilter hoodMeshFilter = null!;
    public MeshFilter hoodLodMeshFilter = null!;

    public MeshFilter steeringWheelMeshFilter = null!;
    public MeshRenderer steeringWheelMesh = null!;

    public MeshFilter frontLeftWheelMeshFilter = null!;
    public MeshFilter frontRightWheelMeshFilter = null!;
    public MeshFilter backLeftWheelMeshFilter = null!;
    public MeshFilter backRightWheelMeshFilter = null!;

    public MeshFilter backLeftBrakeMeshFilter = null!;
    public MeshFilter backRightBrakeMeshFilter = null!;
    //

    // creasman
    //
    public Texture2D creasmanVanTex = null!;

    public GameObject creasmanWindshieldWipersObj = null!;
    public Animator creasmanWindshieldWipersAnimator = null!;

    public AudioClip creasmanRevEngineStart = null!; // crank loop
    public AudioClip creasmanEngineStartSuccessful = null!; // start success
    public AudioClip creasmanEngineRun = null!; // engine audio 1
    public AudioClip creasmanEngineRun2 = null!; // engine audio 2
    public AudioClip creasmanEngineRev = null!; // starter motor engage
    public AudioClip creasmanEngineStall = null!;

    public Camera creasmanLeftCameraMirror = null!;
    public Camera creasmanRightCameraMirror = null!;

    public GameObject creasmanFogBeamContainer = null!;

    public GameObject creasmanLeftMirrorObj = null!;
    public GameObject creasmanRightMirrorObj = null!;

    public MeshRenderer creasmanLeftMirrorMesh = null!;
    public MeshRenderer creasmanRightMirrorMesh = null!;

    public MeshFilter creasmanInteriorMeshFilter = null!;

    public Material creasmanClusterDialsOffMat = null!;
    public Material creasmanClusterDialsOnMat = null!;

    public AnimationCurve creasmanEnginePowerCurve = null!;

    public AudioClip creasmanHornAudioClip = null!;
    public AudioClip creasmanAlarmAudioClip = null!;

    public MeshRenderer creasmanFogLampsMesh = null!;
    public GameObject creasmanFogLampsObj = null!;

    public MeshFilter creasmanMainBodyMeshFilter = null!;
    public MeshFilter creasmanMainBodyLodMeshFilter = null!;

    public MeshFilter creasmanLeftDoorMeshFilter = null!;
    public MeshFilter creasmanLeftDoorLodMeshFilter = null!;

    public MeshFilter creasmanRightDoorMeshFilter = null!;
    public MeshFilter creasmanRightDoorLodMeshFilter = null!;

    public MeshFilter creasmanHoodMeshFilter = null!;
    public MeshFilter creasmanHoodLodMeshFilter = null!;

    public MeshFilter creasmanLowBeamMeshFilter = null!;
    public MeshFilter creasmanLowBeamLodMeshFilter = null!;

    public MeshFilter creasmanHighBeamMeshFilter = null!;
    public MeshFilter creasmanHighBeamLodMeshFilter = null!;

    public MeshFilter creasmanLeftSignalMeshFilter = null!;
    public MeshFilter creasmanLeftSignalLodMeshFilter = null!;

    public MeshFilter creasmanRightSignalMeshFilter = null!;
    public MeshFilter creasmanRightSignalLodMeshFilter = null!;

    public MeshFilter creasmanSteeringWheelMeshFilter = null!;

    public MeshFilter creasmanFrontLeftWheelMeshFilter = null!;
    public MeshFilter creasmanFrontRightWheelMeshFilter = null!;
    public MeshFilter creasmanBackLeftWheelMeshFilter = null!;
    public MeshFilter creasmanBackRightWheelMeshFilter = null!;

    public MeshFilter creasmanBackLeftBrakeMeshFilter = null!;
    public MeshFilter creasmanBackRightBrakeMeshFilter = null!;

    public bool hasVehicleDisplay = true;
    public bool hasFogLamps;
    //

    // jcjenson
    //
    public Texture2D jensonVanTex = null!;
    public Material jensonDialsOnMat = null!;
    public Material jensonClusterOnMat = null!;
    public Material jensonRadioOnMat = null!;
    public Material jensonHeaterOnMat = null!;
    public Material jensonWindowOnMat = null!;
    //

    // generic
    //
    public Light radioLightCol = null!; // special hex #FAFEAA, default hex #D6FFCE // FILTER COLOR
    public Light heaterLightCol = null!;
    public Light clusterLightCol = null!; // special hex #FAFEDE, default hex #C9FFFA // FILTER COLOR

    public Light leftWindowLightCol = null!; // special hex #FAFEAA, default hex #D6FFCE // FILTER COLOR
    public Light rightWindowLightCol = null!; // special hex #FAFEAA, default hex #D6FFCE // FILTER COLOR
    //

    public bool disableElectricMirrors;

    /*
    public float specialVariantChance;
    public bool isSpecialVariant;
    */

    [Header("Physics")]

    public List<WheelCollider> wheels = null!;
    public AnimationCurve steeringWheelCurve = null!;
    public CruiserXLCollisionTrigger collisionTrigger = null!;
    public Rigidbody playerPhysicsBody = null!;

    public AnimationCurve engineCurve = null!;
    public AnimationCurve enginePowerCurve = null!;

    public Vector3 lastVelocity;

    private readonly WheelHit[] wheelHits = new WheelHit[4];
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
    public float maxInclineCompensation = 4.1f;
    public const float minInclineCompensation = 1.78f;
    public const float maxInclineCompensationAngle = 32f;

    public float forwardWheelSpeed;
    public float reverseWheelSpeed;

    public float frontWheelRPM;
    public float backWheelRPM;
    public float wheelRPM;

    private const float maxAutoCenterSpeed = 8f;
    private float steeringAngle;
    //private float steeringDecay = 1f;

    private float forwardsSlip;
    //private float sidewaysSlip;

    public float baseForwardStiffness = 1f;
    public float baseSidewaysStiffness = 0.75f;

    public float currentMotorTorque;
    public float currentBrakeTorque;

    public float maxBrakingPower;

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

    private bool receivedSyncData;

    private bool shakingCamera;

    public VanPhysicsRegion vanZone = null!;
    public VanPlayerZone vanCabinZone = null!;
    public VanPlayerZone vanStorageZone = null!;

    public Collider itemSafetyBounds = null!;
    public Collider vehicleBounds = null!;

    public CapsuleCollider cabinPoint = null!;
    public Collider storageCompartment = null!;

    public PlayerControllerB lastDriver = null!;
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

    public float tyreStress;
    public bool wheelSlipping;
    public bool wheelSkidActive;

    public float syncEffectsInterval;
    public float syncTorqueInterval;
    public float syncDrivetrainInterval;

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

    public InteractTrigger hoodTrigger = null!;

    public Animator leftDoorAnimator = null!;
    public Animator rightDoorAnimator = null!;

    public Coroutine dashboardSweepCoroutine = null!;

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

    public VanWindow frontLeftWindow = null!;
    public VanWindow frontRightWindow = null!;

    public InteractTrigger domeLightTrigger = null!;

    public Animator headlampSwitchAnimator = null!;
    public Animator hvacAnimator = null!;

    private Coroutine blinkersCoroutine = null!;

    public InteractTrigger windscreenWipersTrigger = null!;

    public InteractTrigger startIgnitionTrigger = null!;

    public GameObject reverseLightsContainer = null!;
    public MeshRenderer reverseLightsMesh = null!;

    public Transform tempPushTransform = null!;

    private bool setAudiosMuted;

    public bool treatingPlayerInfection;

    public float maxSpeedometerRot = -225f;
    public float maxTachometerRot = -112.5f;

    public string[] upShiftString = null!;
    public string[] downShiftString = null!;

    public bool smoothRotation;

    public bool heaterOn;
    public int heaterTemperature;
    public int heaterSpeed;

    public float steeringWheelAnimValue;
    public float steeringSpeed;

    public bool inIgnitionAnimation;

    public float odometerValue;

    public bool isHeaterCold;
    public bool isHeaterWarm;

    private const float handbrakePullSpeed = 70f;
    private float handbrakeAnimValue;
    private float timeAtLastHandbrakePull;

    private static readonly Vector3 ignitionKeyScale = Vector3.one;
    private static readonly Vector3 LHD_Pos_Local = new Vector3(0.0489f, 0.1371f, -0.1566f);
    private static readonly Vector3 LHD_Pos_Server = new Vector3(0.0366f, 0.1023f, -0.1088f);
    private static readonly Vector3 LHD_Rot_Local = new Vector3(-3.446f, 3.193f, 172.642f);
    private static readonly Vector3 LHD_Rot_Server = new Vector3(-191.643f, 174.051f, -7.768005f);

    public int currentSweepStage;
    public bool hasSweepedDashboard;
    public bool hazardsBlinking;
    public bool hazardsOn;
    public bool reverseLightsOn;

    private float speedometerValue = 850f;
    private float speedometerFloat;
    private float tachometerFloat;

    public float headlampSwitchAnimFloat;

    public float hvacFanSpeedAnimFloat;
    public float hvacFanDirectionAnimFloat;
    public float hvacFanTemperatureAnimFloat;

    public bool disableAnimations;

    public bool lowBeamsOn;
    public bool highBeamsOn;

    public bool overrideCabinLightSwitch;
    public bool cabinLightSwitchEnabled = true;

    private float oilPressureFloat;
    private float turboPressureFloat;
    public bool overdriveSwitchEnabled;

    public bool ignitionOn;
    public bool isCabinLightOn;

    public bool liftGateOpen;
    public bool sideDoorOpen;

    // animations
    private const string STEERING_WHEEL_SPEED = "steeringWheelTurnSpeed";
    private const string ANIMATION_SPEED = "animationSpeed";
    private const string CAR_ANIM = "SA_CarAnim";
    private const string JUMP_WHILE_IN_CAR = "SA_JumpInCar";

    private const string CAR_HANDBRAKE_ON_ENGAGEMENT = "SA_CarHandbrakeOn";
    private const string CAR_HANDBRAKE_OFF_ENGAGEMENT = "SA_CarHandbrakeOff";
    private const string HANDBRAKE_ENGAGEMENT = "engagement";

    private const string UPPER_BODY_RADIO_ON = "U_Radio_TurnOn_LHD";
    private const string LEFT_ARM_RADIO_ON = "L_Radio_TurnOn_LHD";
    private const string RIGHT_ARM_RADIO_ON = "R_Radio_TurnOn_LHD";

    private const string UPPER_BODY_HANDBRAKE_ON = "U_EngageHandbrake";
    private const string LEFT_ARM_HANDBRAKE_ON = "L_EngageHandbrake";
    private const string RIGHT_ARM_HANDBRAKE_ON = "R_EngageHandbrake";

    private const string UPPER_BODY_HANDBRAKE_OFF = "U_DisengageHandbrake";
    private const string LEFT_ARM_HANDBRAKE_OFF = "L_DisengageHandbrake";
    private const string RIGHT_ARM_HANDBRAKE_OFF = "R_DisengageHandbrake";

    // hvac triggers
    private const string HVAC_TEMPERATURE_FLOAT = "temperature";
    private const string HVAC_FAN_SPEED_FLOAT = "fanSpeed";
    private const string HVAC_DIRECTION_FLOAT = "direction";

    // door animations
    private const string DOOR_ELECTRIC_MIRROR_BOOL = "mirrorOpen";

    // triggers
    private const string HOOD_OPENING_HOLDTIP = "[ Opening hood ]";
    private const string HOOD_CLOSING_HOLDTIP = "[ Closing hood ]";

    private const string HEADLAMP_SWITCH_FLOAT = "switch";

    private const string DOMELIGHT_IGNITION_HOVERTIP = "Switch cabin light : [LMB]";
    private const string DOMELIGHT_OFF_HOVERTIP = "Switch cabin light : [LMB] (Off)";
    private const string DOMELIGHT_ON_HOVERTIP = "Switch cabin light : [LMB] (On)";

    private const string HOOD_ANIMATION_BOOL = "hoodOpen";

    private const string DOOR_ENTER_HOVERTIP = "Use door : [LMB]";
    private const string DOOR_EXIT_HOVERTIP = "Exit : [LMB]";

    private const string WINDOW_DISABLED_HOLDTIP = "[ No ignition power ]";
    /*
    private const string WINDOW_INMOTION_HOVERTIP = "[ Window in motion ]";
    */

    private const string UNTWIST_KEY_HOVERTIP = "Untwist key : [LMB]";
    private const string UNTWIST_KEY_HOLDTIP = "[ Untwisting key ]";

    private const string TRYIGNITION_KEY_HOVERTIP = "Try ignition : [LMB] (Hold)";
    private const string TRYIGNITION_KEY_HOLDTIP = "[ Trying ignition ]";

    private const string REMOVE_KEY_HOVERTIP = "Remove key : [LMB]";
    private const string REMOVE_KEY_HOLDTIP = "[ Removing key ]";

    // player
    private const string PLAYER_MOVEMENT = "Move";
    private const string CAMERA_SHAKE_CONSTANT = "ShakingConstant";

    // some shit

    private const int DEFAULT_PLAYER_ANIMATION = 0; // this is the player animation
    private const int SITTING_IN_SEAT_HANDS_ON_WHEEL = 1; // this is the player animation
    private const int INSERT_IGNITION_KEY = 2; // this is the player animation
    private const int UNTWIST_IGNITION_KEY = 3; // untwist after trying the ignition, this is the player animation
    private const int UNTWIST_IGNITION_KEY_IN = 7; // untwist from engine on, leave the keys in, this is the player animation
    private const int REMOVE_IGNITION_KEY = 8; // this is the player animation
    private const int RETWIST_IGNITION_KEY_AFTER_STALL = 9; // this is the player animation
    private const int RETWIST_IGNITION_KEY_IN_IGNITION = 12; // this is the player animation

    private const int DEFAULT_IGNITION_SLOT = 0; // this is the ignition barrel animation
    private const int STARTED_IGNITION_SLOT = 1; // this is the ignition barrel animation
    private const int UNTWIST_IGNITION_SLOT = 7; // untwist from engine on, leave keys in, this is the ignition barrel animation

    private const int UNTWIST_IGNITION_KEY_FALLBACK = 13; // fallback, this should never really occur but just incase, this is a player animation

    private const float DEFAULT_ANIMATION_SPEED = 0.5f;

    private const string IGNITION_ANIM = "SA_IgnitionAnim";
    private const string REMOVE_IN_IGNITION = "SA_RemoveInIgnition";

    [Header("Audio")]

    public EVAModule voiceModule = null!;
    public RadioBehaviour liveRadioController = null!;

    public AnimationCurve idleVolumeCurve = null!;
    public AnimationCurve revVolumeCurve = null!;

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

    public AudioSource miscEngineAudio = null!;
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

    private Coroutine vanAlarmCoroutine = null!;
    public AudioSource alarmAudio = null!;
    public AudioSource heaterAudio = null!;

    public bool roofRainAudioActive;

    private float engineThrottleVolume = 1f;

    private float timeSinceTogglingRadio;
    public bool alarmDebounce;
    private float timeAtLastAlarmPing;
    private float timeAtLastEVAPing;

    private float lastSongTime;
    public const float minFrequency = 75.55f;
    public const float maxFrequency = 255.50f;

    public bool isFmRadio = false;

    private float syncedSongTime;
    private float timeLastSyncedRadio;
    private float radioPingTimestamp;

    [Header("Materials")]

    public Material blinkerOffMat = null!;
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


    // Thanks TomatoBird8.
    [MethodImpl(MethodImplOptions.AggressiveInlining)] // this replaces any calls to this method with the method body (the code in the method)
    // static is for if you don't need the instance
    public static bool IsInShipPhaseAndNotLoadingLevel(bool checkNotLoadingLevel = true)
    {
        return StartOfRound.Instance.inShipPhase && (!checkNotLoadingLevel || !StartOfRound.Instance.beganLoadingNewLevel);
    }

    // --- INIT ---
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!NetworkManager.IsServer || IsInShipPhaseAndNotLoadingLevel())
            return;
        System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);
        double roll = random.NextDouble() * 100.0;
        float creasmanChance = 1.6f;
        float jcJensonChance = 9f;
        if (ScanVanNetworker.Instance.overrideChance || roll < creasmanChance)
        {
            vanVariant = VanVariant.Creasman;
        }
        else if (roll < creasmanChance + jcJensonChance)
        {
            vanVariant = VanVariant.JCJenson;
        }
        else
        {
            vanVariant = VanVariant.Default;
        }
        ScanVanNetworker.Instance.overrideChance = false;
        SetVanVariantRpc((int)vanVariant);
        SetVanVariant();
        if (random.Next(100) < 37)
        {
            voiceModule.randomServiceAlert = random.Next(18, 21);
        }
        if (random.Next(100) < 10)
        {
            voiceModule.useAltAudio = true;

            voiceModule.voiceAudioClips[4] = voiceModule.keysAlertAlt;
            voiceModule.voiceAudioClips[6] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[7] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[8] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[9] = voiceModule.doorAlertAlt;

            voiceModule.clusterTexts[4] = DONT_FORGET_YOUR_KEYS;
            voiceModule.clusterTexts[6] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[7] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[8] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[9] = A_DOOR_IS_AJAR;
        }
        InitializeVanRpc(voiceModule.randomServiceAlert, voiceModule.useAltAudio);
    }

    [Rpc(SendTo.NotServer)]
    public void SetVanVariantRpc(int setVariant)
    {
        vanVariant = (VanVariant)setVariant;
        SetVanVariant();
    }

    [Rpc(SendTo.NotServer)]
    public void InitializeVanRpc(int serviceAlert, bool useAlt)
    {
        voiceModule.randomServiceAlert = serviceAlert;
        voiceModule.useAltAudio = useAlt;
        if (useAlt)
        {
            voiceModule.voiceAudioClips[4] = voiceModule.keysAlertAlt;
            voiceModule.voiceAudioClips[6] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[7] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[8] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[9] = voiceModule.doorAlertAlt;

            voiceModule.clusterTexts[4] = DONT_FORGET_YOUR_KEYS;
            voiceModule.clusterTexts[6] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[7] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[8] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[9] = A_DOOR_IS_AJAR;
        }
    }

    public void SetVanVariant()
    {
        switch (vanVariant)
        {
            case VanVariant.Default:
                vanMaterial.mainTexture = defaultVanTex;

                headlightsOffMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);
                headlightsOnMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);
                blinkerOffMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);
                break;
            case VanVariant.JCJenson:
                vanMaterial.mainTexture = jensonVanTex;

                headlightsOffMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);
                headlightsOnMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);
                blinkerOffMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);

                radioLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
                leftWindowLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
                rightWindowLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
                heaterLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
                clusterLightCol.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF); // #FAFEDE
                voiceModule.clusterLight.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF); // #FAFEDE

                vehicleDisplay.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF);
                vehicleDisplayLight.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF);

                clusterDialsOnMat = jensonDialsOnMat; // #FCFFB6
                clusterOnMaterial = jensonClusterOnMat; // #FAFF92
                radioOnMaterial = jensonRadioOnMat; // #FAFE7D
                heaterOnMat = jensonHeaterOnMat; // #FAFE7D
                windowOnMaterial = jensonWindowOnMat;

                radioTime.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33
                radioFrequency.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33
                voiceModule.clusterScreen.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33

                voiceModule.clusterTexts[0] = "sys status: [ok]";
                voiceModule.clusterTexts[15] = "high temp!";
                voiceModule.clusterTexts[13] = "error: wd606";
                break;
            case VanVariant.Creasman:
                vanMaterial.mainTexture = creasmanVanTex;

                // stats and other stuff or something idk i'm lazy
                MinEngineRPM = 900f;
                MaxEngineRPM = 6500f;
                engineIntensityPercentage = MaxEngineRPM;
                EngineTorque = 100f;

                maxInclineCompensation = 5.5f;
                steeringWheelTurnSpeed = 5f;
                steeringSpeed = 9.5f;

                mainRigidbody.maxLinearVelocity = 160f;
                mainRigidbody.drag = 0f;

                clutchReturnSpeed = 2.5f;
                clutchSpeed = 4.25f;

                throttleReleaseSpeed = 3.8f;
                maxThrottleSpeed = 3.5f;

                maxSpeedometerRot = -185f;
                maxTachometerRot = -147.5f;

                JointSpring suspensionSpring = new JointSpring
                {
                    spring = 30000f,
                    damper = 4800f,
                    targetPosition = 0.7f,
                };

                FrontRightWheel.suspensionSpring = suspensionSpring;
                FrontLeftWheel.suspensionSpring = suspensionSpring;
                BackRightWheel.suspensionSpring = suspensionSpring;
                BackLeftWheel.suspensionSpring = suspensionSpring;

                // interior
                interiorMeshFilter.mesh = creasmanInteriorMeshFilter.mesh;
                Material[] interiorMaterials = interiorMesh.sharedMaterials;
                clusterDialsOffMat = creasmanClusterDialsOffMat;
                clusterDialsOnMat = creasmanClusterDialsOnMat;
                interiorMaterials[3] = creasmanClusterDialsOffMat;
                interiorMesh.sharedMaterials = interiorMaterials;

                // go's
                roofRailsObj.SetActive(false);
                roofRailsInteractObj.SetActive(false);

                leftMirrorObj.SetActive(false);
                centerMirrorObj.SetActive(false);
                rightMirrorObj.SetActive(false);

                // lights + light go's
                headlightsOffMat.color = new Color32(0xB0, 0xB0, 0xB0, 0xFF);
                headlightsOnMat.color = new Color32(0xB0, 0xB0, 0xB0, 0xFF);
                blinkerOffMat.color = new Color32(0xC9, 0x91, 0x6E, 0xFF);

                frontLeftBlinkerLightObj.transform.localPosition = new Vector3(2f, -0.97f, 5.555f);
                frontLeftBlinkerLightObj.transform.localRotation = Quaternion.Euler(0, 60f, 0);

                frontRightBlinkerLightObj.transform.localPosition = new Vector3(-2f, -0.97f, 5.555f);
                frontRightBlinkerLightObj.transform.localRotation = Quaternion.Euler(0, -60f, 0);

                frontLeftLowBeamLightObj.transform.localPosition = new Vector3(-1.535f, 1.63f, 4.932f);
                frontRightLowBeamLightObj.transform.localPosition = new Vector3(1.535f, 1.63f, 4.932f);

                frontLeftHighBeamLightObj.transform.localPosition = new Vector3(-0.705f, 1.645f, 4.855f);
                frontRightHighBeamLightObj.transform.localPosition = new Vector3(0.705f, 1.645f, 4.855f);

                frontLeftLightObj.transform.localPosition = new Vector3(-1.165f, 1.74f, 5.135f);
                frontRightLightObj.transform.localPosition = new Vector3(1.165f, 1.74f, 5.135f);

                creasmanFogLampsObj.SetActive(true);
                hasFogLamps = true;

                // mesh stuff
                mainBodyMeshFilter.mesh = creasmanMainBodyMeshFilter.mesh;
                mainBodyLodMeshFilter.mesh = creasmanMainBodyLodMeshFilter.mesh;

                leftDoorMeshFilter.mesh = creasmanLeftDoorMeshFilter.mesh;
                leftDoorLodMeshFilter.mesh = creasmanLeftDoorLodMeshFilter.mesh;

                steeringWheelMeshFilter.mesh = creasmanSteeringWheelMeshFilter.mesh;
                Material[] steeringWheelMaterials =
                {
                    genericMaterial,
                    steelBoltMaterial
                };
                steeringWheelMesh.sharedMaterials = steeringWheelMaterials;

                rightDoorMeshFilter.mesh = creasmanRightDoorMeshFilter.mesh;
                rightDoorLodMeshFilter.mesh = creasmanRightDoorLodMeshFilter.mesh;

                hoodMeshFilter.mesh = creasmanHoodMeshFilter.mesh;
                hoodLodMeshFilter.mesh = creasmanHoodLodMeshFilter.mesh;

                lowBeamMeshFilter.mesh = creasmanLowBeamMeshFilter.mesh;
                lowBeamLodMeshFilter.mesh = creasmanLowBeamLodMeshFilter.mesh;

                highBeamMeshFilter.mesh = creasmanHighBeamMeshFilter.mesh;
                highBeamLodMeshFilter.mesh = creasmanHighBeamLodMeshFilter.mesh;

                leftSignalMeshFilter.mesh = creasmanLeftSignalMeshFilter.mesh;
                leftSignalLodMeshFilter.mesh = creasmanLeftSignalLodMeshFilter.mesh;

                rightSignalMeshFilter.mesh = creasmanRightSignalMeshFilter.mesh;
                rightSignalLodMeshFilter.mesh = creasmanRightSignalLodMeshFilter.mesh;

                frontLeftWheelMeshFilter.mesh = creasmanFrontLeftWheelMeshFilter.mesh;
                frontRightWheelMeshFilter.mesh = creasmanFrontRightWheelMeshFilter.mesh;
                backLeftWheelMeshFilter.mesh = creasmanBackLeftWheelMeshFilter.mesh;
                backRightWheelMeshFilter.mesh = creasmanBackRightWheelMeshFilter.mesh;

                backLeftBrakeMeshFilter.mesh = creasmanBackLeftBrakeMeshFilter.mesh;
                backRightBrakeMeshFilter.mesh = creasmanBackRightBrakeMeshFilter.mesh;

                // audio
                hornAudio.Stop();
                hornAudio.clip = creasmanHornAudioClip;
                //alarmAudio.clip = creasmanAlarmAudioClip;
                engineAudio2.Stop();
                engineAudio2.clip = creasmanEngineRun2;

                // misc
                disableElectricMirrors = true;
                hasVehicleDisplay = false;

                vehicleDisplay.enabled = false;
                vehicleDisplayLight.enabled = false;
                speedometerValue = 1200f;

                diffRatio = 4.75f;
                gearRatios = new float[]
                {
                    -4f,
                    4.5f,
                    2.4f,
                    1.6f,
                    1.12f
                };

                windshieldWipersAnimator.enabled = false;
                windshieldWipersObj.SetActive(false);

                creasmanWindshieldWipersObj.SetActive(true);
                creasmanWindshieldWipersAnimator.enabled = true;

                windshieldWipers.triggerAnimator = creasmanWindshieldWipersAnimator;

                wiperAudio.transform.localPosition = new Vector3(0f, 0.5f, 3.327f);

                // hacky stuff
                cameraManager.enabled = false;

                leftCameraMirror.enabled = false;
                centerCameraMirror.enabled = false;
                rightCameraMirror.enabled = false;

                leftMirrorMesh.enabled = false;
                centerMirrorMesh.enabled = false;
                rightMirrorMesh.enabled = false;

                creasmanLeftMirrorObj.SetActive(true);
                creasmanRightMirrorObj.SetActive(true);
                Camera[] cameras =
                {
                    creasmanLeftCameraMirror,
                    creasmanRightCameraMirror
                };
                cameraManager.cameraMirrors = cameras;
                MeshRenderer[] meshes =
                {
                    creasmanLeftMirrorMesh,
                    creasmanRightMirrorMesh
                };
                cameraManager.meshMirrors = meshes;

                cameraManager.enabled = true;

                break;
            default:
                vanMaterial.mainTexture = defaultVanTex;

                headlightsOffMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);
                headlightsOnMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);
                blinkerOffMat.color = new Color32(0x71, 0x6E, 0x65, 0xFF);
                break;
        }
    }

    public void OnEnable()
    {
        VehicleUtils.vanController = this;
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

        usingSwitchIgnition = false;
        InitializeVan();
    }

    private void InitializeVan()
    {
        carTooltips = new string[4]
        {
            /*
            "Gas pedal : [W]",
            "Brake pedal : [S]",
            */
            "Gas/Brake pedal : [W/S]",
            "Clutch pedal : [L-Shift]",
            "Change gear : [Scroll]",
            "Switch action : [LMB]"
        };

        transmissionHP = baseTransmissionHP;
        syncedTransmissionHP = baseTransmissionHP; 

        if (!IsInShipPhaseAndNotLoadingLevel())
        {
            carHP = baseCarHP;
            syncedCarHP = baseCarHP;
        }

        SetWheelFriction();

        mainRigidbody.maxLinearVelocity = carMaxSpeed;
        mainRigidbody.maxAngularVelocity = 4f;

        mainRigidbody.automaticCenterOfMass = false;
        mainRigidbody.centerOfMass = new Vector3(0f, -0.26f, 0.25f);
        mainRigidbody.automaticInertiaTensor = false;

        FrontLeftWheel.sprungMass = 228.2732f;
        FrontRightWheel.sprungMass = 228.2732f;
        BackLeftWheel.sprungMass = 271.7272f;
        BackRightWheel.sprungMass = 271.7272f;
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
        StartCoroutine(SetRainCollision());
        FrontLeftWheel.brakeTorque = maxBrakingPower;
        FrontRightWheel.brakeTorque = maxBrakingPower;
        BackLeftWheel.brakeTorque = maxBrakingPower;
        BackRightWheel.brakeTorque = maxBrakingPower;

        currentRadioClip = 0;
        decals = new DecalProjector[24];

        if (!IsInShipPhaseAndNotLoadingLevel())
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

    public IEnumerator SetRainCollision()
    {
        yield return new WaitForSeconds(4f);

        var particleTriggers = new[]
        {
            ScandalsTweaks.Utils.References.rainParticles,
            ScandalsTweaks.Utils.References.rainHitParticles,
            ScandalsTweaks.Utils.References.stormyRainParticles,
            ScandalsTweaks.Utils.References.stormyRainHitParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneRainParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneRainHitParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneSandParticles,
            ScandalsTweaks.Utils.References.wesleyForsakenRainParticles,
            ScandalsTweaks.Utils.References.wesleyForsakenRainHitParticles,
            ScandalsTweaks.Utils.References.kenjiAcidRainParticles,
            ScandalsTweaks.Utils.References.kenjiAcidRainHitParticles,
            ScandalsTweaks.Utils.References.kenjiAcidStormyRainParticles,
            ScandalsTweaks.Utils.References.kenjiAcidStormyRainHitParticles
        };
        foreach (var particle in particleTriggers)
        {
            if (particle == null)
                continue;
            var trigger = particle.trigger;
            foreach (var blocker in weatherEffectBlockers)
            {
                if (blocker != null)
                    trigger.AddCollider(blocker);
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

        SyncClientDataRpc(carHP, steeringWheelAnimFloat, ignitionStarted, (int)vanVariant, voiceModule.randomServiceAlert, voiceModule.useAltAudio, odometerValue);
    }

    [Rpc(SendTo.NotServer)]
    public void SyncClientDataRpc(int carHealth, float wheelRot, bool setStarted, int setVariant, int serviceAlert, bool useAltVoices, float odometerMileage)
    {
        if (receivedSyncData)
        {
            return;
        }
        receivedSyncData = true;
        vanVariant = (VanVariant)setVariant;
        SetVanVariant();
        carHP = carHealth;
        syncedCarHP = carHealth;
        syncedSteeringWheelRotation = wheelRot;
        steeringWheelAnimFloat = wheelRot;
        disableAnimations = !setStarted;
        ignitionOn = setStarted;
        keyIsInIgnition = setStarted;
        voiceModule.randomServiceAlert = serviceAlert;
        voiceModule.useAltAudio = useAltVoices;
        if (useAltVoices)
        {
            voiceModule.voiceAudioClips[4] = voiceModule.keysAlertAlt;
            voiceModule.voiceAudioClips[6] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[7] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[8] = voiceModule.doorAlertAlt;
            voiceModule.voiceAudioClips[9] = voiceModule.doorAlertAlt;

            voiceModule.clusterTexts[4] = DONT_FORGET_YOUR_KEYS;
            voiceModule.clusterTexts[6] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[7] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[8] = A_DOOR_IS_AJAR;
            voiceModule.clusterTexts[9] = A_DOOR_IS_AJAR;
        }
        odometerValue = odometerMileage;
        SetIgnitionKeyValues(setKeyInHand: false, setKeyInSlot: setStarted);
        SetIgnition(setStarted, setStarted);
        SetIgnitionInteractTrigger();
        if (setStarted)
        {
            dashboardSweepCoroutine = StartCoroutine(DashboardSweepCoroutine());
            TrySetMirrorsFolded(setFolded: setStarted);
        }
        if (hasVehicleDisplay)
        {
            SetSymbolActive(vehicleDisplay, vehicleDisplayLight, setStarted);
        }
        SetWindowInteractTriggers(isIgnitionOn: setStarted);
        ignitionCollider.enabled = setStarted;
        ignitionAnimator.SetInteger(IGNITION_ANIM, setStarted ? 1 : 0);
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
        SetCabinLightSwitchOwnerRpc();
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    private void SetCabinLightSwitchOwnerRpc()
    {
        string message;
        if (!overrideCabinLightSwitch && !cabinLightSwitchEnabled)
        {
            overrideCabinLightSwitch = true;
            cabinLightSwitchEnabled = true;
            roofAudio.PlayOneShot(cabinLightSwitchToggle);
            frontCabinLightContainer.SetActive(true);
            frontCabinLightMesh.material = headlightsOnMat;
            message = DOMELIGHT_ON_HOVERTIP;
            domeLightTrigger.hoverTip = message;
            SetCabinLightSwitchRpc(true, true, true, message);
            return;
        }
        if (overrideCabinLightSwitch)
        {
            overrideCabinLightSwitch = false;
            cabinLightSwitchEnabled = true;
            roofAudio.PlayOneShot(cabinLightSwitchToggle);
            bool isCabinLightOn = this.isCabinLightOn && keyIsInIgnition && ignitionOn && cabinLightSwitchEnabled;
            frontCabinLightContainer.SetActive(isCabinLightOn);
            frontCabinLightMesh.material = isCabinLightOn ? headlightsOnMat : greyLightOffMat;
            message = DOMELIGHT_IGNITION_HOVERTIP;
            domeLightTrigger.hoverTip = message;
            SetCabinLightSwitchRpc(true, isCabinLightOn, false, message);
            return;
        }
        overrideCabinLightSwitch = false;
        cabinLightSwitchEnabled = false;
        roofAudio.PlayOneShot(cabinLightSwitchToggle);
        frontCabinLightContainer.SetActive(false);
        frontCabinLightMesh.material = greyLightOffMat;
        message = DOMELIGHT_OFF_HOVERTIP;
        domeLightTrigger.hoverTip = message;
        SetCabinLightSwitchRpc(false, false, false, message);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetCabinLightSwitchRpc(bool switchState, bool cabLightOn, bool setOverride, string setMessage)
    {
        overrideCabinLightSwitch = setOverride;
        cabinLightSwitchEnabled = switchState;
        roofAudio.PlayOneShot(cabinLightSwitchToggle);
        frontCabinLightContainer.SetActive(cabLightOn);
        frontCabinLightMesh.material = cabLightOn ? headlightsOnMat : greyLightOffMat;
        domeLightTrigger.hoverTip = setMessage;
    }

    public new void SetFrontCabinLightOn(bool setOn)
    {
        isCabinLightOn = setOn;
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


    // --- HANDBRAKE METHODS ---
    public void ToggleHandbrake()
    {
        if (!localPlayerInControl && currentDriver != null)
            return;

        float timeSinceStartup = Time.realtimeSinceStartup;
        if (localPlayerInControl && timeSinceStartup - timeAtLastHandbrakePull < 0.78f)
            return;

        timeAtLastHandbrakePull = timeSinceStartup;
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


    // --- TRIGGER STUFF ---
    public void SetWindowInteractTriggers(bool isIgnitionOn)
    {
        driversSideWindow.holdInteraction = !isIgnitionOn;
        passengersSideWindow.holdInteraction = !isIgnitionOn;
        if (!isIgnitionOn)
        {
            driversSideWindow.holdTip = WINDOW_DISABLED_HOLDTIP;
            passengersSideWindow.holdTip = WINDOW_DISABLED_HOLDTIP;
            return;
        }
        driversSideWindow.holdTip = null;
        passengersSideWindow.holdTip = null;
    }

    public void TrySetMirrorsFolded(bool setFolded)
    {
        if (disableElectricMirrors)
        {
            return;
        }
        leftDoorAnimator.SetBool(DOOR_ELECTRIC_MIRROR_BOOL, setFolded);
        rightDoorAnimator.SetBool(DOOR_ELECTRIC_MIRROR_BOOL, setFolded);
    }


    // --- TRY SET IGNITION METHOD ---
    public void SetIgnitionInteractTrigger()
    {
        if (!localPlayerInControl && currentDriver != null)
            return;

        if (ignitionStarted)
        {
            startIgnitionTrigger.hoverTip = UNTWIST_KEY_HOVERTIP;
            startIgnitionTrigger.holdTip = UNTWIST_KEY_HOLDTIP;

            startIgnitionTrigger.timeToHold = 0.5f;
            startIgnitionTrigger.timeToHoldSpeedMultiplier = 1f;
            return;
        }
        if (keyIsInIgnition)
        {
            bool switchIgnition = usingSwitchIgnition || currentDriver == null;
            startIgnitionTrigger.hoverTip = switchIgnition ? REMOVE_KEY_HOVERTIP : TRYIGNITION_KEY_HOVERTIP;
            startIgnitionTrigger.holdTip = switchIgnition ? REMOVE_KEY_HOLDTIP : TRYIGNITION_KEY_HOLDTIP;

            startIgnitionTrigger.timeToHold = switchIgnition ? 0.5f : 1f;
            startIgnitionTrigger.timeToHoldSpeedMultiplier = switchIgnition ? 1f : 0f;
            return;
        }
        startIgnitionTrigger.hoverTip = TRYIGNITION_KEY_HOVERTIP;
        startIgnitionTrigger.holdTip = TRYIGNITION_KEY_HOLDTIP;

        startIgnitionTrigger.timeToHold = 1f;
        startIgnitionTrigger.timeToHoldSpeedMultiplier = 0f;
    }


    // --- TRY IGNITION METHOD ---
    public new void StartTryCarIgnition()
    {
        if (!localPlayerInControl)
            return;

        if (usingSwitchIgnition)
            return;

        if (ignitionStarted ||
            inIgnitionAnimation ||
            (inIgnitionAnimation && startIgnitionTrigger.isBeingHeldByPlayer))
            return;

        CancelIgnitionCoroutine();
        disableAnimations = true;
        inIgnitionAnimation = true;
        SetIgnitionInteractTrigger();
        bool stalled = engineStalled;
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: true));
        TryIgnitionRpc(setKeyInSlot: keyIsInIgnition, setCabinLight: isCabinLightOn, hasStalled: stalled);
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
            currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, engineStalled ? RETWIST_IGNITION_KEY_AFTER_STALL : RETWIST_IGNITION_KEY_IN_IGNITION);
            int ignitionAnimIndex = currentDriver.playerBodyAnimator.GetInteger(CAR_ANIM);
            if (ignitionAnimIndex == RETWIST_IGNITION_KEY_AFTER_STALL) ignitionAnimIndex = RETWIST_IGNITION_KEY_IN_IGNITION;
            ignitionAnimator.SetInteger(IGNITION_ANIM, ignitionAnimIndex);
            SetIgnitionInteractTrigger();
            engineStalled = false;
            SetIgnitionKeyValues(setKeyInHand: true, setKeyInSlot: true);
            yield return new WaitForSeconds(0.02f);
            carKeySounds.PlayOneShot(twistKey);
            SetIgnitionKeyValues(setKeyInHand: true, setKeyInSlot: true);
            yield return new WaitForSeconds(0.1467f);
        }
        else
        {
            currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, INSERT_IGNITION_KEY);
            ignitionAnimator.SetInteger(IGNITION_ANIM, INSERT_IGNITION_KEY);
            SetIgnitionInteractTrigger();
            SetIgnitionKeyValues(setKeyInHand: true, setKeyInSlot: false);
            yield return new WaitForSeconds(0.6f);
            carKeySounds.PlayOneShot(insertKey);
            SetIgnitionKeyValues(setKeyInHand: true, setKeyInSlot: true);
            SetIgnitionInteractTrigger();
            yield return new WaitForSeconds(0.2f);
            carKeySounds.PlayOneShot(twistKey);
            SetIgnitionKeyValues(setKeyInHand: true, setKeyInSlot: true);
            yield return new WaitForSeconds(0.185f);
        }
        if (!isLocalDriver) yield break;
        bool clutchInterlock = clutchPedalPressed && clutchEngagement <= 0.31f;
        ignitionOn = true;
        SetIgnitionKeyValues(setKeyInHand: true, setKeyInSlot: true);
        SetFrontCabinLightOn(setOn: ignitionOn);
        SetIgnitionInteractTrigger();
        SetWindowInteractTriggers(isIgnitionOn: ignitionOn);
        if (dashboardSweepCoroutine == null && !hasSweepedDashboard)
        {
            dashboardSweepCoroutine = StartCoroutine(DashboardSweepCoroutine());
        }
        TrySetMirrorsFolded(ignitionOn);
        if (hasVehicleDisplay)
        {
            SetSymbolActive(vehicleDisplay, vehicleDisplayLight, true);
        }
        TryStartIgnitionRpc(clutchInterlock);
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
            CancelIgnitionAnimation(isStarted: true, setIgnitionAnim: true);
            disableAnimations = false;
            inIgnitionAnimation = false;
            ignitionOn = true;
            currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, SITTING_IN_SEAT_HANDS_ON_WHEEL);
            SetIgnitionKeyValues(setKeyInHand: false, setKeyInSlot: true);
            SetIgnition(setStarted: true, setCabinLight: true);

            // ?? investigate what i did months ago, wtf is this
            startIgnitionTrigger.StopInteraction();
            SetIgnitionInteractTrigger();
            startIgnitionTrigger.StopInteraction();
            startIgnitionTrigger.currentCooldownValue = 1f;
            // ??

            StartIgnitionRpc(false);
        }
        else
        {
            chanceToStartIgnition += 22f;
            chanceToStartIgnition = Mathf.Clamp(chanceToStartIgnition, 0f, 101f);
        }
        yield break;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void TryIgnitionRpc(bool setKeyInSlot, bool setCabinLight, bool hasStalled)
    {
        CancelIgnitionCoroutine();
        disableAnimations = true;
        inIgnitionAnimation = true;
        SetIgnitionKeyValues(setKeyInHand: false, setKeyInSlot: setKeyInSlot);
        if (isCabinLightOn != setCabinLight)
        {
            SetFrontCabinLightOn(setOn: setCabinLight);
        }
        engineStalled = hasStalled;
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: false));
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void TryStartIgnitionRpc(bool shiftInterlock)
    {
        ignitionOn = true;
        SetIgnitionKeyValues(setKeyInHand: true, setKeyInSlot: true);
        SetFrontCabinLightOn(setOn: ignitionOn);
        SetWindowInteractTriggers(isIgnitionOn: ignitionOn);
        if (dashboardSweepCoroutine == null && !hasSweepedDashboard)
        {
            dashboardSweepCoroutine = StartCoroutine(DashboardSweepCoroutine());
        }
        TrySetMirrorsFolded(ignitionOn);
        if (hasVehicleDisplay)
        {
            SetSymbolActive(vehicleDisplay, vehicleDisplayLight, true);
        }
        if (shiftInterlock)
        {
            PlayIgnitionAudio();
        }
    }

    public void PlayIgnitionAudio()
    {
        engineAudio1.Stop();
        if (vanVariant == VanVariant.Creasman)
        {
            engineAudio1.clip = creasmanRevEngineStart;
            engineAudio1.volume = 0.7f;
            engineAudio1.PlayOneShot(creasmanEngineRev);
        }
        else
        {
            engineAudio1.clip = revEngineStart;
            engineAudio1.volume = 0.7f;
            engineAudio1.PlayOneShot(engineRev);
        }
        engineAudio1.pitch = 1f;
        carEngine1AudioActive = true;
    }


    // --- CANCEL IGNITION METHOD ---
    public new void CancelTryCarIgnition()
    {
        if (!localPlayerInControl)
            return;

        if (usingSwitchIgnition)
            return;

        if (ignitionStarted ||
            !inIgnitionAnimation ||
            (!inIgnitionAnimation && startIgnitionTrigger.isBeingHeldByPlayer))
            return;

        // hopefully fix a bug where the wrong animation can play?
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        int playerAnimIndex = playerController.playerBodyAnimator.GetInteger(CAR_ANIM);
        if (playerAnimIndex == 0 && keyIsInIgnition)
            playerController.playerBodyAnimator.SetInteger(CAR_ANIM, UNTWIST_IGNITION_KEY_FALLBACK);
        else if ((playerAnimIndex == INSERT_IGNITION_KEY || 
            playerAnimIndex == RETWIST_IGNITION_KEY_AFTER_STALL || 
            playerAnimIndex == RETWIST_IGNITION_KEY_IN_IGNITION) && 
            keyIsInIgnition)
            playerController.playerBodyAnimator.SetInteger(CAR_ANIM, UNTWIST_IGNITION_KEY);
        else
            playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        int playerCarAnimIndex = playerController.playerBodyAnimator.GetInteger(CAR_ANIM);

        CancelIgnitionAnimation(isStarted: false, setIgnitionAnim: false);
        carEngine1AudioActive = false;
        disableAnimations = true;
        inIgnitionAnimation = false;
        SetIgnitionInteractTrigger();

        int ignitionAnimIndex = playerCarAnimIndex;
        if (playerAnimIndex == UNTWIST_IGNITION_KEY_FALLBACK) ignitionAnimIndex = UNTWIST_IGNITION_KEY;
        ignitionAnimator.SetInteger(IGNITION_ANIM, ignitionAnimIndex);

        CancelTryIgnitionRpc(setKeyInSlot: keyIsInIgnition, setCabinLight: isCabinLightOn, setIgnitionOn: ignitionOn, playerAnimIndex: playerCarAnimIndex, ignitionAnimIndex: ignitionAnimIndex);
    }


    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void CancelTryIgnitionRpc(bool setKeyInSlot, bool setCabinLight, bool setIgnitionOn, int playerAnimIndex, int ignitionAnimIndex)
    {
        CancelIgnitionAnimation(isStarted: false, setIgnitionAnim: false);
        carEngine1AudioActive = false;
        disableAnimations = true;
        inIgnitionAnimation = false;

        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, playerAnimIndex);
        ignitionAnimator.SetInteger(IGNITION_ANIM, ignitionAnimIndex);

        if (!keyIsInIgnition && setKeyInSlot)
        {
            carKeySounds.PlayOneShot(insertKey);
        }
        SetIgnitionKeyValues(setKeyInHand: false, setKeyInSlot: setKeyInSlot);
        if (isCabinLightOn != setCabinLight)
        {
            SetFrontCabinLightOn(setOn: setCabinLight);
        }
        if (!ignitionOn && setIgnitionOn)
        {
            ignitionOn = true;
            SetWindowInteractTriggers(true);
            if (dashboardSweepCoroutine == null && !hasSweepedDashboard)
            {
                dashboardSweepCoroutine = StartCoroutine(DashboardSweepCoroutine());
            }
            TrySetMirrorsFolded(true);
        }
    }


    // --- START IGNITION METHOD ---
    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void StartIgnitionRpc(bool setTriggers)
    {
        CancelIgnitionAnimation(isStarted: true, setIgnitionAnim: true);
        disableAnimations = false;
        inIgnitionAnimation = false;
        ignitionOn = true;
        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, SITTING_IN_SEAT_HANDS_ON_WHEEL);
        SetIgnitionKeyValues(setKeyInHand: false, setKeyInSlot: true);
        SetIgnition(setStarted: true, setCabinLight: true);
        if (setTriggers) SetIgnitionInteractTrigger();
    }

    public void SetIgnition(bool setStarted, bool setCabinLight)
    {
        SetFrontCabinLightOn(setCabinLight);
        if (ignitionStarted == setStarted)
        {
            return;
        }
        ignitionStarted = setStarted;
        carEngine1AudioActive = setStarted;
        if (setStarted)
        {
            carExhaustParticle.Play();
            miscEngineAudio.Stop();
            engineAudio1.Stop();
            if (vanVariant == VanVariant.Creasman)
            {
                float engineSpeed = (EngineRPM - MinEngineRPM) / (MaxEngineRPM - MinEngineRPM);
                engineAudio1.volume = revVolumeCurve.Evaluate(engineSpeed);
                engineAudio2.volume = 0f;
                miscEngineAudio.PlayOneShot(creasmanEngineStartSuccessful);
                engineAudio1.clip = creasmanEngineRun;
            }
            else
            {
                engineAudio1.PlayOneShot(engineStartSuccessful);
                engineAudio1.clip = engineRun;
            }
            return;
        }
        carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }


    // --- UNTWIST KEY IN IGNITION METHOD ---
    public void UntwistKeyInIgnition()
    {
        if (!localPlayerInControl && currentDriver != null)
            return;

        if (keyIsInIgnition && !ignitionStarted && !usingSwitchIgnition)
            return;

        if (!ignitionStarted || inIgnitionAnimation)
            return;

        CancelIgnitionCoroutine();
        keyIgnitionCoroutine = StartCoroutine(UntwistKey());
        SetIgnitionInteractTrigger();
        UntwistKeyInIgnitionRpc();
    }

    private IEnumerator UntwistKey()
    {
        disableAnimations = true;
        inIgnitionAnimation = false;
        // untwist key while engine running, but keep key in
        currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, UNTWIST_IGNITION_KEY_IN);
        ignitionAnimator.SetInteger(IGNITION_ANIM, UNTWIST_IGNITION_SLOT);
        yield return new WaitForSeconds(0.08f);
        if (dashboardSweepCoroutine != null)
        {
            StopCoroutine(dashboardSweepCoroutine);
            dashboardSweepCoroutine = null!;
            StopDashboardSweepCoroutine();
        }
        carKeySounds.PlayOneShot(twistKey);
        SetIgnitionKeyValues(setKeyInHand: false, setKeyInSlot: true);
        SetIgnition(setStarted: false, setCabinLight: true);
        engineAudio3.volume = 0.775f;
        if (vanVariant == VanVariant.Creasman)
        {
            engineAudio3.PlayOneShot(creasmanEngineStall);
        }
        else
        {
            engineAudio3.PlayOneShot(stallEngine);
        }
        SetIgnitionInteractTrigger();
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
        if (currentDriver != null && !localPlayerInControl)
            return;

        if (keyIsInIgnition && !usingSwitchIgnition && localPlayerInControl)
            return;

        if (ignitionStarted || !keyIsInIgnition || inIgnitionAnimation)
            return;

        CancelIgnitionCoroutine();
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
        SetIgnitionInteractTrigger();
        usingSwitchIgnition = false;
        chanceToStartIgnition = 20f;
        RemoveKeyFromIgnitionRpc();
    }

    private new IEnumerator RemoveKey()
    {
        disableAnimations = true;
        inIgnitionAnimation = false;
        if (currentDriver?.playerBodyAnimator.GetInteger(CAR_ANIM) == DEFAULT_PLAYER_ANIMATION)
            currentDriver?.playerBodyAnimator.SetTrigger(REMOVE_IN_IGNITION);
        else 
            currentDriver?.playerBodyAnimator.SetInteger(CAR_ANIM, REMOVE_IGNITION_KEY);
        ignitionAnimator.SetInteger(IGNITION_ANIM, DEFAULT_IGNITION_SLOT);
        engineStalled = false;
        yield return new WaitForSeconds(ignitionStarted ? 0.18f : 0.26f);
        if (dashboardSweepCoroutine != null)
        {
            StopCoroutine(dashboardSweepCoroutine);
            dashboardSweepCoroutine = null!;
            StopDashboardSweepCoroutine();
        }
        carKeySounds.PlayOneShot(removeKey);
        SetIgnitionKeyValues(setKeyInHand: true, setKeyInSlot: false);
        SetIgnition(setStarted: false, setCabinLight: false);
        ignitionOn = false;
        SetWindowInteractTriggers(false);
        TrySetMirrorsFolded(false);
        if (hasVehicleDisplay)
        {
            SetSymbolActive(vehicleDisplay, vehicleDisplayLight, false);
        }
        SetIgnitionInteractTrigger();
        yield return new WaitForSeconds(0.73f);
        SetIgnitionKeyValues(setKeyInHand: false, setKeyInSlot: false);
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
    public IEnumerator DashboardSweepCoroutine()
    {
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

    private void StopDashboardSweepCoroutine()
    {
        hasSweepedDashboard = false;
        currentSweepStage = 0;
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
    public void CancelIgnitionAnimation(bool isStarted = false, bool setIgnitionAnim = false)
    {
        CancelIgnitionCoroutine();
        keyIsInDriverHand = false;
        if (setIgnitionAnim) ignitionAnimator.SetInteger(IGNITION_ANIM, isStarted ? STARTED_IGNITION_SLOT : DEFAULT_IGNITION_SLOT);
    }

    private void CancelIgnitionCoroutine()
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
            keyIgnitionCoroutine = null;
        }
    }

    public void SetIgnitionKeyValues(bool setKeyInHand, bool setKeyInSlot)
    {
        keyIsInDriverHand = setKeyInHand;
        keyIsInIgnition = setKeyInSlot;
    }


    // --- GENERAL REPEAT METHODS ---
    public void SetTriggerHoverTip(InteractTrigger trigger, string tip, bool setHoldInteract = false)
    {
        trigger.hoverTip = tip;
        trigger.holdInteraction = setHoldInteract;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void CancelSetPlayerInVehicleRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("CancelSetPlayerInVehicleRpc failed to find player reference from network behaviour!");
            return;
        }
        if (!VehicleUtils.IsPlayerLocal(playerController))
            return;
        InteractTriggerPatches.CancelVehicleSeatInteraction();
    }


    // --- DRIVER OCCUPANT METHODS ---
    public void SetDriverInVehicle()
    {
        SetDriverInVehicleServerRpc(GameNetworkManager.Instance.localPlayerController);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetDriverInVehicleServerRpc(NetworkBehaviourReference playerNetObjRef, RpcParams rpcParams = default)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("SetDriverInVehicleServerRpc failed to find player reference from network behaviour!");
            return;
        }
        if (playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentDriver != null)
        {
            CancelSetPlayerInVehicleRpc(playerNetObjRef);
            return;
        }
        currentDriver = playerController;
        NetworkObject.ChangeOwnership(rpcParams.Receive.SenderClientId);
        SetDriverInVehicleOwnerRpc();
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    public void SetDriverInVehicleOwnerRpc()
    {
        frontLeftSeat.SetLocalPlayerIntoSeat();
        ActivateControl();
        disableAnimations = !ignitionStarted;
        inIgnitionAnimation = false;
        usingSwitchIgnition = false;
        SetTriggerHoverTip(driverSideDoorTrigger, DOOR_EXIT_HOVERTIP, true);
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        ignitionCollider.enabled = true;
        engineStalled = false;
        CancelIgnitionAnimation(ignitionStarted, true);
        SetIgnitionInteractTrigger();
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        if (ignitionStarted) playerController.playerBodyAnimator.SetInteger(CAR_ANIM, SITTING_IN_SEAT_HANDS_ON_WHEEL);
        else playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        int playerAnimIndex = playerController.playerBodyAnimator.GetInteger(CAR_ANIM);
        SetIgnition(ignitionStarted, ignitionOn);
        if (driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(playerController);
        HUDManager.Instance.ChangeControlTipMultiple(carTooltips, false, null);
        SetDriverInVehicleNotOwnerRpc(playerController, ignitionOn, keyIsInIgnition, ignitionStarted, playerAnimIndex);
    }

    [Rpc(SendTo.NotOwner)]
    public void SetDriverInVehicleNotOwnerRpc(NetworkBehaviourReference playerNetObjRef, bool setIgnitionOn, bool setKeyInSlot, bool setStarted, int playerAnimIndex)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("SetDriverInVehicleNotOwnerRpc failed to find player reference from network behaviour!");
            return;
        }
        driverSeatTrigger.interactable = false;
        currentDriver = playerController;
        frontLeftSeat.SetPlayerAnimations(playerController, false);
        disableAnimations = !setStarted;
        inIgnitionAnimation = false;
        usingSwitchIgnition = false;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        ignitionCollider.enabled = false;
        engineStalled = false;
        CancelIgnitionAnimation(setStarted, true);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, playerAnimIndex);
        keyIsInIgnition = setKeyInSlot;
        ignitionOn = setIgnitionOn;
        SetIgnition(setStarted, setIgnitionOn);
        TrySetMirrorsFolded(setIgnitionOn);
        if (setIgnitionOn && dashboardSweepCoroutine == null && !hasSweepedDashboard)
        {
            dashboardSweepCoroutine = StartCoroutine(DashboardSweepCoroutine());
        }
    }

    public new void ExitDriverSideSeat()
    {
        if (!localPlayerInControl)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        int exitPoint = CanExitVehicle(passengerSide: false);
        if (exitPoint == 0) if (!driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(playerController);
        if (exitPoint != -1)
        {
            playerController.TeleportPlayer(driverSideExitPoints[exitPoint].position);
            return;
        }
        playerController.TeleportPlayer(driverSideExitPoints[1].position);
    }

    public void OnDriverExitVehicle()
    {
        if (!IsSpawned || 
            NetworkManager == null || 
            !NetworkManager.IsListening)
        {
            return;
        }
        if (!VehicleUtils.IsPlayerLocal(currentDriver))
        {
            return;
        }
        DisableControl();
        disableAnimations = !ignitionStarted;
        inIgnitionAnimation = false;
        usingSwitchIgnition = false;
        SetTriggerHoverTip(driverSideDoorTrigger, DOOR_ENTER_HOVERTIP);
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        ignitionCollider.enabled = true;
        engineStalled = false;
        CancelIgnitionAnimation(ignitionStarted, true);
        SetIgnitionInteractTrigger();
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        SetIgnition(ignitionStarted, ignitionOn);
        chanceToStartIgnition = 20f;
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
        OnDriverExitVehicleServerRpc(
            playerController, 
            syncedPosition, 
            syncedRotation, 
            ignitionStarted, 
            keyIsInIgnition,
            ignitionOn);
    }

    [Rpc(SendTo.Server)]
    public void OnDriverExitVehicleServerRpc(NetworkBehaviourReference playerNetObjRef, Vector3 carLocation, Quaternion carRotation, bool setStarted, bool setKeyInSlot, bool setIgnitionOn)
    {
        OnDriverExitVehicleRpc(playerNetObjRef, carLocation, carRotation, setStarted, setKeyInSlot, setIgnitionOn);
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void OnDriverExitVehicleRpc(NetworkBehaviourReference playerNetObjRef, Vector3 carLocation, Quaternion carRotation, bool setStarted, bool setKeyInSlot, bool setIgnitionOn)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("OnDriverExitVehicleRpc failed to find player reference from network behaviour!");
            return;
        }
        if (VehicleUtils.IsPlayerLocal(playerController))
        {
            Plugin.LogDebug("OnDriverExitVehicleRpc player argument was the previous occupant.");
            return;
        }
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        driverSeatTrigger.interactable = true;
        frontLeftSeat.ReturnPlayerAnimations(playerController, false);
        currentDriver = null;
        drivePedalPressed = false;
        brakePedalPressed = false;
        disableAnimations = !setStarted;
        inIgnitionAnimation = false;
        usingSwitchIgnition = false;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        ignitionCollider.enabled = true;
        engineStalled = false;
        CancelIgnitionAnimation(setStarted, true);
        chanceToStartIgnition = 20f;
        SetIgnitionInteractTrigger();
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        keyIsInIgnition = setKeyInSlot;
        ignitionOn = setIgnitionOn;
        SetIgnition(setStarted, setIgnitionOn);
        TrySetMirrorsFolded(setIgnitionOn);
        if (setIgnitionOn && dashboardSweepCoroutine == null && !hasSweepedDashboard)
        {
            dashboardSweepCoroutine = StartCoroutine(DashboardSweepCoroutine());
        }
    }


    // --- MIDDLE PASSENGER OCCUPANT METHODS ---
    public void SetMiddlePassengerInVehicle()
    {
        SetMiddlePassengerInVehicleServerRpc(GameNetworkManager.Instance.localPlayerController);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetMiddlePassengerInVehicleServerRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("SetMiddlePassengerInVehicleServerRpc failed to find player reference from network behaviour!");
            return;
        }
        if (playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentMiddlePassenger != null)
        {
            CancelSetPlayerInVehicleRpc(playerNetObjRef);
            return;
        }
        currentMiddlePassenger = playerController;
        SetMiddlePassengerInVehicleRpc(playerController);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetMiddlePassengerInVehicleRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("SetMiddlePassengerInVehicleRpc failed to find player reference from network behaviour!");
            return;
        }
        PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        if (playerController == localPlayerController)
        {
            frontMiddleSeat.SetLocalPlayerIntoSeat();
            localPlayerInMiddlePassengerSeat = true;
            SetTriggerHoverTip(driverSideDoorTrigger, DOOR_EXIT_HOVERTIP, true);
            SetTriggerHoverTip(passengerSideDoorTrigger, DOOR_EXIT_HOVERTIP, true);
            if (driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(localPlayerController);
            if (passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(localPlayerController);
        }
        else
        {
            middlePassengerSeatTrigger.interactable = false;
            frontMiddleSeat.SetPlayerAnimations(playerController, false);
        }
        currentMiddlePassenger = playerController;
    }

    public void ExitMiddlePassengerSeat(bool isPassengerSide)
    {
        if (!localPlayerInMiddlePassengerSeat)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        int exitPoint = CanExitVehicle(passengerSide: isPassengerSide);
        if (isPassengerSide)
        {
            if (exitPoint == 0) if (!passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(playerController);
            if (exitPoint != -1)
            {
                playerController.TeleportPlayer(passengerSideExitPoints[exitPoint].position);
                return;
            }
            playerController.TeleportPlayer(passengerSideExitPoints[1].position);
            return;
        }
        if (exitPoint == 0) if (!driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(playerController);
        if (exitPoint != -1)
        {
            playerController.TeleportPlayer(driverSideExitPoints[exitPoint].position);
            return;
        }
        playerController.TeleportPlayer(driverSideExitPoints[1].position);
    }

    public void OnMiddlePassengerExitVehicle()
    {
        if (!IsSpawned || 
            NetworkManager == null || 
            !NetworkManager.IsListening)
        {
            return;
        }
        if (!VehicleUtils.IsPlayerLocal(currentMiddlePassenger))
        {
            return;
        }
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        localPlayerInMiddlePassengerSeat = false;
        SetTriggerHoverTip(driverSideDoorTrigger, DOOR_ENTER_HOVERTIP);
        SetTriggerHoverTip(passengerSideDoorTrigger, DOOR_ENTER_HOVERTIP);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        currentMiddlePassenger = null!;
        OnMiddlePassengerExitVehicleRpc(
            playerController,
            playerController.transform.position);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnMiddlePassengerExitVehicleRpc(NetworkBehaviourReference playerNetObjRef, Vector3 exitPoint)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("OnMiddlePassengerExitVehicleRpc failed to find player reference from network behaviour!");
            return;
        }
        frontMiddleSeat.ReturnPlayerAnimations(playerController, false);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        playerController.TeleportPlayer(exitPoint, false, 0f, false, true);
        currentMiddlePassenger = null!;
        middlePassengerSeatTrigger.interactable = true;
    }


    // --- PASSENGER OCCUPANT METHODS ---
    public void SetPassengerInVehicle()
    {
        SetPassengerInVehicleServerRpc(GameNetworkManager.Instance.localPlayerController);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void SetPassengerInVehicleServerRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("SetPassengerInVehicleServerRpc failed to find player reference from network behaviour!");
            return;
        }
        if (playerController.isPlayerDead ||
            !playerController.isPlayerControlled ||
            currentPassenger != null)
        {
            CancelSetPlayerInVehicleRpc(playerNetObjRef);
            return;
        }
        currentPassenger = playerController;
        SetPassengerInVehicleRpc(playerController);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetPassengerInVehicleRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("SetPassengerInVehicleRpc failed to find player reference from network behaviour!");
            return;
        }
        PlayerControllerB localPlayerController = GameNetworkManager.Instance.localPlayerController;
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        if (playerController == localPlayerController)
        {
            frontRightSeat.SetLocalPlayerIntoSeat();
            localPlayerInPassengerSeat = true;
            SetTriggerHoverTip(passengerSideDoorTrigger, DOOR_EXIT_HOVERTIP, true);
            if (passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(localPlayerController);
        }
        else
        {
            passengerSeatTrigger.interactable = false;
            frontRightSeat.SetPlayerAnimations(playerController, false);
        }
        currentPassenger = playerController;
    }

    public new void ExitPassengerSideSeat()
    {
        if (!localPlayerInPassengerSeat || localPlayerInMiddlePassengerSeat)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        int exitPoint = CanExitVehicle(passengerSide: true);
        if (exitPoint == 0) if (!passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(playerController);
        if (exitPoint != -1)
        {
            playerController.TeleportPlayer(passengerSideExitPoints[exitPoint].position);
            return;
        }
        playerController.TeleportPlayer(passengerSideExitPoints[1].position);
    }

    public void OnPassengerExitVehicle()
    {
        if (!IsSpawned ||
            NetworkManager == null ||
            !NetworkManager.IsListening)
        {
            return;
        }
        if (!VehicleUtils.IsPlayerLocal(currentPassenger))
        {
            return;
        }
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        localPlayerInPassengerSeat = false;
        SetTriggerHoverTip(passengerSideDoorTrigger, DOOR_ENTER_HOVERTIP);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        currentPassenger = null!;
        OnPassengerExitVehicleRpc(
            playerController, 
            playerController.transform.position);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnPassengerExitVehicleRpc(NetworkBehaviourReference playerNetObjRef, Vector3 exitPoint)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("OnPassengerExitVehicleRpc failed to find player reference from network behaviour!");
            return;
        }
        frontRightSeat.ReturnPlayerAnimations(playerController, false);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        playerController.TeleportPlayer(exitPoint, false, 0f, false, true);
        currentPassenger = null;
        passengerSeatTrigger.interactable = true;
    }


    // --- LEAVE OCCUPANT MID-GAME METHODS ---
    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void OnDriverLeaveGameServerRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("OnDriverLeaveGameServerRpc failed to find player reference from network behaviour!");
            return;
        }
        NetworkObject.ChangeOwnership(NetworkManager.ServerClientId);
        syncedPosition = transform.position;
        syncedRotation = transform.rotation;
        OnDriverLeave(playerController, ignitionStarted, keyIsInIgnition, isCabinLightOn, ignitionOn);
        OnDriverLeaveGameRpc(playerNetObjRef, syncedPosition, syncedRotation, ignitionStarted, keyIsInIgnition, isCabinLightOn, ignitionOn);
    }

    public void OnDriverLeave(PlayerControllerB playerController, bool setStarted, bool setKeyInSlot, bool setCabinLight, bool setIgnitionOn)
    {
        frontLeftSeat.ReturnPlayerAnimations(playerController, false);
        currentDriver = null;
        drivePedalPressed = false;
        brakePedalPressed = false;

        disableAnimations = !setStarted;
        inIgnitionAnimation = false;
        usingSwitchIgnition = false;

        startIgnitionTrigger.isBeingHeldByPlayer = false;
        ignitionCollider.enabled = true;
        engineStalled = false;

        CancelIgnitionAnimation(setStarted, true);
        chanceToStartIgnition = 20f;
        SetIgnitionInteractTrigger();

        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        playerController.TeleportPlayer(Vector3.zero, false, 0f, false, true);

        keyIsInIgnition = setKeyInSlot;
        ignitionOn = setIgnitionOn;
        SetIgnition(setStarted, setIgnitionOn);
        driverSeatTrigger.interactable = true;
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void OnDriverLeaveGameRpc(NetworkBehaviourReference playerNetObjRef, Vector3 carLocation, Quaternion carRotation, bool setIgnitionState, bool setKeyInSlot, bool cabinLightOn, bool preIgnition)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("OnDriverLeaveGameRpc failed to find player reference from network behaviour!");
            return;
        }
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        OnDriverLeave(playerController, setIgnitionState, setKeyInSlot, cabinLightOn, preIgnition);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void OnMiddlePassengerLeaveGameRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("OnMiddlePassengerLeaveGameRpc failed to find player reference from network behaviour!");
            return;
        }
        frontMiddleSeat.ReturnPlayerAnimations(playerController, false);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        playerController.TeleportPlayer(Vector3.zero, false, 0f, false, true);
        currentMiddlePassenger = null!;
        middlePassengerSeatTrigger.interactable = true;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void OnPassengerLeaveGameRpc(NetworkBehaviourReference playerNetObjRef)
    {
        if (!playerNetObjRef.TryGet(out PlayerControllerB playerController))
        {
            Plugin.LogError("OnPassengerLeaveGameRpc failed to find player reference from network behaviour!");
            return;
        }
        frontRightSeat.ReturnPlayerAnimations(playerController, false);
        playerController.playerBodyAnimator.SetInteger(CAR_ANIM, DEFAULT_PLAYER_ANIMATION);
        playerController.playerBodyAnimator.SetFloat(ANIMATION_SPEED, DEFAULT_ANIMATION_SPEED);
        playerController.TeleportPlayer(Vector3.zero, false, 0f, false, true);
        currentPassenger = null!;
        passengerSeatTrigger.interactable = true;
    }


    // --- OCCUPANT EXITING METHODS ---
    private int CanExitVehicle(bool passengerSide)
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
        Plugin.LogDebug("Attempted to enable collision, but this is unnecessary.");
        return;
    }

    public new void DisableVehicleCollisionForAllPlayers()
    {
        Plugin.LogDebug("Attempted to disable collision, but this is unnecessary.");
        return;
    }

    public new void SetVehicleCollisionForPlayer(bool setEnabled, PlayerControllerB player)
    {
        Plugin.LogDebug("Attempted to set collision, but this is unnecessary.");
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

        moveInputVector = IngamePlayerSettings.Instance.playerInput.actions.FindAction(PLAYER_MOVEMENT, false).ReadValue<Vector2>();
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

        drivePedalPressed = Plugin.VehicleControlsInstance.GasPedalKey.IsPressed() || 
                           (inReverse && Plugin.VehicleControlsInstance.ReversePedalKey.IsPressed());
        gasInput = Mathf.MoveTowards(gasInput, drivePedalPressed ? 1f : 0f, 10f * Time.deltaTime);

        TryRecenterSteeringWheel();
    }

    private void TryRecenterSteeringWheel()
    {
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
        if (syncedDrivePedalPressed != drivePedalPressed || 
            syncedBrakePedalPressed != brakePedalPressed || 
            syncedClutchPedalPressed != clutchPedalPressed)
        {
            syncedDrivePedalPressed = drivePedalPressed;
            syncedBrakePedalPressed = brakePedalPressed;
            syncedClutchPedalPressed = clutchPedalPressed;
            SyncPlayerInputsRpc(drivePedalPressed, brakePedalPressed, clutchPedalPressed);
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
            Plugin.LogDebug($"Van is too far away from the magnet to toggle it. Distance: {magnetDistance}.");
            return;
        }
        ScandalsTweaks.Utils.References.magnetTrigger!.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
        /*
        string newState = ScandalsTweaks.Utils.References.magnetTrigger.boolValue ? "Activated" : "Deactivated";
        HUDManager.Instance.DisplayGlobalNotification($"Ship Magnet {newState}");
        */
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
            SetIgnitionInteractTrigger();
            HUDManager.Instance.holdFillAmount = 0f;
            startIgnitionTrigger.isBeingHeldByPlayer = false;
        }
    }


    // --- SHIFTING GEARS METHODS ---
    public void SetGearAnimation(string setGear)
    {
        gearStickAnimator.CrossFade(setGear, 0.01f);
        if (ignitionStarted) currentDriver?.playerBodyAnimator.CrossFade(setGear, 0.01f);
        gearStickAudio.PlayOneShot(shiftSounds[UnityEngine.Random.Range(0, shiftSounds.Length)]);
    }

    public new void ShiftGearForwardInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        float timeSinceStartup = Time.realtimeSinceStartup;
        if (!localPlayerInControl ||
            timeSinceStartup - timeSinceTogglingRadio < 0.5f ||
            timeSinceStartup - timeAtLastGearShift < 0.15f)
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
            if (IsPlayerSeatedInCar())
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            }
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

        float timeSinceStartup = Time.realtimeSinceStartup;
        if (!localPlayerInControl ||
            timeSinceStartup - timeSinceTogglingRadio < 0.5f ||
            timeSinceStartup - timeAtLastGearShift < 0.15f)
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
            if (IsPlayerSeatedInCar())
            {
                if (EngineRPM > ((MaxEngineRPM - MinEngineRPM) / 2f))
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                }
                else
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                }
            }
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
            if (IsPlayerSeatedInCar())
            {
                if (EngineRPM > ((MaxEngineRPM - MinEngineRPM) / 2f))
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                }
                else
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                }
            }
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
        if (IsInShipPhaseAndNotLoadingLevel(checkNotLoadingLevel: false) || magnetedToShip ||
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

    [Rpc(SendTo.NotOwner)]
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

        CollectItemsInVan();

        if (IsInShipPhaseAndNotLoadingLevel()) return;
        if (GameNetworkManager.Instance.localPlayerController == null) return;
        MagnetCarRpc(magnetTargetPosition, eulerAngles, magnetStartPosition, magnetStartRotation, tempRotation, averageVelocityAtMagnetStart);
    }

    [Rpc(SendTo.NotOwner)]
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

        CollectItemsInVan();
    }

    public void CollectItemsInVan()
    {   
        foreach (GrabbableObject component in References.itemsInTruck)
        {
            if (component != null &&
                !component.isHeld &&
                !component.isHeldByEnemy &&
                component.transform.parent == transform)
            {
                if (lastDriver != null)
                {
                    lastDriver.SetItemInElevator(magnetedToShip, magnetedToShip, component);
                }
                else
                {
                    GameNetworkManager.Instance.localPlayerController?.SetItemInElevator(magnetedToShip, magnetedToShip, component);
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
    public void SetHeaterOnLocalClient()
    {
        heaterOn = !heaterOn;
        SetHeaterRpc(heaterOn);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterRpc(bool heatOn)
    {
        heaterOn = heatOn;
    }


    public void SetHeaterFanSpeedOnLocalClient(int setSpeed)
    {
        int fanSpeed = Mathf.Clamp(heaterSpeed + setSpeed, 0, 4);
        if (heaterSpeed == fanSpeed) return;
        heaterSpeed = fanSpeed;
        SetHeaterFanSpeedRpc(heaterSpeed);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterFanSpeedRpc(int fanSpeed)
    {
        heaterSpeed = fanSpeed;
    }


    public void SetHeaterDirectionOnLocalClient(int heatTemp)
    {
        if (heaterTemperature == heatTemp) return;
        heaterTemperature = heatTemp;
        isHeaterCold = heatTemp == 0;
        isHeaterWarm = heatTemp == 1;
        SetHeaterDirectionRpc(heaterTemperature);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterDirectionRpc(int temp)
    {
        heaterTemperature = temp;
        isHeaterCold = heaterTemperature == 0;
        isHeaterWarm = heaterTemperature == 1;
    }


    // --- TURBO BOOST AND JUMP ABILITY ---
    private new void DoTurboBoost(InputAction.CallbackContext context)
    {
        if (!context.performed) 
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (playerController == null ||
            playerController.isPlayerDead ||
            !playerController.isPlayerControlled) 
            return;

        if (playerController.isTypingChat ||
            playerController.quickMenuManager.isMenuOpen) 
            return;

        if (!localPlayerInControl || !ignitionStarted ||
            jumpingInCar || keyIgnitionCoroutine != null) return;

        Vector2 dir = IngamePlayerSettings.Instance.playerInput.actions.FindAction(PLAYER_MOVEMENT, false).ReadValue<Vector2>();
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
                StartCoroutine(JerkCarUpward(dir));
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
            }
        }
    }

    private IEnumerator JerkCarUpward(Vector3 dir)
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

    [Rpc(SendTo.NotOwner)]
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
        if (!localPlayerInControl || vanAlarmCoroutine != null)
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
        RemoveRainCollision();
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
                Plugin.LogWarning($"Player {i} setting parent since vehicle was disabled.");
            }
        }
        if (IsPlayerSeatedInCar())
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
            RemoveRainCollision();
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
                    Plugin.LogWarning($"Player {i} setting parent since vehicle was removed.");
                }
            }
            if (IsPlayerSeatedInCar())
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
                CollectItemsInVan();
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

        SetCarEffects(steeringWheelAnimValue);
        UpdateOccupantAnimations();

        if (localPlayerInControl)
        {
            GetVehicleInput();
            return;
        }
        moveInputVector = Vector2.zero;
        steeringAnimValue = 0f;
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
        currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, SITTING_IN_SEAT_HANDS_ON_WHEEL);
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
        player.playerBodyAnimator.SetInteger(CAR_ANIM, animIndex);
    }


    // --- RADIO TIME SYNC ---
    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SyncRadioTimeRpc(float songTime, float syncedTime)
    {
        currentSongTime = songTime;
        syncedSongTime = syncedTime;
        float timeSinceStartup = Time.realtimeSinceStartup;
        timeLastSyncedRadio = timeSinceStartup;
        SetRadioTime(timeSinceStartup);
    }

    public void SetRadioTime(float timeSinceStartup)
    {
        if (!radioOn || radioAudio.clip == null || isFmRadio)
            return;
        float setRadioTime = (syncedSongTime + (timeSinceStartup - timeLastSyncedRadio)) % radioAudio.clip.length;
        if (Mathf.Abs(setRadioTime - radioAudio.time) > 1f)
        {
            radioAudio.time = setRadioTime;
        }
    }


    // --- RADIO TYPE (FM/CD) ---
    public void ChangeRadioType()
    {
        if (ScanVanNetworker.Instance.StreamerRadio.Value)
        {
            HUDManager.Instance.DisplayTip("Live radio disabled", 
                "The host has disabled this feature!");
            return;
        }

        if (!radioOn) return;
        isFmRadio = !isFmRadio;
        float timeSinceStartup = Time.realtimeSinceStartup;
        if (!isFmRadio)
        {
            liveRadioController.TurnRadioOnOff(false);
            radioInterference.Stop();
            SetCurrentRadioClip();
            if (radioAudio.loop) radioAudio.loop = false;
            if (radioAudio.clip != null) radioAudio.Play();
            currentSongTime = lastSongTime;
            syncedSongTime = currentSongTime;
            timeLastSyncedRadio = timeSinceStartup;
            radioAudio.time = currentSongTime;
            radioOn = true;
        }
        else
        {
            liveRadioController.TogglePowerLocalClient(false);
            if (!radioAudio.loop) radioAudio.loop = true;
            lastSongTime = currentSongTime;
            syncedSongTime = lastSongTime;
            timeLastSyncedRadio = timeSinceStartup;
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
        float timeSinceStartup = Time.realtimeSinceStartup;
        if (!isFmRadio)
        {
            liveRadioController.TurnRadioOnOff(false);
            radioInterference.Stop();
            SetCurrentRadioClip();
            if (radioAudio.loop) radioAudio.loop = false;
            if (radioAudio.clip != null) radioAudio.Play();
            currentSongTime = currentTime;
            syncedSongTime = currentTime;
            timeLastSyncedRadio = timeSinceStartup;
            radioAudio.time = currentTime;
            radioOn = true;
            return;
        }
        if (!radioAudio.loop) radioAudio.loop = true;
        lastSongTime = lastTime;
        syncedSongTime = lastTime;
        timeLastSyncedRadio = timeSinceStartup;
        radioAudio.time = lastTime;
        radioOn = true;
    }


    // --- RADIO SEEK (CHANGE CHANNEL/TRACK) ---
    public void ChangeRadioStation(bool seekForward)
    {
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

        if (!ScanVanNetworker.Instance.StreamerRadio.Value)
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
        float timeSinceStartup = Time.realtimeSinceStartup;
        if (localPlayerInControl && keyIgnitionCoroutine == null && ignitionStarted)
        {
            timeSinceTogglingRadio = timeSinceStartup;
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
            timeLastSyncedRadio = timeSinceStartup;
            radioAudio.time = lastSongTime;
        }
        else
        {
            lastSongTime = currentSongTime;
            syncedSongTime = currentSongTime;
            timeLastSyncedRadio = timeSinceStartup;
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
        float timeSinceStartup = Time.realtimeSinceStartup;
        if (on)
        {
            SetCurrentRadioClip();
            if (radioAudio.clip != null) radioAudio.Play();
            currentSongTime = radioTime;
            syncedSongTime = radioTime;
            timeLastSyncedRadio = timeSinceStartup;
            radioAudio.time = radioTime;
        }
        else
        {
            lastSongTime = lastRadioTime;
            syncedSongTime = lastRadioTime;
            timeLastSyncedRadio = timeSinceStartup;
            if (radioAudio.clip != null) radioAudio.Stop();
        }
    }

    private void SetCurrentRadioClip()
    {
        if (!ScanVanNetworker.Instance.StreamerRadio.Value)
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
        float timeSinceStartup = Time.realtimeSinceStartup;
        if (IsServer && radioAudio.isPlaying && timeSinceStartup > radioPingTimestamp)
        {
            radioPingTimestamp = (timeSinceStartup + 1f);
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
            if (timeSinceStartup - timeLastSyncedRadio > 1f)
            {
                timeLastSyncedRadio = timeSinceStartup;
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
        if (vanAlarmCoroutine != null ||
            keyIgnitionCoroutine != null)
        {
            alarmDebounce = false;
            return;
        }
        BeginAlarmRpc();
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void BeginAlarmRpc()
    {
        vanAlarmCoroutine = StartCoroutine(BeginAlarmSound());
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
            leftBlinkerMesh.material = blinkerOffMat;
            leftBlinkerMeshLod.material = blinkerOffMat;
            rightBlinkerMesh.material = blinkerOffMat;
            rightBlinkerMeshLod.material = blinkerOffMat;
            SetSymbolActive(leftSignalSymbol, leftSignalLight, false);
            SetSymbolActive(hazardWarningSymbol, hazardWarningLight, false);
            SetSymbolActive(rightSignalSymbol, rightSignalLight, false);
            blinkerLightsContainer.SetActive(false);
        }
        vanAlarmCoroutine = null!;
        alarmDebounce = false;
    }


    // --- VISUAL EFFECTS ---
    public new void SetCarEffects(float setSteering)
    {
        // steering
        float timeSinceStartup = Time.realtimeSinceStartup;

        setSteering = IsOwner ? setSteering : 0f;
        steeringWheelAnimFloat = Mathf.Clamp(steeringWheelAnimFloat + setSteering * (steeringWheelTurnSpeed * Time.deltaTime / 6f), -1f, 1f);
        steeringWheelAnimator.SetFloat(STEERING_WHEEL_SPEED, Mathf.Clamp((steeringWheelAnimFloat + 1f) / 2f, 0f, 1f));

        // handbrake
        handbrakeAnimValue = Mathf.MoveTowards(handbrakeAnimValue, handbrakeEngaged ? 0f : 1f, handbrakePullSpeed * Time.deltaTime * (timeSinceStartup - timeAtLastHandbrakePull));
        handbrakeAnimator.SetFloat(HANDBRAKE_ENGAGEMENT, handbrakeAnimValue);

        // cluster
        speedometerTransform.localRotation = Quaternion.Euler(0f, maxSpeedometerRot * speedometerFloat, 0f);
        tachometerTransform.localRotation = Quaternion.Euler(0f, maxTachometerRot * tachometerFloat, 0f);
        oilPressureTransform.localRotation = Quaternion.Euler(0f, -135f * oilPressureFloat, 0f);

        if (ignitionStarted)
        {
            float vehicleHPfloat = (float)carHP / baseCarHP;
            float turboPressureInt = turboPressureNeedleCurve.Evaluate(vehicleHPfloat);
            float oilPressureCurve = oilPressureNeedleCurve.Evaluate(
                (float)((carHP / 2) + ((turboBoosts * 3.5) * (turboPressureFloat * (overdriveSwitchEnabled ? 1 : 0)))) / (baseCarHP + 4));
            float speedometerRot = Mathf.Abs(backWheelRPM) / speedometerValue;
            float tachometerRot = EngineRPM / MaxEngineRPM;

            speedometerFloat = Mathf.Lerp(speedometerFloat, speedometerRot, 6f * Time.deltaTime);
            speedometerFloat = Mathf.Clamp01(speedometerFloat);
            tachometerFloat = Mathf.Lerp(tachometerFloat, tachometerRot, 6f * Time.deltaTime);
            tachometerFloat = Mathf.Clamp01(tachometerFloat);
            turboPressureFloat = Mathf.Lerp(turboPressureFloat, turboPressureInt, 6f * Time.deltaTime);
            oilPressureFloat = Mathf.Lerp(oilPressureFloat, oilPressureCurve, 4f * Time.deltaTime);
        }
        else
        {
            bool tryingIgnition = engineAudio1.volume > 0.1f && keyIgnitionCoroutine != null;
            float tachFloat = tryingIgnition ? 0.065f : 0f;
            speedometerFloat = Mathf.Lerp(speedometerFloat, 0f, 6f * Time.deltaTime);
            tachometerFloat = Mathf.Lerp(tachometerFloat, tachFloat, 4.5f * Time.deltaTime);
            turboPressureFloat = Mathf.Lerp(turboPressureFloat, 0f, 6f * Time.deltaTime);
            oilPressureFloat = Mathf.Lerp(oilPressureFloat, 0f, 6f * Time.deltaTime);
        }
        SetCarRadioEffects();
        SetCarInteriorControls();
        SetCarHeaterControls();
        SetCarLighting();
        SetCarAudioEffects();
        bool attachedToShipAndInShipPhase = magnetedToShip && (IsInShipPhaseAndNotLoadingLevel() || !StartOfRound.Instance.shipDoorsEnabled);
        if (setAudiosMuted != attachedToShipAndInShipPhase)
        {
            SetVehicleAudioMuted(attachedToShipAndInShipPhase);
            setAudiosMuted = attachedToShipAndInShipPhase;
        }
        if (attachedToShipAndInShipPhase && windshieldBroken)
        {
            RegenerateWindshield();
        }
        SetCarTyreEffects();
        SetCarIgnitionKey();
        TrySetCarDashboard();
        TryShakePlayerCamera();
        UpdateScanNodeText();
        if (carDestroyed)
            return;
        SetDashboardSymbols();
        if (IsOwner)
        {
            SyncEffectsToOtherClients();
            if (syncedExtremeStress != underExtremeStress)
            {
                syncedExtremeStress = underExtremeStress;
                SyncExtremeStressRpc(underExtremeStress);
            }
            return;
        }
        if (smoothRotation) steeringWheelAnimFloat = Mathf.Lerp(steeringWheelAnimFloat, syncedSteeringWheelRotation, steeringSpeed * Time.deltaTime);
        else steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, syncedSteeringWheelRotation, steeringWheelTurnSpeed * Time.deltaTime / 6f);
    }

    private void TryShakePlayerCamera()
    {
        bool playerInCab = IsPlayerSeatedInCar() || vanCabinZone.playerInZone;
        if (!windshieldBroken || !playerInCab)
        {
            if (shakingCamera)
            {
                HUDManager.Instance.StopShakingCamera();
                shakingCamera = false;
            }
            return;
        }
        bool shouldShake = averageVelocity.magnitude >= 24f;
        if (shouldShake && (!shakingCamera || !HUDManager.Instance.playerScreenShakeAnimator.GetBool(CAMERA_SHAKE_CONSTANT)))
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Constant);
            shakingCamera = true;
        }
        else if (!shouldShake && shakingCamera)
        {
            HUDManager.Instance.StopShakingCamera();
            shakingCamera = false;
        }
    }

    public void SetCarInteriorControls()
    {
        float switchDirection = !lowBeamsOn ? 0f : !highBeamsOn ? 0.5f : 1f;
        headlampSwitchAnimFloat = Mathf.MoveTowards(headlampSwitchAnimFloat, switchDirection, 8f * Time.deltaTime);
        headlampSwitchAnimator.SetFloat(HEADLAMP_SWITCH_FLOAT, headlampSwitchAnimFloat);
    }

    public void SetCarHeaterControls()
    {
        float baseFanSpeed = heaterSpeed - 1f;
        float fanSpeed = baseFanSpeed / 3f;
        hvacFanSpeedAnimFloat = Mathf.MoveTowards(hvacFanSpeedAnimFloat, fanSpeed, 8f * Time.deltaTime);

        float heatDirection = heaterOn ? 1f : 0f;
        hvacFanDirectionAnimFloat = Mathf.MoveTowards(hvacFanDirectionAnimFloat, heatDirection, 8f * Time.deltaTime);

        float fanTemperature = heaterTemperature;
        hvacFanTemperatureAnimFloat = Mathf.MoveTowards(hvacFanTemperatureAnimFloat, 1f - fanTemperature, 8f * Time.deltaTime);

        hvacAnimator.SetFloat(HVAC_FAN_SPEED_FLOAT, hvacFanSpeedAnimFloat);
        hvacAnimator.SetFloat(HVAC_DIRECTION_FLOAT, hvacFanDirectionAnimFloat);
        hvacAnimator.SetFloat(HVAC_TEMPERATURE_FLOAT, hvacFanTemperatureAnimFloat);
    }

    // in-car radio screen
    public void SetCarRadioEffects()
    {
        // time & radio frequency on dash
        if (!ignitionOn && !radioOn)
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
        if (!ScanVanNetworker.Instance.StreamerRadio.Value && radioClips.Length == 0)
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
    public void SetCarLighting()
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

    private void TrySetCarDashboard()
    {
        if (vanVariant != VanVariant.Creasman)
            return;
        string mileage = odometerValue.ToString("000.00");
        voiceModule.clusterScreen.text = ignitionOn ? mileage : null;
        if (voiceModule.clusterLight.enabled != ignitionOn)
            voiceModule.clusterLight.enabled = ignitionOn;
    }

    // --- VEHICLE AUDIO METHODS ---
    /// <summary>
    ///  Available from EnemySoundFixes, licensed under GNU General Public License.
    ///  Source: https://github.com/ButteryStancakes/EnemySoundFixes/blob/master/Patches/CruiserPatches.cs
    /// </summary>
    private void SetVehicleAudioProperties(AudioSource audio, bool audioActive, float lowest, float highest, float lerpSpeed, bool useVolumeInsteadOfPitch = false, float onVolume = 1f, float onVolumeLerpSpeed = 20f)
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
            audio.volume = Mathf.Lerp(audio.volume, onVolume, onVolumeLerpSpeed * Time.deltaTime);
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

    private void SetVehicleAudioMuted(bool muted)
    {
        hornAudio.mute = muted;
        engineAudio1.mute = muted;
        engineAudio2.mute = muted;
        carKeySounds.mute = muted;
        cabinLightSwitchAudio.mute = muted;
        heaterAudio.mute = muted;
        voiceModule.voiceAudio.mute = muted;
        wiperAudio.mute = muted;
        rollingAudio.mute = muted;
        skiddingAudio.mute = muted;
        turbulenceAudio.mute = muted;
        hoodFireAudio.mute = muted;
        extremeStressAudio.mute = muted;
        pushAudio.mute = muted;
        radioAudio.mute = muted;
        radioInterference.mute = muted;
    }

    public void SetCarAudioEffects()
    {
        float timeSinceStartup = Time.realtimeSinceStartup;
        float engineAudioAnimCurve = engineCurve.Evaluate(EngineRPM / engineIntensityPercentage);
        float ignNum = 1f;
        if (ignitionStarted) ignNum = 1.5f;
        float highestAudio1;
        float highestAudio2;
        float wheelSpeed = Mathf.Abs(wheelRPM);
        float highestTyre = Mathf.Clamp(wheelSpeed / (180f * 0.35f), 0f, 1f);
        float heatSpeed = heaterSpeed/4f;
        carEngine2AudioActive = ignitionStarted;
        carRollingAudioActive = !allWheelsAirborne && wheelSpeed > 10f;
        engineThrottleVolume = Mathf.Lerp(engineThrottleVolume, drivePedalPressed ? 1f : 0.18f, 8f * Time.deltaTime);
        if (vanVariant == VanVariant.Creasman)
        {
            float engineSpeed = (EngineRPM - MinEngineRPM) / (MaxEngineRPM - MinEngineRPM);
            highestAudio1 = ignitionStarted ? Mathf.Clamp(engineAudioAnimCurve, 0.7f, 1.5f) / 1.2f : 1f;
            highestAudio2 = Mathf.Clamp(engineAudioAnimCurve, 0.65f, 1.15f) * 1.35f;
            bool engine1Started = carEngine1AudioActive && ignitionStarted;
            float engine1Vol = engine1Started ? revVolumeCurve.Evaluate(engineSpeed) : 0.7f;
            float engine2Vol = carEngine2AudioActive ? idleVolumeCurve.Evaluate(engineSpeed) : 0.5f;
            SetVehicleAudioProperties(engineAudio1, carEngine1AudioActive, 0.7f, highestAudio1, 2f, useVolumeInsteadOfPitch: false, engine1Vol * engineThrottleVolume, 4f);
            SetVehicleAudioProperties(engineAudio2, carEngine2AudioActive, 0.65f * ignNum, highestAudio2, 3f, useVolumeInsteadOfPitch: false, engine2Vol, 4f);
        }
        else
        {
            highestAudio1 = ignitionStarted ? Mathf.Clamp(engineAudioAnimCurve, 0.65f, 1.15f) * 1.35f : 1f;
            highestAudio2 = Mathf.Clamp(engineAudioAnimCurve, 0.7f, 1.5f);
            SetVehicleAudioProperties(engineAudio1, carEngine1AudioActive, 0.65f * ignNum, highestAudio1, 2f, useVolumeInsteadOfPitch: false, 0.7f);
            SetVehicleAudioProperties(engineAudio2, carEngine2AudioActive, 0.7f, highestAudio2, 3f, useVolumeInsteadOfPitch: false, 0.5f);
        }
        SetVehicleAudioProperties(rollingAudio, carRollingAudioActive, 0f, highestTyre, 5f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(extremeStressAudio, underExtremeStress, 0.2f, 1f, 3f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(heaterAudio, heaterOn && ignitionStarted, 0f, heatSpeed, 3f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(backUpBeeperAudio, ignitionStarted && inReverse, 0f, 1f, 4f, useVolumeInsteadOfPitch: true);
        SetRadioValues();
        if (engineAudio1.volume > 0.3f && engineAudio1.isPlaying && timeSinceStartup - timeAtLastEngineAudioPing > 2f)
        {
            timeAtLastEngineAudioPing = timeSinceStartup;
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
        SetVehicleAudioProperties(roofRainAudio, roofRainAudioActive, 0, 1f, 3f, useVolumeInsteadOfPitch: true);
        roofRainAudio.spatialBlend = Mathf.MoveTowards(roofRainAudio.spatialBlend, roofRainAudioActive ? 0f : 1f, 4f * Time.deltaTime);
        if (lastClutchPedalPressed != clutchPedalPressed)
        {
            lastClutchPedalPressed = clutchPedalPressed;
            if (lastClutchPedalPressed)
            {
                int clip = UnityEngine.Random.Range(0, clutchInClips.Length);
                currentDriver?.movementAudio.PlayOneShot(clutchInClips[clip]);
            }
            else
            {
                int clip = UnityEngine.Random.Range(0, clutchOutClips.Length);
                currentDriver?.movementAudio.PlayOneShot(clutchOutClips[clip]);
            }
        }
        if (voiceModule.voiceAudio.isPlaying && timeSinceStartup - timeAtLastEVAPing > 1f)
        {
            timeAtLastEVAPing = timeSinceStartup;
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
        if (vanAlarmCoroutine != null)
        {
            if (timeSinceStartup - timeAtLastAlarmPing > 2f)
            {
                timeAtLastAlarmPing = timeSinceStartup;
                RoundManager.Instance.PlayAudibleNoise(hornAudio.transform.position, 30f, 0.91f, 0, noiseIsInsideClosedShip: false, 106217);
            }
            return;
        }
        if (honkingHorn)
        {
            hornAudio.pitch = 1f;
            if (!hornAudio.isPlaying)
                hornAudio.Play();
            if (timeSinceStartup - timeAtLastHornPing > 2f)
            {
                timeAtLastHornPing = timeSinceStartup;
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
    public void SetCarTyreEffects()
    {
        const float REQUIRED_VELOCITY = 8f;
        if (IsOwner)
        {
            float skiddingHighest = Vector3.Dot(Vector3.Normalize(mainRigidbody.velocity * 1000f), transform.forward);
            float wheelSpeed = Mathf.Abs(backWheelRPM);
            bool skiddingAudioActive = skiddingHighest > -0.6f && skiddingHighest < 0.4f && (averageVelocity.magnitude > 4f || wheelSpeed > 400f);
            if (backWheelsGrounded)
            {
                const float SLIP_TORQUE = 600f;
                const float MAX_TYRE_SLIP = 0.245f;
                const float SLIP_SPEED = 350f;
                bool forwardSlipping = currentMotorTorque > SLIP_TORQUE && Mathf.Abs(forwardsSlip) > MAX_TYRE_SLIP;
                if (forwardSlipping && wheelSpeed > SLIP_SPEED)
                {
                    skiddingHighest = Mathf.Max(skiddingHighest, 0.8f);
                    skiddingAudioActive = true;
                    if (averageVelocity.magnitude > REQUIRED_VELOCITY && !tireSparks.isPlaying)
                    {
                        tireSparks.Play(true);
                    }
                }
                else
                {
                    if (tireSparks.isEmitting)
                    {
                        tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                }
            }
            else
            {
                skiddingAudioActive = false;
                if (tireSparks.isEmitting)
                {
                    tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
            bool sparksEmitting = tireSparks.isEmitting;
            if (Mathf.Abs(tyreStress - skiddingHighest) > 0.02f ||
                wheelSlipping != skiddingAudioActive ||
                wheelSkidActive != sparksEmitting)
            {
                tyreStress = skiddingHighest;
                wheelSlipping = skiddingAudioActive;
                wheelSkidActive = sparksEmitting;
                SetTyreStressRpc(skiddingHighest, skiddingAudioActive, sparksEmitting);
            }
            SetVehicleAudioProperties(skiddingAudio, skiddingAudioActive, 0f, skiddingHighest, 3f, true, 1f);
            return;
        }
        if (wheelSkidActive && !tireSparks.isPlaying)
            tireSparks.Play(true);
        else if (!wheelSkidActive && tireSparks.isEmitting)
            tireSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        SetVehicleAudioProperties(skiddingAudio, wheelSlipping, 0f, tyreStress, 3f, true, 1f);
    }

    public void SetCarIgnitionKey(bool localUseBodyHands = false)
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
            bool useLocalHands = localPlayerInControl && !localUseBodyHands;

            keyParent = useLocalHands
                ? currentDriver.localItemHolder
                : currentDriver.serverItemHolder;

            posOffset = useLocalHands ? LHD_Pos_Local : LHD_Pos_Server;
            rotOffset = useLocalHands ? LHD_Rot_Local : LHD_Rot_Server;

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

        TrySetMileage();

        UpdateCarDrivetrain();
        UpdateCarEngine();

        SyncDrivetrain();
        SyncWheelTorque();

        /*
        SetSteeringDecay();
        */

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


    private void TrySetMileage()
    {
        if (vanVariant != VanVariant.Creasman || magnetedToShip)
            return;
        const float METERS_IN_MILES = 1609.344f;
        const float RPM_TO_RPS = 60f;
        float wheelRadius = (BackLeftWheel.radius + BackRightWheel.radius) / 2f;
        float wheelCircumference = 2f * Mathf.PI * wheelRadius;
        float backWheelSpeed = (BackLeftWheel.rpm + BackRightWheel.rpm) / 2f;
        float distancePerSecond = Mathf.Abs(backWheelSpeed) / RPM_TO_RPS * wheelCircumference;
        odometerValue += distancePerSecond * Time.deltaTime / METERS_IN_MILES;
    }

    private void SetCarPhysicsValuesOnClient()
    {
        currentMotorTorque = syncedCurrentMotorTorque;
        currentBrakeTorque = syncedCurrentBrakeTorque;

        float targetRpm = ignitionStarted ? syncedEngineRPM : 0f;
        EngineRPM = Mathf.Lerp(syncedEngineRPM, targetRpm, 3f * Time.fixedDeltaTime);

        //steeringDecay = 1f;
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
        if (IsInShipPhaseAndNotLoadingLevel() ||
            loadedVehicleFromSave ||
            hasDeliveredVehicle)
            return;

        if (itemShip == null && ScandalsTweaks.Utils.References.itemShip != null)
            itemShip = ScandalsTweaks.Utils.References.itemShip;

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
            timeSinceUntethered = Time.realtimeSinceStartup;
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
        Plugin.LogDebug($"Set 'mainRigidbody' kinematic to: {setKinematic}.");
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

        // 256 is room layer
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
        float absFloat = Mathf.Abs(steeringWheelAnimFloat);
        float signFloat = Mathf.Sign(steeringWheelAnimFloat);
        const float MAX_TURNING_ANGLE = 50f;
        steeringAngle = steeringWheelCurve.Evaluate(absFloat) * MAX_TURNING_ANGLE * signFloat;
        FrontLeftWheel.steerAngle = steeringAngle;
        FrontRightWheel.steerAngle = steeringAngle;
    }

    /*
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
    */

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

    [Rpc(SendTo.NotOwner)]
    public void SyncWheelTorqueRpc(float motorTorque, float brakeTorque)
    {
        syncedCurrentMotorTorque = motorTorque;
        syncedCurrentBrakeTorque = brakeTorque;
    }


    // ??? no idea what is going on here, i need to revisit this.
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
                syncedFrontWheelRPM = frontWheelRPM;
                syncedBackWheelRPM = backWheelRPM;
                syncedWheelRPM = wheelSyncRPM;
                syncedEngineRPM = engineSpeed;
                SyncDrivetrainRpc(frontWheelRPM, backWheelRPM, wheelSyncRPM, engineSpeed, odometerValue);
                return;
            }
        }
        else
        {
            syncDrivetrainInterval += Time.fixedDeltaTime;
        }
    }

    [Rpc(SendTo.NotOwner)]
    public void SyncDrivetrainRpc(float frontWheelSpeed, float backWheelSpeed, float wheelSpeed, float engineSpeed, float odometerMileage)
    {
        syncedFrontWheelRPM = frontWheelSpeed;
        syncedBackWheelRPM = backWheelSpeed;
        syncedWheelRPM = wheelSpeed;
        syncedEngineRPM = engineSpeed;
        odometerValue = odometerMileage;
    }


    // --- ENGINE ---
    private void UpdateCarEngine()
    {
        if (carDestroyed)
        {
            return;
        }
        if (!ignitionStarted)
        {
            enginePower = 0f;
            stallTimer = 0f;
            EngineRPM = Mathf.Lerp(EngineRPM, 0f, 3f * Time.fixedDeltaTime);
            return;
        }

        float selectedGear = Mathf.Abs(gearRatios[currentGear]);
        float wheelSpeed = Mathf.Abs(backWheelRPM) * selectedGear * diffRatio;

        // EngineRPM / MaxEngineRPM
        float engineSpeed = (EngineRPM - MinEngineRPM) / (MaxEngineRPM - MinEngineRPM);

        AnimationCurve powerCurve = vanVariant == VanVariant.Creasman ? creasmanEnginePowerCurve : enginePowerCurve;
        enginePower = powerCurve.Evaluate(engineSpeed) *
          EngineTorque * (selectedGear * diffRatio);

        if (inNeutral || clutchEngagement == 0f)
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

        if (engineLugging || engineOverloaded)
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
        if (vanVariant == VanVariant.Creasman)
        {
            engineAudio3.PlayOneShot(creasmanEngineStall);
        }
        else
        {
            engineAudio3.PlayOneShot(stallEngine);
        }
        if (currentDriver != null)
        {
            currentDriver.playerBodyAnimator.SetInteger(CAR_ANIM, SITTING_IN_SEAT_HANDS_ON_WHEEL);
        }
        ignitionAnimator.SetInteger(IGNITION_ANIM, UNTWIST_IGNITION_KEY_IN);
        chanceToStartIgnition = 101f;
        usingSwitchIgnition = false;
        SetIgnition(false, true);
        SetIgnitionInteractTrigger();
    }

    [Rpc(SendTo.NotOwner)]
    public void StallEngineRpc()
    {
        engineStalled = true;
        stallTimer = 0f;
        StallEngine();
    }


    // --- MISC SYNC METHODS ---
    public void SyncEffectsToOtherClients()
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

    [Rpc(SendTo.NotOwner)]
    public void SyncCarEffectsRpc(float wheelRotation, bool smoothRot)
    {
        syncedSteeringWheelRotation = wheelRotation;
        smoothRotation = smoothRot;
    }

    [Rpc(SendTo.NotOwner)]
    public void SyncPlayerInputsRpc(bool gasPressed, bool brakePressed, bool clutchPressed)
    {
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

    [Rpc(SendTo.NotOwner)]
    public void SyncCarPositionRpc(Vector3 carPosition, Vector3 carRotation)
    {
        syncedPosition = carPosition;
        syncedRotation = Quaternion.Euler(carRotation);
    }

    [Rpc(SendTo.NotOwner)]
    public void SetTyreStressRpc(float wheelStress, bool wheelSkidding, bool sparksEmitting)
    {
        tyreStress = wheelStress;
        wheelSlipping = wheelSkidding;
        wheelSkidActive = sparksEmitting;
    }


    // --- SCAN NODE MISC ---
    public void UpdateScanNodeText()
    {
        if (carDestroyed)
        {
            scanNode.headerText = "Destroyed Van";
            scanNode.subText = "Your once trusty mule..";
            return;
        }
        if (GameNetworkManager.Instance.connectedPlayers == 1) scanNode.subText = "Your only trusty mule..";
        else scanNode.subText = "Your trusty mule!";
    }


    // --- LATE UPDATE METHOD ---
    public new void LateUpdate()
    {
        if (currentDriver != null && lastDriver != currentDriver && !magnetedToShip)
            lastDriver = currentDriver;
        if (carDestroyed)
            return;
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
                CarBump(averageVelocity * 0.7f);
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
                    bool isJensonVariant = vanVariant == VanVariant.JCJenson;
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
                            CarBump(averageVelocity);
                            mainRigidbody.velocity = Vector3.Normalize(-impulse * 100000000f) * 9f;
                            PlayerControllerB playerController;
                            if (currentDriver != null)
                            {
                                playerController = currentDriver;
                            }
                            else
                            {
                                if (currentMiddlePassenger != null)
                                    playerController = currentMiddlePassenger;
                                else if (currentPassenger != null)
                                    playerController = currentPassenger;
                                else
                                    playerController = null!;
                            }

                            if (vel.magnitude > 2f && dealDamage)
                            {
                                enemyScript.HitEnemyOnLocalClient(2, Vector3.zero, playerController, playHitSFX: true, 333);
                            }
                            result = true;
                            if ((isJensonVariant && obstacleSize > 1f) || (!isJensonVariant && obstacleSize > 2f))
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
                            DealPermanentDamage(isJensonVariant ? 1 : 2, position);
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
                        enemyScript.HitEnemyOnLocalClient(20, Vector3.zero, playerWhoHit, playHitSFX: true, 333);
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

        float timeSinceStartup = Time.realtimeSinceStartup;
        if (timeSinceStartup - timeSinceLastCollision < 0.05f ||
            timeSinceStartup - timeSinceUntethered < 4f)
            return;

        timeSinceLastCollision = timeSinceStartup;

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

        float collisionVolume = 0.5f;
        int audioType = -1;

        if (differenceInVelocity >= 11f && lastVelocity.magnitude > 38f)
        {
            if (carHP < 3)
            {
                timeAtLastDamage = timeSinceStartup;
                syncedCarHP = carHP;
                DestroyCarRpc(carHP);
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
        CarBump(Vector3.ClampMagnitude(-collision.relativeVelocity, 40f));
        
    }

    public bool IsPlayerSeatedInCar()
    {
        return localPlayerInControl ||
               localPlayerInMiddlePassengerSeat ||
               localPlayerInPassengerSeat;
    }

    public void CarBump(Vector3 vel)
    {
        CarBumpRpc(vel);
        CarBumpLocalClient(vel);
    }

    public void DamageVehicle(float diff, Vector3 collision, int damageAmount = 1)
    {
        DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision, 60f), diff / 1.5f);
        CarCollisionRpc(Vector3.ClampMagnitude(-collision, 60f), diff / 1.5f);
        if (damageAmount != 0) DealPermanentDamage(damageAmount);        
    }

    public void CarBumpLocalClient(Vector3 vel)
    {
        if (IsPlayerSeatedInCar())
        {
            return;
        }
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!VehicleUtils.IsPlayerInVanBounds(playerController, this))
        {
            return; 
        }
        if (VehicleUtils.IsPlayerInVanCabin(playerController, this)) vel = Vector3.ClampMagnitude(vel, 5);
        else vel = Vector3.ClampMagnitude(vel, 30);
        GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
    }

    [Rpc(SendTo.NotOwner)]
    public void CarBumpRpc(Vector3 vel)
    {
        CarBumpLocalClient(vel);
    }

    [Rpc(SendTo.NotOwner)]
    public void CarCollisionRpc(Vector3 vel, float magn)
    {
        DamagePlayerInVehicle(vel, magn);
    }

    public new void DamagePlayerInVehicle(Vector3 vel, float magnitude)
    {
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!IsPlayerSeatedInCar())
        {
            if (!VehicleUtils.IsPlayerInVanBounds(playerController, this))
            {
                return;
            }
            if (playerController.health < 15)
            {
                playerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f, false);
                return;
            }
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            playerController.DamagePlayer(5, true, true, CauseOfDeath.Inertia, 0, false, vel);
            playerController.externalForceAutoFade += vel;
            return;
        }
        if (magnitude > 27f)
        {
            if (playerController.health < 10)
            {
                playerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f, false);
                return;
            }
            playerController.DamagePlayer((int)vel.magnitude, true, true, CauseOfDeath.Inertia, 0, false, vel);
            return;
        }
        if (magnitude <= 16f && magnitude >= 4f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            playerController.DamagePlayer(5, true, true, CauseOfDeath.Inertia, 0, false, vel);
            return;
        }
        HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
        if (playerController.health < 10)
        {
            playerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f, false);
            return;
        }
        playerController.DamagePlayer(10, true, true, CauseOfDeath.Inertia, 0, false, vel);
    }

    [Rpc(SendTo.NotOwner)]
    public void BreakWindshieldRpc()
    {
        BreakWindshield();
    }

    public new void BreakWindshield()
    {
        if (windshieldBroken)
        {
            return;
        }
        windshieldBroken = true;
        windshieldObject.SetActive(false);
        glassParticle.Play();
        miscAudio.transform.localPosition = windshieldObject.transform.localPosition;
        miscAudio.PlayOneShot(windshieldBreak);
    }

    private void RegenerateWindshield()
    {
        if (!windshieldBroken)
        {
            return;
        }
        windshieldBroken = false;
        windshieldObject.SetActive(true);
    }

    public new void PlayCollisionAudio(Vector3 setPosition, int audioType, float setVolume)
    {
        float timeSinceStartup = Time.realtimeSinceStartup;
        bool audioTime;
        if (timeSinceStartup - audio1Time > timeSinceStartup - audio2Time)
        {
            audioTime = timeSinceStartup - audio1Time >= collisionAudio1.clip.length * 0.8f;
            if (audio1Type <= audioType || audioTime)
            {
                audio1Time = timeSinceStartup;
                audio1Type = audioType;
                collisionAudio1.transform.position = setPosition;
                CarCollisionSFXRpc(collisionAudio1.transform.localPosition, 0, audioType, audioTime, setVolume);
                PlayRandomClipAndPropertiesFromAudio(collisionAudio1, setVolume, audioTime, audioType);
            }
        }
        else
        {
            audioTime = timeSinceStartup - audio2Time >= collisionAudio2.clip.length * 0.8f;
            if (audio2Type <= audioType || audioTime)
            {
                audio2Time = timeSinceStartup;
                audio2Type = audioType;
                collisionAudio2.transform.position = setPosition;
                CarCollisionSFXRpc(collisionAudio2.transform.localPosition, 1, audioType, audioTime, setVolume);
                PlayRandomClipAndPropertiesFromAudio(collisionAudio2, setVolume, audioTime, audioType);
            }
        }
    }

    [Rpc(SendTo.NotOwner)]
    public void CarCollisionSFXRpc(Vector3 audioPosition, int audio, int audioType, bool audioTime, float vol)
    {
        float timeSinceStartup = Time.realtimeSinceStartup;
        AudioSource audioSource;
        //bool audioTime;
        if (audio != 0)
        {
            audio2Time = timeSinceStartup;
            audio2Type = audioType;
            audioSource = collisionAudio2;
            //audioTime = timeSinceStartup - audio2Time >= audioSource.clip.length * 0.8f;
        }
        else
        {
            audio1Time = timeSinceStartup;
            audio1Type = audioType;
            audioSource = collisionAudio1;
            //audioTime = timeSinceStartup - audio1Time >= audioSource.clip.length * 0.8f;
        }
        audioSource.transform.localPosition = audioPosition;
        PlayRandomClipAndPropertiesFromAudio(audioSource, vol, audioTime, audioType);
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

    public new void SetInternalStress(float carStressIncrease = 0f)
    {
        if (!IsOwner || magnetedToShip || carDestroyed)
        {
            return;
        }

        if (carStressIncrease <= 0f) carStressChange = Mathf.Clamp(carStressChange - Time.fixedDeltaTime, -0.25f, 0.5f);
        else carStressChange = Mathf.Clamp(carStressChange + Time.fixedDeltaTime * carStressIncrease, 0f, 10f);

        underExtremeStress = (carStressIncrease >= 1f);
        carStress = Mathf.Clamp(carStress + carStressChange, 0f, 100f);

        if (carStress > 7f)
        {
            carStress = 0f;
            DealPermanentDamage(2, default(Vector3));
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
            DestroyCarRpc(carHP);
            DestroyCar();
            return;
        }
        DealDamageRpc(carHP);
    }

    [Rpc(SendTo.NotOwner)]
    public void DealDamageRpc(int setHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP = setHealth;
        syncedCarHP = carHP;
    }


    // --- DESTRUCTION ---
    [Rpc(SendTo.NotOwner)]
    public void DestroyCarRpc(int setHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP = setHealth;
        syncedCarHP = carHP;
        DestroyCar();
    }

    public new void DestroyCar()
    {
        if (carDestroyed)
            return;

        carDestroyed = true;
        UnMagnetCar();
        StopAudiosPlayback();
        RemoveRainCollision();
        CollectItemsInVan();
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
        CollectItemsInVan();
    }

    private void StopAudiosPlayback()
    {
        underExtremeStress = false;
        engineAudio1.Stop();
        engineAudio2.Stop();
        engineAudio3.Stop();
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
        hazardsOn = false;
        roofRainAudio.Stop();
    }
    private void StopParticleVFX()
    {
        tireSparks.Stop();
        turboBoostParticle.Stop();
    }

    private void SetMaterials()
    {
        leftBlinkerMesh.material = blinkerOffMat;
        leftBlinkerMeshLod.material = blinkerOffMat;
        rightBlinkerMesh.material = blinkerOffMat;
        rightBlinkerMeshLod.material = blinkerOffMat;
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
    }

    private void KillOccupants()
    {
        if (!IsPlayerSeatedInCar())
            return;
        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        playerController.KillPlayer(Vector3.up * 27f + 20f * UnityEngine.Random.insideUnitSphere, spawnBody: true, CauseOfDeath.Blast, 6, Vector3.up * 1.5f, false);
    }

    private void SetInteractions()
    {
        InteractTrigger[] interactTriggers = gameObject.GetComponentsInChildren<InteractTrigger>();
        for (int i = 0; i < interactTriggers.Length; i++)
        {
            if (interactTriggers[i].isLadder)
                continue;
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
    public void RemoveRainCollision()
    {
        var particleTriggers = new[]
        {
            ScandalsTweaks.Utils.References.rainParticles,
            ScandalsTweaks.Utils.References.rainHitParticles,
            ScandalsTweaks.Utils.References.stormyRainParticles,
            ScandalsTweaks.Utils.References.stormyRainHitParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneRainParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneRainHitParticles,
            ScandalsTweaks.Utils.References.wesleyHurricaneSandParticles,
            ScandalsTweaks.Utils.References.wesleyForsakenRainParticles,
            ScandalsTweaks.Utils.References.wesleyForsakenRainHitParticles,
            ScandalsTweaks.Utils.References.kenjiAcidRainParticles,
            ScandalsTweaks.Utils.References.kenjiAcidRainHitParticles,
            ScandalsTweaks.Utils.References.kenjiAcidStormyRainParticles,
            ScandalsTweaks.Utils.References.kenjiAcidStormyRainHitParticles
        };

        foreach (var particle in particleTriggers)
        {
            if (particle == null)
            {
                Plugin.LogDebug("Weather particle or Trigger is null.");
                continue;
            }

            var trigger = particle.trigger;
            for (int j = trigger.colliderCount - 1; j >= 0; j--)
            {
                var collider = trigger.GetCollider(j);
                if (collider != null && weatherEffectBlockers.Contains(collider))
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
            Mathf.Clamp(((float)carHP + transmissionHP) / ((float)baseCarHP + baseTransmissionHP), 0.01f, 1f),
            6f * Time.deltaTime));
        turboMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            turboMeter.localScale.z,
            Mathf.Clamp(turboBoosts / 4f, 0.01f, 1f),
            6f * Time.deltaTime));

        if (transmissionHP <= 12)
        {
            clutchFriction = transmissionHP / 15f;
            clutchFriction = Mathf.Clamp(clutchFriction, 0.4f, 1f);
        }
        else
        {
            clutchFriction = 1f;
        }

        if (!IsOwner)
        {
            return;
        }
        float timeSinceStartup = Time.realtimeSinceStartup;
        if (carHP < 9 && timeSinceStartup - timeAtLastDamage > 8f)
        {
            timeAtLastDamage = timeSinceStartup;
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
    [Rpc(SendTo.NotOwner)]
    private void SyncCarHealthRpc(int carHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        syncedCarHP = carHealth;
        carHP = syncedCarHP;
    }

    [Rpc(SendTo.NotOwner)]
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

    [Rpc(SendTo.NotOwner)]
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
        carHoodAnimator.SetBool(HOOD_ANIMATION_BOOL, carHoodOpen);
        if (carHoodOpen)
        {
            hoodTrigger.holdTip = HOOD_CLOSING_HOLDTIP;
        }
        else
        {
            hoodTrigger.holdTip = HOOD_OPENING_HOLDTIP;
        }
        SetHoodOpenRpc(open: carHoodOpen);
    }

    // used for when the hood is 'on fire'
    public new void SetHoodOpenLocalClient(bool setOpen)
    {
        if (carHoodOpen && carHoodOpen == setOpen)
            return;

        carHoodOpen = setOpen;
        carHoodAnimator.SetBool(HOOD_ANIMATION_BOOL, setOpen);
        if (setOpen)
        {
            hoodTrigger.holdTip = HOOD_CLOSING_HOLDTIP;
        }
        else
        {
            hoodTrigger.holdTip = HOOD_OPENING_HOLDTIP;
        }
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHoodOpenRpc(bool open)
    {
        carHoodOpen = open;
        carHoodAnimator.SetBool(HOOD_ANIMATION_BOOL, carHoodOpen);
        if (open)
        {
            hoodTrigger.holdTip = HOOD_CLOSING_HOLDTIP;
        }
        else
        {
            hoodTrigger.holdTip = HOOD_OPENING_HOLDTIP;
        }
    }


    // --- PUSH METHODS ---
    public new void PushTruckWithArms()
    {
        if (magnetedToShip)
            return;

        PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
        if (!Physics.Raycast(
            playerController.gameplayCamera.transform.position,
            playerController.gameplayCamera.transform.forward,
            out hit,
            10f,
            1073742656,
            QueryTriggerInteraction.Ignore))
            return;

        if (playerController.overridePhysicsParent != null)
            return;

        if (playerController.physicsParent == physicsRegion.physicsTransform)
            return;

        Vector3 point = hit.point;
        Vector3 forward = playerController.gameplayCamera.transform.forward;

        tempPushTransform.position = point;
        Vector3 hitPoint = tempPushTransform.localPosition;

        int clip = UnityEngine.Random.Range(0, minCollisions.Length);
        float turbulence = Mathf.Min(turbulenceAmount + 0.5f, 2f);

        if (IsOwner)
        {
            PlayPushAudio(hitPoint, clip, turbulence);
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(forward * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, tempPushTransform.position - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
            PushTruckFromOwnerRpc(hitPoint, clip, turbulence);
            return;
        }
        PushTruckRpc(hitPoint, forward, clip, turbulence);
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void PushTruckRpc(Vector3 pushPosition, Vector3 dir, int clip, float turbulence)
    {
        PlayPushAudio(pushPosition, clip, turbulence);
        if (IsOwner)
        {
            tempPushTransform.localPosition = pushPosition;
            mainRigidbody.AddForceAtPosition(Vector3.Normalize(dir * 1000f) * UnityEngine.Random.Range(40f, 50f) * pushForceMultiplier, tempPushTransform.position - mainRigidbody.transform.up * pushVerticalOffsetAmount, ForceMode.Impulse);
        }
    }

    [Rpc(SendTo.NotOwner)]
    public void PushTruckFromOwnerRpc(Vector3 pushPosition, int clip, float turbulence)
    {
        PlayPushAudio(pushPosition, clip, turbulence);
    }

    public void PlayPushAudio(Vector3 pos, int clip, float turbulence)
    {
        pushAudio.clip = minCollisions[clip];
        pushAudio.transform.localPosition = pos;
        pushAudio.Play();
        turbulenceAmount = turbulence;
    }

    // --- HAZARD WARNING LAMPS ---
    private IEnumerator FlashBlinkerLights()
    {
        while (hazardsOn)
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

            leftBlinkerMesh.material = blinkerOffMat;
            leftBlinkerMeshLod.material = blinkerOffMat;
            rightBlinkerMesh.material = blinkerOffMat;
            rightBlinkerMeshLod.material = blinkerOffMat;

            SetSymbolActive(leftSignalSymbol, leftSignalLight, false);
            SetSymbolActive(hazardWarningSymbol, hazardWarningLight, false);
            SetSymbolActive(rightSignalSymbol, rightSignalLight, false);

            blinkerLightsContainer.SetActive(false);
        }
    }

    public void SetHazardLightsOnLocalClient()
    {
        hazardsOn = !hazardsOn;
        if (vanAlarmCoroutine == null)
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

                leftBlinkerMesh.material = blinkerOffMat;
                leftBlinkerMeshLod.material = blinkerOffMat;
                rightBlinkerMesh.material = blinkerOffMat;
                rightBlinkerMeshLod.material = blinkerOffMat;

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
        if (vanAlarmCoroutine == null)
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

                leftBlinkerMesh.material = blinkerOffMat;
                leftBlinkerMeshLod.material = blinkerOffMat;
                rightBlinkerMesh.material = blinkerOffMat;
                rightBlinkerMeshLod.material = blinkerOffMat;

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
        if (hasFogLamps)
        {
            creasmanFogBeamContainer.SetActive(highBeamsOn);
        }
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
        if (hasFogLamps)
        {
            creasmanFogBeamContainer.SetActive(highBeamsOn);
        }
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
        leftWindowMats[2] = lowOn ? windowOnMaterial : windowOffMaterial;
        leftDoorMesh.sharedMaterials = leftWindowMats;

        Material[] rightWindowMats = rightDoorMesh.sharedMaterials;
        rightWindowMats[2] = lowOn ? windowOnMaterial : windowOffMaterial;
        rightDoorMesh.sharedMaterials = rightWindowMats;

        radioMesh.material = lowOn ? radioOnMaterial : radioOffMaterial;
        radioPowerDial.material = lowOn ? radioOnMaterial : radioOffMaterial;
        radioVolumeDial.material = lowOn ? radioOnMaterial : radioOffMaterial;

        lowBeamMesh.material = lowOn ? headlightsOnMat : headlightsOffMat;
        lowBeamMeshLod.material = lowOn ? headlightsOnMat : headlightsOffMat;
        highBeamMesh.material = highOn ? headlightsOnMat : headlightsOffMat;
        highBeamMeshLod.material = highOn ? headlightsOnMat : headlightsOffMat;
        creasmanFogLampsMesh.material = highOn ? headlightsOnMat : headlightsOffMat;
        sideTopLightsMesh.material = lowOn ? backLightOnMat : redLightOffMat;
        sideTopLightsMeshLod.material = lowOn ? backLightOnMat : redLightOffMat;
    }
}
