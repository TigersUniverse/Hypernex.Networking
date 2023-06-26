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
    
    public static float3 operator +(float3 x, float3 y) => new float3(x.x + y.x, x.y + y.y, x.z + y.z);
    public static float3 operator -(float3 x, float3 y) => new float3(x.x - y.x, x.y - y.y, x.z - y.z);
    public static float3 operator -(float3 x) => new float3(-x.x, -x.y, -x.z);
    public static float3 operator *(float3 x, float a) => new float3(x.x * a, x.y * a, x.z * a);
    public static float3 operator /(float3 x, float a) => new float3(x.x / a, x.y / a, x.z * a);
    public static bool operator ==(float3 x, float3 y) => x.x == y.x && x.y == y.y && x.z == y.z;
    public static bool operator !=(float3 x, float3 y) => !(x == y);
}