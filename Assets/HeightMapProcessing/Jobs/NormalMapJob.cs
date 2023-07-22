using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static MapFunc;

[BurstCompile]
public struct NormalMapJob : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float> HeightMap;
    [WriteOnly]
    public NativeArray<float3> NormalMap;
    public int Width;
    public int Height;
    public float MaxHeight;
    public float CellLength;
    public bool Reverse;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            int x = i % Width;
            int y = i / Height;
            float2 d1 = GetFirstDerivative(HeightMap, x, y, Width, Height, MaxHeight, CellLength);
            float dx, dy, dz;
            if (!Reverse)
            {
                dx = d1.x * 0.5f + 0.5f;
                dy = -d1.y * 0.5f + 0.5f;
                dz = 1f;
            }
            else
            {
                dx = -d1.x * 0.5f + 0.5f;
                dy = d1.y * 0.5f + 0.5f;
                dz = 1f;
            }

            float3 n = new float3(dx, dy, dz);
            math.normalize(n);
            NormalMap[i] = n;
        }
    }
}
