using GameNetcodeStuff;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace CruiserXL.MonoBehaviours.Vehicles.CruiserXL;

public class DrivetrainModule : NetworkBehaviour
{
    [Header("Drivetrain System")]

    public CruiserXLController controller = null!;
    public Coroutine automaticTransmissionCoroutine = null!;
    public TruckGearShift autoGear;

    public float[] gearRatios = null!;
    public float diffRatio;
    public int currentGear;
    public float upShiftThreshold = 2500f;
    public float downShiftThreshold = 1100f;
    public float lastShiftTime = 0f;
    public float shiftCooldown = 0.6f;
    public float shiftTime = 0.15f;
    public float forwardWheelSpeed;
    public float reverseWheelSpeed;
    public float wheelRPM;

    [Header("Multiplayer")]

    public float syncCarDrivetrainInterval;
    public float syncedMotorTorque;
    public float syncedBrakeTorque;
    public float syncedWheelRPM;

    public void Start()
    {
        upShiftThreshold = 4500f;
        downShiftThreshold = 1100f;
        diffRatio = 5.2f;
    }

    public void FixedUpdate()
    {
        if (controller == null || !controller.IsSpawned ||
            !controller.IsOwner || controller.carDestroyed) return;

        wheelRPM = Mathf.Abs((controller.BackLeftWheel.rpm + controller.BackRightWheel.rpm) / 2f);
        switch (autoGear)
        {
            case TruckGearShift.Reverse:
                currentGear = 0;
                // this has to be inverted for reverse
                forwardWheelSpeed = controller.MaxEngineRPM / (gearRatios[Mathf.Clamp(currentGear, gearRatios.Length - 1, 1)] * diffRatio) * (360f / 60f);
                reverseWheelSpeed = controller.MaxEngineRPM / (gearRatios[0] * diffRatio) * (360f / 60f);
                break;
            case TruckGearShift.Park:
            case TruckGearShift.Neutral:
                currentGear = 1;
                forwardWheelSpeed = 3000f;
                reverseWheelSpeed = -3000f;
                break;
            case TruckGearShift.Drive:
                if (currentGear < 1) // do not let the current gear drop below its minimum
                    currentGear = 1;

                forwardWheelSpeed = controller.MaxEngineRPM /
                    (gearRatios[Mathf.Clamp(currentGear,
                    1,
                    gearRatios.Length - 1)] * diffRatio) * (360f / 60f); // ensure we don't set a reverse speed on the forward speed
                reverseWheelSpeed = controller.MaxEngineRPM /
                    (gearRatios[0] * diffRatio) * (360f / 60f); // 0 in our array is always reverse, so use zero for the backwards speed
                if (Time.time - lastShiftTime > shiftCooldown)
                {
                    // attempt to change up, or down, a gear
                    if (controller.EngineRPM >= upShiftThreshold && currentGear < gearRatios.Length - 1)
                    {
                        TryShiftGear(true);
                    }
                    else if (controller.EngineRPM <= downShiftThreshold && currentGear > 1)
                    {
                        TryShiftGear(false);
                    }
                }
                break;
        }
        SyncCarDrivetrainToOtherClients();
    }

    public void SyncCarDrivetrainToOtherClients()
    {
        if (syncCarDrivetrainInterval > 0.1475f)
        {
            bool shouldSync = false;
            if (syncedWheelRPM != wheelRPM ||
                syncedMotorTorque != controller.wheelTorque ||
                syncedBrakeTorque != controller.wheelBrakeTorque)
            {
                shouldSync = true;
            }
            if (shouldSync)
            {
                syncedWheelRPM = wheelRPM;
                syncedMotorTorque = controller.wheelTorque;
                syncedBrakeTorque = controller.wheelBrakeTorque;
                SyncCarDrivetrainServerRpc(wheelRPM, controller.wheelTorque, controller.wheelBrakeTorque);
            }
            syncCarDrivetrainInterval = 0f;
        }
        else
        {
            syncCarDrivetrainInterval += Time.deltaTime;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SyncCarDrivetrainServerRpc(float wheelRPM, float motorTorque, float brakeTorque)
    {
        SyncCarDrivetrainClientRpc(wheelRPM, motorTorque, brakeTorque);
    }

    [ClientRpc]
    public void SyncCarDrivetrainClientRpc(float wheelRPM, float motorTorque, float brakeTorque)
    {
        if (controller.IsOwner)
            return;

        syncedWheelRPM = wheelRPM;
        syncedMotorTorque = motorTorque;
        syncedBrakeTorque = brakeTorque;
    }

    private void TryShiftGear(bool upOrDown)
    {
        if (automaticTransmissionCoroutine != null)
            return;

        automaticTransmissionCoroutine = StartCoroutine(ChangeGearAfterSeconds(upOrDown));
    }

    private IEnumerator ChangeGearAfterSeconds(bool upOrDown)
    {
        yield return new WaitForSeconds(shiftTime);

        if (upOrDown && controller.EngineRPM >= upShiftThreshold && currentGear < gearRatios.Length - 1)
        {
            currentGear++;
        }
        else if (!upOrDown && controller.EngineRPM <= downShiftThreshold && currentGear > 1)
        {
            currentGear--;
        }

        lastShiftTime = Time.time;
        automaticTransmissionCoroutine = null!;
    }

}
