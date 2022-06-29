using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmManager : MonoBehaviour
{
    public GameObject fishprefab;
    public int numFish = 20;
    [HideInInspector]
    public GameObject[] allfish;
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

    //Schwarm bewegt sich nach der durchschnittlichen position, in die durchschnittliche Richtung, andere sollen nicht getroffen werden 

    // Start is called before the first frame update
    void Start()
    {
        allfish = new GameObject[numFish];
        for(int i = 0; i< numFish; i++)
        {
            Vector3 pos = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(-swimLimits.y, swimLimits.y), Random.Range(-swimLimits.z, swimLimits.z));
            allfish[i] = (GameObject)Instantiate(fishprefab, pos, Quaternion.identity);
            allfish[i].GetComponent<FishAgent>().swarmManager = this;
        }
        goalPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(Random.Range(0,100) < 10)
        {
            goalPos = this.transform.position + new Vector3(Random.Range(-swimLimits.x, swimLimits.x), Random.Range(0, swimLimits.y), Random.Range(-swimLimits.z, swimLimits.z));
            //Debug.LogError("new goal" + goalPos);
        }
    }
}
