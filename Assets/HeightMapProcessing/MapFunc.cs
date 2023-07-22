using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using System.Runtime.CompilerServices;
using Unity.Burst;

[BurstCompile]
public static class MapFunc
{
    public const float EPS = 1e-18f;
    public static readonly float Rad2Deg = 180f / math.PI;
    public static readonly float Deg2Rad = math.PI / 180f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetNormalizedHeight(NativeArray<float> heights, int x, int y, int width, int height)
    {
        int dx = math.clamp(x, 0, width - 1);
        int dy = math.clamp(y, 0, height - 1);

        return heights[dy * width + dx];
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetHeight(NativeArray<float> heights, int x, int y, int width, int height, float maxHeight)
    {
        return GetNormalizedHeight(heights, x, y, width, height) * maxHeight;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 GetFirstDerivative(NativeArray<float> heights, int x, int y, int width, int height, float maxHeight, float cellLength)
    {
        float w = cellLength;
        float z1 = GetHeight(heights, x - 1, y + 1, width, height, maxHeight);
        float z2 = GetHeight(heights, x + 0, y + 1, width, height, maxHeight);
        float z3 = GetHeight(heights, x + 1, y + 1, width, height, maxHeight);
        float z4 = GetHeight(heights, x - 1, y + 0, width, height, maxHeight);
        float z6 = GetHeight(heights, x + 1, y + 0, width, height, maxHeight);
        float z7 = GetHeight(heights, x - 1, y - 1, width, height, maxHeight);
        float z8 = GetHeight(heights, x + 0, y - 1, width, height, maxHeight);
        float z9 = GetHeight(heights, x + 1, y - 1, width, height, maxHeight);
        //p, q
        float zx = (z3 + z6 + z9 - z1 - z4 - z7) / (6.0f * w);
        float zy = (z1 + z2 + z3 - z7 - z8 - z9) / (6.0f * w);
        return new float2(-zx, -zy);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetDerivatives(NativeArray<float> heights, int x, int y, out float2 d1, out float3 d2, int width, int height, float maxHeight, float cellLength)
    {
        float w = cellLength;
        float w2 = w * w;
        float z1 = GetHeight(heights, x - 1, y + 1, width, height, maxHeight);
        float z2 = GetHeight(heights, x + 0, y + 1, width, height, maxHeight);
        float z3 = GetHeight(heights, x + 1, y + 1, width, height, maxHeight);
        float z4 = GetHeight(heights, x - 1, y + 0, width, height, maxHeight);
        float z5 = GetHeight(heights, x + 0, y + 0, width, height, maxHeight);
        float z6 = GetHeight(heights, x + 1, y + 0, width, height, maxHeight);
        float z7 = GetHeight(heights, x - 1, y - 1, width, height, maxHeight);
        float z8 = GetHeight(heights, x + 0, y - 1, width, height, maxHeight);
        float z9 = GetHeight(heights, x + 1, y - 1, width, height, maxHeight);
        //p, q
        float zx = (z3 + z6 + z9 - z1 - z4 - z7) / (6f * w);
        float zy = (z1 + z2 + z3 - z7 - z8 - z9) / (6f * w);

        //r, t, s
        float zxx = (z1 + z3 + z4 + z6 + z7 + z9 - 2f * (z2 + z5 + z8)) / (3f * w2);
        float zyy = (z1 + z2 + z3 + z7 + z8 + z9 - 2f * (z4 + z5 + z6)) / (3f * w2);
        float zxy = (z3 + z7 - z1 - z9) / (4f * w2);

        d1 = new float2(-zx, -zy);
        d2 = new float3(-zxx, -zyy, -zxy);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SafeSqrt(float v)
    {
        if (v <= 0f) return 0f;
        return math.sqrt(v);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SafeDiv(float n, float d, float eps = EPS)
    {
        if (math.abs(d) < eps) return 0f;
        return n / d;
    }
}
