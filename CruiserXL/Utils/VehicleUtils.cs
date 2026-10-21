using GameNetcodeStuff;
using ScanVan.Patches;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ScanVan.Utils;
public static class VehicleUtils
{
    internal static CruiserXLController vanController = null!;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // internal is like public, but only we can use this!
    internal static bool IsPlayerLocal(PlayerControllerB? player)
    {
        return player != null && player == GameNetworkManager.Instance.localPlayerController;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPlayerNearVan(PlayerControllerB? playerScript, CruiserXLController? vanScript)
    {
        return playerScript != null && vanScript != null && Vector3.Distance(playerScript.transform.position, vanScript.mainRigidbody.position) <= 10f;
    }
}