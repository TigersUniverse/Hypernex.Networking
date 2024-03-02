using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Removes a connected user to the Moderator List
/// </summary>
[Msg]
public class RemoveModerator
{
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public string targetUserId;
}