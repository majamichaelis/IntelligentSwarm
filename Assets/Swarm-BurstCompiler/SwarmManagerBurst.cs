using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class SwarmManagerBurst : MonoBehaviour
{
    public GameObject fishprefab;
    public GameObject goalObject;
    public List<Collider> Obstacles;
    public int numFish = 20;
    [HideInInspector] public GameObject[] allfish;
    public NativeList<Vector3> fishPositions;
    public Vector3 swimLimits = new Vector3(3, 3, 3);
    [HideInInspector] public Vector3 goalPos;
    private NativeArray<Bounds> boundsArray;

    [Range(0.0f, 5.0f)]
    public float minSpeed;
    [Range(0.0f, 5.0f)]
    public float maxSpeed;
    [Range(0.0f, 20.0f)]
    public float neighbourDistance;
    [Range(0.0f, 5.0f)]
    public float rotationSpeed;
    [Range(0.1f, 5.0f)]
    public float AvoidValue;

    TransformAccessArray transforms;
    JobFishAgentBurst jobFishAgent;
    JobFishTurningBurst jobFishTurning;
    JobHandle jobHandle;
    JobHandle jobHandle2;

    private void OnDisable()
    {
        jobHandle.Complete();
        jobHandle2.Complete();
        transforms.Dispose();
        boundsArray.Dispose();
        fishPositions.Dispose();
    }

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
        }
        goalPos = goalObject.transform.position;
        boundsArray = getColliderNativeArray(Obstacles);
    }

    // Update is called once per frame
    void Update()
    {
        float time = Time.deltaTime;
        GetAllPositions();

        foreach(Bounds b in boundsArray)
            Debug.LogError(b.Contains(fishPositions.ElementAt(0)));

        if (Random.Range(0, 100) < 10)
        {
           //goalPos = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(0, swimLimits.y), Random.Range(-swimLimits.z, swimLimits.z));
        }
        goalPos = goalObject.transform.position;
        float randomSpeed = Random.Range(minSpeed, maxSpeed);

        bool turnFish = false;
        /*
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
        }*/



        //setup first job
        jobFishTurning = new JobFishTurningBurst()
        {
            deltaTime = time,
            swarmManagerPosition = transform.position,
            swarmManagerSwimLimits = swimLimits,
            swarmManagerRotationSpeed = rotationSpeed,
            turning = turnFish,
            obstacles = boundsArray,
            positions = fishPositions
        
        };

        //schedule first job
        jobHandle = jobFishTurning.Schedule(transforms);
        jobHandle.Complete();


        //setup second job
        jobFishAgent = new JobFishAgentBurst()
        {
            swarmManagerRotationSpeed = rotationSpeed,
            swarmManagerAvoidValue = AvoidValue,
            swarmManagerNeighbourDistance = neighbourDistance,
            swarmManagerGoalpos = goalPos,
            speed = randomSpeed,
            positions = fishPositions,
            deltaTime = time
        };

        //schedule second job
        //depends on first job 
        jobHandle2 = jobFishAgent.Schedule(transforms, jobHandle);

        //Complete all Jobs
        JobHandle.CompleteAll(ref jobHandle,ref jobHandle2);
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

    public NativeArray<Bounds> getColliderNativeArray(List<Collider> obstacles)
    {
        NativeArray<Bounds> boundsArray = new NativeArray<Bounds>(Obstacles.Count, Allocator.TempJob);

            for(int i = 0; i< boundsArray.Length; i++)
            {
                boundsArray[i] = obstacles[i].bounds;
            }
        return boundsArray;
    }
}
