using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ExampleCurvatureMap : Example
{
    public CurvatureType Curvature;

    protected override void SchedulingJob()
    {
        var curvatureMap = new NativeArray<float>(length, Allocator.TempJob);
        var job = HMP.CurvatureMap(heights, curvatureMap, Curvature, width, height, MaxHeight, CellLength);
        JobHandle colorJob;

        if (Curvature == CurvatureType.GAUSSIAN || Curvature == CurvatureType.RING || Curvature == CurvatureType.ACCUMULATION)
        {
            colorJob = new FloatToColorJob
            {
                Floats = curvatureMap,
                Colors = colors,

                Scale = 1000f,
            }.ScheduleBatch(length, length / 16, job);
        }
        else
        {
            colorJob = new FloatToColorJob
            {
                Floats = curvatureMap,
                Colors = colors,

                Scale = 30f,
            }.ScheduleBatch(length, length / 16, job);
        }

        colorJob.Complete();
    }
}
