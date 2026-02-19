using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace CruiserXL.Behaviour;

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

    public float syncCarMotorTorqueInterval;
    public float syncCarWheelSpeedInterval;
    public float syncedMotorTorque;
    public float syncedBrakeTorque;
    public float syncedWheelRPM;

    public void Start()
    {
        upShiftThreshold = 4700f;
        downShiftThreshold = 2100f;
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
                currentGear = 1;
                forwardWheelSpeed = 8000f;
                reverseWheelSpeed = -8000f;
                break;
            case TruckGearShift.Neutral:
                currentGear = 1;
                forwardWheelSpeed = 8000f;
                reverseWheelSpeed = -8000f;
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
    }

    public void Update()
    {
        if (!controller.IsOwner) return;
        SyncCarWheelSpeedToOtherClients();
        SyncCarMotorTorqueToOtherClients();
    }

    public void SyncCarMotorTorqueToOtherClients()
    {
        float syncThreshold = 0.14f * (controller.averageVelocity.magnitude / 2f);
        syncThreshold = Mathf.Clamp(syncThreshold, 0.14f, 0.5f);

        if (syncCarMotorTorqueInterval >= syncThreshold)
        {
            float motorTorqueSync = Mathf.Floor(controller.wheelTorque / 10f) * 10f;
            float brakeTorqueSync = Mathf.Floor(controller.wheelBrakeTorque / 10f) * 10f;

            if (syncedMotorTorque != motorTorqueSync ||
                syncedBrakeTorque != brakeTorqueSync)
            {
                syncCarMotorTorqueInterval = 0f;
                syncedMotorTorque = motorTorqueSync;
                syncedBrakeTorque = brakeTorqueSync;
                SyncCarMotorTorqueRpc(controller.wheelTorque, controller.wheelBrakeTorque);
                return;
            }
        }
        else
        {
            syncCarMotorTorqueInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarMotorTorqueRpc(float motorTorque, float brakeTorque)
    {
        syncedMotorTorque = motorTorque;
        syncedBrakeTorque = brakeTorque;
    }

    public void SyncCarWheelSpeedToOtherClients()
    {
        float syncThreshold = 0.15f * (controller.averageVelocity.magnitude / 2f);
        syncThreshold = Mathf.Clamp(syncThreshold, 0.15f, 0.3f);
        if (syncCarWheelSpeedInterval >= syncThreshold)
        {
            float wheelSyncRPM = Mathf.Floor(wheelRPM / 5f) * 5f;
            if (syncedWheelRPM != wheelSyncRPM)
            {
                syncCarWheelSpeedInterval = 0f;
                syncedWheelRPM = wheelSyncRPM;
                SyncCarWheelSpeedRpc(wheelRPM);
                return;
            }
        }
        else
        {
            syncCarWheelSpeedInterval += Time.deltaTime;
        }
    }

    [Rpc(SendTo.NotOwner, RequireOwnership = false)]
    public void SyncCarWheelSpeedRpc(float wheelRPM)
    {
        syncedWheelRPM = wheelRPM;
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
