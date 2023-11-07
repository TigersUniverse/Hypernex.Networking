#nullable enable
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
    public Action<User> OnUserLoaded { get; set; } = user => { };
    public Action<User> OnClientConnect { get; set; } = user => { };
    public Action<MsgMeta, MessageChannel> OnMessage { get; set; } = (meta, channel) => { };
    public Action<User> OnClientDisconnect { get; set; } = identifier => { };
    public Action OnDisconnect { get; set; } = () => { };
    public bool IsOpen => _client?.IsOpen ?? false;
    public string HostId { get; private set; }

    public List<User> ConnectedUsers
    {
        get
        {
            List<User> knownUsers = new List<User>();
            foreach (User? connectedUsersValue in new List<User?>(connectedUsers.Values))
            {
                if(connectedUsersValue != null)
                    knownUsers.Add(connectedUsersValue);
            }
            return knownUsers;
        }
    }

    private Client _client;
    private HypernexObject _hypernexObject;
    private User _localUser;
    
    private bool justJoined = true;
    private Dictionary<ClientIdentifier, User?> connectedUsers = new ();

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
                if (!connectedUsers.ContainsKey(clientIdentifier))
                    return;
                connectedUsers[clientIdentifier] = result.result.UserData;
                if(sendEvent)
                    OnClientConnect.Invoke(result.result.UserData);
                else
                    OnUserLoaded.Invoke(result.result.UserData);
            }
            else
                AddUserRecursive(clientIdentifier, userId, t++, sendEvent);
        }, userId, isUserId: true);
    }

    private void CheckJoinedUsers(InstancePlayers instancePlayers)
    {
        foreach (KeyValuePair<ClientIdentifier,string> keyValuePair in instancePlayers.UserIds)
        {
            if (connectedUsers.Count(x => x.Key.Compare(keyValuePair.Key)) <= 0)
            {
                connectedUsers.Add(keyValuePair.Key, null);
                AddUserRecursive(keyValuePair.Key, keyValuePair.Value, 0, true);
            }
        }
    }

    private void CheckLeftUsers(InstancePlayers instancePlayers)
    {
        foreach (KeyValuePair<ClientIdentifier,User?> keyValuePair in new Dictionary<ClientIdentifier, User?>(connectedUsers))
        {
            if (instancePlayers.UserIds.Count(x => x.Key.Compare(keyValuePair.Key)) <= 0)
            {
                connectedUsers.Remove(keyValuePair.Key);
                if(keyValuePair.Value != null)
                    OnClientDisconnect.Invoke(keyValuePair.Value);
            }
        }
    }

    private void RegisterEvents()
    {
        _client.OnConnect += OnConnect;
        _client.OnMessage += (meta, channel) =>
        {
            if (meta.TypeOfData == typeof(InstancePlayers))
            {
                InstancePlayers instancePlayers =
                    (InstancePlayers) Convert.ChangeType(meta.Data, typeof(InstancePlayers));
                HostId = instancePlayers.HostId;
                if (justJoined)
                {
                    foreach (KeyValuePair<ClientIdentifier, string> keyValuePair in instancePlayers.UserIds)
                    {
                        connectedUsers.Add(keyValuePair.Key, null);
                        AddUserRecursive(keyValuePair.Key, keyValuePair.Value, 0, false);
                    }
                    justJoined = false;
                }
                else
                {
                    if (instancePlayers.UserIds.Count > connectedUsers.Count)
                    {
                        // Someone Joined
                        CheckJoinedUsers(instancePlayers);
                    }
                    else if (instancePlayers.UserIds.Count < connectedUsers.Count)
                    {
                        // Someone Left
                        CheckLeftUsers(instancePlayers);
                    }
                    else
                    {
                        // Complete a quick check of both
                        CheckJoinedUsers(instancePlayers);
                        CheckLeftUsers(instancePlayers);
                    }
                }
            }
            else
                OnMessage.Invoke(meta, channel);
        };
        /*_client.OnNetworkedClientDisconnect += identifier =>
        {
            if (connectedUsers.Count(x => x.Key.Identifier == identifier.Identifier) > 0)
            {
                User u = connectedUsers.FirstOrDefault(x => x.Key.Compare(identifier)).Value;
                if (u.Id == _localUser.Id)
                    return;
                connectedUsers.Remove(identifier);
                OnClientDisconnect.Invoke(u);
            }
        };*/
        _client.OnDisconnect += () =>
        {
            justJoined = true;
            connectedUsers.Clear();
            OnDisconnect.Invoke();
        };
    }

    public void Open() => _client.Create();
    public void Stop() => _client.Close();

    public void SendMessage(byte[] message, MessageChannel messageChannel = MessageChannel.Reliable) =>
        _client.SendMessage(message, messageChannel);
}