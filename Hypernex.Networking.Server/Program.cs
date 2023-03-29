using Hypernex.Networking;
using Hypernex.Networking.Server;
using HypernexSharp;

HypernexObject hypernexObject;
HypernexSocketServer hypernexSocketServer;

Console.WriteLine("Hypernex GameServer");
Console.WriteLine("-------------------");

Console.WriteLine("Loading Config...");
if (!ServerConfig.LoadConfig())
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Config has not been set!");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("Set the config values to your needs at serverconfig.cfg, then run the program again");
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine("Exiting in 5 seconds...");
    Thread.Sleep(5000);
    return;
}
Console.WriteLine("Loaded Config!");

Console.WriteLine("Creating Connection to Master Server...");
hypernexObject = new HypernexObject(new HypernexSettings
{
    TargetDomain = ServerConfig.LoadedConfig.ServerDomain,
    IsHTTP = ServerConfig.LoadedConfig.IsServerHTTP,
    APIVersion = ServerConfig.LoadedConfig.APIVersion
});
hypernexSocketServer = new HypernexSocketServer(hypernexObject, ServerConfig.LoadedConfig.GlobalIp, 
    ServerConfig.LoadedConfig.GameServerToken, ServerConfig.LoadedConfig.LocalIp, 
    ServerConfig.LoadedConfig.BeginPortRange, ServerConfig.LoadedConfig.EndPortRange, 
    ServerConfig.LoadedConfig.UseMultithreading, 
    ServerConfig.LoadedConfig.ThreadUpdate, 
    ServerConfig.LoadedConfig.UseIPV6, () =>
    {
        Console.WriteLine("Connected to Master Server!");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Ready!");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Input help for a list of commands");
        Console.ForegroundColor = ConsoleColor.White;
    });
HandleCommand(Console.ReadLine() ?? String.Empty);
hypernexObject.CloseGameServerSocket();
Console.ForegroundColor = ConsoleColor.DarkCyan;
Console.WriteLine("Goodbye!");

const string COMMANDS = "instances - Gets a list of all open instances\n" +
                        "exit - Safely shuts down the server and all instances\n" +
                        "help - Shows a list of all commands";

void HandleCommand(string input)
{
    bool end = false;
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
            Console.WriteLine(COMMANDS);
            break;
    }
    if(!end)
        HandleCommand(Console.ReadLine() ?? String.Empty);
}