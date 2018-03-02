using UnityEngine;
using System.Collections;

public class LineFormation : GroupFormation 
{
    public float separation = 0f;

    public override Vector3 GetMemberPosition(int _index,  Vector3 _forward,Vector3 _right, Vector3 _up)
    {
        Vector3 localTraslation = Vector3.zero;

        if (_index % 2 == 0)
        {
            localTraslation = (_right * -separation * _index); 
        }
        else
        {
            localTraslation = (_right* separation * _index); 
        }

        return localTraslation;
    }
}
