using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmManagerNormal : MonoBehaviour
{
    //Schwarm bewegt sich nach der durchschnittlichen position, in die durchschnittliche Richtung, andere sollen nicht getroffen werden 
    public GameObject fishprefab;
    public GameObject goalObject;
    public int numFish = 20;
    [HideInInspector]
    public GameObject[] allfish;
    public List<Collider> Obstacles;
    [HideInInspector] public Bounds[] bounds;
    public Vector3 swimLimits = new Vector3(3, 3, 3);
    public float obstacleDistance;

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

    // Start is called before the first frame update
    void Start()
    {
        allfish = new GameObject[numFish];
        for (int i = 0; i < numFish; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(-swimLimits.y, swimLimits.y), Random.Range(-swimLimits.z, swimLimits.z));
            allfish[i] = (GameObject)Instantiate(fishprefab, pos, Quaternion.identity);
            allfish[i].GetComponent<FishAgentRay>().swarmManager = this;
        }
        goalPos = goalObject.transform.position;
        bounds = getBoundsArray(Obstacles);
    }

    // Update is called once per frame
    void Update()
    {
        goalPos = goalObject.transform.position;
    }

    public Bounds[] getBoundsArray(List<Collider> obstacles)
    {
        Bounds[] boundsArray = new Bounds[Obstacles.Count];

        for (int i = 0; i < boundsArray.Length; i++)
        {
            boundsArray[i] = obstacles[i].bounds;
        }
        return boundsArray;
    }
}
