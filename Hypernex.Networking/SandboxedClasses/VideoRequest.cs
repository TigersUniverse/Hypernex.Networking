using System;
using Nexport;

namespace Hypernex.Networking.SandboxedClasses;

[Msg(true)]
public class VideoRequest
{
    public VideoRequest(){}

    internal VideoRequest(string m, bool n, StreamDownloadOptions o, string d, bool s)
    {
        MediaUrl = m;
        NeedsClientFetch = n;
        Options = o;
        DownloadUrl = d;
        IsStream = s;
    }
    
    [MsgKey(2)] public string MediaUrl { get; internal set; }
    [MsgKey(3)] public bool NeedsClientFetch { get; internal set; }
    [MsgKey(4)] public StreamDownloadOptions Options { get; internal set; }
    [MsgKey(5)] public string DownloadUrl { get; internal set; }
    [MsgKey(6)] public bool IsStream { get; internal set; }
    
    public string GetMediaUrl() => MediaUrl;
    public bool GetNeedsClientFetch() => NeedsClientFetch;
    public StreamDownloadOptions GetOptions() => Options;
    public string GetDownloadUrl() => DownloadUrl;
    public bool GetIsStream() => IsStream;
}

public static class VideoRequestHelper
{
    public static VideoRequest Create(string mediaUrl, bool needsClientFetch) => new VideoRequest(mediaUrl,
        needsClientFetch, new StreamDownloadOptions(), String.Empty, false);
    
    public static VideoRequest Create(string mediaUrl, string downloadUrl, bool isStream, StreamDownloadOptions options) =>
        new VideoRequest(mediaUrl, false, options, downloadUrl, isStream);
    
    public static VideoRequest Create(string mediaUrl, StreamDownloadOptions options) =>
        new VideoRequest(mediaUrl, false, options, String.Empty, false);

    public static VideoRequest Create(string mediaUrl, string downloadUrl, bool isStream) =>
        Create(mediaUrl, downloadUrl, isStream, new StreamDownloadOptions());
    
    public static void SetMediaUrl(ref VideoRequest v, string m) => v.MediaUrl = m;
    public static void SetMediaUrl(VideoRequest v, string m) => v.MediaUrl = m;
    public static void SetNeedsClientFetch(ref VideoRequest v, bool m) => v.NeedsClientFetch = m;
    public static void SetNeedsClientFetch(VideoRequest v, bool m) => v.NeedsClientFetch = m;
    public static void SetOptions(ref VideoRequest v, StreamDownloadOptions m) => v.Options = m;
    public static void SetOptions(VideoRequest v, StreamDownloadOptions m) => v.Options = m;
    public static void SetDownloadUrl(ref VideoRequest v, string m) => v.DownloadUrl = m;
    public static void SetDownloadUrl(VideoRequest v, string m) => v.DownloadUrl = m;
    public static void SetIsStream(ref VideoRequest v, bool m) => v.IsStream = m;
    public static void SetIsStream(VideoRequest v, bool m) => v.IsStream = m;
}