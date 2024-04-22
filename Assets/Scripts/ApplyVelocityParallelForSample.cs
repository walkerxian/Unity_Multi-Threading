using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ApplyVelocityParallelForSample : MonoBehaviour
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

    // private void Update()
    // {
    //     var position = new NativeArray<Vector3>(500, Allocator.Persistent);
    //     var velocity = new NativeArray<Vector3>(500, Allocator.Persistent);
    //
    //     for (var i = 0; i < velocity.Length; i++)
    //     {
    //         velocity[i] = new Vector3(0, 10, 0);
    //     }
    //
    //     var job = new VelocityJob()
    //     {
    //         deltaTime = Time.deltaTime,
    //         position =  position,
    //         velocity = velocity
    //     };
    //     
    //     // run on main thread
    //     job.Run(position.Length);
    //     //run on a single worker thread
    //     // Dependencies are used to ensure that a job executes on worker threads
    //     // after the dependency has completed execution.
    //     JobHandle sheduleJobDependency = new JobHandle();
    //     JobHandle sheduleJobHandle = job.Schedule(position.Length, sheduleJobDependency);
    //     //run on parallel worker threads
    //     JobHandle sheduleParraleJobHandle = job.ScheduleParallel(position.Length, 64, sheduleJobHandle);
    //     
    //     
    //     sheduleParraleJobHandle.Complete();
    //     Debug.Log(job.position[0]);
    //
    //     position.Dispose();
    //     velocity.Dispose();
    // }
    
    
    
    public void Update()
    {
        var position = new NativeArray<Vector3>(500, Allocator.Persistent);

        var velocity = new NativeArray<Vector3>(500, Allocator.Persistent);
        for (var i = 0; i < velocity.Length; i++)
            velocity[i] = new Vector3(0, 10, 0);

        var job = new VelocityJob()
        {
            deltaTime = Time.deltaTime,
            position = position,
            velocity = velocity
        };

        job.Run(position.Length);

        JobHandle sheduleJobDependency = new JobHandle();
        JobHandle sheduleJobHandle = job.Schedule(position.Length, sheduleJobDependency);

        JobHandle sheduleParralelJobHandle = job.ScheduleParallel(position.Length, 64, sheduleJobHandle);

        sheduleParralelJobHandle.Complete();

        Debug.Log(job.position[0]);

        position.Dispose();
        velocity.Dispose();
    }
}
