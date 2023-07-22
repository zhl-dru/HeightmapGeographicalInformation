using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ExampleResidualMap : Example
{
    public ResidualType ResidualType;

    protected override void SchedulingJob()
    {
        var residualMap = new NativeArray<float>(length, Allocator.TempJob);
        var job = HMP.ResidualMap(heights, residualMap, ResidualType, width, height, MaxHeight, CellLength);
        JobHandle colorJob;
        if (ResidualType == ResidualType.DIFFERENCE)
        {
            colorJob = new FloatToColorJob
            {
                Floats = residualMap,
                Colors = colors,

                Scale = 1000f,
            }.ScheduleBatch(length, length / 16, job);
        }
        else
        {
            colorJob = new FloatToColorJob
            {
                Floats = residualMap,
                Colors = colors,

                Scale = 1f,
            }.ScheduleBatch(length, length / 16, job);
        }
        colorJob.Complete();
    }
}
