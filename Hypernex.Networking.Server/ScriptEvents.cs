﻿using Hypernex.Sandboxing.SandboxedTypes;
using Nexbox;

namespace Hypernex.Networking.Server;

public class ScriptEvents
{
    private ScriptHandler ScriptHandler;
    
    /// <summary>
    /// UserId when someone joins
    /// </summary>
    internal Action<string> OnUserJoin = userId => { };
    /// <summary>
    /// UserId when someone leaves
    /// </summary>
    internal Action<string> OnUserLeave = userId => { };
    /// <summary>
    /// When a client invokes a NetworkedEvent, the data is passed here
    /// </summary>
    internal Action<string, string, object[]> OnUserNetworkEvent = (userId, EventName, EventArgs) => { };

    internal ScriptEvents(ScriptHandler s) => ScriptHandler = s;

    public void Subscribe(ScriptEvent scriptEvent, SandboxFunc callback)
    {
        switch (scriptEvent)
        {
            case ScriptEvent.OnUserJoin:
                /*OnUserJoin += userId => SandboxFuncTools.InvokeSandboxFunc(callback, userId);*/
                OnUserJoin += userId =>
                {
                    new Thread(() =>
                    {
                        if(ScriptHandler.m.WaitOne())
                        {
                            ScriptHandler.AwaitingTasks.Enqueue(() => SandboxFuncTools.InvokeSandboxFunc(callback, userId));
                            ScriptHandler.m.ReleaseMutex();
                        }
                    }).Start();
                };
                break;
            case ScriptEvent.OnUserLeave:
                /*OnUserLeave += userId => SandboxFuncTools.InvokeSandboxFunc(callback, userId);*/
                OnUserLeave += userId =>
                {
                    new Thread(() =>
                    {
                        if(ScriptHandler.m.WaitOne())
                        {
                            ScriptHandler.AwaitingTasks.Enqueue(() => SandboxFuncTools.InvokeSandboxFunc(callback, userId));
                            ScriptHandler.m.ReleaseMutex();
                        }
                    }).Start();
                };
                break;
            case ScriptEvent.OnUserNetworkEvent:
                /*OnUserNetworkEvent += (userId, eventName, eventArgs) =>
                    SandboxFuncTools.InvokeSandboxFunc(callback, userId, eventName, eventArgs);*/
                OnUserNetworkEvent += (userId, eventName, eventArgs) =>
                {
                    new Thread(() =>
                    {
                        if (ScriptHandler.m.WaitOne())
                        {
                            ScriptHandler.AwaitingTasks.Enqueue(() =>
                                SandboxFuncTools.InvokeSandboxFunc(callback, userId, eventName, eventArgs));
                            ScriptHandler.m.ReleaseMutex();
                        }
                    }).Start();
                };
                break;
        }
    }
}