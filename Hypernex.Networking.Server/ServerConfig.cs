#nullable disable

using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;

namespace Hypernex.Networking.Server;

public class ServerConfig
{
    public static ServerConfig LoadedConfig { get; private set; } = new ();
    
    [TomlProperty("ServerDomain")]
    [TomlInlineComment("The server domain to connect to")]
    public string ServerDomain { get; set; }
    [TomlProperty("IsServerHTTPS")]
    [TomlInlineComment("Defines if the Server is HTTP")]
    public bool IsServerHTTP { get; set; }
    [TomlProperty("IsServerWS")]
    [TomlInlineComment("Defines if the Server is WS")]
    public bool IsServerWS { get; set; }
    [TomlProperty("APIVersion")]
    [TomlInlineComment("The API Version to Get Requests")]
    public string APIVersion { get; set; } = "v1";
    [TomlProperty("GameServerToken")]
    [TomlInlineComment("GameServer Token; leave Empty if one isn't needed")]
    public string GameServerToken { get; set; }
    [TomlProperty("LocalIp")]
    [TomlInlineComment("The Local IP Address for servers to run on")]
    public string LocalIp { get; set; } = "0.0.0.0";
    [TomlProperty("GlobalIp")]
    [TomlInlineComment("The IP to be shared to the Socket Server")]
    public string GlobalIp { get; set; }
    [TomlProperty("BeginPortRange")]
    [TomlInlineComment("Beginning Port Range for GameServer's Instances")]
    public int BeginPortRange { get; set; } = 10000;
    [TomlProperty("EndPortRange")]
    [TomlInlineComment("Ending Port Range for GameServer's Instances")]
    public int EndPortRange { get; set; } = 10100;
    [TomlProperty("UseMultithreading")]
    [TomlInlineComment("Have Instances use multiple Threads (recommended on)")]
    public bool UseMultithreading { get; set; } = true;
    [TomlProperty("UseMultithreading")]
    [TomlInlineComment("Time for threads to update (in ms)")]
    public int ThreadUpdate { get; set; } = 10;
    [TomlProperty("UseIPV6")]
    [TomlInlineComment("Allow IPs from IPV6 to connect")]
    public bool UseIPV6 { get; set; }

    public static bool LoadConfig(string fileLocation = "serverconfig.cfg")
    {
        if (File.Exists(fileLocation))
        {
            string fileData = File.ReadAllText(fileLocation);
            if (!string.IsNullOrEmpty(fileLocation))
                LoadedConfig = TomletMain.To<ServerConfig>(fileData);
            return true;
        }
        SaveConfig(fileLocation);
        return false;
    }

    public static void SaveConfig(string fileLocation = "serverconfig.cfg")
    {
        TomlDocument tomlDocument = TomletMain.DocumentFrom(LoadedConfig);
        string s = TomletMain.TomlStringFrom(tomlDocument);
        File.AppendAllText(fileLocation, s);
    }
}