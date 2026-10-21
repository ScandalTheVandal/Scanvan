using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace ScanVan.Scripts;

[Serializable]
public struct VoiceStruct : INetworkSerializable
{
    public int clipId;
    public bool isThankYou;
    public bool isImportant;
    public bool isIgnition;
    public int importantType;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clipId);
        serializer.SerializeValue(ref isThankYou);
        serializer.SerializeValue(ref isImportant);
        serializer.SerializeValue(ref isIgnition);
        serializer.SerializeValue(ref importantType);
    }

    /*
    public override string ToString()
    {
        return base.ToString();
    }
    */
}
