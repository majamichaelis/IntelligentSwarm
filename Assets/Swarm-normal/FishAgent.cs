using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAgent : MonoBehaviour
{
    public SwarmManager swarmManager;
    private float speed;
    bool turning = false;

    // Start is called before the first frame update
    void Start()
    {
        speed = Random.Range(swarmManager.minSpeed, swarmManager.maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        
        //kann noch ge?ndert werden
        Bounds bounds = new Bounds(swarmManager.transform.position, swarmManager.swimLimits);

        RaycastHit hit = new RaycastHit();
        Vector3 direction = Vector3.zero;


        if (!bounds.Contains(transform.position))
        {
            turning = true;
            direction = swarmManager.transform.position - transform.position;
        }
        else if (Physics.Raycast(transform.position, this.transform.forward * 1, out hit))
        {
            turning = true;
            Debug.DrawRay(this.transform.position, this.transform.forward * 1, Color.red);
            direction = Vector3.Reflect(this.transform.forward, hit.normal);

        }
        else
        {
            turning = false;
        }


        if (turning)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), swarmManager.rotationSpeed * Time.deltaTime);
        }
        ApplyRules();


        transform.Translate(0, 0, Time.deltaTime * speed);
    }

    private void ApplyRules()
    {
        GameObject[] gos;
        gos = swarmManager.allfish;

        Vector3 vcentre = Vector3.zero;
        Vector3 vavoid = Vector3.zero;
        float gSpeed = 0.01f;
        float nDistance;
        int groupSize = 0;

        foreach(GameObject go in gos)
        {
            if(go != this.gameObject)
            {
                nDistance = Vector3.Distance(go.transform.position, this.transform.position);

                //nur f?r Fische, die sich in der Nachbarschaft befinden
                if(nDistance <= swarmManager.neighbourDistance)
                {
                    //eine Untergruppe, dessen Positionen werden ber?cksichtigt 
                    vcentre += go.transform.position;
                    groupSize++;

                    if(nDistance < swarmManager.AvoidValue)
                    {
                        //wegbewegen 
                        vavoid = vavoid + (this.transform.position - go.transform.position);
                    }

                    FishAgent anotherFish = go.GetComponent<FishAgent>();
                    gSpeed = gSpeed + anotherFish.speed;
                }
            }
        }
        if (groupSize > 0)
        {
            vcentre = vcentre/groupSize + (swarmManager.goalPos - this.transform.position);
            speed = gSpeed/groupSize;

            Vector3 direction = (vcentre + vavoid)-transform.position;

            //schon Richtige Richtung 
            if (direction != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), swarmManager.rotationSpeed * Time.deltaTime);
        }
    }
}
