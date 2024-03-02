using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used to ban a player from an instance
/// </summary>
[Msg]
public class BanPlayer
{
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public string targetUserId;
    [MsgKey(4)] public string message;
}