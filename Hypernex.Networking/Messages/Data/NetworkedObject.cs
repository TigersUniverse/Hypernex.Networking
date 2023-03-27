using Nexport;

namespace Hypernex.Networking.Messages.Data;

[Msg]
public class NetworkedObject
{
    [MsgKey(1)] public string MessageId => typeof(NetworkedObject).FullName;
    // This is for Hierarchy based Games like Unity. This can be left empty, but be sure to handle it in your game.
    [MsgKey(2)] public string ObjectLocation;
    [MsgKey(3)] public bool IgnoreObjectLocation;
    [MsgKey(4)] public float3 Position;
    [MsgKey(5)] public float4 Rotation;
    [MsgKey(6)] public float3 Size;
}