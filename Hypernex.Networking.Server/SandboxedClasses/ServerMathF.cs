using Hypernex.Networking.Messages.Data;

namespace Hypernex.Sandboxing.SandboxedTypes;

public static class ServerMathF
{
    public static float E => MathF.E;
    public static float PI => MathF.PI;
    public static float Tau => MathF.Tau;

    public static float Deg2Rad => PI * 2 / 360;
    public static float Epsilon => Single.Epsilon;
    public static float Infinity => Single.PositiveInfinity;
    public static float NegativeInfinity => Single.NegativeInfinity;
    public static float Rad2Deg => 360 / (PI * 2);
    
    public static float Abs(float x) => MathF.Abs(x);
    public static float Acos(float x) => MathF.Acos(x);
    public static void Acosh(float x) => MathF.Acosh(x);
    public static float Asin(float x) => MathF.Asin(x);
    public static float Asinh(float x) => MathF.Asinh(x);
    public static float Atan(float x) => MathF.Atan(x);
    public static float Atan2(float y, float x) => MathF.Atan2(y, x);
    public static float Atan2(float2 v) => MathF.Atan2(v.y, v.x);
    public static float Atanh(float x) => MathF.Atanh(x);
    public static float Cbrt(float x) => MathF.Cbrt(x);
    public static float Ceiling(float x) => MathF.Ceiling(x);
    public static float Cos(float x) => MathF.Cos(x);
    public static float Cosh(float x) => MathF.Cosh(x);
    public static float Exp(float x) => MathF.Exp(x);
    public static float Floor(float x) => MathF.Floor(x);
    public static float FusedMultiplyAdd(float x, float y, float z) => MathF.FusedMultiplyAdd(x, y, z);
    public static float IEEERemainder(float x, float y) => MathF.IEEERemainder(x, y);
    public static float IEEERemainder(float2 v) => MathF.IEEERemainder(v.x, v.y);
    public static float Log(float x) => MathF.Log(x);
    public static float Log(float x, float y) => MathF.Log(x, y);
    public static float Log(float2 v) => MathF.Log(v.x, v.y);
    public static float Log10(float x) => MathF.Log10(x);
    public static float Max(float x, float y) => MathF.Max(x, y);
    public static float Max(float2 v) => MathF.Max(v.x, v.y);
    public static float Min(float x, float y) => MathF.Min(x, y);
    public static float Min(float2 v) => MathF.Min(v.x, v.y);
    public static float Pow(float x, float y) => MathF.Pow(x, y);
    public static float Pow(float2 v) => MathF.Pow(v.x, v.y);
    public static float Round(float x, int digits, MidpointRounding midpointRounding) =>
        MathF.Round(x, digits, midpointRounding);
    public static float Sign(float x) => MathF.Sign(x);
    public static float Sin(float x) => MathF.Sin(x);
    public static SinCos SinCos(float x)
    {
        (float, float) r = MathF.SinCos(x);
        return new SinCos(r.Item1, r.Item2);
    }
    public static float Sinh(float x) => MathF.Sinh(x);
    public static float Sqrt(float x) => MathF.Sqrt(x);
    public static float Tan(float x) => MathF.Tan(x);
    public static float Tanh(float x) => MathF.Tanh(x);
    public static float Truncate(float x) => MathF.Truncate(x);
}