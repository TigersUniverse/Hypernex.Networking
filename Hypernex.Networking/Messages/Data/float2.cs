using Nexport;

namespace Hypernex.Networking.Messages.Data;

[Msg]
public struct float2
{
    [MsgKey(1)] public string MessageId => typeof(float2).FullName;
    [MsgKey(2)] public float x;
    [MsgKey(3)] public float y;

    public float2()
    {
        x = 0;
        y = 0;
    }

    public float2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
}