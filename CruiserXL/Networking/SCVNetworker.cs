using CruiserXL.Utils;
using GameNetcodeStuff;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Unity.Netcode;
using UnityEngine;

namespace CruiserXL.Networking;

/// <summary>
///  Available from BRBNetworker, licensed under GNU General Public License.
///  Source: https://github.com/ButteryStancakes/ButteRyBalance/blob/master/Network/BRBNetworker.cs
/// </summary>
internal class SCVNetworker : NetworkBehaviour
{
    // --- INIT ---

    internal static GameObject networkPrefab = null!;
    internal static SCVNetworker? Instance { get; private set; }

    internal static void Init()
    {
        if (networkPrefab != null)
        {
            Plugin.Logger.LogDebug("Skipped network handler registration, because it has already been initialized");
            return;
        }
        try
        {
            // create "prefab" to hold our network references
            networkPrefab = new(nameof(SCVNetworker))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            // assign a unique hash so it can be network registered
            NetworkObject netObj = networkPrefab.AddComponent<NetworkObject>();
            byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(typeof(SCVNetworker).Assembly.GetName().Name + networkPrefab.name));
            netObj.GlobalObjectIdHash = System.BitConverter.ToUInt32(hash, 0);

            // and now it holds our network handler!
            networkPrefab.AddComponent<SCVNetworker>();

            // register it, and then it can be spawned
            NetworkManager.Singleton.PrefabHandler.AddNetworkPrefab(networkPrefab);

            Plugin.Logger.LogDebug("Successfully registered network handler. This is good news!");
            return;
        }
        catch (System.Exception e)
        {
            Plugin.Logger.LogError($"Encountered some fatal error while registering network handler. The mod will not function like this!\n{e}");
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
            Plugin.Logger.LogError($"Encountered some fatal error while spawning network handler. It is likely that registration failed earlier on start-up, please consult your logs.");
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

            Plugin.Logger.LogWarning($"There are 2 {nameof(SCVNetworker)}s instantiated, and the wrong one was assigned as Instance. This shouldn't happen, but is recoverable");

            Instance = this;
        }
        Plugin.Logger.LogDebug("Successfully spawned network handler.");
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
    internal NetworkVariable<bool> StreamerRadio { get; private set; } = new NetworkVariable<bool>(value: false, writePerm: NetworkVariableWritePermission.Server, readPerm: NetworkVariableReadPermission.Everyone);

    void UpdateConfig()
    {
        if (!IsServer)
            return;

        // grab all values that should be server synced
        StreamerRadio.Value = (bool)UserConfig.StreamerRadio.Value;
    }
}