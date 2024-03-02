using System;
using Nexport;

namespace Hypernex.Networking.Messages.Bulk;

/// <summary>
/// Combines multiple WeightedObjectUpdates into one for compression
/// </summary>
[Msg]
[MsgCompress(22)]
public class BulkWeightedObjectUpdate
{
    [MsgKey(1)] public JoinAuth Auth;
    [MsgKey(2)] public WeightedObjectUpdate[] WeightedObjectUpdates = Array.Empty<WeightedObjectUpdate>();
    [MsgKey(3)] public bool Reset;
}