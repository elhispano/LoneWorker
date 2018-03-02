using UnityEngine;
using System.Collections;

public abstract class GroupFormation : MonoBehaviour
{
    public abstract Vector3 GetMemberPosition(int _index, Vector3 _forward,Vector3 _right, Vector3 _up);
}
