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
        //kann noch geändert werden
        Bounds bounds = new Bounds(swarmManager.transform.position, swarmManager.swimLimits);

        RaycastHit hit;
        Physics.Raycast(transform.position, this.transform.forward * 50, out hit);
        Debug.DrawRay(this.transform.position, this.transform.forward * 50, Color.red);
        



        if (!bounds.Contains(transform.position))
        {
            turning = true;
        }
        else
        {
            turning = false;
        }
        if (turning)
        {
            Vector3 direction = swarmManager.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), swarmManager.rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Vector3.Reflect(this.transform.forward, hit.normal);
            /*
             * zufällige Geschwindigkeit
            if(Random.Range(0,100) < 10)
            {
                speed = Random.Range(swarmManager.minSpeed, swarmManager.maxSpeed);
            }
            //zufällig nicht immer die Regeln beachten
            if(Random.Range(0,100) < 20)
            {
                ApplyRules();
            }
            */
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

                //nur für Fische, die sich in der Nachbarschaft befinden
                if(nDistance <= swarmManager.neighbourDistance)
                {
                    //eine Untergruppe, dessen Positionen werden berücksichtigt 
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
