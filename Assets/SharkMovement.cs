using UnityEngine;

public class SharkMovement : MonoBehaviour
{
    // Start is called before the first frame update
    private Vector3 dir;

    public float speed;

    private void Start()
    {
        dir = Vector3.right;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.Translate(dir * speed * Time.deltaTime);

        if (transform.position.x <= -4)
        {
            dir = Vector3.right;
        }
        else if (transform.position.x >= 4)
        {
            dir = Vector3.left;
        }
    }
}