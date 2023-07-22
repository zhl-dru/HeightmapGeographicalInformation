using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static MapFunc;

[BurstCompile]
public struct FlowMapPass1Job : IJobParallelForBatch
{
    [WriteOnly]
    public NativeArray<float> WaterMap;
    [WriteOnly]
    public NativeArray<float> OutFlow;
    public float Amount;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            WaterMap[i] = Amount;
            OutFlow[i] = 0f;
        }
    }
}
[BurstCompile]
public struct FlowMapPass2Job : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float> HeightMap;
    [ReadOnly]
    public NativeArray<float> WaterMap;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> OutFlow;

    public int Width;
    public int Height;

    public float Time;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            int x = i % Width;
            int y = i / Height;

            int xn1 = (x == 0) ? 0 : x - 1;
            int xp1 = (x == Width - 1) ? Width - 1 : x + 1;
            int yn1 = (y == 0) ? 0 : y - 1;
            int yp1 = (y == Height - 1) ? Height - 1 : y + 1;

            float waterHt = WaterMap[Index(x, y)];
            float waterHts0 = WaterMap[Index(xn1, y)];
            float waterHts1 = WaterMap[Index(xp1, y)];
            float waterHts2 = WaterMap[Index(x, yn1)];
            float waterHts3 = WaterMap[Index(x, yp1)];

            float landHt = HeightMap[Index(x, y)];
            float landHts0 = HeightMap[Index(xn1, y)];
            float landHts1 = HeightMap[Index(xp1, y)];
            float landHts2 = HeightMap[Index(x, yn1)];
            float landHts3 = HeightMap[Index(x, yp1)];

            float diff0 = (waterHt + landHt) - (waterHts0 + landHts0);
            float diff1 = (waterHt + landHt) - (waterHts1 + landHts1);
            float diff2 = (waterHt + landHt) - (waterHts2 + landHts2);
            float diff3 = (waterHt + landHt) - (waterHts3 + landHts3);

            // 输出流量是先前的流量加上该时间步的流量。
            float flow0 = math.max(0, OutFlow[Index(x, y, 0)] + diff0);
            float flow1 = math.max(0, OutFlow[Index(x, y, 1)] + diff1);
            float flow2 = math.max(0, OutFlow[Index(x, y, 2)] + diff2);
            float flow3 = math.max(0, OutFlow[Index(x, y, 3)] + diff3);

            float sum = flow0 + flow1 + flow2 + flow3;

            if (sum > 0f)
            {
                // 如果流出通量的总和超过单元格中的量
                // 流量值将按系数K缩小以避免负更新。
                float K = waterHt / (sum * Time);
                if (K > 1f) K = 1f;
                if (K < 0f) K = 0f;

                OutFlow[Index(x, y, 0)] = flow0 * K;
                OutFlow[Index(x, y, 1)] = flow1 * K;
                OutFlow[Index(x, y, 2)] = flow2 * K;
                OutFlow[Index(x, y, 3)] = flow3 * K;
            }
            else
            {
                OutFlow[Index(x, y, 0)] = 0f;
                OutFlow[Index(x, y, 1)] = 0f;
                OutFlow[Index(x, y, 2)] = 0f;
                OutFlow[Index(x, y, 3)] = 0f;
            }
        }
    }

    private int Index(int x, int y)
    {
        return y * Width + x;
    }
    private int Index(int x, int y, int z)
    {
        int length = Width * Height;
        return z * length + y * Width + x;
    }
}
[BurstCompile]
public struct FlowMapPass3Job : IJobParallelForBatch
{
    public NativeArray<float> WaterMap;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> OutFlow;

    public int Width;
    public int Height;

    public float Time;

    public int Left;
    public int Right;
    public int Top;
    public int Bottom;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            int x = i % Width;
            int y = i / Height;
            float flowOUT = OutFlow[Index(x, y, 0)] + OutFlow[Index(x, y, 1)] + OutFlow[Index(x, y, 2)] + OutFlow[Index(x, y, 3)];
            float flowIN = 0.0f;

            // 流入是来自邻近单元格的流入。 请注意您需要的左侧单元格
            // 即细胞向右流动（即流入该细胞）
            flowIN += (x == 0) ? 0.0f : OutFlow[Index(x - 1, y, Right)];
            flowIN += (x == Width - 1) ? 0.0f : OutFlow[Index(x + 1, y, Left)];
            flowIN += (y == 0) ? 0.0f : OutFlow[Index(x, y - 1, Top)];
            flowIN += (y == Height - 1) ? 0.0f : OutFlow[Index(x, y + 1, Bottom)];

            float ht = WaterMap[Index(x, y)] + (flowIN - flowOUT) * Time;
            if (ht < 0f) ht = 0f;

            WaterMap[Index(x, y)] = ht;
        }
    }
    private int Index(int x, int y)
    {
        return y * Width + x;
    }
    private int Index(int x, int y, int z)
    {
        int length = Width * Height;
        return z * length + y * Width + x;
    }
}

[BurstCompile]
public struct FlowMapPass4Job : IJobParallelForBatch
{
    [WriteOnly]
    public NativeArray<float> VelocityMap;
    [NativeDisableParallelForRestriction]
    public NativeArray<float> OutFlow;

    public int Width;
    public int Height;

    public int Left;
    public int Right;
    public int Top;
    public int Bottom;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            int x = i % Width;
            int y = i / Height;

            float dl = (x == 0) ? 0.0f : OutFlow[Index(x - 1, y, Right)] - OutFlow[Index(x, y, Left)];

            float dr = (x == Width - 1) ? 0.0f : OutFlow[Index(x, y, Right)] - OutFlow[Index(x + 1, y, Left)];

            float dt = (y == Height - 1) ? 0.0f : OutFlow[Index(x, y + 1, Bottom)] - OutFlow[Index(x, y, Top)];

            float db = (y == 0) ? 0.0f : OutFlow[Index(x, y, Bottom)] - OutFlow[Index(x, y - 1, Top)];

            float vx = (dl + dr) * 0.5f;
            float vy = (db + dt) * 0.5f;

            VelocityMap[Index(x, y)] = math.sqrt(vx * vx + vy * vy);
        }
    }
    private int Index(int x, int y)
    {
        return y * Width + x;
    }
    private int Index(int x, int y, int z)
    {
        int length = Width * Height;
        return z * length + y * Width + x;
    }
}

[BurstCompile]
public struct FlowMapPass5Job : IJob
{
    public NativeArray<float> VelocityMap;
    public int Length;

    public void Execute()
    {
        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;

        for (int i = 0; i < Length; i++)
        {
            float v = VelocityMap[i];
            if (v < min) min = v;
            if (v > max) max = v;
        }

        float size = max - min;

        for (int i = 0; i < Length; i++)
        {
            float v = VelocityMap[i];

            if (size < 1e-12f)
                v = 0;
            else
                v = (v - min) / size;

            VelocityMap[i] = v;
        }
    }
}
