using Nexport;

namespace Hypernex.Networking.Messages;

[Msg]
[MsgCompress(22)]
public class WeightedObjectUpdate
{
    // Player Meta
    [MsgKey(2)] public JoinAuth Auth;

    /// <summary>
    /// What the weight matches to, for example, Parameter or Blendshape
    /// </summary>
    [MsgKey(3)] public string TypeOfWeight;
    /// <summary>
    /// Path to the Object which contains this weight
    /// </summary>
    [MsgKey(4)] public string PathToWeightContainer;
    /// <summary>
    /// The index of this weight, or possibly a name
    /// </summary>
    [MsgKey(5)] public string WeightIndex;
    /// <summary>
    /// The actual weight
    /// </summary>
    [MsgKey(6)] public float Weight;
}