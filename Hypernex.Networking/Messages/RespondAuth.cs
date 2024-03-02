using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used by the server to let the client know it is authed and can start sending messages.
/// </summary>
[Msg]
public class RespondAuth
{
    [MsgKey(2)] public string UserId;
    [MsgKey(3)] public string GameServerId;
    [MsgKey(4)] public string InstanceId;
}