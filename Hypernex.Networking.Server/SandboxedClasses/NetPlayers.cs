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
}