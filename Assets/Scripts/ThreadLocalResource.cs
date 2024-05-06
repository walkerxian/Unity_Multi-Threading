using System;
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Scenes
{
    /// <summary>
    /// 为没给线程分配独立的资源，解决多线程之间的资源竞争
    /// </summary>
    public class ThreadLocalResource : MonoBehaviour
    {
        [BurstCompile]
        struct RandomVelocityJob : IJobFor
        {
            [ReadOnly]
            public NativeArray<float>  speeds;
            
            [ReadOnly]
            public NativeArray<Random> randoms;
            
            public float deltaTime;
            
            /*
             * job执行的过程中Unity会帮我们自动注入这个ID。
             * 这样我们就可以在Execute()方法中利用线程ID获取线程独有的资源了。
             */
            [NativeSetThreadIndex]
            private int m_ThreadIdx;
            
            public NativeArray<float3> positions;
            
            public void Execute(int index)
            {
                //相当于每个线程通过m_ThreadIdx获取自己线程的Random随机变量,最终生成一个位置
                //每一个worker线程拥有独立的random变量
                positions[index] += randoms[m_ThreadIdx].NextFloat3Direction() * speeds[index] * deltaTime;
                
            }
        }
        
        
        private static readonly int POSITION_COUNTS = 100000;
        private NativeArray<Random> m_Randoms;
        private NativeArray<float3> m_Positions;
        
        private NativeArray<float> m_Speeds;
        private JobHandle m_JobHandle;
        
        private StringBuilder logBuilder = new StringBuilder(512);


        private void Start()
        {
            m_Randoms = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent);
            for (int i = 0; i < m_Randoms.Length; i++)
            {
                m_Randoms[i] = Random.CreateFromIndex((uint)i);
            }
            
            m_Speeds = new NativeArray<float>(POSITION_COUNTS, Allocator.Persistent);
            for (var i = 0; i < m_Speeds.Length; i++)
                m_Speeds[i] = m_Randoms[i % m_Randoms.Length].NextFloat(0, 100);
            
            m_Positions= new NativeArray<float3>(POSITION_COUNTS, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            m_JobHandle.Complete();
            m_Randoms.Dispose();
            m_Positions.Dispose();
            m_Speeds.Dispose();
        }

        private void Update()
        {
           
            m_JobHandle.Complete();

            var randomVelocityJob = new RandomVelocityJob()
            {
                speeds = m_Speeds,
                randoms = m_Randoms,
                positions = m_Positions,
                deltaTime = Time.deltaTime,
            };
            //将该job放入JobSystem等待调度
            m_JobHandle = randomVelocityJob.ScheduleParallel(m_Positions.Length, 64, m_JobHandle);
        }
    }
}