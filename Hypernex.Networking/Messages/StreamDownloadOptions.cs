using Nexport;

namespace Hypernex.Networking.Messages;

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