using System.Collections.Generic;
using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used to send data from a Client to a Server, or from a Server to a(n) client(s)
/// </summary>
[Msg]
public class NetworkedEvent
{
    [MsgKey(1)] public string MessageId => typeof(NetworkedEvent).FullName;
    // Optional if from Server to Client
    [MsgKey(2)] public JoinAuth Auth;
    /// <summary>
    /// The name of the event to trigger
    /// </summary>
    [MsgKey(3)] public string EventName;
    /// <summary>
    /// The data to send between the server. This data MUST be serializable!
    /// </summary>
    [MsgKey(4)] public List<object> Data;
}