using UnityEngine;
using System.Collections;

public abstract class GroupMovement : MonoBehaviour 
{
    public float speed = 5f;

    public abstract void Move();

    void Update()
    {
        Move();
    }
}
