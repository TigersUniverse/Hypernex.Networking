using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hypernex.Networking.Messages;
using HypernexSharp.Socketing.SocketResponses;
using HypernexSharp.SocketObjects;
using Nexport;
using Nexport.Transports;

namespace Hypernex.Networking;

public class HypernexInstance
{
    public Action<string> OnClientConnect { get; set; } = userId => { };
    public Action<string, MsgMeta, MessageChannel> OnMessage { get; set; } = (userId, meta, channel) => { };
    public Action<string> OnClientDisconnect { get; set; } = userId => { };
    public bool IsOpen => _server?.IsOpen ?? false;

    internal InstanceMeta _instanceMeta;
    internal ServerSettings _serverSettings;
    internal List<TempUserToken> ValidTokens = new();
    internal List<string> Moderators = new();
    internal List<string> BannedUsers = new();
    internal List<string> SocketConnectedUsers = new();

    private HypernexSocketServer _hypernexSocketServer;
    private Server _server;
    private Dictionary<ClientIdentifier, JoinAuth> AuthedUsers = new();

    private ServerSettings GetServerSettings(ServerSettings settings) => new(settings.Ip,
        settings.Port, true, settings.UseMultithreading,
        settings.ThreadUpdate, settings.UseIPV6)
    {
        ValidateMessage = (identifier, meta, result) =>
        {
            (bool, JoinAuth) msg = SafeMessage.TryGetMessage<JoinAuth>(meta.RawData);
            if (msg.Item1)
            {
                bool v =
                    ValidTokens.Count(x => x.userId == msg.Item2.UserId && x.tempUserToken == msg.Item2.TempToken) >
                    0 && !BannedUsers.Contains(msg.Item2.UserId);
                if (v)
                    AuthedUsers.Add(identifier, msg.Item2);
                result.Invoke(v);
            }
            result.Invoke(false);
            _hypernexSocketServer.GameServerSocket.KickUser(_instanceMeta.InstanceId, msg.Item2.UserId);
        }
    };

    public HypernexInstance(HypernexSocketServer hypernexSocketServer, InstanceMeta instanceMeta,
        ServerSettings Settings)
    {
        ServerSettings settings = GetServerSettings(Settings);
        switch (instanceMeta.InstanceProtocol)
        {
            case InstanceProtocol.KCP:
                _server = Instantiator.InstantiateServer(TransportType.KCP, settings);
                break;
            case InstanceProtocol.TCP:
                _server = Instantiator.InstantiateServer(TransportType.Telepathy, settings);
                break;
            case InstanceProtocol.UDP:
                _server = Instantiator.InstantiateServer(TransportType.LiteNetLib, settings);
                break;
            default:
                _server = Instantiator.InstantiateServer(TransportType.KCP, settings);
                break;
        }
        RegisterEvents();
        _serverSettings = settings;
        _instanceMeta = instanceMeta;
        _hypernexSocketServer = hypernexSocketServer;
    }

    internal void UpdateInstance(InstanceMeta meta)
    {
        Moderators = new(meta.Moderators);
        BannedUsers = new(meta.BannedUsers);
        SocketConnectedUsers = new(meta.ConnectedUsers);
    }

    private void UpdatePlayerList()
    {
        Dictionary<ClientIdentifier, string> clients = new Dictionary<ClientIdentifier, string>();
        foreach (KeyValuePair<ClientIdentifier,JoinAuth> keyValuePair in AuthedUsers)
            clients.Add(keyValuePair.Key, keyValuePair.Value.UserId);
        InstancePlayers instancePlayers = new InstancePlayers {UserIds = clients};
        BroadcastMessage(Msg.Serialize(instancePlayers));
    }

