using System;
using System.Collections;
using System.Collections.Generic;
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
using CruiserXL.Compatibility;
using CruiserXL.Networking;
using UnityEngine.InputSystem.XR;

public class CruiserXLController : VehicleController
{
    [Header("Modules")]
    public EVAModule voiceModule = null!;
    public EngineModule engineModule = null!;
    public DrivetrainModule drivetrainModule = null!;

    [Header("Variation")]
    public Material truckMat = null!;

    public Material rareTruckDialsOn = null!;
    public Material rareTruckClusterOn = null!;
    public Material rareTruckRadioOn = null!;
    public Material rareHeaterOn = null!;

    public Texture2D defaultTruckTex = null!;
    public Texture2D rareTruckTex = null!;

    public Light radioLightCol = null!; // special hex #FAFEAA, default hex #D6FFCE // FILTER COLOR
    public Light heaterLightCol = null!;
    public Light clusterLightCol = null!; // special hex #FAFEDE, default hex #C9FFFA // FILTER COLOR

    public float specialChance;
    public bool isSpecial;

    public GameObject jcJensonSymbolObj = null!;
    public SpriteRenderer jcJensonSymbol = null!;

    [Header("Vehicle Physics")]
    public List<WheelCollider> wheels = null!;
    public AnimationCurve steeringWheelCurve = null!;
    public NavMeshObstacle navObstacle = null!;
    public CapsuleCollider cabinPoint = null!;
    public CruiserXLCollisionTrigger collisionTrigger = null!;
    public Rigidbody playerPhysicsBody = null!;

    private WheelHit[] wheelHits = new WheelHit[4];
    public Vector3 lastVelocity;
    public Vector3 previousVehiclePosition;

    private int syncedCarHP;
    private float timeSinceLastCollision;
    private float steeringDecay = 1f;
    private float sidewaysSlip;
    private float forwardsSlip;
    public float baseForwardStiffness = 1f;
    public float baseSidewaysStiffness = 0.75f;
    public float wheelTorque;
    public float wheelBrakeTorque;
    public bool hasDeliveredVehicle;
    public float maxBrakingPower;
    public float baseSteeringWheelTurnSpeed;

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

    public bool localPlayerInMiddlePassengerSeat;
    private float syncCarEffectsInterval;
    public float syncedWheelRotation;
    private bool syncedDrivePedalPressed;
    private bool syncedBrakePedalPressed;
    private float tyreStress;
    private bool wheelSlipping;

    [Header("Effects")]
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

    public ScanNodeProperties scanNode = null!;

    public GameObject blinkerLightsContainer = null!;

    public GameObject sideDoorContainer = null!;

    public GameObject leftElectricMirror = null!;
    public GameObject rightElectricMirror = null!;

    public Coroutine dashboardSymbolPreStartup = null!;

    public SpriteRenderer hazardWarningSymbol = null!;
    public SpriteRenderer leftSignalSymbol = null!;
    public SpriteRenderer rightSignalSymbol = null!;

    public SpriteRenderer parkingBrakeSymbol = null!;
    public SpriteRenderer checkEngineLightSymbol = null!;
    public SpriteRenderer alertLightSymbol = null!;
    public SpriteRenderer seatbeltLightSymbol = null!;
    public SpriteRenderer oilLevelLightSymbol = null!;
    public SpriteRenderer batteryLightSymbol = null!;
    public SpriteRenderer coolantLevelLightSymbol = null!;
    public SpriteRenderer dippedBeamLightSymbol = null!;
    public SpriteRenderer highBeamLightSymbol = null!;

    public GameObject carKeyContainer = null!;
    public GameObject carKeyInHand = null!;
    public GameObject ignitionBarrel = null!;

    public Transform ignitionBarrelNotTurnedPosition = null!;
    public Transform ignitionBarrelTurnedPosition = null!;
    public Transform ignitionBarrelTryingPos = null!;
    public Transform ignitionTryingPosition = null!;

    public GameObject headlightSwitch = null!;
    public MeshRenderer lowBeamMesh = null!;
    public MeshRenderer highBeamMesh = null!;
    public GameObject highBeamContainer = null!;
    public GameObject clusterLightsContainer = null!;

    public MeshRenderer radioMesh = null!;
    public MeshRenderer radioPowerDial = null!;
    public MeshRenderer radioVolumeDial = null!;
    public GameObject radioLight = null!;
    public GameObject heaterLight = null!;

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

    public GameObject driverCameraNode = null!;
    public GameObject passengerCameraNode = null!;
    public GameObject driverCameraPositionNode = null!;
    public GameObject passengerCameraPositionNode = null!;

    private Coroutine blinkersCoroutine = null!;

    public InteractTrigger startIgnitionTrigger = null!;
    public InteractTrigger stopIgnitionTrigger = null!;

    public GameObject reverseLightsContainer = null!;
    public MeshRenderer reverseLightsMesh = null!;

    public Quaternion lightSwitchPos;
    public Quaternion heatPosition;
    public Quaternion heatSpeedRot;
    public Quaternion heatDirection;

    public bool heaterOn;
    public float heaterTemp;
    public float heaterSpeed;

    public bool windshieldShattered;

    private bool inIgnitionAnimation;

    public bool isHeaterCold;
    public bool isHeaterWarm;

    private Vector3 LHD_Pos_Local = new Vector3(0.0489f, 0.1371f, -0.1566f);
    private Vector3 LHD_Pos_Server = new Vector3(0.0366f, 0.1023f, -0.1088f);
    private Vector3 LHD_Rot_Local = new Vector3(-3.446f, 3.193f, 172.642f);
    private Vector3 LHD_Rot_Server = new Vector3(-191.643f, 174.051f, -7.768005f);

    public float ignitionRotSpeed = 45f;

    private int currentSweepStage;
    private bool hasSweepedDashboard;
    public bool hazardsBlinking;
    private bool hazardsOn;
    private bool reverseLightsOn;

    private float speedometerFloat;
    private float tachometerFloat;

    private bool lowBeamsOn;
    private bool highBeamsOn;
    private bool cabinLightSwitchEnabled;

    private float oilPressureFloat;
    private float turboPressureFloat;
    private bool overdriveSwitchEnabled;

    private bool correctedPosition;
    public bool tryingIgnition;

    public bool liftGateOpen;
    public bool sideDoorOpen;

    [Header("Audio")]
    public AudioSource[] allVehicleAudios = null!;
    public AudioClip[] streamerRadioClips = null!;

    public AudioSource cabinLightSwitchAudio = null!;
    public AudioClip cabinLightSwitchToggle = null!;

    public AudioClip blinkOn = null!;
    public AudioClip blinkOff = null!;

    public AudioSource carKeySounds = null!;
    public AudioSource wiperAudio = null!;

    private Coroutine truckAlarmCoroutine = null!;
    public AudioSource alarmAudio = null!;
    public AudioSource heaterAudio = null!;

    private float timeSinceTogglingRadio;
    public bool alarmDebounce;
    private float timeAtLastAlarmPing;
    private float timeAtLastEVAPing;

    [Header("Radio")]
    public RadioBehaviour liveRadioController = null!;

    private float lastSongTime;
    public float minFrequency = 75.55f;
    public float maxFrequency = 255.50f;

    private bool isFmRadio = false;

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

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void SetSpecialVariantRpc(bool special)
    {
        isSpecial = special;
        SetVariant(isSpecial);
    }

    public void SetVariant(bool isJenson)
    {
        if (isJenson)
        {
            truckMat.mainTexture = rareTruckTex;
            radioLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
            heaterLightCol.color = new Color32(0xFA, 0xFE, 0xAA, 0xFF); // #FAFEAA
            clusterLightCol.color = new Color32(0xFA, 0xFE, 0xDE, 0xFF); // #FAFEDE

            // mat swaps
            clusterDialsOnMat = rareTruckDialsOn; // #FCFFB6
            clusterOnMaterial = rareTruckClusterOn; // #FAFF92
            radioOnMaterial = rareTruckRadioOn; // #FAFE7D
            heaterOnMat = rareHeaterOn; // #FAFE7D
            jcJensonSymbolObj.SetActive(true);

            radioTime.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33
            radioFrequency.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33
            voiceModule.clusterScreen.color = new Color32(0xFF, 0xCC, 0x33, 0xFF); // #FFCC33

            voiceModule.clusterTexts[0] = "sys status: <ok>";
            voiceModule.clusterTexts[15] = "high temp!";
            voiceModule.clusterTexts[13] = "error: 606";
            return;
        }
        truckMat.mainTexture = defaultTruckTex;
    }


    public new void Awake()
    {
        FixCarAudios();
        base.Awake();
        windshieldMesh = windshieldObject.GetComponent<MeshRenderer>();
        playerPhysicsBody.transform.SetParent(RoundManager.Instance.VehiclesContainer);
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

        startIgnitionTrigger.holdTip = "[ Trying ignition ]";
        stopIgnitionTrigger.hoverTip = "Try ignition : [LMB]";

        stopIgnitionTrigger.hoverTip = "Remove key : [LMB]";
        stopIgnitionTrigger.holdTip = "[ No key! ]";

        backDoorOpen = true; // hacky shit
        SetTruckStats();
    }

    public void FixCarAudios()
    {
        // apply the vanilla SFX sound mixer so TZP effects will work on the vehicles audios
        if (References.diageticSFXGroup == null)
            return;

        foreach (var audio in allVehicleAudios)
        {
            audio.outputAudioMixerGroup = References.diageticSFXGroup;
        }
    }

