using Nexbox;
using WebSocketSharp;

namespace Hypernex.Networking.Server.SandboxedClasses;

public class ServerWebSocket
{
    private WebSocket webSocket;

    public bool IsOpen => webSocket?.IsAlive ?? false;
    
    public void Create(string url, SandboxFunc OnOpen = null, SandboxFunc OnMessage = null, SandboxFunc OnClose = null, SandboxFunc OnError = null)
    {
        webSocket = new WebSocket(url);
        if (OnOpen != null)
            webSocket.OnOpen += (sender, args) => SandboxFuncTools.InvokeSandboxFunc(OnOpen);
        if (OnMessage != null)
            webSocket.OnMessage += (sender, args) => SandboxFuncTools.InvokeSandboxFunc(OnMessage, args.Data);
        if (OnClose != null)
            webSocket.OnClose += (sender, args) =>
                SandboxFuncTools.InvokeSandboxFunc(OnClose, args.Code, args.Reason, args.WasClean);
        if (OnError != null)
            webSocket.OnError += (sender, args) => SandboxFuncTools.InvokeSandboxFunc(OnError, args.Message);
    }

    public void Open() => webSocket.Connect();
    public void Send(string message) => webSocket.Send(message);
    public void Close() => webSocket.Close();
}