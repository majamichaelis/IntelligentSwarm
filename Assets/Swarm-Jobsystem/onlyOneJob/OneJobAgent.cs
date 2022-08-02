using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using static OneJobManager;

[BurstCompile]
public struct OneJobAgent : IJobParallelForTransform
{
    [ReadOnly] public float SwarmManagerRotationSpeed;
    [ReadOnly] public float SwarmManagerAvoidValue;
    [ReadOnly] public float SwarmManagerNeighbourDistance;
    [ReadOnly] public Vector3 SwarmManagerPosition;
    [ReadOnly] public Vector3 SwarmManagerSwimLimits;
    [ReadOnly] public Vector3 SwarmManagerGoalpos;
    [ReadOnly] public float Speed;
    [ReadOnly] public float DeltaTime;
    [ReadOnly] public NativeList<Vector3> Positions;
    [ReadOnly] public NativeArray<Bounds> BoundsObstacles;
    [ReadOnly] public NativeArray<RejectionObjectBounds> RejectionObjects;


    public void Execute(int index, TransformAccess transform)
    {
        ApplyRules(transform);

        if (InObstacle(transform.position))
        {
            float directionValue;
            if (transform.rotation.z > 0)
            {
                directionValue = 1f;
            }
            else
                directionValue = -1f;

            Vector3 newPosition = Vector3.Slerp(transform.position, new Vector3(transform.position.x, transform.position.y, (transform.position.z + 3f) * directionValue), DeltaTime * Speed);
            if (!InObstacle(newPosition))
                transform.position = newPosition;

            else
            {
                transform.position += DeltaTime * Speed * (transform.rotation * new Vector3(1, 0, 0));
            }
        }
        else
        {
            transform.position += DeltaTime * Speed * (transform.rotation * new Vector3(0, 0, 1));
        }
        RejectionZ(transform);
    }

    private void ApplyRules(TransformAccess transform)
    {
        Vector3 vcentre = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        float nDistance;
        int groupSize = 0;

        foreach (Vector3 fishposition in Positions)
        {
            if (fishposition != transform.position)
            {
                nDistance = Vector3.Distance(fishposition, transform.position);

                if (nDistance <= SwarmManagerNeighbourDistance)
                {
                    vcentre += fishposition;
                    groupSize++;

                    if (nDistance < SwarmManagerAvoidValue)
                    {
                        vavoid = vavoid + (transform.position - fishposition);
                    }
                }
            }
        }
        if (groupSize > 0)
        {
            vcentre = vcentre / groupSize + (SwarmManagerGoalpos - transform.position);

            Vector3 direction = (vcentre + vavoid) - transform.position;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), SwarmManagerRotationSpeed * DeltaTime);
        }
        
    }

    private bool InObstacle(Vector3 position)
    {
        for (int i = 0; i < BoundsObstacles.Length; i++)
        {
            if (BoundsObstacles[i].Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    private void RejectionZ(TransformAccess transform)
    {
        for (int i = 0; i < RejectionObjects.Length; i++)
        {
            {
                float dist = Vector3.Distance(transform.position, RejectionObjects[i].rejectionObjectBounds.center);

                RejectionMoveRightLeft(transform, i, dist);

            }
        }
    }

    private void RejectionMoveRightLeft(TransformAccess transform, int indexI, float distanceToObstacle)
    {
        float rotationSpeed = RemapValue(Mathf.Abs(distanceToObstacle), 0, RejectionObjects[indexI].dectectionRayValue, RejectionObjects[indexI].rejectionMinSpeed, RejectionObjects[indexI].rejectionMaxSpeed);

        if(!(Mathf.Abs(distanceToObstacle) <= RejectionObjects[indexI].dectectionRayValue))
            return; 

        if (distanceToObstacle < RejectionObjects[indexI].dectectionRayValue && distanceToObstacle >= 0)
        {
            if (transform.position.x > RejectionObjects[indexI].rejectionObjectBounds.center.x)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), rotationSpeed * DeltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), rotationSpeed * DeltaTime);
            }
        }
    }
    /*
     * input: distance Value (from1 to1) to calculate a value between from2 to2
     */
    private float RemapValue(float value, float from2, float to2, float from1, float to1)
    {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}