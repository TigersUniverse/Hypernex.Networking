using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
public class ServerConsoleLog
{
    [MsgKey(2)] public int LogLevel;
    [MsgKey(3)] public string ScriptName;
    [MsgKey(4)] public string Log;
}