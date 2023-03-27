using System.Collections.Generic;
using Hypernex.Networking.Messages.Data;
using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// The main update object for a Player. This will send all data such as location of the player, extra-sensory
/// data, and GameServer-only data.
/// </summary>
[Msg]
public class PlayerUpdate
{
    // Message Meta
    
    [MsgKey(1)] public string MessageId => typeof(PlayerUpdate).FullName;
    
    // Player Meta
    
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public bool IsPlayerVR;
    [MsgKey(4)] public string AvatarId;
    /// <summary>
    /// Can be used for player assigned badges, names, etc. Must be handled by the client.
    /// </summary>
    [MsgKey(5)] public List<string> PlayerAssignedTags;

    // Player Tracking
    
    /// <summary>
    /// A List containing every Tracked Object. Each Value should be the NetworkedObject of the object
    /// (Position, Rotation, Size, etc.) Size can be ignored depending on the object being tracked, but it may vary
    /// depending on what the object being tracked is mapped to. This can be used injunction with IsPlayerVR to know if
    /// only Head needs to be tracked.
    /// </summary>
    [MsgKey(6)] public List<NetworkedObject> TrackedObjects;
    /// <summary>
    /// A Dictionary containing object's weights. This can be used for things like facial tracking, animator
    /// parameters, or other extraneous weights.
    /// </summary>
    [MsgKey(7)] public Dictionary<string, float> WeightedObjects;
}