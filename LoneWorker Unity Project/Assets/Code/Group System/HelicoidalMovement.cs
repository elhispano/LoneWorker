using UnityEngine;
using System.Collections;

public class HelicoidalMovement : GroupFormation
{
    public float separation = 1f;

    public float radius = 1f;

    public float theta = 0.1f;

    public float angleOffset;

    public override Vector3 GetMemberPosition(int _index,  Vector3 _forward,Vector3 _right, Vector3 _up)
    {
        float x = radius * Mathf.Cos(_index + theta + (angleOffset * Mathf.Deg2Rad));
        float y = radius * Mathf.Sin(_index + theta + (angleOffset * Mathf.Deg2Rad));
        float z = -separation * _index;

        return _up * y + _right * x;// + _forward * z;
    }
}
