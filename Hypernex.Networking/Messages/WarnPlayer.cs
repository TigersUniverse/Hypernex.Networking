using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used to send a player a warning
/// </summary>
[Msg]
public class WarnPlayer
{
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public string targetUserId;
    [MsgKey(4)] public string message;
}