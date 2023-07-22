using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ExampleSlopeMap : Example
{
    protected override void SchedulingJob()
    {
        var slopeMap = new NativeArray<float>(length, Allocator.TempJob);
        var job = HMP.SlopeMap(heights, slopeMap, width, height, MaxHeight, CellLength);
        var colorJob = new FloatToColorJob
        {
            Floats = slopeMap,
            Colors = colors,
            Scale = 1f,
        }.ScheduleBatch(length, length / 16, job);
        colorJob.Complete();
    }
}
