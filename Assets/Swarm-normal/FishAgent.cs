using UnityEngine;

public class FishAgent : MonoBehaviour
{
    public SwarmManager swarmManager;
    private float speed;
    private float obstacleDetectionValue = 3f;
    private Vector3 direction;
    private float neighDistance;

    // Start is called before the first frame update
    private void Start()
    {
        speed = Random.Range(swarmManager.minSpeed, swarmManager.maxSpeed);
        direction = swarmManager.goalPos - this.transform.position;
        neighDistance = swarmManager.neighbourDistance;
    }

    // Update is called once per frame
    private void Update()
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
        RejectionDetectionZ();
        RejectionDetectionX();

        ApplyRules();


        if (inObstacle(transform.position))
        {
            float directionValue;
            if (transform.rotation.z > 0)
            {
                directionValue = 1f;
            }
            else
                directionValue = -1f;

            Vector3 newPosition = Vector3.Slerp(transform.position, new Vector3(transform.position.x, transform.position.y, (transform.position.z + obstacleDetectionValue) * directionValue), Time.deltaTime * speed);
            if (!inObstacle(newPosition))
                transform.position = newPosition;
            else
            {
                transform.Translate(Time.deltaTime * directionValue * speed, 0f, 0f);

                /*
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(swarmManager.goalPos), swarmManager.rotationSpeed * Time.deltaTime);
                Vector3 positionToGoal = Vector3.Slerp(transform.position, new Vector3(transform.position.x, transform.position.y, (transform.position.z + 2f)), Time.deltaTime * swarmManager.maxSpeed);

                if (!inObstacle(positionToGoal))
                    transform.position = positionToGoal;*/
            }
            //transform.Translate(0, 0, directionValue * Time.deltaTime * speed);
        }

        //transform.Translate(0, 0, -1 * Time.deltaTime * speed);
        else
        {
            transform.Translate(0, 0, Time.deltaTime * speed);
        }
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

