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
    [ReadOnly] public float swarmManagerRotationSpeed;
    [ReadOnly] public float swarmManagerAvoidValue;
    [ReadOnly] public float swarmManagerNeighbourDistance;
    [ReadOnly] public Vector3 swarmManagerGoalpos;
    [ReadOnly] public float speed;
    [ReadOnly] public float deltaTime;
    [ReadOnly] public NativeList<Vector3> positions;
    [ReadOnly] public NativeArray<Bounds> boundsObstacles;
    [ReadOnly] public Vector3 swarmManagerPosition;
    [ReadOnly] public Vector3 swarmManagerSwimLimits;
    [ReadOnly] public NativeArray<RejectionObjectBounds> rejectionObjects;


    public void Execute(int index, TransformAccess transform)
    {
        ApplyRules(transform);

        if (inObstacle(transform.position))
        {
            float directionValue;
            if (transform.rotation.z > 0)
            {
                directionValue = 1f;
            }
            else
                directionValue = -1f;

            Vector3 newPosition = Vector3.Slerp(transform.position, new Vector3(transform.position.x, transform.position.y, (transform.position.z + 3f) * directionValue), deltaTime * speed);
            if (!inObstacle(newPosition))
                transform.position = newPosition;

            else
            {
                transform.position += deltaTime * speed * (transform.rotation * new Vector3(1, 0, 0));
            }
        }
        else
        {
            transform.position += deltaTime * speed * (transform.rotation * new Vector3(0, 0, 1));
        }
        RejectionZ(transform);
    }

    private void ApplyRules(TransformAccess transform)
    {
        Vector3 vcentre = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        float nDistance;
        int groupSize = 0;

        foreach (Vector3 fishposition in positions)
        {
            if (fishposition != transform.position)
            {
                nDistance = Vector3.Distance(fishposition, transform.position);

                //nur f?r Fische, die sich in der Nachbarschaft befinden
                if (nDistance <= swarmManagerNeighbourDistance)
                {
                    //eine Untergruppe, dessen Positionen werden ber?cksichtigt 
                    vcentre += fishposition;
                    groupSize++;

                    if (nDistance < swarmManagerAvoidValue)
                    {
                        //wegbewegen 
                        vavoid = vavoid + (transform.position - fishposition);
                    }
                }
            }
        }
        if (groupSize > 0)
        {
            vcentre = vcentre / groupSize + (swarmManagerGoalpos - transform.position);

            Vector3 direction = (vcentre + vavoid) - transform.position;

            foreach(RejectionObjectBounds rejectionPosition in rejectionObjects)
            {
                float distance = Vector3.Distance(rejectionPosition.rejectionObjectBounds.center, transform.position);

                if(distance >= rejectionPosition.dectectionRayValue)
                {
                   
                   
                }
            }
            //schon Richtige Richtung 
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), swarmManagerRotationSpeed * deltaTime);
        }
        
    }

    private bool inObstacle(Vector3 position)
    {
        for (int i = 0; i < boundsObstacles.Length; i++)
        {
            if (boundsObstacles[i].Contains(position))
            {
                return true;
            }
        }
        return false;
    }


    private void RejectionDetectionZ(TransformAccess transform)
    {
        Ray rayForward = new Ray(transform.position, Vector3.forward);
        float distanceToObstacle = Mathf.Infinity;

       
        for (int i = 0; i < rejectionObjects.Length; i++)
        {
            if (rejectionObjects[i].rejectionObjectBounds.IntersectRay(rayForward, out distanceToObstacle))
            {
                    RejectionMoveRightLeft(transform, i, distanceToObstacle);
                
            }
        }
    }

    private void RejectionZ(TransformAccess transform)
    {
        for (int i = 0; i < rejectionObjects.Length; i++)
        {
            {
                float dist = Vector3.Distance(transform.position, rejectionObjects[i].rejectionObjectBounds.center);

                RejectionMoveRightLeft(transform, i, dist);

            }
        }
    }

    private void RejectionMoveUpDown(TransformAccess transform, int indexI, float distanceToObstacle)
    {
        float rotationSpeed = RemapValue(Mathf.Abs(distanceToObstacle), 0, rejectionObjects[indexI].dectectionRayValue, rejectionObjects[indexI].rejectionMinSpeed, rejectionObjects[indexI].rejectionMaxSpeed);
        rotationSpeed = Map(Mathf.Abs(distanceToObstacle), 0, rejectionObjects[indexI].dectectionRayValue, rejectionObjects[indexI].rejectionMinSpeed, rejectionObjects[indexI].rejectionMaxSpeed);

        if (distanceToObstacle > rejectionObjects[indexI].dectectionRayValue && distanceToObstacle >= 0)
        {
            if (transform.position.y > rejectionObjects[indexI].rejectionObjectBounds.center.y)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.up), rotationSpeed * deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.down), rotationSpeed * deltaTime);
            }
        }
        else if (distanceToObstacle >rejectionObjects[indexI].dectectionRayValue && distanceToObstacle <= 0)
        {
            if (transform.position.y > rejectionObjects[indexI].rejectionObjectBounds.center.y)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.up), rotationSpeed * deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.down), rotationSpeed * deltaTime);
            }
        }
    }
    private void RejectionMoveRightLeft(TransformAccess transform, int indexI, float distanceToObstacle)
    {
        float rotationSpeed = RemapValue(Mathf.Abs(distanceToObstacle), 0, rejectionObjects[indexI].dectectionRayValue, rejectionObjects[indexI].rejectionMinSpeed, rejectionObjects[indexI].rejectionMaxSpeed);

        if(!(Mathf.Abs(distanceToObstacle) <= rejectionObjects[indexI].dectectionRayValue))
            return; 

        if (distanceToObstacle < rejectionObjects[indexI].dectectionRayValue && distanceToObstacle >= 0)
        {
            if (transform.position.x > rejectionObjects[indexI].rejectionObjectBounds.center.x)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), rotationSpeed * deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), rotationSpeed * deltaTime);
            }
        }
    }

    private void RejectionMoveRightLeftInverse(TransformAccess transform, int indexI, float distanceToObstacle)
    {
        float rotationSpeed = RemapValue(Mathf.Abs(distanceToObstacle), 0, rejectionObjects[indexI].dectectionRayValue, rejectionObjects[indexI].rejectionMinSpeed, rejectionObjects[indexI].rejectionMaxSpeed);

        if (distanceToObstacle < rejectionObjects[indexI].dectectionRayValue && distanceToObstacle >= 0)
        {
            if (transform.position.x > rejectionObjects[indexI].rejectionObjectBounds.center.x)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), rotationSpeed * deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), rotationSpeed * deltaTime);

            }
        }
        else if (distanceToObstacle > -rejectionObjects[indexI].dectectionRayValue && distanceToObstacle <= 0)
        {
            if (transform.position.x > rejectionObjects[indexI].rejectionObjectBounds.center.x)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), rotationSpeed * deltaTime);

            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), rotationSpeed * deltaTime);

            }
        }
    }


    private void RejectionDetectionX(TransformAccess transform)
    {
        Ray rayForward = new Ray(transform.position, Vector3.right);
        float distanceToObstacle = Mathf.Infinity;

        for (int i = 0; i < rejectionObjects.Length; i++)
        {
            if (rejectionObjects[i].rejectionObjectBounds.IntersectRay(rayForward, out distanceToObstacle))
            {
                //if (rejectionObjects[i].UpDown == true)
                //{
                    //RejectionMoveUpDown(transform, i, distanceToObstacle);
               // }
               // else
               // {
                    RejectionMoveRightLeft(transform, i, distanceToObstacle);
                //}
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