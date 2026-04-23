using ScanVan.Utils;
using GameNetcodeStuff;
using UnityEngine;

namespace ScanVan.Behaviour;

public class VehiclePlayerPusher : MonoBehaviour
{
    public CruiserXLController thisController = null!;

    // this is mainly just to push the host-player out the way
    public void OnTriggerStay(Collider other)
    {
        if (thisController.averageVelocity.magnitude > 8f)
            return;

        PlayerControllerB localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (localPlayer == null)
            return;

        if (other.gameObject != localPlayer.gameObject)
            return;

        // do not push a player if they are jumping
        if (localPlayer.isFallingFromJump || localPlayer.isFallingNoJump)
            return;

        Transform physicsTransform = thisController.physicsRegion.physicsTransform;
        if (localPlayer.physicsParent == physicsTransform || localPlayer.overridePhysicsParent == physicsTransform)
            return;

        if (PlayerUtils.seatedInTruck ||
            PlayerUtils.isPlayerOnTruck)
            return;

        Vector3 vehicleVel = (thisController.mainRigidbody.position - thisController.previousVehiclePosition) / Time.fixedDeltaTime;
        Vector3 toPlayer = (localPlayer.transform.position - thisController.mainRigidbody.position).normalized;

        float dirToPlayer = Vector3.Dot(vehicleVel.normalized, toPlayer);
        if (dirToPlayer < 0.8f)
            return;

        localPlayer.externalForceAutoFade += Vector3.ClampMagnitude(vehicleVel.normalized, 5f);
    }
}
