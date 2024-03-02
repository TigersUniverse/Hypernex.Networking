using Nexport;

namespace Hypernex.Networking.Messages.Data;

[Msg]
public struct SinCos
{
    [MsgKey(1)] public float Sin;
    [MsgKey(2)] public float Cos;

    public SinCos()
    {
        Sin = 0;
        Cos = 0;
    }

    public SinCos(float sin, float cos)
    {
        Sin = sin;
        Cos = cos;
    }
}