using Nexport;

namespace Hypernex.Networking.Messages;

/// <summary>
/// The message that handles Player Voice Chat.
/// </summary>
[Msg]
public class PlayerVoice
{
    [MsgKey(2)] public JoinAuth Auth;
    /// <summary>
    /// Bitrate of data
    /// </summary>
    [MsgKey(3)] public int Bitrate;
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
    [MsgKey(7)] public byte[] Bytes;
    /// <summary>
    /// The length of the encoded audio. May be different than Bytes.Length
    /// </summary>
    [MsgKey(8)] public int EncodeLength;
}