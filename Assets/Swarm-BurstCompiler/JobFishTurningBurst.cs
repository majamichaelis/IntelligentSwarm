using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct JobFishTurningBurst: IJobParallelForTransform
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public Vector3 swarmManagerPosition;
    [ReadOnly] public Vector3 swarmManagerSwimLimits;
    [ReadOnly] public float swarmManagerRotationSpeed;
    [ReadOnly] public NativeArray<Bounds> obstacles;
    [ReadOnly] public NativeList<Vector3> positions;

    public bool turning;

    public void Execute(int index, TransformAccess transform)
    {
        Vector3 direction = Vector3.zero;
        foreach (Bounds bounds in obstacles)
        {
            if (bounds.Contains(transform.position))
            {
                turning = true;
                direction = swarmManagerPosition - transform.position;
                Debug.Log("out of bounds");
            }
            else
            {
                turning = false;
            }
        }
        
        if (turning)
        {
           transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), swarmManagerRotationSpeed * deltaTime);
        }
    }
}
