using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using static MapFunc;

[BurstCompile]
public struct LandformMapJob : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float> HeightMap;
    [WriteOnly]
    public NativeArray<float> LandformMap;

    public int Width;
    public int Height;

    public float MaxHeight;
    public float CellLength;

    public LandformType Landform;


    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            int x = i % Width;
            int y = i / Height;

            float2 d1;
            float3 d2;
            GetDerivatives(HeightMap, x, y, out d1, out d2, Width, Height, MaxHeight, CellLength);

            float landform = 0;
            Color color = Color.white;

            switch (Landform)
            {
                case LandformType.GAUSSIAN:
                    landform = GaussianLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case LandformType.SHAPE_INDEX:
                    landform = ShapeIndexLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;

                case LandformType.ACCUMULATION:
                    landform = AccumulationLandform(d1.x, d1.y, d2.x, d2.y, d2.z);
                    break;
            }

            LandformMap[i] = landform;
        }
    }

    private float GaussianLandform(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);
        float H = MeanCurvature(zx, zy, zxx, zyy, zxy);

        // ɽ��Բ����
        if (K > 0 && H > 0)
            return 1;

        // ͹��
        if (K < 0 && H > 0)
            return 0.75f;

        //�������Ρ�Antiform������ɽ������Synform������ɽ�ȣ���Plane��
        //Ӧ�ú��ټ���
        if (K == 0 || H == 0)
            return 0.5f;

        // ����
        if (K < 0 && H < 0)
            return 0.25f;

        // ���ݣ���أ�
        if (K > 0 && H < 0)
            return 0;

        throw new System.Exception("δ����ĵ���");
    }
    private float ShapeIndexLandform(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float K = GaussianCurvature(zx, zy, zxx, zyy, zxy);
        float H = MeanCurvature(zx, zy, zxx, zyy, zxy);

        float d = SafeSqrt(H * H - K);

        float si = 2.0f / math.PI * math.atan(SafeDiv(H, d));

        return si * 0.5f + 0.5f;
    }
    private float AccumulationLandform(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float Kh = HorizontalCurvature(zx, zy, zxx, zyy, zxy);
        float Kv = VerticalCurvature(zx, zy, zxx, zyy, zxy);

        // ��ɢ��
        if (Kh > 0 && Kv > 0)
            return 1;

        // ͹����
        if (Kh > 0 && Kv < 0)
            return 0.75f;

        // ƽ�洫�ݡ�
        // Ӧ�ú��ټ���
        if (Kh == 0 || Kv == 0)
            return 0.5f;

        // ������
        if (Kh < 0 && Kv > 0)
            return 0.25f;

        // �ۻ���
        if (Kh < 0 && Kv < 0)
            return 0;

        throw new System.Exception("Unhandled lanform");
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
    private float GaussianCurvature(float zx, float zy, float zxx, float zyy, float zxy)
    {
        float zx2 = zx * zx;
        float zy2 = zy * zy;
        float p = zx2 + zy2;

        float n = zxx * zyy - zxy * zxy;
        float d = math.pow(p + 1, 2);

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
}

public enum LandformType
{
    /// <summary>
    /// ��˹
    /// </summary>
    GAUSSIAN,
    /// <summary>
    /// ��״ָ��
    /// </summary>
    SHAPE_INDEX,
    /// <summary>
    /// �ѻ���ò
    /// </summary>
    ACCUMULATION
}
