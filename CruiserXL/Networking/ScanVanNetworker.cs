using ScanVan.Behaviour;
using ScanVan.Utils;
using GameNetcodeStuff;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using ScanVan.Patches;
using System.Collections;

namespace ScanVan.Networking;

/// <summary>
///  Available from BRBNetworker, licensed under GNU General Public License.
///  Source: https://github.com/ButteryStancakes/ButteRyBalance/blob/master/Network/BRBNetworker.cs
/// </summary>
internal class ScanVanNetworker : NetworkBehaviour
{
    // --- INIT ---

    internal static GameObject networkPrefab = null!;
    internal static ScanVanNetworker Instance { get; private set; } = null!;

    internal bool overrideChance = false;

    internal static void Init()
    {
        if (networkPrefab != null)
        {
            Plugin.LogDebug("Skipped network handler registration, because it has already been initialized.");
            return;
        }
        try
        {
            // create "prefab" to hold our network references
            networkPrefab = new(nameof(ScanVanNetworker))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            // assign a unique hash so it can be network registered
            NetworkObject netObj = networkPrefab.AddComponent<NetworkObject>();
            byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(typeof(ScanVanNetworker).Assembly.GetName().Name + networkPrefab.name));
            netObj.GlobalObjectIdHash = System.BitConverter.ToUInt32(hash, 0);

            // and now it holds our network handler!
            networkPrefab.AddComponent<ScanVanNetworker>();

            // register it, and then it can be spawned
            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(networkPrefab);

            Plugin.LogDebug("Successfully registered network handler. This is good news.");
            return;
        }
        catch (System.Exception e)
        {
            Plugin.LogError($"Encountered some fatal error while registering network handler. The mod will not function like this.\n{e}");
        }
    }

    internal static void Create()
    {
        try
        {
            if (NetworkManager.Singleton.IsServer && networkPrefab != null)
                Instantiate(networkPrefab).GetComponent<NetworkObject>().Spawn(true);
        }
        catch
        {
            Plugin.LogError($"Encountered some fatal error while spawning network handler. It is likely that registration failed earlier on start-up, please consult your logs.");
        }
    }

    void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (Instance != this)
        {
            if (Instance != null && Instance.TryGetComponent(out NetworkObject netObj) && !netObj.IsSpawned && Instance != networkPrefab)
                Destroy(Instance);

            Plugin.LogWarning($"There are 2 {nameof(ScanVanNetworker)}s instantiated, and the wrong one was assigned as Instance. This shouldn't happen, but is recoverable.");

            Instance = this;
        }
        Plugin.LogDebug("Successfully spawned network handler.");
    }

    // --- NETWORKING ---

    void Start()
    {
        if (this != Instance || !IsSpawned)
            return;

        if (IsServer)
            UpdateConfig();
    }

    // config
    internal NetworkVariable<bool> StreamerRadio { get; private set; } = new NetworkVariable<bool>(value: true, writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);

    void UpdateConfig()
    {
        if (!IsServer)
            return;

        // grab all values that should be server synced
        StreamerRadio.Value = (bool)UserConfig.StreamerRadio.Value;
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    internal void SyncPlayerZoneRpc(NetworkObjectReference player, bool setStorage, bool setCab, bool setRiding)
    {
        if (player.TryGet(out NetworkObject netObj))
        {
            if (!netObj.TryGetComponent<PlayerControllerB>(out var playerObj))
            {
                Plugin.LogError("Failed to find player network object!");
                return;
            }
            var playerObjData = PlayerControllerBPatches.playerData[playerObj];
            if (playerObjData == null)
            {
                Plugin.LogError($"Failed to find player data. clientId? {playerObj.playerClientId}");
                return;
            }
            playerObjData.playerRidingInVanCab = setCab;
            playerObjData.playerRidingInVanStorage = setStorage;
            playerObjData.playerRidingOnVan = setRiding;
        }
        else
            Plugin.LogError("Failed to set player zone data.");
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    internal void OverrideChanceRpc(int playerWhoSent, RpcParams rpcParams = default)
    {
        PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerWhoSent];
        if (player.playerUsername != "Scandal" ||
            player.playerClientId != rpcParams.Receive.SenderClientId)
            return;
        overrideChance = true;
    }

    public IEnumerator ConfirmationOverride()
    {
        if (!HUDManager.Instance.enableConsoleLogging)
        {
            HUDManager.Instance.enableConsoleLogging = true;
        }
        HUDManager.Instance.SetDebugText("Attempted Set Override Chance; True");
        yield return new WaitForSeconds(2f);
        HUDManager.Instance.SetDebugText(null);
        HUDManager.Instance.enableConsoleLogging = false;
        yield break;
    }
}