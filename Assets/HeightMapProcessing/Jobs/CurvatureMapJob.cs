using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static MapFunc;

[BurstCompile]
public struct CurvatureMapJob : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float> HeightMap;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> CurvatureMap;

    public int Width;
    public int Height;

    public float MaxHeight;
    public float CellLength;

    public CurvatureType Curvature;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            int x = i % Width;
            int y = i / Height;

            float2 d1;
            float3 d2;
            GetDerivatives(HeightMap, x, y, out d1, out d2, Width, Height, MaxHeight, CellLength);

            float curvature = 0;
            Color color = Color.white;

            switch (Curvature)
            {
                case CurvatureType.PLAN:
                    curvature = PlanCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.HORIZONTAL:
                    curvature = HorizontalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.VERTICAL:
                    curvature = VerticalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.MEAN:
                    curvature = MeanCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.GAUSSIAN:
                    curvature = GaussianCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.MINIMAL:
                    curvature = MinimalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.MAXIMAL:
                    curvature = MaximalCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.UNSPHERICITY:
                    curvature = UnsphericityCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.ROTOR:
                    curvature = RotorCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.DIFFERENCE:
                    curvature = DifferenceCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.HORIZONTAL_EXCESS:
                    curvature = HorizontalExcessCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.VERTICAL_EXCESS:
                    curvature = VerticalExcessCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.RING:
                    curvature = RingCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case CurvatureType.ACCUMULATION:
                    curvature = AccumulationCurvature(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;
            };
            CurvatureMap[i] = curvature;
        }
    }

    private float PlanCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float zx2 = zx * zx;
        float zy2 = zy * zy;
        float p = zx2 + zy2;

        float n = zy2 * zxx - 2.0f * zxy * zx * zy + zx2 * zyy;
        float d = math.pow(p, 1.5f);

        return SafeDiv(n, d);
    }
    private float HorizontalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float zx2 = zx * zx;
        float zy2 = zy * zy;
        float p = zx2 + zy2;

        float n = zy2 * zxx - 2.0f * zxy * zx * zy + zx2 * zyy;
        float d = p * math.pow(p + 1, 0.5f);

        return SafeDiv(n, d);
    }
    private float VerticalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float zx2 = zx * zx;
        float zy2 = zy * zy;
        float p = zx2 + zy2;

        float n = zx2 * zxx + 2.0f * zxy * zx * zy + zy2 * zyy;
        float d = p * math.pow(p + 1, 1.5f);

        return SafeDiv(n, d);
    }
    private float MeanCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float zx2 = zx * zx;
        float zy2 = zy * zy;
        float p = zx2 + zy2;

        float n = (1 + zy2) * zxx - 2.0f * zxy * zx * zy + (1 + zx2) * zyy;
        float d = 2 * math.pow(p + 1, 1.5f);

        return SafeDiv(n, d);
    }
    private float GaussianCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float zx2 = zx * zx;
        float zy2 = zy * zy;
        float p = zx2 + zy2;

        float n = zxx * zyy - zxy * zxy;
        float d = math.pow(p + 1, 2f);

        return SafeDiv(n, d);
    }
    private float MinimalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float H = MeanCurvature(zx, zy, zxx, zyy, zxy);
        float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);

        return H - SafeSqrt(H * H - K);
    }
    private float UnsphericityCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float H = MeanCurvature(zx, zy, zxx, zyy, zxy);
        float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);

        return SafeSqrt(H * H - K);
    }
    private float MaximalCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float H = MeanCurvature(zx, zy, zxx, zyy, zxy);
        float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);

        return H + SafeSqrt(H * H - K);
    }
    private float RotorCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float zx2 = zx * zx;
        float zy2 = zy * zy;
        float p = zx2 + zy2;

        float n = (zx2 - zy2) * zxy - zx * zy * (zxx - zyy);
        float d = math.pow(p, 1.5f);

        return SafeDiv(n, d);
    }
    private float DifferenceCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);
        float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);

        return 0.5f * (Kv - Kh);
    }
    private float HorizontalExcessCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);
        float Kmin = MinimalCurvature(zx, zy, zxx, zyy, zxy);

        return Kh - Kmin;
    }
    private float VerticalExcessCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);
        float Kmin = MinimalCurvature(zx, zy, zxx, zyy, zxy);

        return Kv - Kmin;
    }
    private float RingCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float H = MeanCurvature(zx, zy, zxx, zyy, zxy);
        float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);
        float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);

        return 2 * H * Kh - Kh * Kh - K;
    }
    private float AccumulationCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);
        float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);

        return Kh * Kv;
    }
}

public enum CurvatureType
{
    /// <summary>
    /// 计划
    /// </summary>
    PLAN,
    /// <summary>
    /// 水平
    /// </summary>
    HORIZONTAL,
    /// <summary>
    /// 垂直
    /// </summary>
    VERTICAL,
    /// <summary>
    /// 平均
    /// </summary>
    MEAN,
    /// <summary>
    /// 高斯
    /// </summary>
    GAUSSIAN,
    /// <summary>
    /// 最小
    /// </summary>
    MINIMAL,
    /// <summary>
    /// 最大
    /// </summary>
    MAXIMAL,
    /// <summary>
    /// 非球面性
    /// </summary>
    UNSPHERICITY,
    /// <summary>
    /// 转子
    /// </summary>
    ROTOR,
    /// <summary>
    /// 差分
    /// </summary>
    DIFFERENCE,
    /// <summary>
    /// 水平超出
    /// </summary>
    HORIZONTAL_EXCESS,
    /// <summary>
    /// 垂直超出
    /// </summary>
    VERTICAL_EXCESS,
    /// <summary>
    /// 环
    /// </summary>
    RING,
    /// <summary>
    /// 累积
    /// </summary>
    ACCUMULATION
}
