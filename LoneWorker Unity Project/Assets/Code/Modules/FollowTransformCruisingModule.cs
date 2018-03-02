using UnityEngine;
using System.Collections;

public class FollowTransformCruisingModule : CruisingModule
{
    Transform followTransform;

    public float amplitude = 1f;
    public float frequency = 2f;

    void Awake()
    {
        followTransform = GameObject.FindWithTag("FollowTransform").transform;
    }

    public override Vector3 Cruising(CBoid _cBoid)
    {
        float diff      = (_cBoid.OverallSpeed - _cBoid.desiredSpeed) / _cBoid.maxSpeed;
        float urgency   = Mathf.Abs (diff);

        // Constraint urgency level
        urgency = Mathf.Clamp (urgency, _cBoid.minUrgency, _cBoid.maxUrgency);

        Vector3 change = followTransform.position - _cBoid.transform.position;


        //change = change.normalized * (urgency * (diff > 0f ? -1f : 1f));
        change = Vector3.ClampMagnitude(change,urgency);


        float offset = amplitude * Mathf.Sin(2 * Mathf.PI * frequency * Time.time);

        Vector3 sinChangeY = transform.up * offset;
        change +=  Vector3.ClampMagnitude(sinChangeY,urgency);

        return change;
    }
}