    private void SetTruckStats()
    {
        idleSpeed = 80f;
        pushForceMultiplier = 162f;

        turboBoostForce = 13500f;
        turboBoostUpwardForce = 32400f;

        baseSteeringWheelTurnSpeed = 4.5f;
        steeringWheelTurnSpeed = baseSteeringWheelTurnSpeed;

        jumpForce = 3600f;

        brakeSpeed = 10000f;
        maxBrakingPower = 12000f;

        speed = 60;
        stability = 0.4f;

        heaterTemp = 1f;
        isHeaterWarm = true;

        torqueForce = 2.5f;
        carMaxSpeed = 60f;
        pushVerticalOffsetAmount = 1.25f;

        baseCarHP = 46;

        if (!StartOfRound.Instance.inShipPhase)
        {
            carHP = baseCarHP;
        }

        MinEngineRPM = 1000f;
        MaxEngineRPM = 5000f;
        engineIntensityPercentage = MaxEngineRPM;

        carAcceleration = 350f;
        EngineTorque = 100f;
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

        JointSpring suspensionSpring = new JointSpring
        {
            spring = 32000f,
            damper = 2000f,
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
        SetCarRainCollisions();
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
        hasBeenSpawned = true;
        StartMagneting();
    }

    public void SetCarRainCollisions()
    {
        var particleTriggers = new[]
        {
           ScandalsTweaks.Utils.References.rainParticles.trigger,
           ScandalsTweaks.Utils.References.rainHitParticles.trigger,
           ScandalsTweaks.Utils.References.stormyRainParticles.trigger,
           ScandalsTweaks.Utils.References.stormyRainHitParticles.trigger
        };

        if (particleTriggers == null)
        {
            Plugin.Logger.LogWarning($"rain particles are null! is application editor? {Application.isEditor}");
            if (Application.isEditor) Plugin.Logger.LogDebug($"application is editor, this is ignorable");
            else Plugin.Logger.LogError("rain particles are null! this will cause issues!");
            return;
        }
        for (int i = 0; i < particleTriggers.Length; i++)
        {
            for (int j = 0; j < weatherEffectBlockers.Length; j++)
            {
                int index = particleTriggers[i].colliderCount + j;
                particleTriggers[i].SetCollider(index, weatherEffectBlockers[j]);
            }
        }
    }


    // --- SYNC DATA ---
    public void SendClientSyncData()
    {
        if (turboBoosts > 0)
            AddTurboBoostRpc(turboBoosts);

        if (ignitionStarted)
            StartIgnitionRpc();

        if (magnetedToShip)
        {
            Vector3 eulerAngles = magnetTargetRotation.eulerAngles;
            MagnetCarRpc(magnetTargetPosition, eulerAngles, magnetStartPosition, magnetStartRotation, averageVelocityAtMagnetStart);
        }
        SyncClientDataRpc(carHP, windshieldShattered, windshieldBroken, (int)drivetrainModule.autoGear, steeringWheelAnimFloat, ignitionStarted, isSpecial);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SyncClientDataRpc(int carHealth, bool windowShattered, bool windowBroken, int gear, float wheelRot, bool ignOn, bool isJenson)
    {
        if (IsHost)
            return;

        isSpecial = isJenson;
        SetVariant(isSpecial);

        carHP = carHealth;
        drivetrainModule.autoGear = (TruckGearShift)gear;
        syncedWheelRotation = wheelRot;
        steeringWheelAnimFloat = wheelRot;

        if (ignOn) dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
        driversSideWindow.interactable = ignOn;

        if (windowShattered) ShatterWindshield();
        if (windowBroken) BreakWindshield();
    }


    // --- STORAGE DOORS ---
    public new void SetBackDoorOpen(bool open)
    {
        RoundManager.Instance.PlayAudibleNoise(backDoorContainer.transform.position, 21f, 0.9f, 0, noiseIsInsideClosedShip: false, 2692);
        liftGateOpen = open;
    }

    public void SetSideDoorOpen(bool open)
    {
        RoundManager.Instance.PlayAudibleNoise(sideDoorContainer.transform.position, 21f, 0.9f, 0, noiseIsInsideClosedShip: false, 2692);
        sideDoorOpen = open;
    }


    // --- CAB LIGHTING ---
    public void SetCabinLightSwitchLocalClient()
    {
        cabinLightSwitchEnabled = !cabinLightSwitchEnabled;
        cabinLightSwitchAudio.PlayOneShot(cabinLightSwitchToggle);
        SetFrontCabinLightOn(keyIsInIgnition);
        SetCabinLightSwitchRpc(cabinLightSwitchEnabled, keyIsInIgnition);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetCabinLightSwitchRpc(bool switchState, bool isKeyInSlot)
    {
        cabinLightSwitchEnabled = switchState;
        cabinLightSwitchAudio.PlayOneShot(cabinLightSwitchToggle);
        SetFrontCabinLightOn(isKeyInSlot);
    }

    private new void SetFrontCabinLightOn(bool setOn)
    {
        if (cabinLightSwitchEnabled)
        {
            frontCabinLightContainer.SetActive(setOn);
            frontCabinLightMesh.material = setOn ? headlightsOnMat : greyLightOffMat;
            return;
        }
        frontCabinLightContainer.SetActive(false);
        frontCabinLightMesh.material = greyLightOffMat;
    }


    // --- TRY IGNITION METHOD ---
    public new void StartTryCarIgnition()
    {
        if (!localPlayerInControl ||
            ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        inIgnitionAnimation = true;
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: true));
        TryIgnitionRpc(keyIsInIgnition);
    }
    
    private new IEnumerator TryIgnition(bool isLocalDriver)
    {
        StopCoroutine(jerkCarUpward(Vector3.zero));
        //GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.ResetTrigger("SA_JumpInCar");
        jumpingInCar = false;
        stopIgnitionTrigger.holdTip = "[ Can't remove yet! ]";
        if (keyIsInIgnition)
        {
            if (isLocalDriver)
            {
                if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 3)
                    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 2);
                else
                    GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 12);
            }
            yield return new WaitForSeconds(0.02f);
            carKeySounds.PlayOneShot(twistKey);
            RoundManager.Instance.PlayAudibleNoise(carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
            SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
            yield return new WaitForSeconds(0.1467f);
        }
        else
        {
            if (isLocalDriver)
                GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 2);
            SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: false);
            yield return new WaitForSeconds(0.6f);
            carKeySounds.PlayOneShot(insertKey);
            RoundManager.Instance.PlayAudibleNoise(carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
            SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: true);
            SetFrontCabinLightOn(setOn: keyIsInIgnition);
            driversSideWindow.interactable = keyIsInIgnition;
            passengersSideWindow.interactable = keyIsInIgnition;
            if (dashboardSymbolPreStartup == null && !hasSweepedDashboard)
            {
                dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
            }
            yield return new WaitForSeconds(0.2f);
            carKeySounds.PlayOneShot(twistKey);
            RoundManager.Instance.PlayAudibleNoise(carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
            SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
        }
        SetKeyIgnitionValues(trying: true, keyInHand: true, keyInSlot: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);

        if (!isLocalDriver) yield break;
        if (!brakePedalPressed) yield break;
        if (truckAlarmCoroutine != null) yield break;

        engineAudio1.Stop();
        engineAudio1.clip = revEngineStart;
        engineAudio1.volume = 0.7f;
        engineAudio1.PlayOneShot(engineRev);
        engineAudio1.pitch = 1f;
        carEngine1AudioActive = true;
        TryStartVehicleRpc();

