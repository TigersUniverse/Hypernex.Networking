using System.Collections.Generic;
using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// A chat message that a Player wants to broadcast
/// </summary>
[Msg]
public class PlayerMessage
{
    [MsgKey(2)] public JoinAuth Auth;
    /// <summary>
    /// Used to define optional tags of a message, such as who can and cannot see a message
    /// </summary>
    [MsgKey(3)] public List<string> MessageTags;
    /// <summary>
    /// The message to send
    /// </summary>
    [MsgKey(4)] public string Message;
}