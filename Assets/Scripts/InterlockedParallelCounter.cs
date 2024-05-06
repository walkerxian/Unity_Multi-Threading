

using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// 利用硬件提供的原子操作避免多线程资源竞争
/// 将数据在硬件层面加锁，在没有处理完成之前，其他的线程排队，依次等候，处理完成之后
/// 按照顺序，放下一个线程进来
/// </summary>
public class InterlockedParallelCounter : MonoBehaviour
{
        public unsafe struct InterlockedParallelCounterJob : IJobFor
        {
                public NativeArray<int> data;

                [NativeDisableUnsafePtrRestriction]public int* sum;
                
                public void Execute(int index)
                {
                        Interlocked.Add(ref UnsafeUtility.AsRef<int>(sum), data[index]);
                }
        }
        
        private static readonly int DATA_COUNT = 100;
        private NativeArray<int> m_Data;


        private void Start()
        {
                m_Data = new NativeArray<int>(DATA_COUNT, Allocator.Persistent);
                
                for (int i = 0; i < DATA_COUNT; i++)
                {
                        m_Data[i] = 1;
                }
        }


        private void OnDestroy()
        {
                m_Data.Dispose();
        }

        private void Update()
        {
                unsafe
                {
                        int* sum = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>(), UnsafeUtility.AlignOf<int>(), Allocator.TempJob);

                        *sum = 0;

                        var interlockedParallelCounterJob = new InterlockedParallelCounterJob()
                        {
                                data = m_Data,
                                sum = sum
                        };
                        
                        //只用一个job处理
                        var interlockedCounterJobHandle = interlockedParallelCounterJob.ScheduleParallel(m_Data.Length, 64, new JobHandle());
                        interlockedCounterJobHandle.Complete();
                        
                        Debug.Log($"[InterlockedParallelCounterJob] Sum = {*sum}");

                        UnsafeUtility.Free(sum, Allocator.TempJob);
                        
                        
                }
        }
}