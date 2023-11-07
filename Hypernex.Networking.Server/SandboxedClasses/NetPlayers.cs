using Hypernex.Networking.Messages;

namespace Hypernex.Networking.Server.SandboxedClasses;

public class NetPlayers
{
    private HypernexInstance _instance;

    public NetPlayers()
    {
        throw new Exception("Cannot instantiate NetPlayers!");
    }

    internal NetPlayers(HypernexInstance instance) => _instance = instance;

    public string[] UserIds => _instance.ConnectedClients.ToArray();
    public string InstanceCreatorId => _instance.InstanceCreatorId;
    public string HostId => _instance.HostId;

    public NetPlayer GetNetPlayerUpdate(string userid)
    {
        if (!UserIds.Contains(userid))
            return null;
        foreach (KeyValuePair<string,PlayerUpdate> playerUpdate in MessageHandler.PlayerHandler.PlayerUpdates)
        {
            if (playerUpdate.Key == userid)
                return new NetPlayer(_instance, playerUpdate.Value);
        }
        return null;
    }
}