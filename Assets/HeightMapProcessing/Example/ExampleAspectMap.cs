using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ExampleAspectMap : Example
{
    protected override void SchedulingJob()
    {
        var aspectMap = new NativeArray<float>(length, Allocator.TempJob);
        var job = HMP.AspectMap(heights, aspectMap, width, height, MaxHeight, CellLength);
        var colorJob = new FloatToColorJob
        {
            Floats = aspectMap,
            Colors = colors,
            Scale = 1f,
        }.ScheduleBatch(length, length / 16, job);
        colorJob.Complete();
    }
}
