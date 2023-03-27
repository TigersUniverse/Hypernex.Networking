using Nexport;

namespace Hypernex.Networking.Messages.Data;

[Msg]
public struct float4
{
    [MsgKey(1)] public string MessageId => typeof(float4).FullName;
    [MsgKey(2)] public float x;
    [MsgKey(3)] public float y;
    [MsgKey(4)] public float z;
    [MsgKey(5)] public float w;

    public float4()
    {
        x = 0;
        y = 0;
        z = 0;
        w = 0;
    }

    public float4(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
}