        foreach (GameObject go in gos)
        {
            if (go != this.gameObject)
            {
                nDistance = Vector3.Distance(go.transform.position, this.transform.position);

                //nur f?r Fische, die sich in der Nachbarschaft befinden
                if (nDistance <= neighDistance)
                {
                    //eine Untergruppe, dessen Positionen werden ber?cksichtigt
                    vcentre += go.transform.position;
                    groupSize++;

                    if (nDistance < swarmManager.AvoidValue)
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
            vcentre = vcentre / groupSize + direction;
            speed = gSpeed / groupSize;
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
        for (int i = 0; i < swarmManager.boundsObstacle.Length; i++)
        {
            if (swarmManager.boundsObstacle[i].Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    /**
     * die Fische sollen von der mittes des HindernisObjektes nach außen wegbewegt werden
     * zu chaotisch, viele kommen durch und das Ziel wird etwas vernachlässigt 
     */
    private void Rejection()
    {
        direction = swarmManager.goalPos - this.transform.position;
        Ray rayForward = new Ray(transform.position, Vector3.right);
        float distanceToObstacle = Mathf.Infinity;

        for (int i = 0; i < swarmManager.boundsRejection.Length; i++)
        {
            if (swarmManager.boundsRejection[i].IntersectRay(rayForward, out distanceToObstacle))
            {
                if (distanceToObstacle < 3f)
                {
                    Vector3 directionNew = transform.position - swarmManager.boundsObstacle[i].center;

                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionNew), Time.deltaTime);
                    transform.Translate(0, 0, Time.deltaTime);
                    Debug.LogError("hai");
                }
                else
                    ApplyRules();
            }
            else
            {
                ApplyRules();
            }
        }
    }

    /**
     * funktioniert sehr schlecht
     * die fische sollen sich vom Hindernisobjekt wegbewegen und ihre Nachbarn vernachlässigen, damit sie
     * den Weg zum Ziel noch finden
     **/
    private void Rejection1()
    {
        Ray rayForward = new Ray(transform.position, Vector3.right);
        float distanceToObstacle = Mathf.Infinity;

        for (int i = 0; i < swarmManager.boundsRejection.Length; i++)
        {
            if (swarmManager.boundsRejection[i].IntersectRay(rayForward, out distanceToObstacle))
            {
                if (distanceToObstacle < 6f && distanceToObstacle >= 0)
                {
                    Debug.LogError("hai");
                    direction = (transform.position - swarmManager.boundsObstacle[i].center);
                    neighDistance = 0.1f;
                }
                else
                {
                    direction = swarmManager.goalPos - this.transform.position;
                    neighDistance = swarmManager.neighbourDistance;
                }
            }
            else
            {
                direction = swarmManager.goalPos - this.transform.position;
                neighDistance = swarmManager.neighbourDistance;
            }
        }
    }

    //speedEinstellung? 
    private void RejectionDetectionZ()
    {
        direction = swarmManager.goalPos - this.transform.position;
        Ray rayForward = new Ray(transform.position, Vector3.forward);
        float distanceToObstacle = Mathf.Infinity;

        for (int i = 0; i < swarmManager.rejections.Capacity; i++)
        {
            if (swarmManager.rejections[i].rejectionObjectCollider.bounds.IntersectRay(rayForward, out distanceToObstacle))
            {
                float speed = swarmManager.rejections[i].speed;
                RejectionMoveRightLeft(i, distanceToObstacle, speed);
                RejectionMoveUpDown(i, distanceToObstacle, speed);

                if (swarmManager.rejections[i].UpDown == true)
                {
                   // RejectionMoveUpDown(i, distanceToObstacle, speed);
                }
                else
                {
                   // RejectionMoveRightLeft(i, distanceToObstacle, speed);
                }
            }
        }
    }

    private void RejectionMoveUpDown(int indexI, float distanceToObstacle, float rotationSpeed)
    {
        if (distanceToObstacle < swarmManager.rejections[indexI].dectectionRayValue && distanceToObstacle >= 0)
        {
            if (transform.position.y > swarmManager.boundsRejection[indexI].center.y)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.up), rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.down), rotationSpeed * Time.deltaTime);
            }
        }
        else if (distanceToObstacle > -swarmManager.rejections[indexI].dectectionRayValue && distanceToObstacle <= 0)
        {
            if (transform.position.y > swarmManager.boundsRejection[indexI].center.y)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.up), rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.down), rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void RejectionMoveRightLeft(int indexI, float distanceToObstacle, float rotationSpeed)
    {
        if (distanceToObstacle < swarmManager.rejections[indexI].dectectionRayValue && distanceToObstacle >= 0)
        {
            if (transform.position.x > swarmManager.boundsRejection[indexI].center.x)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), rotationSpeed * Time.deltaTime);
            }
        }
        else if (distanceToObstacle > -swarmManager.rejections[indexI].dectectionRayValue && distanceToObstacle <= 0)
        {
            if (transform.position.x > swarmManager.boundsRejection[indexI].center.x)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void RejectionDetectionX()
    {
        direction = swarmManager.goalPos - this.transform.position;
        Ray rayForward = new Ray(transform.position, Vector3.right);
        float distanceToObstacle = Mathf.Infinity;

        for (int i = 0; i < swarmManager.rejections.Capacity; i++)
        {
            if (swarmManager.rejections[i].rejectionObjectCollider.bounds.IntersectRay(rayForward, out distanceToObstacle))
            {
                float speed = swarmManager.rejections[i].speed;
                RejectionMoveRightLeft(i, distanceToObstacle, speed);
                RejectionMoveUpDown(i, distanceToObstacle, speed);
                if (swarmManager.rejections[i].UpDown == true)
                {
                    //RejectionMoveUpDown(i, distanceToObstacle, speed);
                }
                else
                {
                  // RejectionMoveRightLeft(i, distanceToObstacle, speed);
                }
            }
        }
    }

}

