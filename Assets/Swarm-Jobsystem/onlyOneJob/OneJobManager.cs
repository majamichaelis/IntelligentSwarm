using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class OneJobManager : MonoBehaviour
{
    public GameObject fishprefab;
    public GameObject goalGameobject;
    public List<Collider> Obstacles;
    private NativeArray<Bounds> boundsArray;
    public int numFish = 20;
    [HideInInspector]
    public GameObject[] allfish;
    public NativeList<Vector3> fishPositions;
    public Vector3 swimLimits = new Vector3(3, 3, 3);

    [HideInInspector]
    public Vector3 goalPos;

    [Range(0.0f, 5.0f)]
    public float minSpeed;
    [Range(0.0f, 5.0f)]
    public float maxSpeed;
    [Range(0.0f, 20.0f)]
    public float neighbourDistance;
    [Range(0.1f, 5.0f)]
    public float rotationSpeed;
    [Range(0.1f, 5.0f)]
    public float AvoidValue;

    TransformAccessArray transforms;
    OneJobAgent jobFishAgent;
    JobHandle jobHandle;

    private void OnDisable()
    {
        jobHandle.Complete();
        transforms.Dispose();
        fishPositions.Dispose();
        boundsArray.Dispose();
    }

    // Start is called before the first frame update
    void Start()
    {
        transforms = new TransformAccessArray(0, -1);
        boundsArray = getColliderNativeArray(Obstacles);

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
        goalPos = goalGameobject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        goalPos = goalGameobject.transform.position;
        float time = Time.deltaTime;
        jobHandle.Complete();
        GetAllPositions();
       
        bool turnFish = false;

        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        jobFishAgent = new OneJobAgent()
        {
            swarmManagerRotationSpeed = rotationSpeed,
            swarmManagerAvoidValue = AvoidValue,
            swarmManagerNeighbourDistance = neighbourDistance,
            swarmManagerGoalpos = goalPos,

            bounds = boundsArray,

            speed = randomSpeed,
            positions = fishPositions,
            deltaTime = time
        };
        jobHandle = jobFishAgent.Schedule(transforms);

        //JobHandle.CompleteAll(ref jobHandle);
        //jobHandle.Complete();

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
        NativeArray<Bounds> boundsArray = new NativeArray<Bounds>(Obstacles.Count, Allocator.Persistent);

        for (int i = 0; i < boundsArray.Length; i++)
        {
            boundsArray[i] = obstacles[i].bounds;
        }
        return boundsArray;
    }

}
