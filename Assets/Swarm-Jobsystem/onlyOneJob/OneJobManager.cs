using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class OneJobManager : MonoBehaviour
{
    [HideInInspector] public GameObject[] allfish;
    public GameObject fishprefab;
    public GameObject goalGameobject;
    public Vector3 swimLimits = new Vector3(3, 3, 3);
    public int numFish = 20;

    public List<Collider> Obstacles;
    public List<RejectionObjectCollider> rejectionsColliderList;
    public List<GameObject> rejectionObjects;

    private NativeList<Vector3> fishPositions;
    private NativeList<Vector3> rejectionPositions;
    private NativeArray<Bounds> obstacleArray;
    private NativeArray<RejectionObjectBounds> RejectionsBoundsNativeArray;

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

    private TransformAccessArray transforms;
    private OneJobAgent jobFishAgent;
    private JobHandle jobHandle;

    [System.Serializable]
    public struct RejectionObjectCollider
    {
        public Collider rejectionObjectCollider;
        public float dectectionRayValue;
        public float rejectionMaxSpeed;
        public float rejectionMinSpeed;
    }

    public struct RejectionObjectBounds
    {
        public Bounds rejectionObjectBounds;
        public float dectectionRayValue;
        public float rejectionMaxSpeed;
        public float rejectionMinSpeed;
    }

    private void OnDisable()
    {
        jobHandle.Complete();

        transforms.Dispose();
        fishPositions.Dispose();
        obstacleArray.Dispose();
        RejectionsBoundsNativeArray.Dispose();
        rejectionPositions.Dispose();
    }

    // Start is called before the first frame update
    void Start()
    {
        transforms = new TransformAccessArray(0, -1);
        obstacleArray = getColliderNativeArray(Obstacles);
        RejectionsBoundsNativeArray = FillRejectionsArray(rejectionsColliderList);

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
        goalPos = goalGameobject.transform.position;

        rejectionPositions = new NativeList<Vector3>(Allocator.TempJob);
        rejectionObjects.ForEach(i => rejectionPositions.Add(i.transform.position));
    }

    // Update is called once per frame
    void Update()
    {
        jobHandle.Complete();
        GetAllPositions();

        RejectionsBoundsNativeArray = FillRejectionsArray(rejectionsColliderList);
        goalPos = goalGameobject.transform.position;
        float time = Time.deltaTime;
        float randomSpeed = Random.Range(minSpeed, maxSpeed);

        jobFishAgent = new OneJobAgent()
        {
            swarmManagerRotationSpeed = rotationSpeed,
            swarmManagerAvoidValue = AvoidValue,
            swarmManagerNeighbourDistance = neighbourDistance,
            swarmManagerGoalpos = goalPos,

            boundsObstacles = obstacleArray,
            rejectionObjects = RejectionsBoundsNativeArray,
           
            speed = randomSpeed,
            positions = fishPositions,
            deltaTime = time
        };
        jobHandle = jobFishAgent.Schedule(transforms);
    }

    private void GetAllPositions()
    {
        fishPositions.Dispose();
        fishPositions = new NativeList<Vector3>(Allocator.TempJob);
        for (int i = 0; i < numFish; i++)
        {
            fishPositions.Add(allfish[i].transform.position);
        }
        rejectionPositions = new NativeList<Vector3>(Allocator.TempJob);
        rejectionObjects.ForEach(i => rejectionPositions.Add(i.transform.position));
    }

    private NativeArray<Bounds> getColliderNativeArray(List<Collider> obstacles)
    {
        NativeArray<Bounds> newArray = new NativeArray<Bounds>(obstacles.Count, Allocator.Persistent);

        for (int i = 0; i < newArray.Length; i++)
        {
            newArray[i] = obstacles[i].bounds;
        }
        return newArray;
    }

    private NativeArray<RejectionObjectBounds> FillRejectionsArray(List<RejectionObjectCollider> rejectionObjects)
    {
        NativeArray<RejectionObjectBounds> newArray = new NativeArray<RejectionObjectBounds>(rejectionObjects.Count, Allocator.Persistent);

        for (int i = 0; i < newArray.Length; i++)
        {
            RejectionObjectBounds arrayObject = new RejectionObjectBounds();
            arrayObject.rejectionObjectBounds = rejectionObjects[i].rejectionObjectCollider.bounds;
            arrayObject.rejectionMinSpeed = rejectionObjects[i].rejectionMinSpeed;
            arrayObject.rejectionMaxSpeed = rejectionObjects[i].rejectionMaxSpeed;
            arrayObject.dectectionRayValue = rejectionObjects[i].dectectionRayValue;
          
            newArray[i] = arrayObject;
        }
        return newArray;
    }
}
