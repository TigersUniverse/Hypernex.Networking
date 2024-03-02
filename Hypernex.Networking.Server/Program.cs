using Hypernex.CCK;
using Hypernex.Networking;
using Hypernex.Networking.Messages;
using Hypernex.Networking.Server;
using HypernexSharp;
using Nexport;

HypernexObject hypernexObject;
HypernexSocketServer hypernexSocketServer;
bool end = false;

ServerLogger logger = new ServerLogger();
logger.SetLogger();

logger.Log("Hypernex GameServer");
logger.Log("-------------------");

logger.Log("Loading Config...");
if (!ServerConfig.LoadConfig())
{
    logger.Warn("Config has not been set!");
    logger.Warn("Set the config values to your needs at serverconfig.cfg, then run the program again");
    logger.Log("Exiting in 5 seconds...");
    Thread.Sleep(5000);
    return;
}
logger.Log("Loaded Config!");

logger.Log("Registering Events...");
HypernexSocketServer.OnInstance += instance =>
{
    instance.OnMessage += (id, meta, channel) =>
    {
        ClientIdentifier clientIdentifier = instance.GetClientIdentifierFromUserId(id);
        if (clientIdentifier != null)
            MessageHandler.HandleMessage(instance, meta, channel, clientIdentifier);
    };
    instance.OnClientConnect += userid =>
    {
        if (MessageHandler.ObjectHandler.Objects.ContainsKey(instance.InstanceId))
        {
            Dictionary<string, List<WorldObjectUpdate>> d =
                MessageHandler.ObjectHandler.Objects[instance.InstanceId];
            foreach (WorldObjectUpdate worldObjectUpdate in d.Values.SelectMany(
                         worldObjectUpdates => worldObjectUpdates))
            {
                ClientIdentifier clientIdentifier = instance.GetClientIdentifierFromUserId(userid);
                byte[] msg = Msg.Serialize(worldObjectUpdate);
                instance.SendMessageToClient(clientIdentifier, msg);
            }
        }
    };
    instance.OnClientDisconnect +=
        userid => MessageHandler.ObjectHandler.RemovePlayerFromWorldObjects(instance, userid);
};
logger.Log("Registered Events!");

logger.Log("Creating Connection to Master Server...");
hypernexObject = new HypernexObject(new HypernexSettings
{
    TargetDomain = ServerConfig.LoadedConfig.ServerDomain,
    IsHTTP = ServerConfig.LoadedConfig.IsServerHTTP,
    APIVersion = ServerConfig.LoadedConfig.APIVersion
});
hypernexSocketServer = new HypernexSocketServer(hypernexObject, ServerConfig.LoadedConfig.GlobalIp, scripts =>
    {
        foreach (NexboxScript nexboxScript in scripts.Item3)
        {
            ScriptHandler s = new ScriptHandler(scripts.Item1, scripts.Item2);
            s.LoadAndExecuteScript(nexboxScript, ServerConfig.LoadedConfig.UseMultithreading);
        }
    },
    ServerConfig.LoadedConfig.GameServerToken, ServerConfig.LoadedConfig.LocalIp, 
    ServerConfig.LoadedConfig.BeginPortRange, ServerConfig.LoadedConfig.EndPortRange, 
    ServerConfig.LoadedConfig.UseMultithreading, 
    ServerConfig.LoadedConfig.ThreadUpdate, 
    ServerConfig.LoadedConfig.UseIPV6, () =>
    {
        logger.Log("Connected to Master Server!");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Ready!");
        logger.Log("Input help for a list of commands");
    }, instance =>
    {
        Logger.CurrentLogger.Log("Closed Instance " + instance.InstanceId);
        foreach (ScriptHandler scriptHandler in new List<ScriptHandler>(ScriptHandler.Instances))
        {
            if (scriptHandler.Compare(instance))
                scriptHandler.Dispose();
        }
        MessageHandler.ObjectHandler.RemoveInstanceFromWorldObjects(instance);
    });
HandleCommand(Console.ReadLine() ?? String.Empty);
hypernexObject.CloseGameServerSocket();
Console.ForegroundColor = ConsoleColor.DarkCyan;
Console.WriteLine("Goodbye!");
Environment.Exit(0);

const string COMMANDS = "instances - Gets a list of all open instances\n" +
                        "exit - Safely shuts down the server and all instances\n" +
                        "help - Shows a list of all commands";

void HandleCommand(string input)
{
    string[] s = input.Split(" ");
    if(s.Length <= 0)
        HandleCommand(Console.ReadLine() ?? String.Empty);
    switch (s[0].ToLower())
    {
        case "instances":
            foreach (HypernexInstance hypernexInstance in hypernexSocketServer.Instances)
                Console.WriteLine(hypernexInstance);
            break;
        case "exit":
            end = true;
            break;
        case "help":
            logger.Log(COMMANDS);
            break;
    }
    if(!end)
        HandleCommand(Console.ReadLine() ?? String.Empty);
}