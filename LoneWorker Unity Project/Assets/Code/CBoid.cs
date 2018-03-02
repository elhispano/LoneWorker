using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CBoid : MonoBehaviour
{
	public CBoid[] visibleFriends = new CBoid[15];

	public bool reactToEnemies;

	public float maxAcceleration;

	public float maxSpeed;

	public float desiredSpeed;

	public float maxUrgency;

	public float minUrgency;

	public float minDistanceToEnemies;

	public float desiredSeparationForFriends;

	public int maxVisibleFriends;

	public bool infinitePerception;

	public float perceptionRadius;

    public Vector3 Velocity { get; private set; }

    public float OverallSpeed { get; private set; }

    public CruisingModule cruisingModule;

	private Transform myTransform;

	private CFlock parentFlock;

	private bool initialized;

	private int visibleFriendsIndex = 0;

	private CBoid next;

	private CBoid previous;

	private ushort numOfFlockmatesSeen;

	private ushort numOfEnemiesSeen;

	private CBoid nearestFlockmate;

	private CBoid nearestEnemy;

	private float distanceToNearestFlockmate;

	private float distanceToNearestEnemy;

	private Vector3 oldPosition;

	private Vector3 oldVelocity;

	void Awake ()
	{
		myTransform = transform;

        visibleFriends = new CBoid[maxVisibleFriends];
	}

	// Update is called once per frame
	public void FlockIt ()
	{
		if (initialized)
        {
			// Step 1: Update position based on the velocity computed last time around
			Vector3 acceleration = Vector3.zero;

			oldPosition = myTransform.position;

            myTransform.position += Velocity * Time.deltaTime;

			// Step2 - Search for nearest flockmates
			SeeFriends (parentFlock.GetFirstMember ());

			// Step3 - Flocking behavior. We apply the first three rules
			// they don't matter if we can't see anybody
            if (numOfFlockmatesSeen > 0)
            {
				// Step 4:SEPARATION
				// Try to maintain the desired separation distance from our nearest flockmate
				AccumulateChanges (ref acceleration, KeepDistance ());

				// Step 5: ALIGNMENT
				// Try to move the same way our neares flockmates does
				AccumulateChanges (ref acceleration, MatchHeading ());

				// Step 6: COhesion
				// Try to move towars the center of the flock
				AccumulateChanges (ref acceleration, SteerToCenter ());
			}

			// Step 6: ENEMIES
			if (reactToEnemies) {
				SeeEnemies (parentFlock);
				AccumulateChanges (ref acceleration, FleeEnemies ());
			}

			// Step 7: CRUISING
            AccumulateChanges (ref acceleration,cruisingModule.Cruising (this));

			// Step 8: CONSTRAINTS
			acceleration = Vector3.ClampMagnitude (acceleration, maxAcceleration);

			// Step 9: IMPLEMENTATION
			// Apply the new acceleration so we can use in the next update cycle
			oldVelocity = Velocity;

			Velocity += acceleration; // Apply the acceleration

			// Step 10: constraint Y velocity changes  to get a more realistic flight
			//velocity.y *= maxUrgency;

			// Stepn 11: Constraint velocity
			Velocity = Vector3.ClampMagnitude (Velocity, maxSpeed);

            Debug.DrawRay(transform.position, Velocity, Color.green);
            Debug.DrawRay(transform.position, acceleration, Color.yellow);

			OverallSpeed = Velocity.magnitude;

			// Step 12: Compute roll/pitch/yaw
			ComputeRPY ();

			// STep13: Keep boid inside boundaries
			WorldBounds ();
		}
	}

	#region MISC METHODS

	/// <summary>
	/// Initialize a Boid with random rotation, position and velocity.
	/// </summary>
	public void Initialize (CFlock _sourceFlock)
	{
//			Vector3 randomPosition = _sourceFlock.transform.position + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range (0f, 3f);
//			Vector3	randomRotation	= new Vector3 (UnityEngine.Random.Range (0f, 360f), UnityEngine.Random.Range (0f, 360f), UnityEngine.Random.Range (0f, 360f));
//			Vector3 randomVelocity	= UnityEngine.Random.insideUnitSphere;
//	
//			Initialize (randomPosition, randomRotation, randomVelocity, _sourceFlock);

        Initialize (_sourceFlock.transform.position, Vector3.zero, Vector3.zero, _sourceFlock);
	}

	/// <summary>
	/// Basic boid initialization
	/// </summary>
	void Initialize (Vector3 _position, Vector3 _rotation, Vector3 _velocity, CFlock _sourceFlock)
	{
		parentFlock = _sourceFlock;

		myTransform.position = _position;

		myTransform.eulerAngles = _rotation;

		Velocity = _velocity;
		OverallSpeed = Velocity.magnitude;

		numOfFlockmatesSeen = 0;
		nearestFlockmate = null;
		distanceToNearestFlockmate = Mathf.Infinity;

		numOfEnemiesSeen = 0;
		nearestEnemy = null;
		distanceToNearestEnemy = Mathf.Infinity;

		next = previous = null;

		initialized = true;
	}

	/// <summary>
	/// Adds a new visible friendly boid. Take note that visible friends are recalculated each frame.
	/// </summary>
	public void AddToVisibilityList (CBoid _boid)
	{
		if (numOfFlockmatesSeen < maxVisibleFriends)
        {
			visibleFriends [numOfFlockmatesSeen] = _boid;
			numOfFlockmatesSeen++;
		}
	}

	/// <summary>
	/// Clears the visiblelist.
	/// </summary>
	public void ClearVisiblelist ()
	{
		for (int i = 0; i < numOfFlockmatesSeen; i++) {
				visibleFriends [i] = null;	
		}

		numOfFlockmatesSeen = 0;
		nearestFlockmate = null;
		distanceToNearestEnemy = float.MaxValue;
        distanceToNearestFlockmate = float.MaxValue;
	}


	public CBoid GetNext ()
	{
		return next;	
	}

	public CBoid GetPrevious ()
	{
		return previous;	
	}

	public void LinkOut ()
	{
		next = null;
		previous = null;
	}

	public void SetNext (CBoid _next)
	{
		next = _next;
	}

	public void SetPrevious (CBoid _previous)
	{
		previous = _previous;
	}

	#endregion

	#region FLOCKING METHODS

	/// <summary>
	/// Generates a vector for a flock boid to avoid the nearest enemy it sees.
	/// </summary>
	private Vector3 FleeEnemies ()
	{
		Vector3 change = Vector3.zero;

		if (distanceToNearestEnemy < minDistanceToEnemies) {
				// Vector in the oposite direction to the enemy
				change = myTransform.position - nearestEnemy.transform.position;	
		}

		return change;
	}

	/// <summary>
	/// Keeps the distance from the nearest friend.
	/// </summary>
	private Vector3 KeepDistance ()
	{
		float ratio = distanceToNearestFlockmate / desiredSeparationForFriends;

        if (nearestFlockmate == null)
        {
            Debug.LogWarning("NEarest flockmate can't be null: "+name);
            return Vector3.zero;
        }

		// Vector towards our nearest friend
        Vector3 change = nearestFlockmate.transform.position - myTransform.position;

		// Clamp ratio
		ratio = Mathf.Clamp (ratio, minUrgency, maxUrgency);

		if (distanceToNearestFlockmate < desiredSeparationForFriends)
        {
			// We are too close to our flockmate ! Move away from it

			change = change.normalized * -ratio;
		} 
        else if (distanceToNearestFlockmate > desiredSeparationForFriends) 
        {
			change = change.normalized * ratio;	
		}
        else
        {
			// Rare case but if we are here, do nothing
			change = Vector3.zero;
		}
	
		return change;
	}

	/// <summary>
	/// Try to match the heading of its nearest flockmate
	/// </summary>
	private Vector3 MatchHeading ()
	{
        if (nearestFlockmate == null)
        {
            return Vector3.zero;
        }

		Vector3 change = nearestFlockmate.Velocity;

		// Limit change so we can't instantly snap to a new heading
		change = change.normalized * minUrgency;

		return change;
	}

	/// <summary>
	/// Guide a boid towards the center of the mass of the flockmates he can see
	/// </summary>
	private Vector3	SteerToCenter ()
	{
		Vector3 change = Vector3.zero;
		Vector3 center = Vector3.zero;

		for (int i = 0; i < numOfFlockmatesSeen; i++) 
        {
			if (visibleFriends [i] != null)
		        center += visibleFriends [i].transform.position;	
		}

		// Average position to get the center of the flock
		center /= numOfFlockmatesSeen;

		// Go towards the enter
		change = center - myTransform.position;

		// Clamp to avoid sharp direction changes
		change = center.normalized * minUrgency;
	
		return change;
	}

	#endregion

	#region CONTEXT METHODS

	/// <summary>
	/// Search for friends 
	/// </summary>
	private void SeeFriends (CBoid _firstMember)
	{
		float distance;

		ClearVisiblelist ();

        for (int i = 0; i < parentFlock.GetCount(); i++)
        {
            CBoid flockmate = parentFlock.GetBoids()[i];

            // Test: Within sight of this boid?

            if ((distance = CanISee (flockmate)) != float.MaxValue)
            {
                // Add to the list  

                AddToVisibilityList (flockmate);

                if (distance < distanceToNearestFlockmate)
                {
                    distanceToNearestFlockmate = distance;
                    nearestFlockmate = flockmate;
                }
            }

            if (nearestFlockmate == null && numOfFlockmatesSeen > 0)
            {
                Debug.LogError("RARE CASE");
            }
        }
	}

	/// <summary>
	/// See enemies !
	/// </summary>
	private int SeeEnemies (CFlock _flock)
	{
			float distance;
	
			CBoid enemy;
	
			numOfEnemiesSeen = 0;
			nearestEnemy = null;
			distanceToNearestEnemy = float.MaxValue;
	
			for (int i = 0; i < CFlock.flockCount; i++) {
					if (CFlock.listOfFlocks [i] == parentFlock)
							continue;
		
					enemy = CFlock.listOfFlocks [i].GetFirstMember ();
		
					while (enemy != null) {
							if ((distance = CanISee (enemy)) != float.MaxValue) {
									numOfEnemiesSeen++;
				
									// Test: Closets enemy ?
									if (distance < minDistanceToEnemies) {
											distanceToNearestEnemy = distance;
											nearestEnemy = enemy;
									}
							}
			
							enemy = enemy.GetNext ();
					}
			}
	
			return numOfEnemiesSeen;
	}

	#endregion

	#region MISCS

	/// <summary>
	/// Accumulates the changes into the accumulator vector.
	/// </summary>
	public float AccumulateChanges (ref Vector3 _accumulator, Vector3 _changes)
	{
		_accumulator += _changes;

		return _accumulator.magnitude;
	}

	/// <summary>
	/// Determines whether a given invoking boid can see the boid in question.
	/// </summary>
	private float CanISee (CBoid cboid)
	{
		if (cboid == this)
			return float.MaxValue;

		float distance = Vector3.Distance (myTransform.position, cboid.transform.position);

		if (infinitePerception)
			return distance;

        if (distance < perceptionRadius)
			return distance;

		return float.MaxValue;
	}

	/// <summary>
	/// Computes the roll/pitch/yaw based on its latest velocity vector changes.
	/// Roll/Pitch/Yaw is stored in X/Y/Z axis of a Vector3.
	/// 
	/// All calculations assume a right-handed coordinate system.
	/// +x = left side of the object
	/// +y = up
	/// +z = through the nose of the model
	/// 
	/// All angles are computed in radians
	/// 
	/// </summary>
	private void ComputeRPY ()
    {
		float roll, pitch, yaw;

		// Determine the direction of the lateral acceleration
		Vector3 lateralDir = Vector3.Cross (Vector3.Cross (Velocity, (Velocity - oldVelocity)), Velocity); 

		lateralDir.Normalize ();

		// Set lateral acceleration's magnitude. The magintude is the vector projection of the appliedAcceleration vector onto the direction of the
		// lateral acceleration

		float lateralMag = Vector3.Dot((Velocity - oldVelocity),lateralDir);

		if (lateralMag == 0) {
				roll = 0f;
		} else {
				roll = -Mathf.Atan2 (9.8f, lateralMag) + (Mathf.PI / 2f);
		}

		// Pitch!
		pitch = -Mathf.Atan(Velocity.y / Mathf.Sqrt((Velocity.z*Velocity.z) + (Velocity.x*Velocity.x)));

		//Yaw
		yaw = Mathf.Atan2(Velocity.x,Velocity.z);

    //pitch = 0f;
   // roll = 0f;

        transform.rotation = Quaternion.Euler(new Vector3 (Mathf.Rad2Deg*pitch,Mathf.Rad2Deg*yaw,Mathf.Rad2Deg*roll));
	}

	private void WorldBounds()
	{
		float maxX = parentFlock.worldBounds.GetBoxWidth ()/2f;
		float maxY = parentFlock.worldBounds.GetBoxHeight ()/2f;
		float maxZ = parentFlock.worldBounds.GetBoxLength ()/2f;

		float minX = -maxX;
		float minY = -maxY;
		float minZ = -maxZ;

        float clampedX = transform.position.x;
        float clampedY = transform.position.y;
        float clampedZ = transform.position.z;

        if (transform.position.x > maxX)
        {
            clampedX = minX;
        }
        else if (transform.position.x < minX)
        {
            clampedX = maxX;
        }

        if (transform.position.y > maxY)
        {
            clampedY = minY;
        }
        else if (transform.position.y < minY)
        {
            clampedY = maxY;
        }

        if (transform.position.z > maxZ)
        {
            clampedZ = minZ;
        }
        else if (transform.position.z < minZ)
        {
            clampedZ = maxZ;
        }

		transform.position = new Vector3 (clampedX,clampedY,clampedZ);
	}

		#endregion
}
