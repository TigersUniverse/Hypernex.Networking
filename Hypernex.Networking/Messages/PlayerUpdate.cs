using System;
using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// The main update object for a Player. This will send all data such as location of the player, extra-sensory
/// data, and GameServer-only data.
/// </summary>
[Msg]
[MsgCompress(22)]
public class PlayerUpdate
{
    // Player Meta
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public bool IsPlayerVR;
    [MsgKey(4)] public string AvatarId;
    [MsgKey(5)] public bool IsSpeaking;
    [MsgKey(6)] public bool IsFBT;
    [MsgKey(7)] public string VRIKJson;

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Type type = obj.GetType();
        if (type != typeof(PlayerUpdate)) return false;
        PlayerUpdate compare = (PlayerUpdate) obj;
        if (compare.Auth.UserId != Auth.UserId) return false;
        if (compare.Auth.TempToken != Auth.TempToken) return false;
        if (compare.IsPlayerVR != IsPlayerVR) return false;
        if (compare.AvatarId != AvatarId) return false;
        if (compare.IsSpeaking != IsSpeaking) return false;
        if (compare.IsFBT != IsFBT) return false;
        if (compare.VRIKJson != VRIKJson) return false;
        return true;
    }
}