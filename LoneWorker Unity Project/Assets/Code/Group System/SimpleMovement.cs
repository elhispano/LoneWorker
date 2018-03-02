using UnityEngine;
using System.Collections;

public class SimpleMovement : GroupMovement 
{
    public float rotationSpeed = 0f;

    public override void Move()
    {
        transform.Translate(Time.deltaTime * Vector3.forward * speed);

        transform.Rotate(Time.deltaTime * Vector3.forward * rotationSpeed);
    }
}
