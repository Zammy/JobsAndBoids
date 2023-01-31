
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public static class MathEx
{
    static readonly float3 up = new float3(0, 1, 0);
    public static float3 float3_Up => up;

    static readonly float3 forward = new float3(0, 0, 1);
    public static float3 float3_Forward => forward;

    static readonly float3 right = new float3(1, 0, 0);
    public static float3 float3_Right => right;

    [BurstCompile]
    public static float RangeRandomFloat(ref Random randomValue, in float2 range)
    {
        return randomValue.NextFloat(range.x, range.y);
    }

    [BurstCompile]
    public static float AnglesToRadians(in float angle)
    {
        return angle * math.PI / 180;
    }
}