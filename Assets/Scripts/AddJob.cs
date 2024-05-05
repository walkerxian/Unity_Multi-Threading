using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;


public class AddJobTest : MonoBehaviour
{

    //public bool longRunningJob;
    private JobHandle handle;
    
    private NativeArray<float> result;
    
    
    public struct AddJob : IJob
    {
        public float a;
        public float b;
    
        //blittable types :com.unity.collections package
        public NativeArray<float> result;

        public void Execute()
        {
            result[0] = a + b;
        }
    }

    private void Start()
    {
        //NativeContainer requires explicit memory management
        result = new NativeArray<float>(1, Allocator.Persistent);
        
        var job = new AddJob
        {
            a = 1,
            b = 2,
            result = result
        };

        handle = job.Schedule();

        // if (!longRunningJob)
        // {
        //     handle.Complete();//wait until the job has been executed
        //     Debug.Log($"result = {result[0]}");
        // }
    }

    private void Update()
    {
        //A Job itself is not constrained by the Update() and can run across frames
        if (handle.IsCompleted)
        {
            handle.Complete();
            Debug.Log($"result = {result[0]}");
        }
    }

    private void OnDestroy()
    {
        if (result.IsCreated)
            result.Dispose();
    }
}



