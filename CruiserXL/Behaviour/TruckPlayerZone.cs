using ScanVan.Patches;
using ScanVan.Utils;
using GameNetcodeStuff;
using System.ComponentModel;
using UnityEngine;

namespace ScanVan.Behaviour;

public class TruckPlayerZone : MonoBehaviour
{
    public CruiserXLController controller = null!;
    public bool hasLocalPlayer;
    public int priority;
    public float checkInterval;

    public void OnTriggerStay(Collider other)
    {
        if (controller == null) return;
        if (other.gameObject != GameNetworkManager.Instance.localPlayerController.gameObject) return;
        if (PlayerUtils.seatedInTruck) return;

        switch (priority)
        {
            case 1:
                if (!PlayerUtils.isPlayerInCab && !PlayerUtils.isPlayerInStorage)
                {
                    PlayerUtils.isPlayerOnTruck = true;
                    ResetTimer(true);
                }
                break;
            case 2:
                if (!PlayerUtils.isPlayerInStorage)
                {
                    PlayerUtils.isPlayerOnTruck = true;
                    PlayerUtils.isPlayerInCab = true;
                    ResetTimer(true);
                }
                break;
            case 3:
                if (!PlayerUtils.isPlayerInCab)
                {
                    PlayerUtils.isPlayerOnTruck = true;
                    PlayerUtils.isPlayerInStorage = true;
                    ResetTimer(true);
                }
                break;
        }
    }

    private void ResetTimer(bool hasPlayer)
    {
        checkInterval = 0f;
        hasLocalPlayer = hasPlayer;
    }

    private void Update()
    {
        if (PlayerUtils.seatedInTruck)
        {
            checkInterval = 0f;
            if (priority == 1 || priority == 2) hasLocalPlayer = true;
            else hasLocalPlayer = false;
            return;
        }
        else if ((PlayerUtils.isPlayerInStorage || PlayerUtils.isPlayerInCab) && 
            priority == 1)
        {
            ResetTimer(true);
            return;
        }
        if (!hasLocalPlayer)
        {
            return;
        }
        if (checkInterval <= 0.2f)
        {
            checkInterval += Time.deltaTime;
            return;
        }
        ResetTimer(false);
        if (priority == 1) PlayerUtils.isPlayerOnTruck = false;
        else if (priority == 3) PlayerUtils.isPlayerInStorage = false;
        else PlayerUtils.isPlayerInCab = false;
    }
}