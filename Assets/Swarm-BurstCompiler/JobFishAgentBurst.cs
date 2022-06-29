using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct JobFishAgentBurst : IJobParallelForTransform
{
    [ReadOnly] public float swarmManagerRotationSpeed;
    [ReadOnly] public float swarmManagerAvoidValue;
    [ReadOnly] public float swarmManagerNeighbourDistance;
    [ReadOnly] public Vector3 swarmManagerGoalpos;
    [ReadOnly] public float speed;
    [ReadOnly] public float deltaTime;
    [ReadOnly] public NativeList<Vector3> positions;

    public void Execute(int index, TransformAccess transform)
    {
        ApplyRules(transform);
        transform.position += deltaTime * speed * (transform.rotation * new Vector3(0, 0, 1));
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
                    //vielleicht neue zufällige Zeit?
                    //FishAgent anotherFish = go.GetComponent<FishAgent>();
                    //gSpeed = gSpeed + anotherFish.speed;
                }
            }
        }
        if (groupSize > 0)
        {
            vcentre = vcentre / groupSize + (swarmManagerGoalpos - transform.position);
            //speed = gSpeed / groupSize;

            Vector3 direction = (vcentre + vavoid) - transform.position;

            //schon Richtige Richtung 
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), swarmManagerRotationSpeed * deltaTime);
        }
    }
}
