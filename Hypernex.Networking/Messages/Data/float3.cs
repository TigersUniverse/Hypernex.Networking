using Nexport;

namespace Hypernex.Networking.Messages.Data;

[Msg]
public struct float3
{
    [MsgKey(1)] public string MessageId => typeof(float3).FullName;
    [MsgKey(2)] public float x;
    [MsgKey(3)] public float y;
    [MsgKey(4)] public float z;

    public float3()
    {
        x = 0;
        y = 0;
        z = 0;
    }

    public float3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}