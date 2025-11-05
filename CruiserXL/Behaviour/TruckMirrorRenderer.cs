using CruiserXL.Utils;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Scripting.APIUpdating;

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

    // zaggy zagster, thank you so much for helping me out with this!
    public void Awake()
    {
        mirrorCameras[0].farClipPlane = 30f;
        mirrorCameras[2].farClipPlane = 30f;

        // ??
        mirrorCameras[0].gameObject.GetComponent<HDAdditionalCameraData>().hasPersistentHistory = false;
        mirrorCameras[1].gameObject.GetComponent<HDAdditionalCameraData>().hasPersistentHistory = false;
        mirrorCameras[2].gameObject.GetComponent<HDAdditionalCameraData>().hasPersistentHistory = false;

        mirrorCameras[0].gameObject.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
        mirrorCameras[1].gameObject.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
        mirrorCameras[2].gameObject.GetComponent<HDAdditionalCameraData>().antialiasing = HDAdditionalCameraData.AntialiasingMode.None;

        //mirrorCameras[0].gameObject.GetComponent<HDAdditionalCameraData>().volumeAnchorOverride = null;
        //mirrorCameras[1].gameObject.GetComponent<HDAdditionalCameraData>().volumeAnchorOverride = null;
        //mirrorCameras[2].gameObject.GetComponent<HDAdditionalCameraData>().volumeAnchorOverride = null;

        mirrorCameras[0].gameObject.GetComponent<HDAdditionalCameraData>().stopNaNs = false;
        mirrorCameras[1].gameObject.GetComponent<HDAdditionalCameraData>().stopNaNs = false;
        mirrorCameras[2].gameObject.GetComponent<HDAdditionalCameraData>().stopNaNs = false;

        mirrorCameras[0].gameObject.GetComponent<HDAdditionalCameraData>().dithering = false;
        mirrorCameras[1].gameObject.GetComponent<HDAdditionalCameraData>().dithering = false;
        mirrorCameras[2].gameObject.GetComponent<HDAdditionalCameraData>().dithering = false;

        mirrorCameras[0].gameObject.GetComponent<HDAdditionalCameraData>().allowDynamicResolution = false;
        mirrorCameras[1].gameObject.GetComponent<HDAdditionalCameraData>().allowDynamicResolution = false;
        mirrorCameras[2].gameObject.GetComponent<HDAdditionalCameraData>().allowDynamicResolution = false;

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

        if (mirrorCameras[0] == null ||
            mirrorCameras[1] == null ||
            mirrorCameras[2] == null ||
            leftMirrorMesh == null ||
            centerMirrorMesh == null ||
            rightMirrorMesh == null)
            return;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null) return;

        playerInTruck = player == mainTruckScript.currentDriver ||
            player == mainTruckScript.currentMiddlePassenger ||
            player == mainTruckScript.currentPassenger;

        foreach (var camera in mirrorCameras)
        {
            if (camera == null)
                continue;
            camera.enabled = false;
        }

        if (!playerInTruck)
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
