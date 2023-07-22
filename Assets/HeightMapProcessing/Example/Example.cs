using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using System.Diagnostics;
using System;
using System.IO;
using System.Runtime.CompilerServices;

public abstract class Example : MonoBehaviour
{
    public RawImage Image;
    public Texture2D HeightMap;
    public float MaxHeight = 3000;
    public float CellLength = 5;

    protected int width;
    protected int height;
    protected int length;

    protected NativeArray<Color> colors;
    protected NativeArray<float> heights;

    [Button]
    protected virtual void Generate()
    {
        width = HeightMap.width;
        height = HeightMap.height;
        length = width * height;
        heights = new NativeArray<float>(length, Allocator.TempJob);
        colors = new NativeArray<Color>(length, Allocator.TempJob);
        GetHeightValues();
        SchedulingJob();
        CreateVisualization();
        Dispose();
    }

    protected abstract void SchedulingJob();

    private void GetHeightValues()
    {
        Color[] colors = HeightMap.GetPixels();
        for (int i = 0; i < colors.Length; i++)
        {
            heights[i] = colors[i].r;
        }
    }

    private void CreateVisualization()
    {
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(colors.ToArray());
        texture.Apply();
        Image.texture = texture;
    }

    private void Dispose()
    {
        heights.Dispose();
        colors.Dispose();
    }
}

[BurstCompile]
public struct FloatToColorJob : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float> Floats;
    [WriteOnly]
    public NativeArray<Color> Colors;

    public float Scale;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            float n = Floats[i] * Scale;
            Color c = new Color(n, n, n);
            Colors[i] = c;
        }
    }
}
[BurstCompile]
public struct Float3ToColorJob : IJobParallelForBatch
{
    [ReadOnly]
    public NativeArray<float3> Floats;
    [WriteOnly]
    public NativeArray<Color> Colors;

    public void Execute(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            float3 n = Floats[i];
            Color c = new Color(n.x, n.y, n.z);
            Colors[i] = c;
        }
    }
}
