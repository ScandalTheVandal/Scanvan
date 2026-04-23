using Unity.Netcode;
using UnityEngine;

namespace ScanVan.Networking;

static class NetworkVariableInitalizer
{
    internal static void Init()
    {
        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<bool>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<bool>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<float>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<float>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<double>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<double>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<int>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<int>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<Vector2>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<Vector2>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<Vector3>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<Vector3>();

        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<bool>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<bool>();
    }
}