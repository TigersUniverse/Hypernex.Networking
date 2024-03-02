using Nexport;

namespace Hypernex.Networking.Messages.Data;

[Msg]
public struct float4
{
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
    
    public static float4 operator +(float4 x, float4 y) => new float4(x.x + y.x, x.y + y.y, x.z + y.z, x.w + y.w);
    public static float4 operator -(float4 x, float4 y) => new float4(x.x - y.x, x.y - y.y, x.z - y.z, x.w - y.w);
    public static float4 operator -(float4 x) => new float4(-x.x, -x.y, -x.z, -x.w);
    public static float4 operator *(float4 x, float a) => new float4(x.x * a, x.y * a, x.z * a, x.w * a);
    public static float4 operator /(float4 x, float a) => new float4(x.x / a, x.y / a, x.z * a, x.w * a);
    public static bool operator ==(float4 x, float4 y) => x.x == y.x && x.y == y.y && x.z == y.z && x.w == y.w;
    public static bool operator !=(float4 x, float4 y) => !(x == y);
}