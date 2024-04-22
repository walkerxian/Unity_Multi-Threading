using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

public class IJobForSample : MonoBehaviour
{
    struct VelocityJob : IJobFor
    {
        //Read the contents of the velocity array in multiple parallel jobs without triggering the safety system
        [ReadOnly]
        public NativeArray<Vector3> velocity;
        public NativeArray<Vector3> position;
        public float deltaTime;
        
        //Through this index parameter,we can access the NativeContainer container in the job
        // and perform relatively independent operations on the elements within the container
        public void Execute(int index)
        {
            position[index] = position[index] + velocity[index] * deltaTime;
        }
    }

    private void Update()
    {
        var position = new NativeArray<Vector3>(500, Allocator.Persistent);
        var velocity = new NativeArray<Vector3>(500, Allocator.Persistent);
    
        for (var i = 0; i < velocity.Length; i++)
        {
            velocity[i] = new Vector3(0, 10, 0);
        }
    
        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            position =  position,
            velocity = velocity
        };
       
        // Schedule job to run immediately on main thread. First parameter is how many for-each iterations to perform.
        // run on main thread
        
        Profiler.BeginSample("Job.Run");
        job.Run(position.Length);
        Profiler.EndSample();
        
        //run on a single worker thread
        // Dependencies are used to ensure that a job executes on worker threads after the dependency has completed execution.
        
        Profiler.BeginSample("Job.Schedule");
        JobHandle scheduleJobDependency = new JobHandle();
        JobHandle scheduleJobHandle = job.Schedule(position.Length, scheduleJobDependency);
        scheduleJobHandle.Complete();
        Profiler.EndSample();
        
        //run on parallel worker threads
        Profiler.BeginSample("Job.ScheduleParallel");
        JobHandle sheduleParraleJobHandle = job.ScheduleParallel(position.Length, 64, scheduleJobHandle);
        sheduleParraleJobHandle.Complete();
        Profiler.EndSample();
        
        Debug.Log(job.position[0]);
    
        position.Dispose();
        velocity.Dispose();
    }
}
