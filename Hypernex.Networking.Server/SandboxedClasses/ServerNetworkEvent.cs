﻿using Hypernex.Networking.Messages;
using Nexport;

namespace Hypernex.Networking.Server.SandboxedClasses;

public class ServerNetworkEvent
{
    private ScriptHandler _scriptHandler;

    internal ServerNetworkEvent(ScriptHandler scriptHandler) => _scriptHandler = scriptHandler;

    public void SendToClient(string userid, string eventName, MessageChannel messageChannel = MessageChannel.Reliable,
        object[] data = null)
    {
        ClientIdentifier clientIdentifier = _scriptHandler.Instance.GetClientIdentifierFromUserId(userid);
        if (clientIdentifier != null)
        {
            NetworkedEvent networkedEvent = new NetworkedEvent
            {
                EventName = eventName,
                Data = data?.ToList() ?? Array.Empty<object>().ToList()
            };
            _scriptHandler.Instance.SendMessageToClient(clientIdentifier, Msg.Serialize(networkedEvent),
                messageChannel);
        }
    }

    public void SendToAllClients(string eventName, MessageChannel messageChannel = MessageChannel.Reliable,
        object[] data = null)
    {
        NetworkedEvent networkedEvent = new NetworkedEvent
        {
            EventName = eventName,
            Data = data?.ToList() ?? Array.Empty<object>().ToList()
        };
        _scriptHandler.Instance.BroadcastMessage(Msg.Serialize(networkedEvent), messageChannel);
    }
}