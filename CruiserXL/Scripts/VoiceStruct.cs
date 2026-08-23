using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

namespace ScanVan.Scripts;

[Serializable]
public struct VoiceStruct : INetworkSerializable
{
    public int clipId;
    public bool hasThankYou;
    public bool isImportant;
    public bool isIgnition;
    public int importantType;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clipId);
        serializer.SerializeValue(ref hasThankYou);
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
