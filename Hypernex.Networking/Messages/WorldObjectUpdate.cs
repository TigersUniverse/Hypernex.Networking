using Hypernex.Networking.Messages.Data;
using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
[MsgCompress(22)]
public class WorldObjectUpdate
{
    [MsgKey(2)] public JoinAuth Auth;
    
    /// <summary>
    /// This defines what the client wishes to do with the object, if they are given authority over it.
    /// </summary>
    [MsgKey(3)] public WorldObjectAction Action;

    /// <summary>
    /// Defines if a WorldObjectUpdate can be stolen by another client.
    /// </summary>
    [MsgKey(4)] public bool CanBeStolen;
    
    /// <summary>
    /// The Object to Track. Each Value should be the NetworkedObject of the object
    /// (Position, Rotation, Size, etc.) Size can be ignored depending on the object being tracked, but it may vary
    /// depending on what the object being tracked is mapped to.
    /// </summary>
    [MsgKey(5)] public NetworkedObject Object;

    /// <summary>
    /// Used to compensate for NetworkLoss. Prone to DeSync.
    /// </summary>
    [MsgKey(6)] public float3 Velocity;
}