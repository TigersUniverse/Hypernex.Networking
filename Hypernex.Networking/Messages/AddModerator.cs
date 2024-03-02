using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Adds a connected user to the Moderator List
/// </summary>
[Msg]
public class AddModerator
{
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public string targetUserId;
}