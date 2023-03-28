using Tomlet;
using Tomlet.Attributes;
using Tomlet.Models;

namespace Hypernex.Networking.Server;

public class ServerConfig
{
    public static ServerConfig LoadedConfig { get; private set; } = new ();

    [TomlProperty("ServerDomain")]
    [TomlPrecedingComment("The server domain to connect to")]
    public string ServerDomain { get; set; } = String.Empty;
    [TomlProperty("IsServerHTTP")]
    [TomlPrecedingComment("Defines if the Server is HTTP")]
    public bool IsServerHTTP { get; set; }
    [TomlProperty("IsServerWS")]
    [TomlPrecedingComment("Defines if the Server is WS")]
    public string APIVersion { get; set; } = "v1";
    [TomlProperty("GameServerToken")]
    [TomlPrecedingComment("GameServer Token; leave Empty if one isn't needed")]
    public string GameServerToken { get; set; } = String.Empty;
    [TomlProperty("LocalIp")]
    [TomlPrecedingComment("The Local IP Address for servers to run on")]
    public string LocalIp { get; set; } = "0.0.0.0";
    [TomlProperty("GlobalIp")]
    [TomlPrecedingComment("The IP to be shared to the Socket Server")]
    public string GlobalIp { get; set; } = String.Empty;
    [TomlProperty("BeginPortRange")]
    [TomlPrecedingComment("Beginning Port Range for GameServer's Instances")]
    public int BeginPortRange { get; set; } = 10000;
    [TomlProperty("EndPortRange")]
    [TomlPrecedingComment("Ending Port Range for GameServer's Instances")]
    public int EndPortRange { get; set; } = 10100;
    [TomlProperty("UseMultithreading")]
    [TomlPrecedingComment("Have Instances use multiple Threads (recommended on)")]
    public bool UseMultithreading { get; set; } = true;
    [TomlProperty("ThreadUpdate")]
    [TomlPrecedingComment("Time for threads to update (in ms)")]
    public int ThreadUpdate { get; set; } = 10;
    [TomlProperty("UseIPV6")]
    [TomlPrecedingComment("Allow IPs from IPV6 to connect")]
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
        TomlDocument tomlDocument = TomletMain.DocumentFrom(typeof(ServerConfig), LoadedConfig);
        string s = tomlDocument.SerializedValue;
        File.AppendAllText(fileLocation, s);
    }
}