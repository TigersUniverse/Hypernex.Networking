using Hypernex.Networking.Messages.Data;
using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
public class PlayerObjectUpdate
{
    [MsgKey(1)] public string MessageId => typeof(PlayerObjectUpdate).FullName;
    
    // Player Meta
    
    [MsgKey(2)] public JoinAuth Auth;
    
    /// <summary>
    /// The Object to Track. Each Value should be the NetworkedObject of the object
    /// (Position, Rotation, Size, etc.) Size can be ignored depending on the object being tracked, but it may vary
    /// depending on what the object being tracked is mapped to.
    /// </summary>
    [MsgKey(3)] public NetworkedObject Object;
}