        if (drivetrainModule.autoGear == TruckGearShift.Park)
        {
            float ignMultiplier = 1f;
            float healthPercent = (float)carHP / baseCarHP;
            float baseChance = UnityEngine.Random.Range(68f, 78f);
            chanceToStartIgnition = baseChance * healthPercent;
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.8f * ignMultiplier, 2f * ignMultiplier)); //0.4, 1.1f
            if ((float)UnityEngine.Random.Range(0, 100) < chanceToStartIgnition &&
                drivetrainModule.autoGear == TruckGearShift.Park)
            {
                inIgnitionAnimation = false;
                GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
                SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: true);
                SetIgnition(started: true);
                SetFrontCabinLightOn(setOn: keyIsInIgnition);
                CancelIgnitionAnimation(ignitionOn: true);
                StartIgnitionRpc();
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

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void TryIgnitionRpc(bool setKeyInSlot)
    {
        if (ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: setKeyInSlot);
        SetFrontCabinLightOn(setKeyInSlot);
        keyIgnitionCoroutine = StartCoroutine(TryIgnition(isLocalDriver: false));
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void TryStartVehicleRpc()
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
        if (!localPlayerInControl ||
            ignitionStarted)
            return;
        inIgnitionAnimation = false;

        // hopefully fix a bug where the wrong animation can play?
        if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 2 &&
            keyIsInIgnition)
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 3);
        else if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 12)
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 3);
        else
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);

        CancelIgnitionAnimation(ignitionOn: false);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        CancelTryIgnitionRpc(keyIsInIgnition);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void CancelTryIgnitionRpc(bool setKeyInSlot)
    {
        // account for netlag when the key is first inserted
        if (setKeyInSlot == true && (keyIsInIgnition != setKeyInSlot))
        {
            carKeySounds.PlayOneShot(insertKey);
            RoundManager.Instance.PlayAudibleNoise(carKeySounds.transform.position, 10f, 0.4f, 0, noiseIsInsideClosedShip: false, 2692);
            if (dashboardSymbolPreStartup == null && !hasSweepedDashboard)
            {
                dashboardSymbolPreStartup = StartCoroutine(PreIgnitionSymbolCheck());
            }
        }
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: setKeyInSlot);
        driversSideWindow.interactable = keyIsInIgnition;
        passengersSideWindow.interactable = keyIsInIgnition;
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        CancelIgnitionAnimation(ignitionOn: false);
    }


    // --- START IGNITION METHOD ---
    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void StartIgnitionRpc()
    {
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: true);
        SetIgnition(started: true);
        SetFrontCabinLightOn(setOn: keyIsInIgnition);
        CancelIgnitionAnimation(ignitionOn: true);
    }

    public new void SetIgnition(bool started)
    {
        SetFrontCabinLightOn(keyIsInIgnition);
        carEngine1AudioActive = started;
        if (started)
        {
            startIgnitionTrigger.holdTip = "[ Already started! ]";
            stopIgnitionTrigger.hoverTip = "Untwist key : [LMB]";
            stopIgnitionTrigger.holdTip = "[ Untwisting key ]";

            if (started == ignitionStarted)
                return;

            ignitionStarted = true;
            carExhaustParticle.Play();
            engineAudio1.Stop();
            engineAudio1.PlayOneShot(engineStartSuccessful);
            engineAudio1.clip = engineRun;
            return;
        }
        startIgnitionTrigger.holdTip = "[ Trying ignition ]";
        stopIgnitionTrigger.hoverTip = "Remove key : [LMB]";
        stopIgnitionTrigger.holdTip = keyIsInIgnition ? "[ Removing key ]" : "[ No key! ]";

        ignitionStarted = false;
        carExhaustParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void UntwistKeyInIgnition()
    {
        if (currentDriver != null && !localPlayerInControl)
            return;

        if (!ignitionStarted)
            return;

        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(UntwistKey());
        UntwistKeyInIgnitionRpc();

        if (localPlayerInControl)
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 7); // untwist key while engine running, but keep key in
    }


    // --- UNTWIST KEY IN IGNITION METHOD ---
    private IEnumerator UntwistKey()
    {
        yield return new WaitForSeconds(0.08f);
        carKeySounds.PlayOneShot(twistKey);
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: true);
        SetIgnition(started: false);
        //timeSinceIgnitionOff = 0f;
        yield return new WaitForSeconds(0.08f);
        keyIgnitionCoroutine = null;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void UntwistKeyInIgnitionRpc()
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(UntwistKey());
    }


    // --- REMOVE IGNITION METHOD ---
    public new void RemoveKeyFromIgnition()
    {
        if (currentDriver != null && !localPlayerInControl)
            return;

        if (ignitionStarted || !keyIsInIgnition)
            return;

        if (keyIgnitionCoroutine != null)
        {
            if (inIgnitionAnimation) return;
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
        RemoveKeyFromIgnitionRpc();

        if (localPlayerInControl)
        {
            if (GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.GetInteger("SA_CarAnim") == 0)
            {
                GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetTrigger("SA_RemoveInIgnition");
                return;
            }
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 8);
        }
    }

    private new IEnumerator RemoveKey()
    {
        yield return new WaitForSeconds(ignitionStarted ? 0.18f : 0.26f);
        if (dashboardSymbolPreStartup != null)
        {
            StopCoroutine(dashboardSymbolPreStartup);
            dashboardSymbolPreStartup = null!;
            StopPreIgnitionChecks();
        }
        carKeySounds.PlayOneShot(removeKey);
        SetKeyIgnitionValues(trying: false, keyInHand: true, keyInSlot: false);
        SetIgnition(started: false);
        driversSideWindow.interactable = false;
        passengersSideWindow.interactable = false;
        yield return new WaitForSeconds(0.73f);
        SetKeyIgnitionValues(trying: false, keyInHand: false, keyInSlot: false);
        keyIgnitionCoroutine = null;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void RemoveKeyFromIgnitionRpc()
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
        }
        keyIgnitionCoroutine = StartCoroutine(RemoveKey());
    }

    // --- DASHBOARD LIGHTING METHODS ---
    // there will be a better way to go about this
    // but i'm lazy right now
    public IEnumerator PreIgnitionSymbolCheck()
    {
        if (jcJensonSymbolObj.activeSelf) jcJensonSymbol.enabled = true;
        parkingBrakeSymbol.enabled = true;
        checkEngineLightSymbol.enabled = true;
        alertLightSymbol.enabled = true;
        seatbeltLightSymbol.enabled = true;
        dippedBeamLightSymbol.enabled = true;
        highBeamLightSymbol.enabled = true;
        oilLevelLightSymbol.enabled = true;
        batteryLightSymbol.enabled = true;
        coolantLevelLightSymbol.enabled = true;
        currentSweepStage = 1;
        yield return new WaitForSeconds(1.0f);

        dippedBeamLightSymbol.enabled = lowBeamsOn;
        highBeamLightSymbol.enabled = highBeamsOn;
        currentSweepStage = 2;
        yield return new WaitForSeconds(1.0f);

        seatbeltLightSymbol.enabled = false;
        parkingBrakeSymbol.enabled = drivetrainModule.autoGear == TruckGearShift.Park;
        currentSweepStage = 3;
        yield return new WaitForSeconds(1.0f);

        oilLevelLightSymbol.enabled = carHP <= 15;
        batteryLightSymbol.enabled = !ignitionStarted;
        coolantLevelLightSymbol.enabled = carHP <= 19;
        alertLightSymbol.enabled = carHP <= 12;
        checkEngineLightSymbol.enabled = carHP <= 21;
        currentSweepStage = 4;
        hasSweepedDashboard = true;
    }

    private void StopPreIgnitionChecks()
    {
        hasSweepedDashboard = false;
        currentSweepStage = 0;
        if (jcJensonSymbolObj.activeSelf) jcJensonSymbol.enabled = false;
        parkingBrakeSymbol.enabled = false;
        checkEngineLightSymbol.enabled = false;
        alertLightSymbol.enabled = false;
        seatbeltLightSymbol.enabled = false;
        dippedBeamLightSymbol.enabled = false;
        highBeamLightSymbol.enabled = false;
        oilLevelLightSymbol.enabled = false;
        batteryLightSymbol.enabled = false;
        coolantLevelLightSymbol.enabled = false;
    }


    // --- MISC IGNITION STUFF ---
    public void CancelIgnitionAnimation(bool ignitionOn)
    {
        if (keyIgnitionCoroutine != null)
        {
            StopCoroutine(keyIgnitionCoroutine);
            keyIgnitionCoroutine = null;
        }
        keyIsInDriverHand = false;
        tryingIgnition = false;
        carEngine1AudioActive = ignitionOn;
    }

    public void SetKeyIgnitionValues(bool trying, bool keyInHand, bool keyInSlot)
    {
        tryingIgnition = trying;
        keyIsInDriverHand = keyInHand;
        keyIsInIgnition = keyInSlot;
    }


    // --- GENERAL REPEAT METHODS ---
    public void SetTriggerHoverTip(InteractTrigger trigger, string tip)
    {
        trigger.hoverTip = tip;
    }


    // --- DRIVER OCCUPANT METHODS ---
    public void SetDriverInCar()
    {
        if (!hasBeenSpawned || carDestroyed) return;
        if (currentDriver != null)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            HUDManager.Instance.DisplayTip("Seat occupied",
                "You cannot enter an occupied seat! aborting!");
            return;
        }
        SetDriverInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
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
        currentDriver = StartOfRound.Instance.allPlayerScripts[playerId];
        NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[playerId].actualClientId);
        SetDriverInCarOwnerRpc();
        SetDriverInCarClientsRpc(playerId);
    }

    [Rpc(SendTo.Owner, RequireOwnership = false)]
    public void SetDriverInCarOwnerRpc()
    {
        SetVehicleCollisionForPlayer(setEnabled: false, GameNetworkManager.Instance.localPlayerController);
        InteractTriggerPatches.specialInteractCoroutine =
            StartCoroutine(InteractTriggerPatches.SpecialTruckInteractAnimation(
                trigger: driverSeatTrigger,
                playerController: GameNetworkManager.Instance.localPlayerController,
                controller: this,
                isPassenger: false));

        ActivateControl();
        SetTriggerHoverTip(driverSideDoorTrigger, "Exit : [LMB]");
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
        if (keyIsInIgnition) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
        if (ignitionStarted) GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 1);
        if (driverSideDoor.boolValue) driverSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetDriverInCarClientsRpc(int playerId)
    {
        currentDriver = StartOfRound.Instance.allPlayerScripts[playerId];
        SetVehicleCollisionForPlayer(setEnabled: false, currentDriver);
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;
        PlayerUtils.ReplaceClientPlayerAnimator(playerId);
    }

    public new void ExitDriverSideSeat()
    {
        if (!localPlayerInControl)
            return;

        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetInteger("SA_CarAnim", 0);
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
        localPlayerInControl = false;
        DisableVehicleCollisionForAllPlayers();
        SetTriggerHoverTip(driverSideDoorTrigger, "Use door : [LMB]");
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;
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
            transform.position,
            transform.rotation);
        OnDriverExitRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId, 
            syncedPosition, 
            syncedRotation, 
            ignitionStarted, 
            keyIsInIgnition);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    public void OnDriverExitServerRpc(Vector3 carLocation, Quaternion carRotation)
    {
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        drivePedalPressed = false;
        brakePedalPressed = false;
        currentDriver = null;
        NetworkObject.ChangeOwnership(StartOfRound.Instance.allPlayerScripts[0].actualClientId);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnDriverExitRpc(int playerId, Vector3 carLocation, Quaternion carRotation, bool setIgnitionState, bool setKeyInSlot)
    {
        syncedPosition = carLocation;
        syncedRotation = carRotation;
        drivePedalPressed = false;
        brakePedalPressed = false;
        currentDriver = null;
        keyIsInIgnition = setKeyInSlot;
        ignitionStarted = setIgnitionState;
        startIgnitionTrigger.isBeingHeldByPlayer = false;
        stopIgnitionTrigger.isBeingHeldByPlayer = false;
        PlayerUtils.ReturnClientPlayerAnimator(playerId);
        CancelIgnitionAnimation(ignitionOn: ignitionStarted);
        SetIgnition(started: ignitionStarted);
        if (localPlayerInPassengerSeat || localPlayerInMiddlePassengerSeat)
            SetVehicleCollisionForPlayer(setEnabled: false, GameNetworkManager.Instance.localPlayerController);
    }


    // --- MIDDLE PASSENGER OCCUPANT METHODS ---
    public void SetMiddlePassengerInCar()
    {
        if (!hasBeenSpawned || carDestroyed) return;
        if (currentMiddlePassenger != null)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            HUDManager.Instance.DisplayTip("Seat occupied",
                "You cannot enter an occupied seat! aborting!");
            return;
        }
        SetMiddlePassengerInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
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
        SetVehicleCollisionForPlayer(setEnabled: false, playerController);
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId) 
            SetPassengerIntoMiddleSeat();
        currentMiddlePassenger = playerController;
        SetMiddlePassengerInCarRpc(playerId);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SetMiddlePassengerInCarRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        SetVehicleCollisionForPlayer(setEnabled: false, playerController);
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId) SetPassengerIntoMiddleSeat();
        currentMiddlePassenger = playerController;
    }

    public void SetPassengerIntoMiddleSeat()
    {
        InteractTriggerPatches.specialInteractCoroutine =
            StartCoroutine(InteractTriggerPatches.SpecialTruckInteractAnimation(
                trigger: middlePassengerSeatTrigger,
                playerController: GameNetworkManager.Instance.localPlayerController,
                controller: this,
                isPassenger: true));
        localPlayerInMiddlePassengerSeat = true;
        SetTriggerHoverTip(driverSideDoorTrigger, "Exit : [LMB]");
        SetTriggerHoverTip(passengerSideDoorTrigger, "Exit : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
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
        SetVehicleCollisionForPlayer(setEnabled: true, GameNetworkManager.Instance.localPlayerController);
        localPlayerInMiddlePassengerSeat = false;
        SetTriggerHoverTip(driverSideDoorTrigger, "Use door : [LMB]");
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
        OnMiddlePassengerExitRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            GameNetworkManager.Instance.localPlayerController.transform.position);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnMiddlePassengerExitRpc(int playerId, Vector3 exitPoint)
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


    // --- PASSENGER OCCUPANT METHODS ---
    public void SetPassengerInCar()
    {
        if (!hasBeenSpawned || carDestroyed) return;
        if (currentPassenger != null)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            HUDManager.Instance.DisplayTip("Seat occupied",
                "You cannot enter an occupied seat! aborting!");
            return;
        }
        SetPassengerInCarServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
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
        SetVehicleCollisionForPlayer(setEnabled: false, playerController);
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId)
            SetPassengerIntoPassengerSeat();
        currentPassenger = playerController;
        SetPassengerInCarRpc(playerId);
    }

    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SetPassengerInCarRpc(int playerId)
    {
        PlayerControllerB playerController = StartOfRound.Instance.allPlayerScripts[playerId];
        SetVehicleCollisionForPlayer(setEnabled: false, playerController);
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId == playerId) SetPassengerIntoPassengerSeat();
        currentPassenger = playerController;
    }

    public void SetPassengerIntoPassengerSeat()
    {
        InteractTriggerPatches.specialInteractCoroutine =
            StartCoroutine(InteractTriggerPatches.SpecialTruckInteractAnimation(
                trigger: passengerSeatTrigger,
                playerController: GameNetworkManager.Instance.localPlayerController,
                controller: this,
                isPassenger: true));
        localPlayerInPassengerSeat = true;
        SetTriggerHoverTip(passengerSideDoorTrigger, "Exit : [LMB]");
        GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetFloat("animationSpeed", 0.5f);
        if (passengerSideDoor.boolValue) passengerSideDoor.TriggerAnimation(GameNetworkManager.Instance.localPlayerController);
    }

    public new void ExitPassengerSideSeat()
    {
        if (!localPlayerInPassengerSeat)
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
        OnPassengerExitRpc(
            (int)GameNetworkManager.Instance.localPlayerController.playerClientId,
            GameNetworkManager.Instance.localPlayerController.transform.position);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void OnPassengerExitRpc(int playerId, Vector3 exitPoint)
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


    // --- CANCEL OCCUPANT METHOD ---
    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    public void CancelSetPlayerInVehicleClientRpc(int playerId)
    {
        if ((int)GameNetworkManager.Instance.localPlayerController.playerClientId != playerId)
            return;

        HUDManager.Instance.DisplayTip("Kicked from vehicle",
            "You have been forcefully kicked to prevent a softlock!");
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
    public new void EnableVehicleCollisionForAllPlayers()
    {
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            if (StartOfRound.Instance.allPlayerScripts[i] != currentPassenger &&
                StartOfRound.Instance.allPlayerScripts[i] != currentMiddlePassenger)
            {
                if (StartOfRound.Instance.allPlayerScripts[i] != GameNetworkManager.Instance.localPlayerController)
                {
                    // local clients
                    StartOfRound.Instance.allPlayerScripts[i].thisController.excludeLayers = (1 << 12) | (1 << 30);
                    StartOfRound.Instance.allPlayerScripts[i].playerRigidbody.excludeLayers = (1 << 12) | (1 << 30);
                    return;
                }
                StartOfRound.Instance.allPlayerScripts[i].thisController.excludeLayers = 0;
                StartOfRound.Instance.allPlayerScripts[i].playerRigidbody.excludeLayers = 0;
            }
        }
    }

    public new void DisableVehicleCollisionForAllPlayers()
    {
        // 1073741824
        for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
        {
            if (!localPlayerInControl && !localPlayerInMiddlePassengerSeat && !localPlayerInPassengerSeat &&
                StartOfRound.Instance.allPlayerScripts[i] == GameNetworkManager.Instance.localPlayerController)
            {
                StartOfRound.Instance.allPlayerScripts[i].thisController.excludeLayers = 0;
                StartOfRound.Instance.allPlayerScripts[i].playerRigidbody.excludeLayers = 0;
            }
            else
            {
                StartOfRound.Instance.allPlayerScripts[i].thisController.excludeLayers = (1 << 12) | (1 << 30);
                StartOfRound.Instance.allPlayerScripts[i].playerRigidbody.excludeLayers = (1 << 12) | (1 << 30);
            }
        }
    }

    public new void SetVehicleCollisionForPlayer(bool setEnabled, PlayerControllerB player)
    {
        if (!setEnabled)
        {
            player.thisController.excludeLayers = (1 << 12) | (1 << 30);
            player.playerRigidbody.excludeLayers = (1 << 12) | (1 << 30);
            return;
        }
        player.thisController.excludeLayers = 0;
        player.playerRigidbody.excludeLayers = 0;
    }


    // --- PLAYER INPUT TO VEHICLE INPUT & VEHICLE CONTROL METHODS ---
    private new void GetVehicleInput()
    {
        if (GameNetworkManager.Instance.localPlayerController == null)
            return;

        if (GameNetworkManager.Instance.localPlayerController.isTypingChat ||
            GameNetworkManager.Instance.localPlayerController.quickMenuManager.isMenuOpen)
            return;

        if (syncedDrivePedalPressed != drivePedalPressed ||
            syncedBrakePedalPressed != brakePedalPressed)
        {
            syncedDrivePedalPressed = drivePedalPressed;
            syncedBrakePedalPressed = brakePedalPressed;
            SyncPedalInputsRpc(drivePedalPressed, brakePedalPressed);
        }
        brakePedalPressed = Plugin.VehicleControlsInstance.BrakePedalKey.IsPressed();
        if (!ignitionStarted)
        {
            steeringAnimValue = 0f;
            return;
        }

        if (Plugin.VehicleControlsInstance.SteerLeftKey.IsPressed()) steeringAnimValue = -1f;
        else if (Plugin.VehicleControlsInstance.SteerRightKey.IsPressed()) steeringAnimValue = 1f;
        else steeringAnimValue = 0f;

        drivePedalPressed = Plugin.VehicleControlsInstance.GasPedalKey.IsPressed();
        engineModule.throttleInput = drivePedalPressed ? Mathf.MoveTowards(
            engineModule.throttleInput, 1f, 8f * Time.deltaTime) :
            Mathf.MoveTowards(engineModule.throttleInput, 0f, 8f * Time.deltaTime);

        if (steeringAnimValue == 0f && UserConfig.RecenterWheel.Value)
            steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, 0, steeringWheelTurnSpeed * Time.deltaTime / 6f);
    }

    private new void ActivateControl()
    {
        Plugin.VehicleControlsInstance.JumpKey.performed += DoTurboBoost;
        Plugin.VehicleControlsInstance.GearShiftForwardKey.performed += ShiftGearForwardInput;
        Plugin.VehicleControlsInstance.GearShiftBackwardKey.performed += ShiftGearBackInput;
        Plugin.VehicleControlsInstance.ToggleHeadlightsKey.performed += ActivateHeadlights;
        Plugin.VehicleControlsInstance.ToggleWipersKey.performed += ActivateWipers;
        Plugin.VehicleControlsInstance.ActivateHornKey.performed += ActivateHorn;
        Plugin.VehicleControlsInstance.ActivateHornKey.canceled += ActivateHorn;

        currentDriver = GameNetworkManager.Instance.localPlayerController;
        localPlayerInControl = true;
        steeringAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
    }

    private new void DisableControl()
    {
        Plugin.VehicleControlsInstance.JumpKey.performed -= DoTurboBoost;
        Plugin.VehicleControlsInstance.GearShiftForwardKey.performed -= ShiftGearForwardInput;
        Plugin.VehicleControlsInstance.GearShiftBackwardKey.performed -= ShiftGearBackInput;
        Plugin.VehicleControlsInstance.ToggleHeadlightsKey.performed -= ActivateHeadlights;
        Plugin.VehicleControlsInstance.ToggleWipersKey.performed -= ActivateWipers;
        Plugin.VehicleControlsInstance.ActivateHornKey.performed -= ActivateHorn;
        Plugin.VehicleControlsInstance.ActivateHornKey.canceled -= ActivateHorn;

        currentDriver = null;
        localPlayerInControl = false;
        steeringAnimValue = 0f;
        drivePedalPressed = false;
        brakePedalPressed = false;
    }


    // --- SHIFTING GEARS METHODS ---
    public new void ShiftGearForwardInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!localPlayerInControl ||
            Time.realtimeSinceStartup - timeSinceTogglingRadio < 0.5f ||
            Time.realtimeSinceStartup - timeAtLastGearShift < 0.15f)
            return;

        ShiftGearForward();
    }

    public new void ShiftGearBackInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (!localPlayerInControl ||
            Time.realtimeSinceStartup - timeSinceTogglingRadio < 0.5f ||
            Time.realtimeSinceStartup - timeAtLastGearShift < 0.15f)
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
        if (drivetrainModule.autoGear == (TruckGearShift)setGear)
            return;

        if (Time.realtimeSinceStartup - timeSinceTogglingRadio < 0.5f)
            return;

        // prevent shifting from park if the brake pedal isn't pressed down
        if (drivetrainModule.autoGear == TruckGearShift.Park && setGear != 4 && 
            !brakePedalPressed)
            return;

        timeAtLastGearShift = Time.realtimeSinceStartup;
        drivetrainModule.autoGear = (TruckGearShift)setGear;
        gearStickAudio.PlayOneShot(gearStickAudios[setGear - 1]);
        RoundManager.Instance.PlayAudibleNoise(gearStickAudio.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
        ShiftToGearRpc(setGear);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ShiftToGearRpc(int setGear)
    {
        timeAtLastGearShift = Time.realtimeSinceStartup;
        drivetrainModule.autoGear = (TruckGearShift)setGear;
        gearStickAudio.PlayOneShot(gearStickAudios[setGear - 1]);
        RoundManager.Instance.PlayAudibleNoise(gearStickAudio.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
    }


    // --- AUTOPILOT MAGNET ---
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

        if (StartOfRound.Instance.inShipPhase) return;
        if (GameNetworkManager.Instance.localPlayerController == null) return;
        if (!base.IsOwner) return;
        MagnetCarRpc(magnetTargetPosition, eulerAngles, magnetStartPosition, magnetStartRotation, averageVelocityAtMagnetStart);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void MagnetCarRpc(Vector3 targetPosition, Vector3 targetRotation, Vector3 startPosition, Quaternion startRotation, Vector3 avgVel)
    {
        mainRigidbody.isKinematic = true;
        magnetedToShip = true;
        magnetTime = 0f;
        magnetRotationTime = 0f;
        averageVelocityAtMagnetStart = avgVel;

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
        Collider[] array = Physics.OverlapSphere(transform.position, 25f, 64, QueryTriggerInteraction.Collide);
        for (int i = 0; i < array.Length; i++)
        {
            GrabbableObject component = array[i].GetComponent<GrabbableObject>();
            if (component != null &&
                !component.isHeld &&
                !component.isHeldByEnemy &&
                array[i].transform.parent == transform)
            {
                // only credit the last driver (kudos to Buttery for figuring this out!)
                if (References.lastDriver != null)
                {
                    References.lastDriver.SetItemInElevator(magnetedToShip, magnetedToShip, component);
                }
                else if (References.lastDriver == null && 
                    GameNetworkManager.Instance.localPlayerController != null)
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
        int setEngineHealth = Mathf.Min(carHP + 2, baseCarHP);
        AddEngineOilOnLocalClient(setEngineHealth);
        AddEngineOilRpc(setEngineHealth);
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
        int setTurboBoosts = Mathf.Min(turboBoosts + 1, 5);
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
        heatDirection = heaterOn ? Quaternion.Euler(0f, -0.5f, 0f) : Quaternion.Euler(0f, 20f, 0f);
        SetHeaterRpc(heaterOn);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterRpc(bool heatOn)
    {
        heaterOn = heatOn;
        heatDirection = heaterOn ? Quaternion.Euler(0f, -0.5f, 0f) : Quaternion.Euler(0f, 20f, 0f);
    }


    public void SetHeaterFanSpeedOnLocalClient(float fanSpeed)
    {
        if (heaterSpeed == fanSpeed) return;
        heaterSpeed = fanSpeed;

        if (heaterSpeed == 1) heatSpeedRot = Quaternion.Euler(-12.75f, 0f, 0f);
        else if (heaterSpeed == 2) heatSpeedRot = Quaternion.Euler(-4.2f, 0f, 0f);
        else if (heaterSpeed == 3) heatSpeedRot = Quaternion.Euler(4.2f, 0f, 0f);
        else heatSpeedRot = Quaternion.Euler(12.75f, 0f, 0f);
        SetHeaterFanSpeedRpc(heaterSpeed);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterFanSpeedRpc(float fanSpeed)
    {
        heaterSpeed = fanSpeed;

        if (heaterSpeed == 1) heatSpeedRot = Quaternion.Euler(-12.75f, 0f, 0f);
        else if (heaterSpeed == 2) heatSpeedRot = Quaternion.Euler(-4.2f, 0f, 0f);
        else if (heaterSpeed == 3) heatSpeedRot = Quaternion.Euler(4.2f, 0f, 0f);
        else heatSpeedRot = Quaternion.Euler(12.75f, 0f, 0f);
    }


    public void SetHeaterDirectionOnLocalClient(float heatTemp)
    {
        if (heaterTemp == heatTemp) return;
        heaterTemp = heatTemp;
        isHeaterCold = heatTemp == 0;
        isHeaterWarm = heatTemp == 1;

        if (isHeaterCold) heatPosition = Quaternion.Euler(0f, 20, 0f);
        else heatPosition = Quaternion.Euler(0f, -20f, 0f);
        SetHeaterDirectionRpc(heaterTemp);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void SetHeaterDirectionRpc(float temp)
    {
        heaterTemp = temp;
        isHeaterCold = heaterTemp == 0;
        isHeaterWarm = heaterTemp == 1;
        if (isHeaterCold) heatPosition = Quaternion.Euler(0f, 20, 0f);
        else heatPosition = Quaternion.Euler(0f, -20f, 0f);
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
        if (base.IsOwner)
        {
            GameNetworkManager.Instance.localPlayerController.updatePlayerAnimationsInterval = 0.15f;
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetTrigger("SA_JumpInCar");
            if (turboBoosts == 0 || 
                !overdriveSwitchEnabled)
            {
                jumpingInCar = true;
                StartCoroutine(jerkCarUpward(dir));
                if (currentDriver != null)
                    currentDriver.movementAudio.PlayOneShot(jumpInCarSFX);
            }
            else
            {
                Vector3 boostForce = base.transform.TransformDirection(new Vector3(dir.x, 0f, dir.y));
                mainRigidbody.AddForce(boostForce * turboBoostForce + Vector3.up * turboBoostUpwardForce * 0.6f, ForceMode.Impulse);
            }
        }
        if (turboBoosts > 0 && 
            overdriveSwitchEnabled)
        {
            turboBoosts = Mathf.Max(0, turboBoosts - 1);
            turboBoostAudio.PlayOneShot(turboBoostSFX);
            engineAudio1.PlayOneShot(turboBoostSFX2);
            if (currentDriver != null)
                currentDriver.movementAudio.PlayOneShot(jumpInCarSFX);
            turboBoostParticle.Play(true);
            if (Vector3.Distance(StartOfRound.Instance.audioListener.transform.position, turboBoostAudio.transform.position) < 10f)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                return;
            }
        }
        else
        {
            if (currentDriver != null)
                currentDriver.movementAudio.PlayOneShot(jumpInCarSFX);
        }
    }

    private new IEnumerator jerkCarUpward(Vector3 dir)
    {
        if (!base.IsOwner)
        {
            jumpingInCar = false;
            yield break;
        }
        yield return new WaitForSeconds(0.16f);
        Vector3 jerkForce = base.transform.TransformDirection(new Vector3(dir.x, 0f, dir.y));
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

        // this needs to be cached, but i'm too lazy right now
        InteractTrigger? windscreenWipers = transform.Find("Assets/BodyContainer/Interior/ColumnStocks/RightStockAnimContainer/RightStock/ToggleWiper")?.GetComponent<InteractTrigger>();
        windscreenWipers?.onInteract.Invoke(StartOfRound.Instance.localPlayerController);
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
    public override void OnDestroy()
    {
        truckMat.mainTexture = defaultTruckTex;
        base.OnDestroy();
    }

    public new void OnDisable()
    {
        truckMat.mainTexture = defaultTruckTex;
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


    // --- UPDATE ---
    public new void Update()
    {
        if (destroyNextFrame)
        {
            if (base.IsOwner)
            {
                Plugin.Logger.LogInfo(string.Format("is networkobject spawned?: {0}", base.NetworkObject.IsSpawned));
                Plugin.Logger.LogInfo($"destroying {gameObject.name} on local client");
                UnityEngine.Object.Destroy(base.windwiperPhysicsBody1.gameObject);
                UnityEngine.Object.Destroy(base.windwiperPhysicsBody2.gameObject);
                UnityEngine.Object.Destroy(base.ragdollPhysicsBody.gameObject);
                UnityEngine.Object.Destroy(this.playerPhysicsBody.gameObject);
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
            Plugin.Logger.LogInfo($"destroying {gameObject.name} on local client, next frame");
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
        {
            driverSeatTrigger.interactable = false;
            middlePassengerSeatTrigger.interactable = false;
            passengerSeatTrigger.interactable = false;
            return;
        }

        driverSeatTrigger.interactable = hasBeenSpawned;
        middlePassengerSeatTrigger.interactable = hasBeenSpawned;
        passengerSeatTrigger.interactable = hasBeenSpawned;

        if (currentDriver != null && currentDriver.playerBodyAnimator != null)
        {
            // vehicle gearstick --> player gearstick animation position
            currentDriver.playerBodyAnimator.SetFloat("SA_CarMotionTime", gearStickAnimValue);
            if (localPlayerInControl && ignitionStarted &&
                keyIgnitionCoroutine == null)
            {
                // steering animation
                currentDriver.playerBodyAnimator.SetFloat("animationSpeed", steeringWheelAnimFloat + 0.5f);
                int currentAnimIndex = 1;
                if (Time.realtimeSinceStartup - timeAtLastGearShift < 1.7f) currentAnimIndex = 5;         
                currentDriver.playerBodyAnimator.SetInteger("SA_CarAnim", currentAnimIndex);
            }
        }
        SetCarEffects(steeringAnimValue);
        if (!ignitionStarted) EngineRPM = Mathf.Lerp(EngineRPM, 0f, 3f * Time.deltaTime);
        if (base.IsOwner)
        {
            float vehicleStress = 0f;
            if (ignitionStarted && drivetrainModule.autoGear == TruckGearShift.Park)
            {
                if (BackLeftWheel.isGrounded && BackRightWheel.isGrounded &&
                    averageVelocity.magnitude > 14f)
                {
                    vehicleStress += Mathf.Clamp(((averageVelocity.magnitude * 165f) - 200f) / 150f, 0f, 4f);
                    lastStressType += "; In park while at a high speed";
                }
            }
            SetInternalStress(vehicleStress);
            stressPerSecond = vehicleStress;
        }
        if (localPlayerInControl && currentDriver != null)
        {
            GetVehicleInput();
            return;
        }
        moveInputVector = Vector2.zero;
    }


    // --- RADIO TIME SYNC ---
    [Rpc(SendTo.NotServer, RequireOwnership = false)]
    public void SyncRadioTimeRpc(float songTime)
    {
        currentSongTime = songTime;
        SetRadioTime();
    }

    public void SetRadioTime()
    {
        if (radioAudio.clip == null) return;
        float songTime = currentSongTime % radioAudio.clip.length;
        if (songTime < 0) songTime += radioAudio.clip.length;
        radioAudio.time = songTime;
    }


    // --- RADIO TYPE (FM/CD) ---
    public void ChangeRadioType()
    {
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        if (SCVNetworker.Instance.StreamerRadio.Value)
        {
            HUDManager.Instance.DisplayTip("Live radio disabled", 
                "The host has disabled this feature!");
            return;
        }
        #pragma warning restore CS8602 // Dereference of a possibly null reference.

        if (!radioOn) return;
        isFmRadio = !isFmRadio;
        if (!isFmRadio)
        {
            liveRadioController.TurnRadioOnOff(false);
            radioInterference.Stop();
            if (radioAudio.loop) radioAudio.loop = false;

            if (radioClips.Length > 0)
                radioAudio.clip = radioClips[currentRadioClip];
            else
            {
                radioAudio.clip = null;
                Plugin.Logger.LogWarning("No music found to play!");
            }

            currentSongTime = lastSongTime;
            SetRadioTime();
            if (!radioAudio.isPlaying && radioAudio.clip != null) radioAudio.Play();
            radioOn = true;
        }
        else
        {
            if (!radioAudio.loop) radioAudio.loop = true;
            lastSongTime = currentSongTime;
            liveRadioController.TogglePowerLocalClient(false);
            radioOn = true;
        }
        ChangeRadioTypeRpc(isFmRadio, lastSongTime, currentSongTime);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ChangeRadioTypeRpc(bool isFmStation, float lastTime, float currentTime)
    {
        isFmRadio = isFmStation;
        if (!isFmRadio)
        {
            liveRadioController.TurnRadioOnOff(false);
            radioInterference.Stop();
            if (radioAudio.loop) radioAudio.loop = false;

            if (radioClips.Length > 0)
                radioAudio.clip = radioClips[currentRadioClip];
            else
            {
                radioAudio.clip = null;
                Plugin.Logger.LogWarning("No music found to play!");
            }

            currentSongTime = currentTime;
            SetRadioTime();
            if (!radioAudio.isPlaying && radioAudio.clip != null) radioAudio.Play();
            radioOn = true;
            return;
        }
        if (!radioAudio.loop) radioAudio.loop = true;
        lastSongTime = lastTime;
    }


    // --- RADIO SEEK (CHANGE CHANNEL/TRACK) ---
    public void ChangeRadioStation(bool seekForward)
    {
        if (isFmRadio)
        {
            liveRadioController.ToggleStationLocalClient();
            return;
        }
        if (!radioOn) return;

        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        if (seekForward)
        {
            if (!SCVNetworker.Instance.StreamerRadio.Value)
            {
                if (radioClips.Length > 0)
                    currentRadioClip = (currentRadioClip + 1) % radioClips.Length; // seek forwards
                else
                {
                    currentRadioClip = 0;
                    Plugin.Logger.LogWarning("No music found to play!");
                }
            }
            else
                currentRadioClip = (currentRadioClip + 1) % streamerRadioClips.Length;
        }
        else
        {
            if (!SCVNetworker.Instance.StreamerRadio.Value)
            {
                if (radioClips.Length > 0)
                    currentRadioClip = (currentRadioClip - 1 + radioClips.Length) % radioClips.Length; // seek backwards
                else
                {
                    currentRadioClip = 0;
                    Plugin.Logger.LogWarning("No music found to play!");
                }
            }
            else
                currentRadioClip = (currentRadioClip - 1 + streamerRadioClips.Length) % streamerRadioClips.Length;
        }

        if (!SCVNetworker.Instance.StreamerRadio.Value)
        {
            if (radioClips.Length > 0)
                radioAudio.clip = radioClips[currentRadioClip];
            else
            {
                radioAudio.clip = null;
                Plugin.Logger.LogWarning("No music found to play!");
            }
        }
        else
            radioAudio.clip = streamerRadioClips[currentRadioClip];
        #pragma warning restore CS8602 // Dereference of a possibly null reference.

        currentSongTime = 0f;
        lastSongTime = 0f;
        SetRadioTime();
        if (!radioAudio.isPlaying && radioAudio.clip != null) radioAudio.Play();
        SetRadioStationRpc(currentRadioClip);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetRadioStationRpc(int radioStation)
    {
        currentRadioClip = radioStation;

        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        if (!SCVNetworker.Instance.StreamerRadio.Value)
        {
            if (radioClips.Length > 0)
                radioAudio.clip = radioClips[currentRadioClip];
            else
            {
                radioAudio.clip = null;
                Plugin.Logger.LogWarning("No music found to play!");
            }
        }
        else
            radioAudio.clip = streamerRadioClips[currentRadioClip];
        #pragma warning restore CS8602 // Dereference of a possibly null reference.

        currentSongTime = 0f;
        lastSongTime = 0f;
        SetRadioTime();
        if (!radioAudio.isPlaying && radioAudio.clip != null) radioAudio.Play();
    }


    // --- RADIO TOGGLE ---
    public new void SwitchRadio()
    {
        timeSinceTogglingRadio = Time.realtimeSinceStartup;
        if (localPlayerInControl && 
            ignitionStarted)
            GameNetworkManager.Instance.localPlayerController.playerBodyAnimator.SetTrigger("SA_RadioTurnOn");

        if (isFmRadio)
        {
            liveRadioController.TogglePowerLocalClient(true);
            return;
        }
        if (radioAudio.loop) radioAudio.loop = false;
        radioOn = !radioOn;
        if (radioOn)
        {
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (!SCVNetworker.Instance.StreamerRadio.Value)
            {
                if (radioClips.Length > 0)
                    radioAudio.clip = radioClips[currentRadioClip];
                else
                {
                    radioAudio.clip = null;
                    Plugin.Logger.LogWarning("No music found to play!");
                }
            }
            else
                radioAudio.clip = streamerRadioClips[currentRadioClip];
            #pragma warning restore CS8602 // Dereference of a possibly null reference.

            currentSongTime = lastSongTime;
            SetRadioTime();
            if (!radioAudio.isPlaying && radioAudio.clip != null) radioAudio.Play();
        }
        else
        {
            lastSongTime = currentSongTime;
            if (radioAudio.isPlaying && radioAudio.clip != null) radioAudio.Stop();
        }
        SetRadioRpc(radioOn, currentRadioClip, lastSongTime);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetRadioRpc(bool on, int currentClip, float lastRadioTime)
    {
        if (radioOn == on) return;
        radioOn = on;
        currentRadioClip = currentClip;

        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        if (!SCVNetworker.Instance.StreamerRadio.Value)
        {
            if (radioClips.Length > 0)
                radioAudio.clip = radioClips[currentRadioClip];
            else
            {
                radioAudio.clip = null;
                Plugin.Logger.LogWarning("No music found to play!");
            }
        }
        else
            radioAudio.clip = streamerRadioClips[currentRadioClip];
        #pragma warning restore CS8602 // Dereference of a possibly null reference.

        if (!radioOn) lastSongTime = lastRadioTime;
        else currentSongTime = lastRadioTime;
        SetRadioTime();
        if (radioOn && !radioAudio.isPlaying && radioAudio.clip != null) radioAudio.Play();
        else if (!radioOn && radioAudio.isPlaying && radioAudio.clip != null) radioAudio.Stop();
    }


    // --- RADIO VALUES ---
    public new void SetRadioValues()
    {
        if (radioAudio.clip == null)
            return;
        if (IsServer && radioAudio.isPlaying && 
            Time.realtimeSinceStartup > radioPingTimestamp)
        {
            radioPingTimestamp = (Time.realtimeSinceStartup + 1f);
            RoundManager.Instance.PlayAudibleNoise(radioAudio.transform.position, 16f, Mathf.Min((radioAudio.volume + radioInterference.volume) * 0.5f, 0.9f), 0, false, 2692);
        }
        if (!radioOn || isFmRadio) 
            return;
        if (IsHost)
        {
            currentSongTime += Time.deltaTime;
            if (Time.realtimeSinceStartup - timeLastSyncedRadio > 1f)
            {
                timeLastSyncedRadio = Time.realtimeSinceStartup;
                SyncRadioTimeRpc(currentSongTime);
            }
            if (!radioAudio.isPlaying)
            {
                ChangeRadioStation(true);
            }
        }
    }


    // --- WHEEL VISUALS ---
    private void MatchWheelMeshToCollider(MeshRenderer wheelMesh, MeshRenderer brakeMesh, WheelCollider wheelCollider)
    {
        Vector3 position;
        Quaternion rotation;
        wheelCollider.GetWorldPose(out position, out rotation);

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

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    public void BeginAlarmRpc()
    {
        truckAlarmCoroutine = StartCoroutine(BeginAlarmSound());
    }

    private IEnumerator BeginAlarmSound()
    {
        alarmDebounce = true;
        hornAudio.Stop();
        honkingHorn = false;
        alarmAudio.Play();
        if (blinkersCoroutine != null)
        {
            StopCoroutine(blinkersCoroutine);
            blinkersCoroutine = null!;
        }
        blinkersCoroutine = StartCoroutine(FlashBlinkerLights());
        yield return new WaitForSeconds(alarmAudio.clip.length - 0.01f);
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

            SetSymbolActive(leftSignalSymbol, false);
            SetSymbolActive(hazardWarningSymbol, false);
            SetSymbolActive(rightSignalSymbol, false);

            blinkerLightsContainer.SetActive(false);
        }
        truckAlarmCoroutine = null!;
        alarmDebounce = false;
    }


    // --- VISUAL EFFECTS ---
    public new void SetCarEffects(float setSteering)
    {
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

        // steering
        setSteering = IsOwner ? setSteering : 0f;
        steeringWheelAnimFloat = Mathf.Clamp(steeringWheelAnimFloat + setSteering * (steeringWheelTurnSpeed * Time.deltaTime / 6f), -1f, 1f);
        steeringWheelAnimator.SetFloat("steeringWheelTurnSpeed", Mathf.Clamp((steeringWheelAnimFloat + 1f) / 2f, 0f, 1f));

        // cluster
        speedometerTransform.localRotation = Quaternion.Euler(0f, -225f * speedometerFloat, 0f);
        tachometerTransform.localRotation = Quaternion.Euler(0f, -135f * tachometerFloat, 0f);
        oilPressureTransform.localRotation = Quaternion.Euler(0f, -135f * oilPressureFloat, 0f);

        if (ignitionStarted)
        {
            float vehicleHPfloat = (float)carHP / baseCarHP;
            float turboPressureInt = turboPressureNeedleCurve.Evaluate(vehicleHPfloat);
            float oilPressureCurve = oilPressureNeedleCurve.Evaluate(
                (float)((carHP / 2) + ((turboBoosts * 3.5) * (turboPressureFloat * (overdriveSwitchEnabled ? 1 : 0)))) / (baseCarHP + 5));
            float speedometerRot = drivetrainModule.wheelRPM / 850f;
            float tachometerRot = EngineRPM / MaxEngineRPM;

            speedometerFloat = Mathf.Lerp(speedometerFloat, speedometerRot, 6f * Time.deltaTime);
            tachometerFloat = Mathf.Lerp(tachometerFloat, tachometerRot, 6f * Time.deltaTime);
            turboPressureFloat = Mathf.Lerp(turboPressureFloat, turboPressureInt, 6f * Time.deltaTime);
            oilPressureFloat = Mathf.Lerp(oilPressureFloat, oilPressureCurve, 4f * Time.deltaTime);
        }
        else
        {
            bool ignitionTurnOver = engineAudio1.volume > 0.1f && tryingIgnition;

            speedometerFloat = Mathf.Lerp(speedometerFloat, 0f, 6f * Time.deltaTime);
            tachometerFloat = Mathf.Lerp(tachometerFloat, (ignitionTurnOver ? 0.065f : 0f), 4.5f * Time.deltaTime);
            turboPressureFloat = Mathf.Lerp(turboPressureFloat, 0f, 6f * Time.deltaTime);
            oilPressureFloat = Mathf.Lerp(oilPressureFloat, 0f, 6f * Time.deltaTime);
        }

        // animator replacement stuff
        Quaternion leftMirrorAngle = keyIsInIgnition ? Quaternion.identity : Quaternion.Euler(0f, -77.5f, 0f);
        Quaternion rightMirrorAngle = keyIsInIgnition ? Quaternion.identity : Quaternion.Euler(0f, 77.5f, 0f);

        SetObjectRotation(headlightSwitch, lightSwitchPos, 750f, true);
        SetObjectRotation(leftElectricMirror, leftMirrorAngle, 38f, true);
        SetObjectRotation(rightElectricMirror, rightMirrorAngle, 38f, true);
        SetObjectRotation(heaterTempLever, heatPosition, 50f, false);
        SetObjectRotation(fanSpeedLever, heatSpeedRot, 50f, false);
        SetObjectRotation(heaterDirectionLever, heatDirection, 50f, false);

        SetCarRadio();
        SetCarAutomaticShifter();
        SetCarLightingEffects();
        SetCarAudioEffects();
        CalculateTyreSlip();
        SetCarKeyEffects();

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
        if (ignitionStarted) EngineRPM = Mathf.Lerp(EngineRPM, engineModule.syncedEngineRPM, 6f * Time.deltaTime);
        drivetrainModule.wheelRPM = Mathf.Lerp(drivetrainModule.wheelRPM, drivetrainModule.syncedWheelRPM, 6f * Time.deltaTime);
        steeringWheelAnimFloat = Mathf.MoveTowards(steeringWheelAnimFloat, syncedWheelRotation, (steeringWheelTurnSpeed * Time.deltaTime / 6f));
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
        if (!keyIsInIgnition && !radioOn)
        {
            radioTime.text = null;
            radioFrequency.text = null;
            return;
        }
        if (radioOn)
        {
            // display the current time on the dashboard
            radioTime.text = SetRadioClock(HUDManager.Instance.clockNumber.text);
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
        if (vol >= 1f)
            return "VOL MAX";

        int display = Mathf.RoundToInt(vol * 10f);
        return $"VOL {display:00}";
    }

    // return the current CD track
    private string SetRadioCDTrack(int currentTrack)
    {
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        if (!SCVNetworker.Instance.StreamerRadio.Value && 
            radioClips.Length == 0)
        {
            return "NO CD";
        }
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
        int displayTrack = currentTrack + 1;
        return $"CD PLAY {displayTrack:00}";
    }

    // trim and return the radio time text to not display 
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

    // brake-lights and reverse-lights, any additional
    // lights can be added here i guess
    public void SetCarLightingEffects()
    {
        bool brakeLightsOn = drivetrainModule.autoGear != TruckGearShift.Park && wheelBrakeTorque > 100f && 
            ignitionStarted;
        bool backingUpLightsOn = drivetrainModule.autoGear == TruckGearShift.Reverse && 
            ignitionStarted;
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
        if (audioActive && ((audio == extremeStressAudio && magnetedToShip) || ((audio == rollingAudio || audio == skiddingAudio) && (magnetedToShip || 
            (!FrontLeftWheel.isGrounded && !FrontRightWheel.isGrounded && !BackLeftWheel.isGrounded && !BackRightWheel.isGrounded)))))
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
        float engineAudioAnimCurve = engineModule.engineCurve.Evaluate(EngineRPM / engineIntensityPercentage);
        float highestAudio1 = ignitionStarted ? Mathf.Clamp(engineAudioAnimCurve, 0.65f, 1.15f) : 1f; // 0.65f, 1.15f
        float highestAudio2 = Mathf.Clamp(engineAudioAnimCurve, 0.7f, 1.5f);
        float highestTyre = Mathf.Clamp(drivetrainModule.wheelRPM / (180f * 0.35f), 0f, 1f);
        float heatSpeed = heaterSpeed/4f;
        carEngine2AudioActive = ignitionStarted;
        carRollingAudioActive = (FrontLeftWheel.isGrounded || FrontRightWheel.isGrounded || BackLeftWheel.isGrounded || BackRightWheel.isGrounded) && drivetrainModule.wheelRPM > 10f;
        SetVehicleAudioProperties(engineAudio1, carEngine1AudioActive, 0.65f, highestAudio1, 2f, useVolumeInsteadOfPitch: false, 0.7f);
        SetVehicleAudioProperties(engineAudio2, carEngine2AudioActive, 0.7f, highestAudio2, 3f, useVolumeInsteadOfPitch: false, 0.5f);
        SetVehicleAudioProperties(rollingAudio, carRollingAudioActive, 0f, highestTyre, 5f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(extremeStressAudio, underExtremeStress, 0.2f, 1f, 3f, useVolumeInsteadOfPitch: true);
        SetVehicleAudioProperties(heaterAudio, heaterOn && ignitionStarted, 0f, heatSpeed, 3f, useVolumeInsteadOfPitch: true);
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
    public void CalculateTyreSlip()
    {
        if (base.IsOwner)
        {
            float vehicleSpeed = Vector3.Dot(Vector3.Normalize(mainRigidbody.velocity * 1000f), transform.forward);
            bool wheelsGrounded = BackLeftWheel.isGrounded && BackRightWheel.isGrounded;
            bool audioActive = vehicleSpeed > -0.6f && vehicleSpeed < 0.4f && (averageVelocity.magnitude > 4f || drivetrainModule.wheelRPM > 85f);

            if (wheelsGrounded)
            {
                bool sidewaySlipping = (wheelTorque > 900f) && Mathf.Abs(sidewaysSlip) > 0.35f;
                bool forwardSlipping = (wheelTorque > 4800f) && Mathf.Abs(forwardsSlip) > 0.25f;

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
                SetTyreStressRpc(vehicleSpeed, audioActive);

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
        Transform ignBarrelRot = (ignitionStarted || tryingIgnition)
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

            Transform ignKeyRot = (ignitionStarted || tryingIgnition)
                ? ignitionTryingPosition
                : ignitionNotTurnedPosition;

            if (!correctedPosition)
            {
                correctedPosition = true;
                keyObject.transform.localPosition = ignKeyRot.localPosition;
                keyObject.transform.localRotation = ignKeyRot.localRotation;
            }

            keyObject.transform.localPosition = ignKeyRot.localPosition;
            keyObject.transform.localRotation = Quaternion.Lerp(
                keyObject.transform.localRotation,
                ignKeyRot.localRotation,
                Time.deltaTime * ignitionRotSpeed
            );
        }
        else
        {
            if (!keyIsInDriverHand && keyObject.enabled)
                keyObject.enabled = false;
            correctedPosition = false;
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


    // --- PHYSICS UPDATE ---
    public new void FixedUpdate()
    {
        lastVelocity = mainRigidbody.velocity;
        previousVehiclePosition = base.transform.position;

        bool frontWheelsGrounded = FrontLeftWheel.isGrounded &&
                                   FrontRightWheel.isGrounded;

        bool allWheelsAirborne = !FrontLeftWheel.isGrounded &&
                                 !FrontRightWheel.isGrounded &&
                                 !BackLeftWheel.isGrounded &&
                                 !BackRightWheel.isGrounded;

        Vector3 groundNormal = Vector3.zero;
        int groundedWheelCount = 0;

        if (!carDestroyed)
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
            if (itemShip == null && References.itemShip != null)
                itemShip = References.itemShip;

            if (itemShip != null &&
                !hasBeenSpawned)
            {
                if (itemShip.untetheredVehicle)
                {
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
                    inDropshipAnimation = true;
                    mainRigidbody.isKinematic = true;
                    mainRigidbody.MovePosition(itemShip.deliverVehiclePoint.position);
                    mainRigidbody.MoveRotation(itemShip.deliverVehiclePoint.rotation);
                    averageVelocity = Vector3.zero;
                    syncedPosition = transform.position;
                    syncedRotation = transform.rotation;
                }
            }
            else if (itemShip == null)
            {
                inDropshipAnimation = false;
                mainRigidbody.isKinematic = true;
                mainRigidbody.MovePosition(StartOfRound.Instance.notSpawnedPosition.position + Vector3.forward * 30f);
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
            }
        }
        if (magnetedToShip)
        {
            syncedPosition = transform.position;
            syncedRotation = transform.rotation;
            mainRigidbody.MovePosition(Vector3.Lerp(magnetStartPosition, StartOfRound.Instance.elevatorTransform.position + magnetTargetPosition, magnetPositionCurve.Evaluate(magnetTime)));
            mainRigidbody.MoveRotation(Quaternion.Lerp(magnetStartRotation, magnetTargetRotation, magnetRotationCurve.Evaluate(magnetRotationTime)));
            averageVelocityAtMagnetStart = Vector3.Lerp(averageVelocityAtMagnetStart, Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 4f), 4f * Time.fixedDeltaTime);

            if (!finishedMagneting)
                magnetStartPosition += Vector3.ClampMagnitude(averageVelocityAtMagnetStart, 5f) * Time.fixedDeltaTime;
        }
        else
        {
            if (!base.IsOwner && !inDropshipAnimation)
            {
                mainRigidbody.isKinematic = true;
                Vector3 syncVel = syncedPosition + (averageVelocity * Time.fixedDeltaTime);
                syncSpeedMultiplier = Mathf.Clamp(syncSpeedMultiplier * Vector3.Distance(transform.position, syncVel), 1.3f, 300f);
                Vector3 position = Vector3.Lerp(transform.position, syncVel, Time.fixedDeltaTime * syncSpeedMultiplier);
                mainRigidbody.MovePosition(position);
                mainRigidbody.MoveRotation(Quaternion.Lerp(transform.rotation, syncedRotation, syncRotationSpeed));
            }
        }

        if (base.IsOwner && !inDropshipAnimation)
            averageVelocity += (mainRigidbody.velocity - averageVelocity) / (movingAverageLength + 1);

        ragdollPhysicsBody.Move(
            transform.position, 
            transform.rotation);
        playerPhysicsBody.Move(
            transform.position, 
            transform.rotation);
        windwiperPhysicsBody1.Move(
            windwiper1.position, 
            windwiper1.rotation);
        windwiperPhysicsBody2.Move(
            windwiper2.position, 
            windwiper2.rotation);

        if (carDestroyed) 
            return;

        // simulated steering decay at speed
        if (averageVelocity.magnitude < 28f || !frontWheelsGrounded)
        {
            steeringDecay = Mathf.MoveTowards(steeringDecay, 1f, 4f * Time.deltaTime);
        }
        else if (averageVelocity.magnitude > 28f && frontWheelsGrounded)
        {
            steeringDecay = Mathf.Lerp(1f, 0.65f, (averageVelocity.magnitude - 28f) / 50f);
            steeringDecay = Mathf.Max(steeringDecay, 0.65f);
        }

        float steeringAngle = steeringWheelCurve.Evaluate(Mathf.Abs(steeringWheelAnimFloat)) * 50f;
        FrontLeftWheel.steerAngle = (steeringAngle * Mathf.Sign(steeringWheelAnimFloat)) * steeringDecay;
        FrontRightWheel.steerAngle = (steeringAngle * Mathf.Sign(steeringWheelAnimFloat)) * steeringDecay;

        foreach (WheelCollider drivenWheel in wheels)
        {
            drivenWheel.motorTorque = wheelTorque;
            drivenWheel.brakeTorque = wheelBrakeTorque;
            drivenWheel.rotationSpeed = Mathf.Clamp(drivenWheel.rotationSpeed, drivetrainModule.reverseWheelSpeed, drivetrainModule.forwardWheelSpeed);
        }

        if (!base.IsOwner)
        {
            wheelTorque = drivetrainModule.syncedMotorTorque;
            wheelBrakeTorque = drivetrainModule.syncedBrakeTorque;
            engineModule.enginePower = 0f;
            drivetrainModule.currentGear = 1;

            drivetrainModule.forwardWheelSpeed = 8000f;
            drivetrainModule.reverseWheelSpeed = -8000f;

            forwardsSlip = 0f;
            sidewaysSlip = 0f;
            return;
        }
        UpdateCarDrivetrain();
        if (mainRigidbody.IsSleeping() || magnetedToShip || allWheelsAirborne)
        {
            forwardsSlip = 0f;
            sidewaysSlip = 0f;
            return;
        }
        forwardsSlip = (wheelHits[2].forwardSlip + wheelHits[3].forwardSlip) * 0.5f;
        sidewaysSlip = (wheelHits[2].sidewaysSlip + wheelHits[3].sidewaysSlip) * 0.5f;

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

    // --- TYRE SURFACE SLIP ---
    private void SetWheelStiffness(WheelCollider wheel, bool isSnow)
    {
        // set the tyres to be slippy, should the
        // truck be on a snowy surface
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;

        forwardFriction.stiffness = isSnow ? 0.5f : baseForwardStiffness;
        sidewaysFriction.stiffness = isSnow ? 0.45f : baseSidewaysStiffness;

        wheel.forwardFriction = forwardFriction;
        wheel.sidewaysFriction = sidewaysFriction;
    }

    // --- DRIVETRAIN (WHEELS) ---
    private void UpdateCarDrivetrain()
    {
        bool engineOn = ignitionStarted;
        bool gasPressed = drivePedalPressed && engineOn;
        bool atIdle = !drivePedalPressed && engineOn;
        TruckGearShift currentGear = drivetrainModule.autoGear;

        if (currentGear == TruckGearShift.Park)
            wheelBrakeTorque = 12000f;
        else
        {
            if (brakePedalPressed) wheelBrakeTorque = Mathf.MoveTowards(wheelBrakeTorque, maxBrakingPower, brakeSpeed * Time.fixedDeltaTime);
            else wheelBrakeTorque = 0f;
        }
        switch (currentGear)
        {
            case TruckGearShift.Park:
                {
                    wheelBrakeTorque = 12000f;
                    wheelTorque = 0f;
                    break;
                }
            case TruckGearShift.Reverse:
                {
                    if (gasPressed) wheelTorque = -engineModule.engineReversePower;
                    else if (atIdle) wheelTorque = idleSpeed * -1f;
                    break;
                }
            case TruckGearShift.Neutral:
                {
                    wheelTorque = 0f;
                    break;
                }
            case TruckGearShift.Drive:
                {
                    if (gasPressed) wheelTorque = drivetrainModule.automaticTransmissionCoroutine == null ? Mathf.Clamp(Mathf.MoveTowards(wheelTorque, engineModule.enginePower * 2f, carAcceleration * Time.fixedDeltaTime), 2000f, engineModule.enginePower * 2f) : 0f;
                    else if (atIdle) wheelTorque = idleSpeed * 1f;
                    // does not work as i had hoped for
                    //{
                    //    float deaccelMultiplier = Mathf.Clamp((drivetrainModule.wheelRPM - idleSpeed) / idleSpeed, 0.2f, 8f);
                    //    wheelTorque = Mathf.MoveTowards(wheelTorque, idleSpeed, carAcceleration * deaccelMultiplier * 4f * Time.fixedDeltaTime);
                    //    wheelTorque = Mathf.Max(wheelTorque, idleSpeed);
                    //    HUDManager.Instance.SetDebugText($"tor: {wheelTorque}");
                    //}
                    break;
                }
        }
    }


    // --- MISC SYNC METHODS ---
    public void SyncCarEffectsToOtherClients()
    {
        if (syncCarEffectsInterval > 0.045f)
        {
            if (syncedWheelRotation != steeringWheelAnimFloat)
            {
                syncCarEffectsInterval = 0f;
                syncedWheelRotation = steeringWheelAnimFloat;
                SyncCarEffectsRpc(steeringWheelAnimFloat);
                return;
            }
        }
        else
        {
            syncCarEffectsInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarEffectsRpc(float wheelRotation)
    {
        syncedWheelRotation = wheelRotation;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncPedalInputsRpc(bool gasPressed, bool brakePressed)
    {
        drivePedalPressed = gasPressed;
        brakePedalPressed = brakePressed;
    }

    private void SyncCarPositionToOtherClients()
    {
        mainRigidbody.isKinematic = false;
        if (syncCarPositionInterval >= (0.12f * (averageVelocity.magnitude / 200f)))
        {
            if (Vector3.Distance(syncedPosition, transform.position) > 0.012f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
                SyncCarPositionRpc(transform.position, transform.eulerAngles, averageVelocity);
                return;
            }
            if (Vector3.Angle(transform.forward, syncedRotation * Vector3.forward) > 2f)
            {
                syncCarPositionInterval = 0f;
                syncedPosition = transform.position;
                syncedRotation = transform.rotation;
                SyncCarPositionRpc(transform.position, transform.eulerAngles, averageVelocity);
                return;
            }
        }
        else
        {
            syncCarPositionInterval += Time.deltaTime;
        }
        syncCarPositionInterval = Mathf.Clamp(syncCarPositionInterval, 0.002f, 0.2f);
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarPositionRpc(Vector3 carPosition, Vector3 carRotation, Vector3 averageSpeed)
    {
        syncedPosition = carPosition;
        syncedRotation = Quaternion.Euler(carRotation);
        averageVelocity = averageSpeed;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetTyreStressRpc(float stress, bool wheelSkidding)
    {
        tyreStress = stress;
        wheelSlipping = wheelSkidding;
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

        if (GameNetworkManager.Instance.connectedPlayers == 1)
        {
            scanNode.subText = "Your only trusty mule..";
        }
        else scanNode.subText = "Your trusty mule!";
    }


    // --- LATE UPDATE METHOD ---
    public new void LateUpdate()
    {
        UpdateScanNodeText();
        if (carDestroyed)
            return;
        SetDashboardSymbols();

        bool inOrbit = magnetedToShip &&
            (StartOfRound.Instance.inShipPhase || !StartOfRound.Instance.shipDoorsEnabled);

        hornAudio.mute = inOrbit;
        engineAudio1.mute = inOrbit;
        engineAudio2.mute = inOrbit;
        carKeySounds.mute = inOrbit;
        cabinLightSwitchAudio.mute = inOrbit;
        heaterAudio.mute = inOrbit;
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

        //if ((windshieldBroken && windshieldShattered) && StartOfRound.Instance.inShipPhase)
        //    RegenerateWindshield();

        if (currentDriver != null && References.lastDriver != currentDriver && !magnetedToShip)
            References.lastDriver = currentDriver;

        if (honkingHorn && hornAudio.isPlaying && hornAudio.pitch < 1f)
            hornAudio.Stop();
    }


    // --- DASHBOARD SYMBOLS ---
    public void SetDashboardSymbols()
    {
        if (!keyIsInIgnition)
        {
            if (parkingBrakeSymbol.enabled) parkingBrakeSymbol.enabled = false;
            if (checkEngineLightSymbol.enabled) checkEngineLightSymbol.enabled = false;
            if (alertLightSymbol.enabled) alertLightSymbol.enabled = false;
            if (seatbeltLightSymbol.enabled) seatbeltLightSymbol.enabled = false;
            if (oilLevelLightSymbol.enabled) oilLevelLightSymbol.enabled = false;
            if (batteryLightSymbol.enabled) batteryLightSymbol.enabled = false;
            if (coolantLevelLightSymbol.enabled) coolantLevelLightSymbol.enabled = false;
            if (dippedBeamLightSymbol.enabled) dippedBeamLightSymbol.enabled = false;
            if (highBeamLightSymbol.enabled) highBeamLightSymbol.enabled = false;
            return;
        }

        if (!hasSweepedDashboard)
            return;

        SetSymbolActive(dippedBeamLightSymbol, currentSweepStage > 1 && lowBeamsOn);
        SetSymbolActive(highBeamLightSymbol, currentSweepStage > 1 && highBeamsOn);
        SetSymbolActive(parkingBrakeSymbol, currentSweepStage > 2 && drivetrainModule.autoGear == TruckGearShift.Park);
        SetSymbolActive(oilLevelLightSymbol, currentSweepStage > 3 && carHP <= 15);
        SetSymbolActive(batteryLightSymbol, currentSweepStage > 3 && !ignitionStarted);
        SetSymbolActive(coolantLevelLightSymbol, currentSweepStage > 3 && carHP <= 19);
        SetSymbolActive(alertLightSymbol, currentSweepStage > 3 && carHP <= 12);
        SetSymbolActive(checkEngineLightSymbol, currentSweepStage > 3 && carHP <= 21);
    }

    public void SetSymbolActive(SpriteRenderer symbol, bool active)
    {
        if (symbol.enabled != active)
            symbol.enabled = active;
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
                CarBumpRpc(averageVelocity * 0.7f);
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
                            CarBumpRpc(averageVelocity);
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

    public new void OnCollisionEnter(Collision collision)
    {
        if (!base.IsOwner)
            return;

        if (magnetedToShip || !hasBeenSpawned)
            return;

        if (collision.collider.gameObject.layer != 8)
            return;

        if (Time.realtimeSinceStartup - timeSinceLastCollision < 0.02f)
            return;

        timeSinceLastCollision = Time.realtimeSinceStartup;
        float differenceInVelocity = Mathf.Abs(lastVelocity.magnitude - mainRigidbody.velocity.magnitude);

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

        if (carBump < minimalBumpForce || lastVelocity.magnitude < 4f)
        {
            if (carBump > 3 && lastVelocity.magnitude > 2.5f)
            {
                SetInternalStress(0.35f);
                lastStressType = "Scraping?";
            }
        }
        else
        {
            //HUDManager.Instance.SetDebugText($"diff: {differenceInVelocity}, lastVel: {lastVelocity.magnitude}");

            float setVolume = 0.5f;
            int audioType = -1;

            if (differenceInVelocity >= 9f)
            {
                if (lastVelocity.magnitude > 31f)
                {
                    if (carHP < 3)
                    {
                        DestroyCar();
                        DestroyCarRpc();
                        return;
                    }
                    audioType = 2;
                    setVolume = Mathf.Clamp((carBump - maximumBumpForce) / 20000f, 0.8f, 1f);
                    setVolume = Mathf.Clamp(setVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);
                    if (!windshieldShattered)
                    {
                        ShatterWindshield();
                        ShatterWindshieldRpc();
                    }
                    else if (windshieldShattered && !windshieldBroken)
                    {
                        BreakWindshield();
                        BreakWindshieldRpc();
                    }
                    DamageVehicle((float)UnityEngine.Random.Range(40, 60), collision.relativeVelocity, carHP - 2);
                }
            }

            if (carBump > maximumBumpForce && lastVelocity.magnitude >= 11f)
            {
                audioType = 2;
                setVolume = Mathf.Clamp((carBump - maximumBumpForce) / 20000f, 0.8f, 1f);
                setVolume = Mathf.Clamp(setVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.7f, 1f);
                DamageVehicle(differenceInVelocity, collision.relativeVelocity, 2);
            }
            else if (carBump > mediumBumpForce && lastVelocity.magnitude >= 3f)
            {
                audioType = 1;
                setVolume = Mathf.Clamp((carBump - mediumBumpForce) / (maximumBumpForce - mediumBumpForce), 0.67f, 1f);
                setVolume = Mathf.Clamp(setVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.5f, 1f);
                DamageVehicle(differenceInVelocity, collision.relativeVelocity, 2);
            }
            else if (lastVelocity.magnitude >= 1.5f)
            {
                audioType = 0;
                setVolume = Mathf.Clamp((carBump - minimalBumpForce) / (mediumBumpForce - minimalBumpForce), 0.25f, 1f);
                setVolume = Mathf.Clamp(setVolume + UnityEngine.Random.Range(-0.15f, 0.25f), 0.25f, 1f);
                DamageVehicle(differenceInVelocity, collision.relativeVelocity, 1);
            }

            if (audioType != -1)
            {
                PlayCollisionAudio(zero, audioType, setVolume);
                if (carBump > maximumBumpForce + 10000f && lastVelocity.magnitude > 18f)
                {
                    if (differenceInVelocity >= 8.5f)
                    {
                        DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                        if (!windshieldShattered)
                        {
                            ShatterWindshield();
                            ShatterWindshieldRpc();
                        }
                        else if (windshieldShattered && !windshieldBroken)
                        {
                            BreakWindshield();
                            BreakWindshieldRpc();
                        }
                        CarCollisionRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 60f), differenceInVelocity);
                        DealPermanentDamage(2);
                        return;
                    }
                }
                CarBumpRpc(Vector3.ClampMagnitude(-collision.relativeVelocity, 40f));
            }
        }
    }

    public void DamageVehicle(float diff, Vector3 collision, int damageAmount = 2)
    {
        if (diff >= 7f && diff < 8.5f)
        {
            DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision, 60f), diff);
            CarCollisionRpc(Vector3.ClampMagnitude(-collision, 60f), diff);
            if (damageAmount != 0) DealPermanentDamage(damageAmount);
        }
        else if (diff > 4f && diff < 7f)
        {
            DamagePlayerInVehicle(Vector3.ClampMagnitude(-collision, 60f), diff/2f);
            CarCollisionRpc(Vector3.ClampMagnitude(-collision, 60f), diff/2f);
            if (damageAmount != 0) DealPermanentDamage(damageAmount);
        }
    }


    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void CarBumpRpc(Vector3 vel)
    {
        if (localPlayerInControl ||
            localPlayerInMiddlePassengerSeat ||
            localPlayerInPassengerSeat)
            return;
        if (!VehicleUtils.IsPlayerInVehicleBounds())
            return;
        if (PlayerUtils.isPlayerInCab) vel = Vector3.ClampMagnitude(vel, 5);
        else if (PlayerUtils.isPlayerInStorage) vel = Vector3.ClampMagnitude(vel, 30);
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
            if (!VehicleUtils.IsPlayerInVehicleBounds())
                return;
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
            GameNetworkManager.Instance.localPlayerController.DamagePlayer(5, true, true, CauseOfDeath.Inertia, 0, false, vel);
            GameNetworkManager.Instance.localPlayerController.externalForceAutoFade += vel;
            return;
        }
        if (magnitude > (28f))
        {
            GameNetworkManager.Instance.localPlayerController.DamagePlayer((int)vel.magnitude, true, true, CauseOfDeath.Inertia, 0, false, vel);
            return;
        }
        if (magnitude <= 24f)
        {
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            GameNetworkManager.Instance.localPlayerController.DamagePlayer(15, true, true, CauseOfDeath.Inertia, 0, false, vel);
            return;
        }
        HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
        if (GameNetworkManager.Instance.localPlayerController.health < 10)
        {
            GameNetworkManager.Instance.localPlayerController.KillPlayer(vel, true, CauseOfDeath.Inertia, 0, transform.up * 0.77f);
            return;
        }
        GameNetworkManager.Instance.localPlayerController.DamagePlayer(20, true, true, CauseOfDeath.Inertia, 0, false, vel);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void ShatterWindshieldRpc()
    {
        ShatterWindshield();
    }

    public void ShatterWindshield()
    {
        if (windshieldShattered)
            return;

        windshieldShattered = true;
        Material[] array = windshieldMesh.sharedMaterials;
        array[0] = windshieldBrokenMat;
        windshieldMesh.sharedMaterials = array;
    }

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
        windshieldObject.SetActive(false);
        glassParticle.Play();
        miscAudio.transform.localPosition = windshieldObject.transform.localPosition;
        miscAudio.PlayOneShot(windshieldBreak);
    }

    private void RegenerateWindshield()
    {
        windshieldBroken = false;
        windshieldShattered = false;
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
        if (StartOfRound.Instance.inShipPhase)
            return;

        if (carStressIncrease <= 0f) carStressChange = Mathf.Clamp(carStressChange - Time.deltaTime, -0.25f, 0.5f);
        else carStressChange = Mathf.Clamp(carStressChange + Time.deltaTime * carStressIncrease, 0f, 10f);

        underExtremeStress = carStressIncrease >= 1f && ignitionStarted;
        carStress = Mathf.Clamp(carStress + carStressChange, 0f, 100f);

        if (carStress < 7f)
            return;

        carStress = 0f;
        DealPermanentDamage(2);
        lastDamageType = "Stress";
    }

    public new void DealPermanentDamage(int damageAmount, Vector3 damagePosition = default(Vector3))
    {
        if (StartOfRound.Instance.inShipPhase || magnetedToShip || 
            carDestroyed || !base.IsOwner)
            return;

        //if (Time.realtimeSinceStartup - timeAtLastDamage < 0.4f)
        //    return;

        timeAtLastDamage = Time.realtimeSinceStartup;
        carHP -= damageAmount;
        if (carHP <= 0)
        {
            DestroyCar();
            DestroyCarRpc();
        }
        else
        {
            DealDamageRpc(carHP);
        }
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
        if (carDestroyed)
            return;

        DestroyCar();
    }

    public new void DestroyCar()
    {
        if (carDestroyed)
            return;

        carDestroyed = true;
        magnetedToShip = false;

        if (blinkersCoroutine != null)
        {
            StopCoroutine(blinkersCoroutine);
            blinkersCoroutine = null!;
        }

        RemoveCarRainCollision();
        CollectItemsInTruck();

        underExtremeStress = false;
        hoodPoppedUp = true;

        keyObject.enabled = false;
        engineAudio1.Stop();
        engineAudio2.Stop();
        turbulenceAudio.Stop();
        rollingAudio.Stop();
        if (radioOn) liveRadioController.TurnRadioOnOff(false);
        radioAudio.Stop();
        radioInterference.Stop();
        heaterOn = false;
        radioOn = false;
        extremeStressAudio.Stop();
        carKeySounds.Stop();
        cabinLightSwitchAudio.Stop();
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
        liftGateOpen = true;
        backDoorOpen = true;
        sideDoorOpen = true;

        if (localPlayerInControl || localPlayerInMiddlePassengerSeat || localPlayerInPassengerSeat)
        {
            GameNetworkManager.Instance.localPlayerController.CancelSpecialTriggerAnimations();
            GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.up * 27f + 20f * UnityEngine.Random.insideUnitSphere, spawnBody: true, CauseOfDeath.Blast, 6, Vector3.up * 1.5f);
        }

        InteractTrigger[] componentsInChildren2 = gameObject.GetComponentsInChildren<InteractTrigger>();
        for (int k = 0; k < componentsInChildren2.Length; k++)
        {
            componentsInChildren2[k].interactable = false;
            componentsInChildren2[k].CancelAnimationExternally();
        }

        driverSeatTrigger.interactable = false;
        middlePassengerSeatTrigger.interactable = false;
        passengerSeatTrigger.interactable = false;

        currentDriver = null!;
        currentMiddlePassenger = null!;
        currentPassenger = null!;

        Landmine.SpawnExplosion(transform.position + transform.forward + Vector3.up * 1.5f, spawnExplosionEffect: true, 6f, 10f, 30, 200f, truckDestroyedExplosion, goThroughCar: true);
        mainRigidbody.AddExplosionForce(800f * 50f, transform.position, 12f, 3f * 6f, ForceMode.Impulse);
        pushTruckTrigger.interactable = true;
    }


    // --- REMOVAL MISC ---
    public void RemoveCarRainCollision()
    {
        var particleTriggers = new[]
        {
           ScandalsTweaks.Utils.References.rainParticles.trigger,
           ScandalsTweaks.Utils.References.rainHitParticles.trigger,
           ScandalsTweaks.Utils.References.stormyRainParticles.trigger,
           ScandalsTweaks.Utils.References.stormyRainHitParticles.trigger
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


    // --- IDK ---
    private new void ReactToDamage()
    {
        healthMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            healthMeter.localScale.z,
            Mathf.Clamp((float)carHP / (float)baseCarHP, 0.01f, 1f),
            6f * Time.deltaTime));
        turboMeter.localScale = new Vector3(1f, 1f, Mathf.Lerp(
            turboMeter.localScale.z,
            Mathf.Clamp((float)turboBoosts / 5f, 0.01f, 1f),
            6f * Time.deltaTime));

        if (!base.IsOwner)
            return;
        if (carHP < 7 && Time.realtimeSinceStartup - timeAtLastDamage > 16f)
        {
            timeAtLastDamage = Time.realtimeSinceStartup;
            carHP++;
            syncedCarHP = carHP;
            SyncCarHealthRpc(carHP);
        }
        if (carDestroyed)
        {
            if (carHP < 3)
            {
                if (!isHoodOnFire)
                {
                    isHoodOnFire = true;
                    hoodFireAudio.Play();
                    hoodFireParticle.Play();
                    SetHoodOnFireRpc(isHoodOnFire);
                }
            }
            else if (isHoodOnFire && carHP >= 3)
            {
                isHoodOnFire = false;
                hoodFireAudio.Stop();
                hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
                SetHoodOnFireRpc(isHoodOnFire);
            }
            return;
        }
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
                SetHoodOnFireRpc(isHoodOnFire);
            }
        }
        else if (isHoodOnFire && carHP >= 3)
        {
            isHoodOnFire = false;
            hoodFireAudio.Stop();
            hoodFireParticle.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
            SetHoodOnFireRpc(isHoodOnFire);
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    private void SyncCarHealthRpc(int carHealth)
    {
        timeAtLastDamage = Time.realtimeSinceStartup;
        syncedCarHP = carHealth;
        carHP = syncedCarHP;
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SetHoodOnFireRpc(bool onFire)
    {
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


    // --- PUSH METHODS ---
    public new void PushTruckWithArms()
    {
        if (magnetedToShip)
            return;

        if (UserConfig.PreventPushInPark.Value &&
            drivetrainModule.autoGear == TruckGearShift.Park && !ignitionStarted)
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

        if (VehicleUtils.IsPlayerInVehicleBounds())
            return;

        Vector3 point = hit.point;
        Vector3 forward = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.forward;
        int clip = UnityEngine.Random.Range(0, minCollisions.Length);

        if (base.IsOwner)
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
        if (base.IsOwner)
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


    // --- HOOD THINGAMJIG ---
    public new void ToggleHoodOpenLocalClient()
    {
        RoundManager.Instance.PlayAudibleNoise(carHoodAnimator.gameObject.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
        carHoodOpen = !carHoodOpen;
        if (!carHoodOpen) hoodPoppedUp = false;
        carHoodAnimator.SetBool("hoodOpen", carHoodOpen);
        SetHoodOpenRpc(carHoodOpen);
    }

    public new void SetHoodOpenLocalClient(bool setOpen)
    {
        if (carHoodOpen == setOpen)
            return;

        RoundManager.Instance.PlayAudibleNoise(carHoodAnimator.gameObject.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
        carHoodOpen = setOpen;
        carHoodAnimator.SetBool("hoodOpen", setOpen);
        SetHoodOpenRpc(open: true);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    public void SetHoodOpenRpc(bool open)
    {
        if (carHoodOpen == open)
            return;

        RoundManager.Instance.PlayAudibleNoise(carHoodAnimator.gameObject.transform.position, 18f, 0.7f, 0, noiseIsInsideClosedShip: false, 2692);
        carHoodOpen = open;
        if (!carHoodOpen) hoodPoppedUp = false;
        carHoodAnimator.SetBool("hoodOpen", open);
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

            SetSymbolActive(leftSignalSymbol, true);
            SetSymbolActive(hazardWarningSymbol, true);
            SetSymbolActive(rightSignalSymbol, true);

            blinkerLightsContainer.SetActive(true);
            yield return new WaitForSeconds(0.4f);
            hazardsBlinking = false;
            steeringWheelAudio.PlayOneShot(blinkOff);

            leftBlinkerMesh.material = headlightsOffMat;
            leftBlinkerMeshLod.material = headlightsOffMat;
            rightBlinkerMesh.material = headlightsOffMat;
            rightBlinkerMeshLod.material = headlightsOffMat;

            SetSymbolActive(leftSignalSymbol, false);
            SetSymbolActive(hazardWarningSymbol, false);
            SetSymbolActive(rightSignalSymbol, false);

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

                SetSymbolActive(leftSignalSymbol, false);
                SetSymbolActive(hazardWarningSymbol, false);
                SetSymbolActive(rightSignalSymbol, false);

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

                SetSymbolActive(leftSignalSymbol, false);
                SetSymbolActive(hazardWarningSymbol, false);
                SetSymbolActive(rightSignalSymbol, false);

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

        float lightSwitchY = !lowBeamsOn ? 0f : !highBeamsOn ? 82.5f : 165f;
        lightSwitchPos = Quaternion.Euler(0f, lightSwitchY, 0f);

        cabinLightSwitchAudio.PlayOneShot(headlightsToggleSFX);
        headlightsContainer.SetActive(lowBeamsOn);
        highBeamContainer.SetActive(highBeamsOn);
        radioLight.SetActive(lowBeamsOn);
        heaterLight.SetActive(lowBeamsOn);
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

        float lightSwitchZ = !lowBeamsOn ? 0f : !highBeamsOn ? 82.5f : 165f;
        lightSwitchPos = Quaternion.Euler(0f, 0f, lightSwitchZ);

        cabinLightSwitchAudio.PlayOneShot(headlightsToggleSFX);
        headlightsContainer.SetActive(lowBeamsOn);
        highBeamContainer.SetActive(highBeamsOn);
        radioLight.SetActive(lowBeamsOn);
        heaterLight.SetActive(lowBeamsOn);
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
