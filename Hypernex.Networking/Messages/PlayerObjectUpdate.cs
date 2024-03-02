using System.Collections.Generic;
using Hypernex.Networking.Messages.Data;
using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
[MsgCompress(22)]
public class PlayerObjectUpdate
{
    // Player Meta
    [MsgKey(2)] public JoinAuth Auth;

    /// <summary>
    /// The CoreObjects to synchronize
    /// </summary>
    [MsgKey(3)] public Dictionary<int, NetworkedObject> Objects;
}