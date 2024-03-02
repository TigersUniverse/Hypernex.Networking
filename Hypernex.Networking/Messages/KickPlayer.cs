using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used to remove a player from an instance
/// </summary>
[Msg]
public class KickPlayer
{
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public string targetUserId;
    [MsgKey(4)] public string message;
}