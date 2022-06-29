using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class JobSwarmManager : MonoBehaviour
{
    public GameObject fishprefab;
    public int numFish = 20;
    [HideInInspector]
    public GameObject[] allfish;
    public NativeList<Vector3> fishPositions;

    public Vector3 swimLimits = new Vector3(3, 3, 3);

    //oder soll es ein festes Ziel geben
    [HideInInspector]
    public Vector3 goalPos;

    [Range(0.0f, 5.0f)]
    public float minSpeed;
    [Range(0.0f, 5.0f)]
    public float maxSpeed;
    [Range(0.0f, 20.0f)]
    public float neighbourDistance;
    [Range(0.0f, 5.0f)]
    public float rotationSpeed;
    [Range(1.0f, 5.0f)]
    public float AvoidValue;

    TransformAccessArray transforms;
    JobFishAgent jobFishAgent;
    JobFishTurning jobFishTurning;
    JobHandle jobHandle;
    JobHandle jobHandle2;

    private void OnDisable()
    {
        jobHandle.Complete();
        jobHandle2.Complete();
        transforms.Dispose();
        fishPositions.Dispose();
        
    }

    //Schwarm bewegt sich nach der durchschnittlichen position, in die durchschnittliche Richtung, andere sollen nicht getroffen werden 

    // Start is called before the first frame update
    void Start()
    {
        transforms = new TransformAccessArray(0, -1);

        allfish = new GameObject[numFish];
        fishPositions = new NativeList<Vector3>(Allocator.TempJob);
        transforms.capacity = transforms.length + numFish;
        for (int i = 0; i < numFish; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(-swimLimits.y, swimLimits.y), Random.Range(-swimLimits.z, swimLimits.z));
            allfish[i] = (GameObject)Instantiate(fishprefab, pos, Quaternion.identity);
            transforms.Add(allfish[i].transform);
            fishPositions.Add(allfish[i].transform.position);
            //allfishNative[i] = allfish[i].TryGetComponent<Transform>();
        }
        goalPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.deltaTime;
        jobHandle.Complete();
        GetAllPositions();

        if (Random.Range(0, 100) < 10)
        {
            goalPos = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(0, swimLimits.y), Random.Range(-swimLimits.z, swimLimits.z));
        }
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        jobFishAgent = new JobFishAgent()
        {
            swarmManagerRotationSpeed = rotationSpeed,
            swarmManagerAvoidValue = AvoidValue,
            swarmManagerNeighbourDistance = neighbourDistance,
            swarmManagerGoalpos = goalPos,

            speed = randomSpeed,
            //turning = false,
            positions = fishPositions,
            deltaTime = time
        };
        jobHandle = jobFishAgent.Schedule(transforms);
       
        bool turnFish = false;

        foreach(GameObject fish in allfish)
        {
            RaycastHit hit = new RaycastHit();
            Vector3 direction = Vector3.zero;
            Vector3 forwardVector = transform.rotation * Vector3.forward;
            if (Physics.Raycast(fish.transform.position, forwardVector * 4, out hit))
            {
                turnFish = true;
                direction = Vector3.Reflect(forwardVector, hit.normal);
            }
        }
            

        jobFishTurning = new JobFishTurning()
        {
            deltaTime = time,
            swarmManagerPosition = transform.position,
            swarmManagerSwimLimits = swimLimits,
            swarmManagerRotationSpeed = rotationSpeed,
            turning = turnFish
        };


        /*
        else if (Physics.Raycast(transform.position, forwardVector * 3, out hit))
        {
            turning = true;
            Debug.DrawRay(transform.position, forwardVector * 3, Color.red);
            direction = Vector3.Reflect(forwardVector, hit.normal);

        }
        */


       jobHandle2 = jobFishTurning.Schedule(transforms);
        JobHandle.ScheduleBatchedJobs();

        

    }
    private void GetAllPositions()
    {
        fishPositions.Dispose();
        fishPositions = new NativeList<Vector3>(Allocator.TempJob);
        for (int i = 0; i < numFish; i++)
        {
            fishPositions.Add(allfish[i].transform.position);
        }
    }
        
}
