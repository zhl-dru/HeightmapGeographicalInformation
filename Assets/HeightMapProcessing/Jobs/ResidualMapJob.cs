using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static MapFunc;

[BurstCompile]
public struct ResidualMapJob : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float> HeightMap;
    [WriteOnly]
    public NativeArray<float> ResidualMap;
    public int Width;
    public int Height;
    public float MaxHeight;
    public float CellLength;
    public ResidualType Residual;

    public void Execute(int startIndex, int count)
    {
        var elevations = new NativeList<float>(Allocator.Temp);
        for (int i = startIndex; i < startIndex + count; i++)
        {
            elevations.Clear();
            int x = i % Width;
            int y = i / Height;

            int window = 3;

            for (int m = -window; m <= window; m++)
            {
                for (int n = -window; n <= window; n++)
                {
                    int xm = x + m;
                    int yn = y + n;

                    if (xm < 0 || xm >= Width) continue;
                    if (yn < 0 || yn >= Height) continue;

                    float h = GetNormalizedHeight(HeightMap, xm, yn, Width, Height);
                    elevations.Add(h);
                }
            }

            float residual = 0;
            float h0 = GetNormalizedHeight(HeightMap, x, y, Width, Height);

            switch (Residual)
            {
                case ResidualType.ELEVATION:
                    residual = h0;
                    break;

                case ResidualType.MEAN:
                    residual = MeanElevation(elevations);
                    break;

                case ResidualType.DIFFERENCE:
                    residual = DifferenceFromMeanElevation(h0, elevations);
                    break;

                case ResidualType.STDEV:
                    residual = DeviationFromMeanElevation(h0, elevations);
                    break;

                case ResidualType.DEVIATION:
                    residual = DeviationFromMeanElevation(h0, elevations);
                    break;

                case ResidualType.PERCENTILE:
                    residual = Percentile(h0, elevations);
                    break;
            }
            ResidualMap[i] = residual;
        }
        elevations.Dispose();
    }

    private float MeanElevation(NativeList<float> elevations)
    {
        return Mean(elevations);
    }
    private float StdevElevation(NativeList<float> elevations)
    {
        var mean = MeanElevation(elevations);
        return math.sqrt(Variance(elevations, mean));
    }
    private float DifferenceFromMeanElevation(float h, NativeList<float> elevations)
    {
        return h - MeanElevation(elevations);
    }
    private float DeviationFromMeanElevation(float h, NativeList<float> elevations)
    {
        var o = StdevElevation(elevations);
        var d = DifferenceFromMeanElevation(h, elevations);

        return SafeDiv(d, o);
    }
    private float Percentile(float h, NativeList<float> elevations)
    {
        int count = elevations.Length;
        float num = 0f;

        for (int i = 0; i < count; i++)
            if (elevations[i] < h) num++;

        if (num == 0f) return 0f;
        return num / count;
    }
    private float Mean(NativeList<float> data)
    {
        int count = data.Length;
        if (count == 0f) return 0f;

        float u = 0f;
        for (int i = 0; i < count; i++)
            u += data[i];

        return u / count;
    }
    private float Variance(NativeList<float> data, float mean)
    {
        int count = data.Length;
        if (count == 0f) return 0f;

        float v = 0f;
        for (int i = 0; i < count; i++)
        {
            float diff = data[i] - mean;
            v += diff * diff;
        }

        return v / count;
    }
}

public enum ResidualType
{
    /// <summary>
    /// 海拔
    /// </summary>
    ELEVATION,
    /// <summary>
    /// 平均值
    /// </summary>
    MEAN,
    /// <summary>
    /// 差
    /// </summary>
    DIFFERENCE,
    /// <summary>
    /// 标准差
    /// </summary>
    STDEV,
    /// <summary>
    /// 偏差
    /// </summary>
    DEVIATION,
    /// <summary>
    /// 百分位
    /// </summary>
    PERCENTILE
}
