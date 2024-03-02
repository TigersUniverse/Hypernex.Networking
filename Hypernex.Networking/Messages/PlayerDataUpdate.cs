using System;
using System.Collections.Generic;
using System.Linq;
using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
[MsgCompress(22)]
public class PlayerDataUpdate
{
    // Player Meta
    [MsgKey(2)] public JoinAuth Auth;

    /// <summary>
    /// Any extraneous data to forward
    /// </summary>
    [MsgKey(3)] public Dictionary<string, object> ExtraneousData;

    /// <summary>
    /// Any Player Assigned tags for things such as badges
    /// </summary>
    [MsgKey(4)] public List<string> PlayerAssignedTags;

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Type type = obj.GetType();
        if (type != typeof(PlayerDataUpdate)) return false;
        PlayerDataUpdate compare = (PlayerDataUpdate) obj;
        if (compare.Auth.UserId != Auth.UserId) return false;
        if (compare.Auth.TempToken != Auth.TempToken) return false;
        if ((ExtraneousData != null && compare.ExtraneousData == null) ||
            (ExtraneousData == null && compare.ExtraneousData != null)) return false;
        if(ExtraneousData != null && compare.ExtraneousData != null)
        {
            if (ExtraneousData.Count != compare.ExtraneousData.Count) return false;
            for (int i = 0; i < ExtraneousData.Count; i++)
            {
                string key1 = ExtraneousData.Keys.ElementAt(i);
                object value1 = ExtraneousData.Values.ElementAt(i);
                string key2 = compare.ExtraneousData.Keys.ElementAt(i);
                object value2 = compare.ExtraneousData.Values.ElementAt(i);
                if (key1 != key2 || value1 != value2) return false;
            }
        }
        if ((PlayerAssignedTags != null && compare.PlayerAssignedTags == null) ||
            (PlayerAssignedTags == null && compare.PlayerAssignedTags != null)) return false;
        if(PlayerAssignedTags != null && compare.PlayerAssignedTags != null)
        {
            if (PlayerAssignedTags.Count != compare.PlayerAssignedTags.Count) return false;
            for (int i = 0; i < PlayerAssignedTags.Count; i++)
            {
                string tag1 = PlayerAssignedTags.ElementAt(i);
                string tag2 = compare.PlayerAssignedTags.ElementAt(i);
                if (tag1 != tag2) return false;
            }
        }
        return true;
    }
}