using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishAgent : MonoBehaviour
{
    public SwarmManager swarmManager;
    private float speed;
    bool turning = false;
    //private Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        speed = Random.Range(swarmManager.minSpeed, swarmManager.maxSpeed);
        //direction = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //kann noch ge?ndert werden
        //Bounds bounds = new Bounds(swarmManager.transform.position, swarmManager.swimLimits);                                      
        /*
         * 
        if (!bounds.Contains(transform.position))
        {
            turning = true;
            direction = swarmManager.transform.position - transform.position;
            Debug.LogError("Bounds");
        }
        /*
        else if (Physics.Raycast(transform.position, this.transform.forward * 1, out hit))
        {
            turning = true;
            Debug.DrawRay(this.transform.position, this.transform.forward * 1, Color.red);
            direction = Vector3.Reflect(this.transform.forward, hit.normal);

        }*/
        /*
        for(int i=0;  i< swarmManager.bounds.Length; i++)
        {
            //Debug.Log(transform.position);
            //Debug.Log(boundsObstacles.center);
            //boundsObstacles.Expand(2f);
            //Debug.Log(swarmManager.bounds[i].center);

            if (swarmManager.bounds[i].Contains(transform.position))
            {
                turning = true;
                //direction = transform.position - boundsObstacles.center;
                //direction = transform.position - swarmManager.goalPos;
                break;
            }
            else
                turning = false;
        }
        /*
        if (turning)
        {
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(swarmManager.goalPos), swarmManager.rotationSpeed * Time.deltaTime);
            //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Inverse(transform.rotation) , speed * Time.deltaTime);
            //transform.rotation = Quaternion.Inverse(transform.rotation);

            //transform.Translate(0, 0, Time.deltaTime * speed);
            //transform.rotation = Quaternion.Euler(0f, 0f, -1f) * swarmManager.rotationSpeed*Time.deltaTime;
            //transform.rotation = Quaternion.EulerAngles(transform.rotation.x, transform.rotation.y, transform.rotation.z)
        }
        //else
        */
       ApplyRules();

       if (inObstacle(transform.position))
       {
            float directionValue = 0;
            if (transform.rotation.z > 0)
            {
                directionValue = 1f;
            }
            else
                directionValue = -1f;

            Vector3 newPosition = Vector3.Slerp(transform.position, new Vector3(transform.position.x, transform.position.y, (transform.position.z +(directionValue* 2f))), Time.deltaTime * speed);
            if(!inObstacle(newPosition))
                transform.position = newPosition;
       }

        //transform.Translate(0, 0, -1 * Time.deltaTime * speed);
       else
       {
        transform.Translate(0, 0, Time.deltaTime * speed);
       }
        //Vector3 moveVector = direction * Time.deltaTime * speed;
        //transform.Translate(0, 0,moveVector.z);
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
            Vector3 directionNew = Vector3.zero;

            directionNew = (vcentre + vavoid) - transform.position;
            //if (turning)
               // directionNew = direction;
            
            //schon Richtige Richtung 
            if (directionNew != Vector3.zero)
               transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionNew), swarmManager.rotationSpeed * Time.deltaTime);
        }
    }

    private bool inObstacle(Vector3 position)
    {
        for (int i = 0; i < swarmManager.bounds.Length; i++)
        {
            if (swarmManager.bounds[i].Contains(position))
            {
                return true;
            }
        }
        return false;
    }
}
