using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ExampleFlowMap : Example
{
    protected override void SchedulingJob()
    {
        var flowMap = new NativeArray<float>(length, Allocator.TempJob);
        var job = HMP.FlowMap(heights, flowMap, width, height);
        var colorJob = new FloatToColorJob
        {
            Floats = flowMap,
            Colors = colors,

            Scale = 1f,
        }.ScheduleBatch(length, length / 16, job);

        colorJob.Complete();
    }
}
