using UnityEngine;
using System.Collections;

public abstract class AbstractPath : MonoBehaviour
{
    public float pathDuration = 5f;

    public abstract Vector3 GetPoint(float _progress);

    public abstract Vector3 GetDirection(float _progress);
}
