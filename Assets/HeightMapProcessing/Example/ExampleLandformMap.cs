using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ExampleLandformMap : Example
{
    public LandformType Landform;

    protected override void SchedulingJob()
    {
        var landformMap = new NativeArray<float>(length, Allocator.TempJob);
        var job = HMP.LandformMap(heights, landformMap, Landform, width, height, MaxHeight, CellLength);
        var colorJob = new FloatToColorJob
        {
            Floats = landformMap,
            Colors = colors,

            Scale = 1f,
        }.ScheduleBatch(length, length / 16, job);

        colorJob.Complete();
    }
}
