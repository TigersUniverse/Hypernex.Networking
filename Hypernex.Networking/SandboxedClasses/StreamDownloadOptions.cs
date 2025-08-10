using Nexport;

namespace Hypernex.Networking.SandboxedClasses;

[Msg]
public class StreamDownloadOptions
{
    [MsgKey(2)] public bool AudioOnly;

    public StreamDownloadOptions(){}
    
    public StreamDownloadOptions(bool audioOnly = false)
    {
        AudioOnly = audioOnly;
    }
}