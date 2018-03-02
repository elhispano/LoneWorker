using UnityEngine;
using System.Collections;

public class FollowPointsMovement : GroupMovement {

    public Transform[] points;

    int currentIndex = 0;
 

    public override void Move()
    {
        Transform nextPoint           = points [currentIndex];
        Vector3 movementDirection     = (nextPoint.position - transform.position);

        transform.Translate(movementDirection.normalized * Time.deltaTime * speed,Space.World);
        transform.LookAt(nextPoint);

        float dot = Vector3.Dot(transform.forward, movementDirection);

        if (dot < 0f)
        {
            currentIndex++;

            if (currentIndex >= points.Length)
                currentIndex = 0;
        }
    }

    void OnDrawGizmos()
    {
        if (points== null || points.Length == 0)
            return;
        
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 point1 = points [i].position;
            Vector3 point2 = i == points.Length - 1 ? points [0].position : points [i + 1].position;

            Gizmos.DrawLine(point1, point2);
        }
    }
}
