using System.Collections.Generic;
using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used to tell clients when a User Joins/Leaves. Also used to get a List of players when a player joins.
/// </summary>
[Msg]
public class InstancePlayers
{
    [MsgKey(2)] public Dictionary<ClientIdentifier, string> UserIds = new ();
    [MsgKey(3)] public string HostId;
}