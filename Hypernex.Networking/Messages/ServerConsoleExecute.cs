using Hypernex.CCK;
using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
public class ServerConsoleExecute
{
    [MsgKey(2)] public JoinAuth Auth;
    [MsgKey(3)] public NexboxLanguage Language;
    [MsgKey(4)] public string ScriptText;
}