using WebSocketSharp;

namespace Hypernex.Networking.Server.SandboxedClasses;

public class ServerWebSocket
{
    private WebSocket webSocket;
    
    public void Create(string url, Delegate OnOpen, Delegate OnMessage, Delegate OnClose, Delegate OnError)
    {
        using (webSocket = new WebSocket(url))
        {
            webSocket.OnOpen += (sender, args) => OnOpen.DynamicInvoke();
            webSocket.OnMessage += (sender, args) => OnOpen.DynamicInvoke(args.Data);
            webSocket.OnClose += (sender, args) => OnClose.DynamicInvoke();
            webSocket.OnError += (sender, args) => OnError.DynamicInvoke();
        }
    }

    public void Open() => webSocket.Connect();
    public void Send(string message) => webSocket.Send(message);
    public void Close() => webSocket.Close();
}