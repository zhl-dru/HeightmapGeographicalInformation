using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static MapFunc;

[BurstCompile]
public struct AspectMapJob : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float> HeightMap;
    [WriteOnly]
    public NativeArray<float> AspectMap;
    public int Width;
    public int Height;
    public float MaxHeight;
    public float CellLength;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            int x = i % Width;
            int y = i / Height;
            float2 d1 = GetFirstDerivative(HeightMap, x, y, Width, Height, MaxHeight, CellLength);
            float aspect = Aspect(d1.x, d1.y);
            AspectMap[i] = aspect;
        }
    }

    private float Aspect(float zx, float zy)
    {
        float gyx = SafeDiv(zy, zx);
        float gxx = SafeDiv(zx, math.abs(zx));

        float aspect = 180f - math.atan(gyx) * Rad2Deg + 90f * gxx;
        aspect /= 360f;

        return aspect;
    }
}
