using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
public class VideoRequest
{
    [MsgKey(2)] public string MediaUrl;
    [MsgKey(3)] public bool NeedsClientFetch;
    [MsgKey(4)] public StreamDownloadOptions Options;
    [MsgKey(5)] public string DownloadUrl;
    [MsgKey(6)] public bool IsStream;
}