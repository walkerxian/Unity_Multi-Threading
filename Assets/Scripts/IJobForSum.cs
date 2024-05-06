using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class IJobForSum : MonoBehaviour
{
    struct AddJob : IJobFor
    {
        public NativeArray<int> termsLeft;
        public NativeArray<int> termsRight;
        
        public NativeArray<int> sums;


        public void Execute(int index)
        {
            //throw new NotImplementedException();
            sums[index] = termsLeft[index] + termsRight[index];
        }
    }

    private void Start()
    {
        //prepare the input data
        int numCount = 18;
        var termsLeftData = new NativeArray<int>(numCount, Allocator.TempJob);
        var termsRightData = new NativeArray<int>(numCount, Allocator.TempJob);
        
        var sumsData = new NativeArray<int>(numCount, Allocator.TempJob);

        var addJob = new AddJob()
        {
            termsLeft = termsLeftData,
            termsRight = termsRightData,
            sums =  sumsData
        };
        
        //Test1
        addJob.Run(numCount);//主线程执行numCount次Execute()
        
        //Test2
        var handle = addJob.Schedule(numCount, new JobHandle());
        
        //Test3:created numCount jobs,and every job execute 1 time
        handle = addJob.ScheduleParallel(numCount, 1, handle);
        
        handle.Complete();

        for (int i = 0; i < numCount; i++)
        {
            Debug.Log($"Sum[{i}] = {sumsData[i]}");
        }
        
        //must be Dispose,or lead to memory leak
        termsLeftData.Dispose();
        termsRightData.Dispose();
        sumsData.Dispose();
    }
}