using System.Collections.ObjectModel;
using Hypernex.Networking.Libs;
using Hypernex.Networking.Messages.Data;
using Nexbox;
using Nexbox.Interpreters;

namespace Hypernex.Networking.Server;

public class ScriptHandler
{
    internal static readonly List<ScriptHandler> Instances = new();

    private static readonly ReadOnlyDictionary<string, object> GlobalsToForward =
        new(new Dictionary<string, object>
        {
            
        });
    private static readonly ReadOnlyDictionary<string, Type> TypesToForward = new(
        new Dictionary<string, Type>
        {
            ["float2"] = typeof(float2),
            ["float3"] = typeof(float3),
            ["float4"] = typeof(float4)
        });
    private HypernexSocketServer Server;
    private HypernexInstance Instance;
    private Dictionary<NexboxScript, IInterpreter> RunningScripts = new();

    public ScriptHandler(HypernexSocketServer socketServer, HypernexInstance instance)
    {
        Server = socketServer;
        Instance = instance;
        Instances.Add(this);
    }

    private void CreateGlobalsForInterpreter(IInterpreter interpreter)
    {
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

    public void LoadAndExecuteScript(NexboxScript script, bool multithread = true)
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

    public void Stop()
    {
        foreach (KeyValuePair<NexboxScript, IInterpreter> keyValuePair in new Dictionary<NexboxScript, IInterpreter>(
                     RunningScripts))
        {
            keyValuePair.Value.Stop();
            RunningScripts.Remove(keyValuePair.Key);
        }
        Instances.Remove(this);
    }

    public bool Compare(HypernexInstance instance) => Instance.Equals(instance);
}