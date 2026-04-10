using CruiserXL.Utils;
using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace CruiserXL.Behaviour;

public class TruckMirrorRenderer : MonoBehaviour
{
    public CruiserXLController mainTruckScript = null!;

    public MeshRenderer leftMirrorMesh = null!;
    public MeshRenderer centerMirrorMesh = null!;
    public MeshRenderer rightMirrorMesh = null!;

    public MeshRenderer[] mirrorMeshes = null!;
    public Camera[] mirrorCameras = null!;

    public bool playerInTruck;
    public float elapsed;
    public float cameraFramerate;
    public int camerasToRenderPerFrame = 1;
    public int nextCameraToRender = 0;
    public float cameraRenderCountRemainder = 0f;

    // kudos to Zaggy for being my teacher through-out this! /p1
    public void Awake()
    {
        mirrorCameras[0].farClipPlane = 30f;
        mirrorCameras[2].farClipPlane = 30f;
        cameraFramerate = 40f;
    }

    /// <summary>
    ///  Available from Black Mesa, licensed under MIT License.
    ///  Source: https://github.com/PlasteredCrab/BlackMesa/commit/59738a8107bc7c6846a175fcd4420b4da80483d2
    /// </summary>
    public void LateUpdate()
    {
        if (mainTruckScript == null)
            return;

        if (!mainTruckScript.IsSpawned || mainTruckScript.carDestroyed)
            return;

        if (mirrorCameras[0] == null ||
            mirrorCameras[1] == null ||
            mirrorCameras[2] == null ||
            leftMirrorMesh == null ||
            centerMirrorMesh == null ||
            rightMirrorMesh == null)
            return;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null) 
            return;

        foreach (var camera in mirrorCameras)
        {
            if (camera == null)
                continue;
            camera.enabled = false;
        }

        if (!PlayerUtils.seatedInTruck)
        {
            leftMirrorMesh.enabled = false;
            centerMirrorMesh.enabled = false;
            rightMirrorMesh.enabled = false;
            return;
        }

        elapsed += Time.deltaTime;
        if (elapsed < 1f / cameraFramerate)
            return;
        elapsed = 0f;

        leftMirrorMesh.enabled = true;
        centerMirrorMesh.enabled = true;
        rightMirrorMesh.enabled = true;

        var activeCamCount = 0;
        for (var i = 0; i < 3; i++)
        {
            if (!mirrorMeshes[i].IsVisibleToPlayersLocalCamera(player.gameplayCamera))
                continue;
            activeCamCount++;
        }

        var renderCountIncrement = (float)camerasToRenderPerFrame * activeCamCount / 3;
        cameraRenderCountRemainder += renderCountIncrement;

        var stopIndex = (nextCameraToRender + mirrorCameras.Length - 1) % mirrorCameras.Length;
        while (cameraRenderCountRemainder >= 0)
        {
            if (mirrorMeshes[nextCameraToRender].IsVisibleToPlayersLocalCamera(player.gameplayCamera))
            {
                mirrorCameras[nextCameraToRender].enabled = true;
                cameraRenderCountRemainder--;
            }
            nextCameraToRender = (nextCameraToRender + 1) % mirrorCameras.Length;
            if (nextCameraToRender == stopIndex)
                break;
        }
    }
}
