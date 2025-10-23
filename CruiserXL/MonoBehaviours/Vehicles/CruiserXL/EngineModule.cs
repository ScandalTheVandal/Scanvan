using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace CruiserXL.MonoBehaviours.Vehicles.CruiserXL;

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
    public bool tryingIgnition;

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
            controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, 0f,
                3f * Time.deltaTime);
            enginePower = 0f;
            return;
        }
        if (!controller.IsOwner) return;
        SyncCarEngineSpeedToOtherClients();
        float selectedGear = Mathf.Abs(transmissionModule.gearRatios[transmissionModule.currentGear]);
        enginePower = enginePowerCurve.Evaluate(controller.EngineRPM / controller.MaxEngineRPM) *
            controller.EngineTorque * (selectedGear * transmissionModule.diffRatio) * 5252f / controller.EngineRPM;

        switch (transmissionModule.autoGear)
        {
            case TruckGearShift.Park:
                {
                    controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, controller.drivePedalPressed ? controller.MinEngineRPM + 2500f : controller.MinEngineRPM,
                        controller.drivePedalPressed ? 0.45f * Time.deltaTime : Time.deltaTime * 5f);
                    break;
                }
            case TruckGearShift.Reverse:
                {
                    controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, Mathf.Clamp(controller.drivetrainModule.wheelRPM * selectedGear * transmissionModule.diffRatio,
                        controller.MinEngineRPM, controller.MaxEngineRPM), Time.deltaTime * 5f);
                    break;
                }
            case TruckGearShift.Neutral:
                {
                    controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, controller.drivePedalPressed ? controller.MaxEngineRPM : controller.MinEngineRPM,
                        controller.drivePedalPressed ? 1f * Time.deltaTime : Time.deltaTime * 1.8f);
                    break;
                }
            case TruckGearShift.Drive:
                {
                    controller.EngineRPM = Mathf.Lerp(controller.EngineRPM, Mathf.Clamp(controller.drivetrainModule.wheelRPM * selectedGear * transmissionModule.diffRatio,
                        controller.MinEngineRPM, controller.MaxEngineRPM), Time.deltaTime * 5f);
                    break;
                }
        }
    }

    public void SyncCarEngineSpeedToOtherClients()
    {
        if (!controller.ignitionStarted)
            return;

        if (syncCarEngineSpeedInterval > 0.165f)
        {
            int engineSpeedToSync = Mathf.RoundToInt(controller.EngineRPM / 100f);
            if (syncedEngineRPM != engineSpeedToSync)
            {
                syncCarEngineSpeedInterval = 0f;
                syncedEngineRPM = engineSpeedToSync;
                SyncCarEngineSpeedServerRpc(engineSpeedToSync);
                return;
            }
        }
        else
        {
            syncCarEngineSpeedInterval += Time.deltaTime;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncCarEngineSpeedServerRpc(float engineSpeed)
    {
        SyncCarEngineSpeedClientRpc(engineSpeed);
    }

    [ClientRpc]
    public void SyncCarEngineSpeedClientRpc(float engineSpeed)
    {
        if (controller.IsOwner)
            return;

        syncedEngineRPM = engineSpeed * 100f;
    }
}
