using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static MapFunc;

[BurstCompile]
public struct SlopeMapJob : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float> HeightMap;
    [WriteOnly]
    public NativeArray<float> SlopeMap;
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
            float slope = Slope(d1.x, d1.y);
            SlopeMap[i] = slope;
        }
    }

    private float Slope(float zx, float zy)
    {
        float p = zx * zx + zy * zy;
        float g = SafeSqrt(p);
        return math.atan(g) * Rad2Deg / 90f;
    }
}
