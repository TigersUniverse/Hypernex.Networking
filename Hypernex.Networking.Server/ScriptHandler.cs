using System.Collections.ObjectModel;
using Hypernex.Networking.Libs;
using Hypernex.Networking.Messages;
using Hypernex.Networking.Messages.Data;
using Hypernex.Networking.Server.SandboxedClasses;
using Nexbox;
using Nexbox.Interpreters;
using Nexport;

namespace Hypernex.Networking.Server;

public class ScriptHandler
{
    internal static readonly List<ScriptHandler> Instances = new();
    
    internal HypernexSocketServer Server;
    internal HypernexInstance Instance;
    internal readonly ScriptEvents Events = new ();

    private static readonly ReadOnlyDictionary<string, object> GlobalsToForward =
        new(new Dictionary<string, object>
        {
            
        });
    private static readonly ReadOnlyDictionary<string, Type> TypesToForward = new(
        new Dictionary<string, Type>
        {
            ["float2"] = typeof(float2),
            ["float3"] = typeof(float3),
            ["float4"] = typeof(float4),
            ["Http"] = typeof(HTTP),
            ["WebSocket"] = typeof(ServerWebSocket),
            ["MessageChannel"] = typeof(MessageChannel),
            ["ScriptEvent"] = typeof(ScriptEvent)
        });
    private Dictionary<NexboxScript, IInterpreter> RunningScripts = new();

    internal ScriptHandler(HypernexSocketServer socketServer, HypernexInstance instance)
    {
        Server = socketServer;
        Instance = instance;
        Instance.OnClientConnect += Events.OnUserJoin;
        Instance.OnMessage += (userId, meta, channel) =>
        {
            if (meta.TypeOfData == typeof(NetworkedEvent))
            {
                NetworkedEvent networkedEvent = (NetworkedEvent) Convert.ChangeType(meta.Data, typeof(NetworkedEvent))!;
                Events.OnUserNetworkEvent.Invoke(userId, networkedEvent.EventName, networkedEvent.Data.ToArray());
            }
        };
        Instance.OnClientDisconnect += Events.OnUserLeave;
        Instances.Add(this);
    }

    private void CreateGlobalsForInterpreter(IInterpreter interpreter)
    {
        interpreter.CreateGlobal("script", this);
        interpreter.CreateGlobal("Events", Events);
        interpreter.CreateGlobal("NetworkEvent", new ServerNetworkEvent(this));
        foreach (KeyValuePair<string,object> keyValuePair in GlobalsToForward)
            interpreter.CreateGlobal(keyValuePair.Key, keyValuePair.Value);
    }

    private void ForwardTypesToInterpreter(IInterpreter interpreter)
    {
        foreach (KeyValuePair<string,Type> keyValuePair in TypesToForward)
            interpreter.ForwardType(keyValuePair.Key, keyValuePair.Value);
    }

    private void execute(IInterpreter interpreter, string script, bool multithread)
    {
        if (multithread)
            new Thread(() => interpreter.RunScript(script));
        else
            Task.Factory.StartNew(() => interpreter.RunScript(script));
    }

    internal void LoadAndExecuteScript(NexboxScript script, bool multithread = true)
    {
        switch (script.Language)
        {
            case NexboxLanguage.JavaScript:
                JavaScriptInterpreter javaScriptInterpreter = new JavaScriptInterpreter();
                javaScriptInterpreter.StartSandbox(Console.WriteLine);
                CreateGlobalsForInterpreter(javaScriptInterpreter);
                ForwardTypesToInterpreter(javaScriptInterpreter);
                RunningScripts.Add(script, javaScriptInterpreter);
                execute(javaScriptInterpreter, script.Script, multithread);
                break;
            case NexboxLanguage.Lua:
                LuaInterpreter luaInterpreter = new LuaInterpreter();
                luaInterpreter.StartSandbox(Console.WriteLine);
                CreateGlobalsForInterpreter(luaInterpreter);
                ForwardTypesToInterpreter(luaInterpreter);
                RunningScripts.Add(script, luaInterpreter);
                execute(luaInterpreter, script.Script, multithread);
                break;
        }
    }

    internal void Stop()
    {
        foreach (KeyValuePair<NexboxScript, IInterpreter> keyValuePair in new Dictionary<NexboxScript, IInterpreter>(
                     RunningScripts))
        {
            keyValuePair.Value.Stop();
            RunningScripts.Remove(keyValuePair.Key);
        }
        Instances.Remove(this);
    }

    internal bool Compare(HypernexInstance instance) => Instance.Equals(instance);
}