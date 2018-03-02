using UnityEngine;
using System.Collections;
using System;

public class GroupMember : MonoBehaviour
{
    public event Action<GroupMember> OnKilled;

    public int publicIndex;

    public float maxAcceleration = 1f;

    public float maxSpeed = 5f;

    public float desiredSpeed = 2.5f;

    public float minUrgency, maxUrgency;


    private Vector3 targetPosition;

    private Vector3 initialPosition;

    public float currentPathProgress = 0f;

    public float slowDistance = 1f;

    private float overallSpeed;

    private Vector3 velocity;

    private Vector3 oldVelocity;

    public int Index { get; private set; }

    void OnDisable()
    {
        if (OnKilled != null)
            OnKilled(this);
    }

    public void Initialize(int _index)
    {
        Index               = _index;
        publicIndex = Index;
        initialPosition     = transform.position;
    }

    public void Move(Vector3 _newPosition,Vector3 _direction)
    {
//        transform.rotation = Quaternion.LookRotation(_direction);
//        transform.position = _newPosition;

        Vector3 accelerationVector      = _newPosition - transform.position;
        float distance                  = accelerationVector.magnitude;
        float brakingProgress           = distance / slowDistance;

        float acceleration              = (overallSpeed - desiredSpeed) / maxSpeed;
        acceleration                    = Mathf.Abs(acceleration); // The sign is determined by the direction of accelerationVector
        acceleration                    = Mathf.Clamp(acceleration,minUrgency,maxUrgency);
        acceleration                    = Mathf.Clamp(acceleration, 0, maxAcceleration);

        accelerationVector              = Vector3.ClampMagnitude(accelerationVector, acceleration);

        oldVelocity = velocity;
        velocity += accelerationVector;
        velocity = Vector3.ClampMagnitude(velocity*brakingProgress,maxSpeed);
        overallSpeed = velocity.magnitude;

        transform.position += velocity * Time.deltaTime;

        ComputeRPY();
    }

    private void ComputeRPY ()
    {
        float roll, pitch, yaw;

        // Determine the direction of the lateral acceleration
        Vector3 lateralDir = Vector3.Cross (Vector3.Cross (velocity, (velocity - oldVelocity)), velocity); 

        lateralDir.Normalize ();

        // Set lateral acceleration's magnitude. The magintude is the vector projection of the appliedAcceleration vector onto the direction of the
        // lateral acceleration

        float lateralMag = Vector3.Dot((velocity - oldVelocity),lateralDir);

        if (lateralMag == 0) {
            roll = 0f;
        } else {
            roll = -Mathf.Atan2 (9.8f, lateralMag) + (Mathf.PI / 2f);
        }

        // Pitch!
        pitch = -Mathf.Atan(velocity.y / Mathf.Sqrt((velocity.z*velocity.z) + (velocity.x*velocity.x)));

        //Yaw
        yaw = Mathf.Atan2(velocity.x,velocity.z);

        //pitch = 0f;
        // roll = 0f;

        transform.rotation = Quaternion.Euler(new Vector3 (Mathf.Rad2Deg*pitch,Mathf.Rad2Deg*yaw,Mathf.Rad2Deg*roll));
    }

    public bool IsNewPath(AbstractPath _path)
    {
        // TODO: Return true if the path is new
        return false;
    }

    public void ResetProgress()
    {
        currentPathProgress = 0f;
    }

    public void IncreasePathProgress(AbstractPath _path)
    {
        currentPathProgress += Time.deltaTime / _path.pathDuration;
    }

    public float GetCurrentPathProgress()
    {
        return currentPathProgress;
    }

    public void SetProgress(float _newProgress)
    {
        currentPathProgress = _newProgress;
    }

    public void ModifyIndex(int _offset)
    {
        Index       += _offset;
        publicIndex = Index;

        if (Index < 0)
            Debug.LogError("Negative Index");
    }
}
