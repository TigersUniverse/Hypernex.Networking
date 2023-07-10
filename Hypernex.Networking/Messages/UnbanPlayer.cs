using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used to unban a player from an instance
/// </summary>
[Msg]
public class UnbanPlayer
{
    [MsgKey(1)] public string MessageId => typeof(UnbanPlayer).FullName;
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public string targetUserId;
}