using CruiserXL.Utils;
using GameNetcodeStuff;
using UnityEngine;

namespace CruiserXL.Behaviour;

public class VehiclePlayerPusher : MonoBehaviour
{
    public CruiserXLController thisController = null!;

    public void OnTriggerStay(Collider other)
    {
        if (thisController.averageVelocity.magnitude > 8f)
            return;

        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (localPlayer == null)
            return;

        if (other.gameObject != localPlayer.gameObject)
            return;

        if (PlayerUtils.seatedInTruck ||
            PlayerUtils.isPlayerOnTruck)
            return;

        Vector3 vel = thisController.mainRigidbody.position - thisController.previousVehiclePosition;
        localPlayer.externalForceAutoFade += (vel * 1.5f) / Time.fixedDeltaTime;
    }
}