    private void RegisterEvents()
    {
        _server.OnConnect += identifier =>
        {
            foreach (KeyValuePair<ClientIdentifier,JoinAuth> keyValuePair in new List<KeyValuePair<ClientIdentifier, JoinAuth>>(AuthedUsers))
            {
                if (keyValuePair.Key.Identifier == identifier.Identifier)
                {
                    ValidTokens.Remove(ValidTokens.FirstOrDefault(x => x.userId == keyValuePair.Value.UserId));
                    OnClientConnect.Invoke(keyValuePair.Value.UserId);
                    UpdatePlayerList();
                    break;
                }
            }
        };
        _server.OnMessage += (identifier, meta, messageChannel) =>
        {
            try
            {
                bool didInvoke = false;
                foreach (FieldInfo fieldInfo in meta.Data.GetType().GetFields())
                {
                    if (fieldInfo.FieldType == typeof(JoinAuth))
                    {
                        JoinAuth joinAuth = (JoinAuth) Convert.ChangeType(fieldInfo, typeof(JoinAuth));
                        if (!didInvoke && AuthedUsers.Count(x =>
                                x.Value.UserId == joinAuth.UserId && x.Value.TempToken == joinAuth.TempToken) > 0)
                        {
                            didInvoke = true;
                            OnMessage.Invoke(joinAuth.UserId, meta, messageChannel);
                        }
                    }
                }
                if (!didInvoke)
                {
                    foreach (PropertyInfo propertyInfo in meta.Data.GetType().GetProperties())
                    {
                        if (propertyInfo.PropertyType == typeof(JoinAuth))
                        {
                            JoinAuth joinAuth = (JoinAuth) Convert.ChangeType(propertyInfo, typeof(JoinAuth));
                            if (!didInvoke && AuthedUsers.Count(x =>
                                    x.Value.UserId == joinAuth.UserId && x.Value.TempToken == joinAuth.TempToken) > 0)
                            {
                                didInvoke = true;
                                OnMessage.Invoke(joinAuth.UserId, meta, messageChannel);
                            }
                        }
                    }
                }
            }
            catch(Exception){}
        };
        _server.OnDisconnect += identifier =>
        {
            string userid = String.Empty;
            foreach (KeyValuePair<ClientIdentifier,JoinAuth> keyValuePair in new List<KeyValuePair<ClientIdentifier, JoinAuth>>(AuthedUsers))
            {
                if (keyValuePair.Key.Identifier == identifier.Identifier)
                {
                    ValidTokens.Remove(ValidTokens.FirstOrDefault(x => x.userId == keyValuePair.Value.UserId));
                    userid = keyValuePair.Value.UserId;
                }
            }
            AuthedUsers.Remove(AuthedUsers.FirstOrDefault(x => x.Key.Identifier == identifier.Identifier).Key);
            if(!string.IsNullOrEmpty(userid))
                OnClientDisconnect.Invoke(userid);
            if (AuthedUsers.Count <= 0)
            {
                _hypernexSocketServer.GameServerSocket.RemoveInstance(_instanceMeta.InstanceId);
                _server.Stop();
            }
            else
                UpdatePlayerList();
        };
        OnMessage += (userId, meta, messageChannel) =>
        {
            if (meta.TypeOfData == typeof(WarnPlayer))
            {
                if (Moderators.Contains(userId))
                {
                    WarnPlayer o = (WarnPlayer) Convert.ChangeType(meta.Data, typeof(WarnPlayer));
                    WarnPlayer warnPlayer = new WarnPlayer
                    {
                        targetUserId = o.targetUserId,
                        message = o.message
                    };
                    ClientIdentifier c = GetClientIdentifierFromUserId(o.targetUserId);
                    if (c == null)
                        return;
                    SendMessageToClient(c, Msg.Serialize(warnPlayer));
                }
            }
            else if (meta.TypeOfData == typeof(KickPlayer))
            {
                if (Moderators.Contains(userId))
                {
                    KickPlayer o = (KickPlayer) Convert.ChangeType(meta.Data, typeof(KickPlayer));
                    KickPlayer kickPlayer = new KickPlayer
                    {
                        targetUserId = o.targetUserId,
                        message = o.message
                    };
                    ClientIdentifier c = GetClientIdentifierFromUserId(o.targetUserId);
                    if (c == null)
                        return;
                    BanUser(c, Msg.Serialize(kickPlayer));
                }
            }
            else if (meta.TypeOfData == typeof(BanPlayer))
            {
                if (Moderators.Contains(userId))
                {
                    BanPlayer o = (BanPlayer) Convert.ChangeType(meta.Data, typeof(BanPlayer));
                    BanPlayer banPlayer = new BanPlayer
                    {
                        targetUserId = o.targetUserId,
                        message = o.message
                    };
                    ClientIdentifier c = GetClientIdentifierFromUserId(o.targetUserId);
                    if (c == null)
                        return;
                    BanUser(c, Msg.Serialize(banPlayer));
                }
            }
        };
    }

    public ClientIdentifier GetClientIdentifierFromUserId(string userid)
    {
        foreach (KeyValuePair<ClientIdentifier,JoinAuth> keyValuePair in AuthedUsers)
        {
            if (keyValuePair.Value.UserId == userid)
                return keyValuePair.Key;
        }
        return null;
    }

    public string GetUserIdFromClientIdentifier(ClientIdentifier clientIdentifier)
    {
        foreach (KeyValuePair<ClientIdentifier,JoinAuth> keyValuePair in AuthedUsers)
        {
            if (keyValuePair.Key.Compare(clientIdentifier))
                return keyValuePair.Value.UserId;
        }
        return String.Empty;
    }

    public void StartServer() => _server.Create();
    public void StopServer() => _server.Stop();

    public void KickUser(ClientIdentifier client, byte[] optionalMessage = null)
    {
        string uid = GetUserIdFromClientIdentifier(client);
        if (string.IsNullOrEmpty(uid))
            return;
        _hypernexSocketServer.GameServerSocket.KickUser(_instanceMeta.InstanceId, uid);
        _server.KickClient(client, optionalMessage);
    }

    public void BanUser(ClientIdentifier client, byte[] optionalMessage = null)
    {
        string uid = GetUserIdFromClientIdentifier(client);
        if (string.IsNullOrEmpty(uid) || Moderators.Contains(uid))
            return;
        _hypernexSocketServer.GameServerSocket.BanUser(_instanceMeta.InstanceId, uid);
        _server.KickClient(client, optionalMessage);
    }

    public void SendMessageToClient(ClientIdentifier identifier, byte[] message,
        MessageChannel messageChannel = MessageChannel.Reliable) =>
        _server.SendMessage(identifier, message, messageChannel);

    public void BroadcastMessage(byte[] message, MessageChannel messageChannel = MessageChannel.Reliable) =>
        _server.BroadcastMessage(message, messageChannel);

    public void BroadcastMessageWithExclusion(ClientIdentifier excludingIdentifier, byte[] message,
        MessageChannel messageChannel = MessageChannel.Reliable) =>
        _server.BroadcastMessage(message, messageChannel, excludingIdentifier);

    public override string ToString() => $"Id: {_instanceMeta.InstanceId}, Players: {SocketConnectedUsers}";
}