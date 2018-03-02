using UnityEngine;
using System.Collections;

public class DefaultCruisingModule : CruisingModule
{
    public override Vector3 Cruising(CBoid _cBoid)
    {
        Vector3 change = _cBoid.Velocity;

        float diff = (_cBoid.OverallSpeed - _cBoid.desiredSpeed) / _cBoid.maxSpeed;
        float urgency = Mathf.Abs (diff);

        // Note: If diff > 0, boid want to speed up, in other case, wants to slow down.

        // Constraint urgency level
        urgency = Mathf.Clamp (urgency, _cBoid.minUrgency, _cBoid.maxUrgency);

        // Add some jitter to keep things interesting

        float jitter = UnityEngine.Random.Range (0f, 1f);

        float randomJitter = _cBoid.minUrgency * Mathf.Sign (diff);

        if (jitter < 0.45f) {
            change = new Vector3 (change.x + randomJitter, change.y, change.z); 
        } else if (jitter < 0.90f) {
            change = new Vector3 (change.x, change.y, change.z + randomJitter); 
        } else {
            change = new Vector3 (change.x, change.y + randomJitter, change.z); // We don't like vertical motion all mutch
        }

        change = change.normalized * (urgency * (diff > 0f ? -1f : 1f));

        return change;
    }
}
