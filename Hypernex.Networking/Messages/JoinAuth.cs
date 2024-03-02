using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used by the Client to verify with the connecting GameServer. The TempToken will be given to the User from the
/// SocketServer.
/// </summary>
[Msg]
public class JoinAuth
{
    [MsgKey(2)] public string UserId;
    [MsgKey(3)] public string TempToken;
}