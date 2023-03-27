using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// The message that handles Player Voice Chat.
/// </summary>
[Msg]
public class PlayerVoice
{
    [MsgKey(1)] public string MessageId => typeof(PlayerUpdate).FullName;
    [MsgKey(2)] public JoinAuth Auth;
    /// <summary>
    /// Size of each Frame measured in ms (Can be 2.5, 5, 10, 20, 40, or 60)
    /// </summary>
    [MsgKey(3)] public double FrameSize;
    /// <summary>
    /// Sample Rate measured in Hz (Can be 48000, 24000, 16000, 12000, or 8000)
    /// </summary>
    [MsgKey(4)] public int SampleRate;
    /// <summary>
    /// Gets the channels, mono or stereo (Can be 2 or 1)
    /// </summary>
    [MsgKey(5)] public int Channels;
    /// <summary>
    /// The encoder used to encode the audio.
    /// </summary>
    [MsgKey(6)] public string Encoder;
    /// <summary>
    /// The buffer given after encoding audio
    /// </summary>
    [MsgKey(6)] public byte[] Bytes;
}