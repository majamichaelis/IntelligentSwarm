using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct JobFishTurning : IJobParallelForTransform
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public Vector3 swarmManagerPosition;
    [ReadOnly] public Vector3 swarmManagerSwimLimits;
    [ReadOnly] public float swarmManagerRotationSpeed;
    public bool turning;
    
    public void Execute(int index, TransformAccess transform)
    {
        Bounds bounds = new Bounds(swarmManagerPosition, swarmManagerSwimLimits);
        Vector3 direction = Vector3.zero;

        if (!bounds.Contains(transform.position))
        {
            turning = true;
            direction = swarmManagerPosition - transform.position;
        }
       
        else
        {
            turning = false;
        }


        if (turning)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), swarmManagerRotationSpeed * deltaTime);
        }
    }
}
