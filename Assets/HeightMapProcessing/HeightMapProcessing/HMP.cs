using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public static class HMP
{
    public static JobHandle NormalMap(NativeArray<float> heights, NativeArray<float3> normals, int width, int height, float maxHeight, float cellLength, bool reverse = false, JobHandle dependsOn = default)
    {
        int length = width * height;
        var job = new NormalMapJob
        {
            HeightMap = heights,
            NormalMap = normals,
            Width = width,
            Height = height,
            MaxHeight = maxHeight,
            CellLength = cellLength,
            Reverse = reverse
        }.ScheduleBatch(length, length / 16, dependsOn);
        return job;
    }
    public static JobHandle SlopeMap(NativeArray<float> heights, NativeArray<float> slopes, int width, int height, float maxHeight, float cellLength, JobHandle dependsOn = default)
    {
        int length = width * height;
        var job = new SlopeMapJob
        {
            HeightMap = heights,
            SlopeMap = slopes,
            Width = width,
            Height = height,
            MaxHeight = maxHeight,
            CellLength = cellLength
        }.ScheduleBatch(length, length / 16, dependsOn);
        return job;
    }
    public static JobHandle AspectMap(NativeArray<float> heights, NativeArray<float> aspects, int width, int height, float maxHeight, float cellLength, JobHandle dependsOn = default)
    {
        int length = width * height;
        var job = new AspectMapJob
        {
            HeightMap = heights,
            AspectMap = aspects,
            Width = width,
            Height = height,
            MaxHeight = maxHeight,
            CellLength = cellLength
        }.ScheduleBatch(length, length / 16, dependsOn);
        return job;
    }
    public static JobHandle ResidualMap(NativeArray<float> heights, NativeArray<float> residuals, ResidualType residual, int width, int height, float maxHeight, float cellLength, JobHandle dependsOn = default)
    {
        int length = width * height;
        var job = new ResidualMapJob
        {
            HeightMap = heights,
            ResidualMap = residuals,
            Width = width,
            Height = height,
            MaxHeight = maxHeight,
            CellLength = cellLength,
            Residual = residual
        }.ScheduleBatch(length, length / 16, dependsOn);
        return job;
    }
    public static JobHandle FlowMap(NativeArray<float> heights, NativeArray<float> flows, int width, int height, JobHandle dependsOn = default)
    {
        int length = width * height;

        const int LEFT = 0;
        const int RIGHT = 1;
        const int BOTTOM = 2;
        const int TOP = 3;
        const float TIME = 0.2f;
        int iterations = 5;

        var waterMap = new NativeArray<float>(length, Allocator.TempJob);
        var outFlow = new NativeArray<float>(length * 4, Allocator.TempJob);

        var pass1 = new FlowMapPass1Job
        {
            WaterMap = waterMap,
            OutFlow = outFlow,
            Amount = 0.0001f
        }.ScheduleBatch(length, length / 16, dependsOn);

        JobHandle pass2;
        JobHandle pass3 = pass1;
        for (int i = 0; i < iterations; i++)
        {
            var computeOutflow = new FlowMapPass2Job
            {
                HeightMap = heights,
                WaterMap = waterMap,
                OutFlow = outFlow,
                Width = width,
                Height = height,
                Time = TIME
            }.ScheduleBatch(length, length / 16, pass3);
            pass2 = computeOutflow;
            var updateWaterMap = new FlowMapPass3Job
            {
                WaterMap = waterMap,
                OutFlow = outFlow,
                Width = width,
                Height = height,
                Time = TIME,
                Left = LEFT,
                Right = RIGHT,
                Top = TOP,
                Bottom = BOTTOM
            }.ScheduleBatch(length, length / 16, pass2);
            pass3 = updateWaterMap;
        }

        var pass4 = new FlowMapPass4Job
        {
            VelocityMap = flows,
            OutFlow = outFlow,
            Width = width,
            Height = height,
            Left = LEFT,
            Right = RIGHT,
            Top = TOP,
            Bottom = BOTTOM
        }.ScheduleBatch(length, length / 16, pass3);

        var pass5 = new FlowMapPass5Job
        {
            VelocityMap = flows,
            Length = length
        }.Schedule(pass4);

        waterMap.Dispose(pass5);
        outFlow.Dispose(pass5);
        return pass5;
    }
    public static JobHandle LandformMap(NativeArray<float> heights, NativeArray<float> landforms, LandformType landform, int width, int height, float maxHeight, float cellLength, JobHandle dependsOn = default)
    {
        int length = width * height;
        var job = new LandformMapJob
        {
            HeightMap = heights,
            LandformMap = landforms,
            Width = width,
            Height = height,
            MaxHeight = maxHeight,
            CellLength = cellLength,
            Landform = landform
        }.ScheduleBatch(length, length / 16, dependsOn);
        return job;
    }
    public static JobHandle CurvatureMap(NativeArray<float> heights, NativeArray<float> curvatures, CurvatureType curvature, int width, int height, float maxHeight, float cellLength, JobHandle dependsOn = default)
    {
        int length = width * height;
        var job = new CurvatureMapJob
        {
            HeightMap = heights,
            CurvatureMap = curvatures,
            Width = width,
            Height = height,
            MaxHeight = maxHeight,
            CellLength = cellLength,
            Curvature = curvature
        }.ScheduleBatch(length, length / 16, dependsOn);
        return job;
    }
}
