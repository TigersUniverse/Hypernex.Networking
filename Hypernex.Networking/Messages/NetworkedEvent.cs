using System.Collections.Generic;
using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// Used to send data from a Client to a Server, or from a Server to a(n) client(s)
/// </summary>
[Msg]
public class NetworkedEvent
{
    [MsgKey(1)] public string MessageId => typeof(NetworkedEvent).FullName;
    // Optional if from Server to Client
    [MsgKey(2)] public JoinAuth Auth;
    /// <summary>
    /// The name of the event to trigger
    /// </summary>
    [MsgKey(3)] public string EventName;
    /// <summary>
    /// The data to send between the server. This data MUST be serializable!
    /// </summary>
    [MsgKey(4)] public List<object> Data;

    /// <summary>
    /// Sends data from the specified Server to the specified client
    /// </summary>
    /// <param name="server">The server to send data from</param>
    /// <param name="toClient">The ClientIdentifier to send data to</param>
    /// <param name="messageChannel">What channel the message should be sent over</param>
    public void FireClient(Server server, ClientIdentifier toClient,
        MessageChannel messageChannel = MessageChannel.Reliable) =>
        server.SendMessage(toClient, Msg.Serialize(this), messageChannel);

    /// <summary>
    /// Sends data from the specified Server to all of its connected clients
    /// </summary>
    /// <param name="server">The server to send data from</param>
    /// <param name="messageChannel">What channel the message should be sent over</param>
    public void FireAllClients(Server server, MessageChannel messageChannel = MessageChannel.Reliable) =>
        server.BroadcastMessage(Msg.Serialize(this), messageChannel);

    /// <summary>
    /// Sends data from the specified Client to the connected Server
    /// </summary>
    /// <param name="client">The client to send data from</param>
    /// <param name="messageChannel">What channel the message should be sent over</param>
    public void FireServer(Client client, MessageChannel messageChannel = MessageChannel.Reliable) =>
        client.SendMessage(Msg.Serialize(this), messageChannel);
}