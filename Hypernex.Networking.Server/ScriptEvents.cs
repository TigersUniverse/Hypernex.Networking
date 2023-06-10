using Nexbox;

namespace Hypernex.Networking.Server;

public class ScriptEvents
{
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

    public void Subscribe(ScriptEvent scriptEvent, SandboxFunc callback)
    {
        switch (scriptEvent)
        {
            case ScriptEvent.OnUserJoin:
                OnUserJoin += userId => SandboxFuncTools.InvokeSandboxFunc(callback, userId);
                break;
            case ScriptEvent.OnUserLeave:
                OnUserLeave += userId => SandboxFuncTools.InvokeSandboxFunc(callback, userId);
                break;
            case ScriptEvent.OnUserNetworkEvent:
                OnUserNetworkEvent += (userId, eventName, eventArgs) =>
                    SandboxFuncTools.InvokeSandboxFunc(callback, userId, eventName, eventArgs);
                break;
        }
    }
}

public enum ScriptEvent
{
    OnUserJoin,
    OnUserLeave,
    OnUserNetworkEvent
}