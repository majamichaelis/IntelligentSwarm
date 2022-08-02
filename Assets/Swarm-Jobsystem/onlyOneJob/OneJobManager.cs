using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class OneJobManager : MonoBehaviour
{
    [HideInInspector] public GameObject[] allfish;
    public GameObject Fishprefab;
    public GameObject GoalGameobject;
    public Vector3 SwimLimits = new Vector3(3, 3, 3);
    public int NumFish = 20;

    public List<Collider> Obstacles;
    public List<RejectionObjectCollider> RejectionsColliderList;

    private NativeList<Vector3> _fishPositions;
    private NativeArray<Bounds> _obstacleArray;
    private NativeArray<RejectionObjectBounds> _rejectionsBoundsNativeArray;

    [HideInInspector]
    public Vector3 GoalPos;

    [Range(0.0f, 5.0f)]
    public float MinSpeed;
    [Range(0.0f, 5.0f)]
    public float MaxSpeed;
    [Range(0.0f, 20.0f)]
    public float NeighbourDistance;
    [Range(0.1f, 5.0f)]
    public float RotationSpeed;
    [Range(0.1f, 5.0f)]
    public float AvoidValue;

    private TransformAccessArray _transforms;
    private OneJobAgent _jobFishAgent;
    private JobHandle _jobHandle;

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
        _jobHandle.Complete();

        _transforms.Dispose();
        _fishPositions.Dispose();
        _obstacleArray.Dispose();
        _rejectionsBoundsNativeArray.Dispose();
    }

    // Start is called before the first frame update
    void Start()
    {
        _transforms = new TransformAccessArray(0, -1);
        _obstacleArray = GetColliderNativeArray(Obstacles);
        _rejectionsBoundsNativeArray = FillRejectionsArray(RejectionsColliderList);

        allfish = new GameObject[NumFish];
        _fishPositions = new NativeList<Vector3>(Allocator.TempJob);
        _transforms.capacity = _transforms.length + NumFish;
        for (int i = 0; i < NumFish; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-SwimLimits.x, SwimLimits.x), Random.Range(-SwimLimits.y, SwimLimits.y), Random.Range(-SwimLimits.z, SwimLimits.z));
            allfish[i] = (GameObject)Instantiate(Fishprefab, pos, Quaternion.identity);
            _transforms.Add(allfish[i].transform);
            _fishPositions.Add(allfish[i].transform.position);
        }
        GoalPos = GoalGameobject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        _jobHandle.Complete();
        GetAllPositions();

        _rejectionsBoundsNativeArray = FillRejectionsArray(RejectionsColliderList);
        GoalPos = GoalGameobject.transform.position;
        float time = Time.deltaTime;
        float randomSpeed = Random.Range(MinSpeed, MaxSpeed);

        _jobFishAgent = new OneJobAgent()
        {
            SwarmManagerRotationSpeed = RotationSpeed,
            SwarmManagerAvoidValue = AvoidValue,
            SwarmManagerNeighbourDistance = NeighbourDistance,
            SwarmManagerGoalpos = GoalPos,

            BoundsObstacles = _obstacleArray,
            RejectionObjects = _rejectionsBoundsNativeArray,
           
            Speed = randomSpeed,
            Positions = _fishPositions,
            DeltaTime = time
        };
        _jobHandle = _jobFishAgent.Schedule(_transforms);
    }

    private void GetAllPositions()
    {
        _fishPositions.Dispose();
        _fishPositions = new NativeList<Vector3>(Allocator.TempJob);
        for (int i = 0; i < NumFish; i++)
        {
            _fishPositions.Add(allfish[i].transform.position);
        }
    }

    private NativeArray<Bounds> GetColliderNativeArray(List<Collider> obstacles)
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
