using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBlock : MonoBehaviour
{
    private float t;
    [SerializeField] private Vector3 pointA = new Vector3(-2, 0, 0);
    [SerializeField] private Vector3 pointB = new Vector3(2, 0, 0);
    [SerializeField] private float speed = 1;
    
    // Start is called before the first frame update
    void Start()
    {

    }
    
    // Update is called once per frame
    void Update()
    {
        t += Time.deltaTime * speed;

        // Moves the object to target position
        transform.position = Vector3.Lerp(pointA, pointB, t);

        // Flip the points once it has reached the target
        if (t >= 1)
        {
            var b = pointB;
            var a = pointA;
            pointA = b;
            pointB = a;
            t = 0;
        }
    }
}
