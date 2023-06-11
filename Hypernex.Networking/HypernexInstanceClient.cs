using System;
using System.Collections.Generic;
using System.Linq;
using Hypernex.Networking.Messages;
using HypernexSharp;
using HypernexSharp.APIObjects;
using HypernexSharp.SocketObjects;
using Nexport;
using Nexport.Transports;

namespace Hypernex.Networking;

public class HypernexInstanceClient
{
    public Action OnConnect { get; set; } = () => { };
    public Action<User> OnClientConnect { get; set; } = identifier => { };
    public Action<MsgMeta, MessageChannel> OnMessage { get; set; } = (meta, channel) => { };
    public Action<User> OnClientDisconnect { get; set; } = identifier => { };
    public Action OnDisconnect { get; set; } = () => { };
    public bool IsOpen => _client?.IsOpen ?? false;
    public List<User> ConnectedUsers => new (connectedUsers.Values);

    private Client _client;
    private HypernexObject _hypernexObject;
    private User _localUser;
    
    private bool justJoined = true;
    private Dictionary<ClientIdentifier, User> connectedUsers = new ();

    public HypernexInstanceClient(HypernexObject hypernexObject, User localUser, InstanceProtocol instanceProtocol, ClientSettings settings)
    {
        _hypernexObject = hypernexObject;
        _localUser = localUser;
        switch (instanceProtocol)
        {
            case InstanceProtocol.KCP:
                _client = Instantiator.InstantiateClient(TransportType.KCP, settings);
                break;
            case InstanceProtocol.TCP:
                _client = Instantiator.InstantiateClient(TransportType.Telepathy, settings);
                break;
            case InstanceProtocol.UDP:
                _client = Instantiator.InstantiateClient(TransportType.LiteNetLib, settings);
                break;
            default:
                _client = Instantiator.InstantiateClient(TransportType.KCP, settings);
                break;
        }
        RegisterEvents();
    }

    private void AddUserRecursive(ClientIdentifier clientIdentifier, string userId, int t, bool sendEvent)
    {
        if(t > 3 || _localUser.Id == userId)
            return;
        _hypernexObject.GetUser(result =>
        {
            if (result.success)
            {
                connectedUsers.Add(clientIdentifier, result.result.UserData);
                if(sendEvent)
                    OnClientConnect.Invoke(result.result.UserData);
            }
            else
                AddUserRecursive(clientIdentifier, userId, t++, sendEvent);
        }, userId, isUserId: true);
    }

    private void RegisterEvents()
    {
        _client.OnConnect += OnConnect;
        _client.OnMessage += (meta, channel) =>
        {
            if (meta.TypeOfData == typeof(InstancePlayers))
            {
                (bool, InstancePlayers) instancePlayers = SafeMessage.TryGetMessage<InstancePlayers>(meta.RawData);
                if (instancePlayers.Item1)
                {
                    if (justJoined)
                    {
                        foreach (KeyValuePair<ClientIdentifier,string> keyValuePair in instancePlayers.Item2.UserIds)
                            AddUserRecursive(keyValuePair.Key, keyValuePair.Value, 0, false);
                        justJoined = false;
                    }
                    else
                    {
                        foreach (KeyValuePair<ClientIdentifier,string> keyValuePair in instancePlayers.Item2.UserIds)
                        {
                            if(connectedUsers.ContainsKey(keyValuePair.Key))
                                AddUserRecursive(keyValuePair.Key, keyValuePair.Value, 0, true);
                        }
                    }
                }
            }
            else
                OnMessage.Invoke(meta, channel);
        };
        _client.OnNetworkedClientDisconnect += identifier =>
        {
            if (connectedUsers.Count(x => x.Key.Identifier == identifier.Identifier) > 0)
            {
                User u = connectedUsers.FirstOrDefault(x => x.Key.Identifier == identifier.Identifier).Value;
                if (u.Id == _localUser.Id)
                    return;
                connectedUsers.Remove(identifier);
                OnClientDisconnect.Invoke(u);
            }
        };
        _client.OnDisconnect += () =>
        {
            justJoined = true;
            connectedUsers.Clear();
            OnDisconnect.Invoke();
        };
    }

    public void Open() => _client.Create(true);
    public void Stop() => _client.Stop();

    public void SendMessage(byte[] message, MessageChannel messageChannel = MessageChannel.Reliable) =>
        _client.SendMessage(message, messageChannel);
}