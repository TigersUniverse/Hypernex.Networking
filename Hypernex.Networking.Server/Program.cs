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
        Console.ForegroundColor = ConsoleColor.White;
    });
Console.ReadKey(true);