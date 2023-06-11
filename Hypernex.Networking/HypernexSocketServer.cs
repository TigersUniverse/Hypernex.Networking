using System;
using System.Collections.Generic;
using System.Linq;
using Hypernex.CCK;
using HypernexSharp;
using HypernexSharp.Socketing;
using HypernexSharp.Socketing.SocketResponses;
using Nexport;

namespace Hypernex.Networking;

public class HypernexSocketServer
{
    public static Action<HypernexInstance> OnInstance { get; set; } = instance => { };

    internal HypernexObject _hypernexObject;
    private List<HypernexInstance> _instances = new ();
    public List<HypernexInstance> Instances => new (_instances);
    public GameServerSocket GameServerSocket { get; }

    internal void RemoveInstance(HypernexInstance instance) => _instances.Remove(instance);

    private Dictionary<string, int> TemporaryInstances = new ();

    private int GetAvailablePortForInstance(int BeginPortRange, int EndPortRange)
    {
        Random r = new Random();
        int selectedPort = r.Next(BeginPortRange, EndPortRange);
        while (Instances.Count(x => x._serverSettings.Port == selectedPort) > 0 ||
               new Dictionary<string, int>(TemporaryInstances).ContainsValue(selectedPort))
            selectedPort = r.Next(BeginPortRange, EndPortRange);
        return selectedPort;
    }

    private HypernexInstance GetInstanceById(string instanceId) =>
        Instances.FirstOrDefault(x => x._instanceMeta.InstanceId == instanceId);

    public HypernexSocketServer(HypernexObject hypernexObject, string globalIp,
        Action<(HypernexSocketServer, HypernexInstance, List<NexboxScript>)> OnScripts, string gameServerToken = "",
        string localIp = "0.0.0.0", int beginPortRange = 15000, int endPortRange = 25000, bool useMultithreading = true,
        int threadUpdateTime = 10, bool useIPV6 = false, Action onOpen = null, Action<HypernexInstance> onStop = null)
    {
        GameServerSocket = hypernexObject.OpenGameServerSocket(gameServerToken);
        GameServerSocket.OnOpen += () => onOpen?.Invoke();
        GameServerSocket.OnSocketEvent += response =>
        {
            switch (response.message.ToLower())
            {
                case "requestedinstancecreated":
                {
                    // Sent when a User requests an instance
                    RequestedInstanceCreated requestedInstanceCreated =
                        GameServerSocket.TryParseData<RequestedInstanceCreated>(response);
                    if (_instances.Count < endPortRange - beginPortRange)
                    {
                        // Request this instance
                        int p = GetAvailablePortForInstance(beginPortRange, endPortRange);
                        GameServerSocket.ClaimInstanceRequest(requestedInstanceCreated.temporaryId,
                            globalIp + ":" + p);
                        TemporaryInstances.Add(requestedInstanceCreated.temporaryId, p);
                    }
                    break;
                }
                case "selectedgameserver":
                {
                    SelectedGameServer selectedGameServer =
                        GameServerSocket.TryParseData<SelectedGameServer>(response);
                    int p = TemporaryInstances[selectedGameServer.instanceMeta.TemporaryId];
                    HypernexInstance instance = new HypernexInstance(this, selectedGameServer.instanceMeta,
                        new ServerSettings(localIp, p, useMultithreading: useMultithreading,
                            threadUpdateMs: threadUpdateTime, useIPV6: useIPV6), i =>
                        {
                            i.StartServer();
                        }, tuple =>
                        {
                            OnScripts.Invoke((this, tuple.Item1, tuple.Item2));
                            // Start the game instance AFTER we download and load all scripts
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.WriteLine("Loaded All Scripts from Instance " + selectedGameServer.instanceMeta.InstanceId);
                            Console.ForegroundColor = ConsoleColor.White;
                            GameServerSocket.InstanceReady(tuple.Item1._instanceMeta.InstanceId, globalIp + ":" + p);
                        });
                    TemporaryInstances.Remove(selectedGameServer.instanceMeta.TemporaryId);
                    _instances.Add(instance);
                    instance.UpdateInstance(selectedGameServer.instanceMeta);
                    OnInstance.Invoke(instance);
                    break;
                }
                case "notselectedgameserver":
                {
                    NotSelectedGameServer notSelectedGameServer =
                        GameServerSocket.TryParseData<NotSelectedGameServer>(response);
                    if (TemporaryInstances.ContainsKey(notSelectedGameServer.TemporaryId))
                        TemporaryInstances.Remove(notSelectedGameServer.TemporaryId);
                    break;
                }
                case "updatedinstance":
                {
                    UpdatedInstance updatedInstance = GameServerSocket.TryParseData<UpdatedInstance>(response);
                    HypernexInstance instance = GetInstanceById(updatedInstance.instanceMeta.InstanceId);
                    if(instance != null)
                        instance.UpdateInstance(updatedInstance.instanceMeta);
                    break;
                }
                case "tempusertoken":
                {
                    TempUserToken tempUserToken = GameServerSocket.TryParseData<TempUserToken>(response);
                    HypernexInstance instance = GetInstanceById(tempUserToken.instanceId);
                    if (instance != null)
                    {
                        if (instance.ValidTokens.Count(x => x.userId == tempUserToken.userId) > 0)
                            instance.ValidTokens.Remove(
                                instance.ValidTokens.First(x => x.userId == tempUserToken.userId));
                        instance.ValidTokens.Add(tempUserToken);
                    }
                    break;
                }
            }
        };
        _hypernexObject = hypernexObject;
    }
}