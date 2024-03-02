using Nexport;

namespace Hypernex.Networking.Messages.Data;

[Msg]
public struct float2
{
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

    public static float2 operator +(float2 x, float2 y) => new float2(x.x + y.x, x.y + y.y);
    public static float2 operator -(float2 x, float2 y) => new float2(x.x - y.x, x.y - y.y);
    public static float2 operator -(float2 x) => new float2(-x.x, -x.y);
    public static float2 operator *(float2 x, float a) => new float2(x.x * a, x.y * a);
    public static float2 operator /(float2 x, float a) => new float2(x.x / a, x.y / a);
    public static bool operator ==(float2 x, float2 y) => x.x == y.x && x.y == y.y;
    public static bool operator !=(float2 x, float2 y) => !(x == y);
}