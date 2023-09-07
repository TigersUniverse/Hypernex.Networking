using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using Hypernex.CCK;
using Hypernex.Networking.Messages;
using HypernexSharp;
using HypernexSharp.APIObjects;
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
    public List<string> ConnectedClients => new (AuthedUsers.Values.Select(x => x.UserId));
    public string InstanceId => _instanceMeta?.InstanceId ?? String.Empty;
    public string WorldOwnerId => _worldMeta?.OwnerId ?? String.Empty;
    public string InstanceCreatorId => _instanceMeta?.InstanceCreatorId;
    public string[] Moderators => new List<string>(moderators).ToArray();
    public HypernexSocketServer SocketServer => _hypernexSocketServer;

    internal InstanceMeta _instanceMeta;
    internal ServerSettings _serverSettings;
    internal List<TempUserToken> ValidTokens = new();
    internal List<string> moderators = new();
    internal List<string> BannedUsers = new();
    internal List<string> SocketConnectedUsers = new();

    private HypernexSocketServer _hypernexSocketServer;
    private Server _server;
    private Dictionary<ClientIdentifier, JoinAuth> AuthedUsers = new();
    private WorldMeta _worldMeta;
    private Action<HypernexInstance> onStop;
    private int loadedScripts;
    private Timer joinTimer;

    private ServerSettings GetServerSettings(ServerSettings settings) => new(settings.Ip,
        settings.Port, true, settings.UseMultithreading,
        settings.ThreadUpdate, settings.UseIPV6)
    {
        ValidateMessage = (identifier, meta, result) =>
        {
            JoinAuth msg = (JoinAuth) Convert.ChangeType(meta.Data, typeof(JoinAuth));
            bool v =
                ValidTokens.Count(x => x.userId == msg.UserId && x.tempUserToken == msg.TempToken) >
                0 && !BannedUsers.Contains(msg.UserId);
            if (v)
            {
                AuthedUsers.Add(identifier, msg);
            }
            result.Invoke(v);
        }
    };

    private void ForceGetWorldMeta(string worldId, HypernexObject hypernexObject, Action<WorldMeta> OnGet)
    {
        hypernexObject.GetWorldMeta(result =>
        {
            if (result.success)
                OnGet.Invoke(result.result.Meta);
            else
                ForceGetWorldMeta(worldId, hypernexObject, OnGet);
        }, worldId);
    }

    private NexboxLanguage GetLanguageFromFileName(string fileName)
    {
        string[] s = fileName.Split('.');
        switch (s.Last().ToLower())
        {
            case "js":
                return NexboxLanguage.JavaScript;
            case "lua":
                return NexboxLanguage.Lua;
        }
        return NexboxLanguage.Unknown;
    }

    private void ForceGetAllScripts(List<string> scriptsToGet, List<NexboxScript> scriptsReceived, Action<List<NexboxScript>> OnDone)
    {
        Uri u = new Uri(scriptsToGet.ElementAt(0));
        string p = u.AbsolutePath;
        if (p.Last() == '/')
            p = p.Remove(p.Length - 1);
        string f = p.Split('/').Last();
        _hypernexSocketServer.GameServerSocket.GetServerScript((fileName, stream) =>
        {
            if (stream != Stream.Null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string script = reader.ReadToEnd();
                    NexboxScript nexboxScript = new NexboxScript(GetLanguageFromFileName(fileName), script)
                        {Name = Path.GetFileNameWithoutExtension(fileName)};
                    scriptsReceived.Add(nexboxScript);
                    scriptsToGet.RemoveAt(0);
                    loadedScripts++;
                    if (scriptsToGet.Count <= 0)
                        OnDone.Invoke(scriptsReceived);
                    else
                        ForceGetAllScripts(scriptsToGet, scriptsReceived, OnDone);
                }
            }
            else
                ForceGetAllScripts(scriptsToGet, scriptsReceived, OnDone);
        }, _worldMeta.OwnerId, f);
    }

    public HypernexInstance(HypernexSocketServer hypernexSocketServer, InstanceMeta instanceMeta,
        ServerSettings Settings, Action<HypernexInstance> initializedWorld,
        Action<(HypernexInstance,List<NexboxScript>)> scriptHandler, Action<HypernexInstance> OnStop = null)
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
        onStop = OnStop;
        RegisterEvents();
        _serverSettings = settings;
        _instanceMeta = instanceMeta;
        _hypernexSocketServer = hypernexSocketServer;
        ForceGetWorldMeta(instanceMeta.WorldId, hypernexSocketServer._hypernexObject, worldMeta =>
        {
            _worldMeta = worldMeta;
            Builds targetBuild = null;
            foreach (Builds worldMetaBuild in worldMeta.Builds)
            {
                if (worldMetaBuild.BuildPlatform != BuildPlatform.Windows) continue;
                targetBuild = worldMetaBuild;
                break;
            }
            if (targetBuild == null && _worldMeta.Builds.Count > 0)
                targetBuild = _worldMeta.Builds[0];
            if(targetBuild != null && targetBuild.ServerScripts.Count > 0)
                ForceGetAllScripts(new List<string>(targetBuild.ServerScripts), new List<NexboxScript>(),
                    scripts =>
                    {
                        scriptHandler.Invoke((this, scripts));
                        initializedWorld.Invoke(this);
                    });
            else
            {
                scriptHandler.Invoke((this, new List<NexboxScript>()));
                initializedWorld.Invoke(this);
            }
        });
    }

    internal void UpdateInstance(InstanceMeta meta)
    {
        moderators = new(meta.Moderators);
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
                    RespondAuth respondAuth = new RespondAuth
                    {
                        UserId = keyValuePair.Value.UserId,
                        GameServerId = _instanceMeta.GameServerId,
                        InstanceId = _instanceMeta.InstanceId
                    };
                    SendMessageToClient(identifier, Msg.Serialize(respondAuth));
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
                        JoinAuth joinAuth = (JoinAuth) Convert.ChangeType(fieldInfo.GetValue(meta.Data), typeof(JoinAuth));
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
                            JoinAuth joinAuth = (JoinAuth) Convert.ChangeType(propertyInfo.GetValue(meta.Data), typeof(JoinAuth));
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
                if (!keyValuePair.Key.Compare(identifier)) continue;
                userid = keyValuePair.Value.UserId;
                AuthedUsers.Remove(keyValuePair.Key);
                _hypernexSocketServer.GameServerSocket.KickUser(_instanceMeta.InstanceId,
                    keyValuePair.Value.UserId);
            }
            if (!string.IsNullOrEmpty(userid))
            {
                OnClientDisconnect.Invoke(userid);
                foreach (TempUserToken tempUserToken in new List<TempUserToken>(ValidTokens))
                {
                    if (tempUserToken.userId == userid)
                        ValidTokens.Remove(tempUserToken);
                }
            }
            if (_server.ConnectedClients.Count <= 0)
            {
                StopServer();
            }
            else
                UpdatePlayerList();
        };
        OnMessage += (userId, meta, messageChannel) =>
        {
            if (meta.TypeOfData == typeof(WarnPlayer))
            {
                if (moderators.Contains(userId))
                {
                    WarnPlayer o = (WarnPlayer) Convert.ChangeType(meta.Data, typeof(WarnPlayer));
                    WarnPlayer warnPlayer = new WarnPlayer
                    {
                        targetUserId = o.targetUserId,
                        message = o.message
                    };
                    if(moderators.Contains(warnPlayer.targetUserId) && userId != _instanceMeta.InstanceCreatorId)
                        return;
                    ClientIdentifier c = GetClientIdentifierFromUserId(o.targetUserId);
                    if (c == null)
                        return;
                    SendMessageToClient(c, Msg.Serialize(warnPlayer));
                }
            }
            else if (meta.TypeOfData == typeof(KickPlayer))
            {
                if (moderators.Contains(userId))
                {
                    KickPlayer o = (KickPlayer) Convert.ChangeType(meta.Data, typeof(KickPlayer));
                    KickPlayer kickPlayer = new KickPlayer
                    {
                        targetUserId = o.targetUserId,
                        message = o.message
                    };
                    if(moderators.Contains(kickPlayer.targetUserId) && userId != _instanceMeta.InstanceCreatorId)
                        return;
                    ClientIdentifier c = GetClientIdentifierFromUserId(o.targetUserId);
                    if (c == null)
                        return;
                    KickUser(c, Msg.Serialize(kickPlayer));
                }
            }
            else if (meta.TypeOfData == typeof(BanPlayer))
            {
                if (moderators.Contains(userId))
                {
                    BanPlayer o = (BanPlayer) Convert.ChangeType(meta.Data, typeof(BanPlayer));
                    BanPlayer banPlayer = new BanPlayer
                    {
                        targetUserId = o.targetUserId,
                        message = o.message
                    };
                    if(moderators.Contains(banPlayer.targetUserId) && userId != _instanceMeta.InstanceCreatorId)
                        return;
                    ClientIdentifier c = GetClientIdentifierFromUserId(o.targetUserId);
                    BanUser(banPlayer.targetUserId, c, Msg.Serialize(banPlayer));
                }
            }
            else if (meta.TypeOfData == typeof(UnbanPlayer))
            {
                if (moderators.Contains(userId))
                {
                    UnbanPlayer u = (UnbanPlayer) Convert.ChangeType(meta.Data, typeof(UnbanPlayer));
                    UnbanUser(u.targetUserId);
                }
            }
            else if (meta.TypeOfData == typeof(AddModerator))
            {
                if (moderators.Contains(userId))
                {
                    AddModerator a = (AddModerator) Convert.ChangeType(meta.Data, typeof(AddModerator));
                    AddModerator(a.targetUserId);
                }
            }
            else if (meta.TypeOfData == typeof(RemoveModerator))
            {
                if (moderators.Contains(userId))
                {
                    RemoveModerator r = (RemoveModerator) Convert.ChangeType(meta.Data, typeof(RemoveModerator));
                    if(moderators.Contains(r.targetUserId) && userId != _instanceMeta.InstanceCreatorId)
                        return;
                    RemoveModerator(r.targetUserId);
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

    public void StartServer()
    {
        _server.Create();
        joinTimer = new Timer(30000);
        joinTimer.Elapsed += (sender, args) =>
        {
            if (_server.ConnectedClients.Count <= 0)
                StopServer();
        };
        joinTimer.Start();
    }

    public void StopServer()
    {
        AuthedUsers.Clear();
        ValidTokens.Clear();
        onStop?.Invoke(this);
        _server?.Stop();
        _hypernexSocketServer.RemoveInstance(this);
        joinTimer?.Stop();
        joinTimer?.Dispose();
        _hypernexSocketServer.GameServerSocket.RemoveInstance(_instanceMeta.InstanceId);
    }

    public void KickUser(ClientIdentifier client, byte[] optionalMessage = null)
    {
        string uid = GetUserIdFromClientIdentifier(client);
        if (string.IsNullOrEmpty(uid))
            return;
        _hypernexSocketServer.GameServerSocket.KickUser(_instanceMeta.InstanceId, uid);
        _server.KickClient(client, optionalMessage);
    }

    public void BanUser(string userid, ClientIdentifier client, byte[] optionalMessage = null)
    {
        _hypernexSocketServer.GameServerSocket.BanUser(_instanceMeta.InstanceId, userid);
        if(client != null)
            _server.KickClient(client, optionalMessage);
    }

    public void UnbanUser(string userid) =>
        _hypernexSocketServer.GameServerSocket.UnbanUser(_instanceMeta.InstanceId, userid);

    public void AddModerator(string userid) =>
        _hypernexSocketServer.GameServerSocket.AddModerator(_instanceMeta.InstanceId, userid);
    
    public void RemoveModerator(string userid) =>
        _hypernexSocketServer.GameServerSocket.RemoveModerator(_instanceMeta.InstanceId, userid);

    public void SendMessageToClient(ClientIdentifier identifier, byte[] message,
        MessageChannel messageChannel = MessageChannel.Reliable) =>
        _server.SendMessage(identifier, message, messageChannel);

    public void BroadcastMessage(byte[] message, MessageChannel messageChannel = MessageChannel.Reliable) =>
        _server.BroadcastMessage(message, messageChannel);

    public void BroadcastMessageWithExclusion(ClientIdentifier excludingIdentifier, byte[] message,
        MessageChannel messageChannel = MessageChannel.Reliable) =>
        _server.BroadcastMessage(message, messageChannel, excludingIdentifier);
    
    private string GetListAsString<T>(List<T> l)
    {
        string g = String.Empty;
        foreach (T t in l)
            g += t + " ";
        return g;
    }

#if DEBUG
    public override string ToString() =>
        $"Id: {_instanceMeta.InstanceId}, SocketPlayers: [{GetListAsString(SocketConnectedUsers)}], Players: [{GetListAsString(_server?.ConnectedClients)}] LoadedScripts: {loadedScripts}";
#else
    public override string ToString() =>
        $"Id: {_instanceMeta.InstanceId}, Players: [{GetListAsString(SocketConnectedUsers)}], LoadedScripts: {loadedScripts}";
#endif
}