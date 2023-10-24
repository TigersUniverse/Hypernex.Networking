using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
public class ResetWeightedObjects
{
    [MsgKey(1)] public string MessageId => typeof(ResetWeightedObjects).FullName;
    [MsgKey(2)] public JoinAuth Auth;
}