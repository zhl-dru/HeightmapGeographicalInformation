using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;


public class ExampleNormalMap : Example
{
    public bool Reverse;

    protected override void SchedulingJob()
    {
        var normalMap = new NativeArray<float3>(length, Allocator.TempJob);
        var job = HMP.NormalMap(heights, normalMap, width, height, MaxHeight, CellLength, Reverse);
        var colorJob = new Float3ToColorJob
        {
            Floats = normalMap,
            Colors = colors
        }.ScheduleBatch(length, length / 16, job);
        colorJob.Complete();
    }

    protected override void Generate()
    {
        base.Generate();
    }
}


