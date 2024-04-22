using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

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



