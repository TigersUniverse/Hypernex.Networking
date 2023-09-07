using Hypernex.CCK;
using Hypernex.Networking.Messages;
using Hypernex.Networking.Messages.Data;
using Hypernex.Networking.Server.SandboxedClasses;
using Hypernex.Sandboxing.SandboxedTypes;
using Nexbox;
using Nexbox.Interpreters;
using Nexport;

namespace Hypernex.Networking.Server;

public class ScriptHandler : IDisposable
{
    internal static readonly List<ScriptHandler> Instances = new();
    
    internal HypernexSocketServer Server;
    internal HypernexInstance Instance;
    internal readonly ScriptEvents Events;
    internal readonly Queue<Action> AwaitingTasks = new();
    internal readonly Mutex m = new ();
    private readonly CancellationTokenSource cts = new();
    private bool disposed;

    private static readonly Dictionary<string, object> GlobalsToForward = new Dictionary<string, object>
    {

    };

    private static readonly Dictionary<string, Type> TypesToForward = new Dictionary<string, Type>
    {
        ["float2"] = typeof(float2),
        ["float3"] = typeof(float3),
        ["float4"] = typeof(float4),
        ["Http"] = typeof(HTTP),
        ["WebSocket"] = typeof(ServerWebSocket),
        ["ServerNetworkEvent"] = typeof(ServerNetworkEvent),
        ["MessageChannel"] = typeof(MessageChannel),
        ["ScriptEvent"] = typeof(ScriptEvent),
        ["HttpMediaType"] = typeof(HttpMediaType),
        ["OfflineNetworkedObject"] = typeof(OfflineNetworkedObject),
        ["NetPlayer"] = typeof(NetPlayer),
        ["Time"] = typeof(Time),
        ["UtcTime"] = typeof(UtcTime),
        ["ScriptEvents"] = typeof(ScriptEvents)
    };
    
    private Dictionary<NexboxScript, IInterpreter> RunningScripts = new();

    internal ScriptHandler(HypernexSocketServer socketServer, HypernexInstance instance)
    {
        Events = new ScriptEvents(this);
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
        interpreter.CreateGlobal("Events", Events);
        interpreter.CreateGlobal("NetworkEvent", new ServerNetworkEvent(this));
        interpreter.CreateGlobal("Players", new NetPlayers(Instance));
        foreach (KeyValuePair<string,object> keyValuePair in GlobalsToForward)
            interpreter.CreateGlobal(keyValuePair.Key, keyValuePair.Value);
    }

    private void ForwardTypesToInterpreter(IInterpreter interpreter)
    {
        foreach (KeyValuePair<string,Type> keyValuePair in TypesToForward)
            interpreter.ForwardType(keyValuePair.Key, keyValuePair.Value);
    }

    private void e(IInterpreter interpreter, string script)
    {
        try
        {
            interpreter.RunScript(script, Console.WriteLine);
            while (!cts.IsCancellationRequested)
            {
                if (AwaitingTasks.Count > 0)
                {
                    if (m.WaitOne(5))
                    {
                        Action a = AwaitingTasks.Dequeue();
                        a.Invoke();
                        m.ReleaseMutex();
                    }
                }
                Thread.Sleep(ServerConfig.LoadedConfig.ThreadUpdate);
            }
        }catch(Exception){Dispose();}
    }

    private void execute(IInterpreter interpreter, string script, bool multithread)
    {
        if (multithread)
            new Thread(() => e(interpreter, script)).Start();
        else
            Task.Factory.StartNew(() => e(interpreter, script));
    }

    private void OnPrint(string scriptName, object o)
    {
        foreach (string userid in Instance.ConnectedClients)
            if (userid == Instance.WorldOwnerId)
            {
                // Send the world owner the server log
                ServerConsoleLog serverConsoleLog = new ServerConsoleLog
                {
                    ScriptName = scriptName,
                    Log = o.ToString()
                };
                byte[] msg = Msg.Serialize(serverConsoleLog);
                Instance.SendMessageToClient(Instance.GetClientIdentifierFromUserId(userid), msg);
                // We can also send all moderators of the instance this message, IF the instance owner is also the world owner
                if (userid != Instance.InstanceCreatorId) continue;
                foreach (string moderator in Instance.Moderators)
                    Instance.SendMessageToClient(Instance.GetClientIdentifierFromUserId(moderator), msg);
            }
    }

    internal void LoadAndExecuteScript(NexboxScript script, bool multithread = true)
    {
        if (disposed)
            return;
        switch (script.Language)
        {
            case NexboxLanguage.JavaScript:
                JavaScriptInterpreter javaScriptInterpreter = new JavaScriptInterpreter();
                javaScriptInterpreter.StartSandbox(o => OnPrint(script.Name + script.GetExtensionFromLanguage(), o));
                CreateGlobalsForInterpreter(javaScriptInterpreter);
                ForwardTypesToInterpreter(javaScriptInterpreter);
                RunningScripts.Add(script, javaScriptInterpreter);
                execute(javaScriptInterpreter, script.Script, multithread);
                break;
            case NexboxLanguage.Lua:
                LuaInterpreter luaInterpreter = new LuaInterpreter();
                luaInterpreter.StartSandbox(o => OnPrint(script.Name + script.GetExtensionFromLanguage(), o));
                CreateGlobalsForInterpreter(luaInterpreter);
                ForwardTypesToInterpreter(luaInterpreter);
                RunningScripts.Add(script, luaInterpreter);
                execute(luaInterpreter, script.Script, multithread);
                break;
        }
    }

    internal bool Compare(HypernexInstance instance) => Instance.Equals(instance);

    public void Dispose()
    {
        if (disposed)
            return;
        foreach (KeyValuePair<NexboxScript, IInterpreter> keyValuePair in new Dictionary<NexboxScript, IInterpreter>(
                     RunningScripts))
        {
            keyValuePair.Value.Stop();
            RunningScripts.Remove(keyValuePair.Key);
        }
        Instances.Remove(this);
        m?.Dispose();
        cts?.Dispose();
        disposed = true;
    }
}