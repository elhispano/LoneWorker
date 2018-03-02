using UnityEngine;
using System.Collections;

public class LinePath : AbstractPath
{
    public Transform point1,point2;

    public override Vector3 GetPoint(float _progress)
    {
        return Vector3.Lerp(point1.position, point2.position,_progress);
    }

    public override Vector3 GetDirection(float _progress)
    {
        return (point2.position - point1.position).normalized;
    }

    void OnDrawGizmos()
    {
        if (point1 == null || point2 == null)
            return;

        Gizmos.DrawLine(point1.position, point2.position);
    }
}
