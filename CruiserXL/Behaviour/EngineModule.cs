using Unity.Netcode;
using UnityEngine;

namespace CruiserXL.Behaviour;

public class EngineModule : NetworkBehaviour
{
    [Header("Engine")]

    public CruiserXLController controller = null!;
    public DrivetrainModule transmissionModule = null!;
    public AnimationCurve engineCurve = null!;
    public AnimationCurve enginePowerCurve = null!;

    public float enginePower;
    public float engineReversePower;
    public float throttleInput;

    [Header("Multiplayer")]

    public float syncedEngineRPM;
    public float syncCarEngineSpeedInterval;

    public void FixedUpdate()
    {
        if (controller == null ||
            !controller.IsSpawned ||
            controller.carDestroyed) return;

        if (!controller.ignitionStarted)
        {
            enginePower = 0f;
            return;
        }
        if (!controller.FrontLeftWheel.enabled ||
            !controller.FrontRightWheel.enabled ||
            !controller.BackLeftWheel.enabled ||
            !controller.BackRightWheel.enabled)
            return;
        if (!controller.IsOwner) return;
        float selectedGear = Mathf.Abs(transmissionModule.gearRatios[transmissionModule.currentGear]);
        //enginePower = enginePowerCurve.Evaluate(controller.EngineRPM / controller.MaxEngineRPM) *
        //    controller.EngineTorque * (selectedGear * transmissionModule.diffRatio) * 5252f / controller.EngineRPM;

        enginePower = enginePowerCurve.Evaluate(controller.EngineRPM / controller.MaxEngineRPM) *
              controller.EngineTorque * transmissionModule.diffRatio * 5252f / controller.EngineRPM;

        switch (transmissionModule.autoGear)
        {
            case TruckGearShift.Park:
                {
                    controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, controller.drivePedalPressed ? controller.MinEngineRPM + 2500f : controller.MinEngineRPM,
                        controller.drivePedalPressed ? 0.45f * Time.fixedDeltaTime : Time.fixedDeltaTime * 5f);
                    break;
                }
            case TruckGearShift.Reverse:
                {
                    controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, Mathf.Clamp(controller.drivetrainModule.wheelRPM * selectedGear * transmissionModule.diffRatio,
                        controller.MinEngineRPM, controller.MaxEngineRPM), Time.fixedDeltaTime * 5f);
                    break;
                }
            case TruckGearShift.Neutral:
                {
                    controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, controller.drivePedalPressed ? controller.MaxEngineRPM : controller.MinEngineRPM,
                        controller.drivePedalPressed ? 1f * Time.fixedDeltaTime : Time.fixedDeltaTime * 1.8f);
                    break;
                }
            case TruckGearShift.Drive:
                {
                    controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, Mathf.Clamp(controller.drivetrainModule.wheelRPM * selectedGear * transmissionModule.diffRatio,
                        controller.MinEngineRPM, controller.MaxEngineRPM), Time.fixedDeltaTime * 5f);
                    break;
                }
        }
    }

    public void Update()
    {
        if (!controller.IsOwner) return;
        SyncCarEngineSpeedToOtherClients();
    }

    public void SyncCarEngineSpeedToOtherClients()
    {
        if (!controller.ignitionStarted)
            return;

        float syncThreshold = 0.16f * (controller.averageVelocity.magnitude / 2f);
        syncThreshold = Mathf.Clamp(syncThreshold, 0.16f, 0.38f);
        if (syncCarEngineSpeedInterval > syncThreshold)
        {
            float engineSpeedToSync = Mathf.Round(controller.EngineRPM / 100f) * 100f;
            if (syncedEngineRPM != engineSpeedToSync)
            {
                syncCarEngineSpeedInterval = 0f;
                syncedEngineRPM = engineSpeedToSync;
                SyncCarEngineSpeedRpc(engineSpeedToSync);
                return;
            }
        }
        else
        {
            syncCarEngineSpeedInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarEngineSpeedRpc(float engineSpeed)
    {
        syncedEngineRPM = engineSpeed;
    }
}
