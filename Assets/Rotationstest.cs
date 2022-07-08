using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotationstest : MonoBehaviour
{
    public GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, 0,-1* Time.deltaTime);

        Vector3 toTarget = target.transform.position - transform.position;
        Vector3 fromTarget = transform.position - target.transform.position;
        //transform.rotation = Quaternion.LookRotation(toTarget);
        //if (transform.position == target.transform.position)
           // transform.rotation = Quaternion.LookRotation(fromTarget);
    }
}
