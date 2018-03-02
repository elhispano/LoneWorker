using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CFlock : MonoBehaviour
{
    /// <summary>
    /// Number of total flocks
    /// </summary>
    public static int flockCount;
	
    /// <summary>
    /// Reference to all flocks
    /// </summary>
    public static CFlock[] listOfFlocks;

    public CBox worldBounds;
	
    private List<CBoid> boids = new List<CBoid>();
	
    private int numberOfMembers;
	
    private CBoid firstMember;
	
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < numberOfMembers; i++)
        {
            boids [i].FlockIt();
        }
    }

    public void AddTo(CBoid _boid)
    {
        if (firstMember == null)
        {
            firstMember = _boid;
            firstMember.SetNext(null);
            firstMember.SetPrevious(null);

        }
        else
        {
            _boid.SetPrevious(boids [boids.Count-1]);
            boids [boids.Count-1].SetNext(_boid);
        }

        _boid.Initialize(this);

        boids.Add(_boid);

        numberOfMembers++;
    }

    public int GetCount()
    {
        return numberOfMembers;
    }

    public CBoid GetFirstMember()
    {
        return boids [0];	
    }

    public List<CBoid> GetBoids()
    {
        return boids;
    }

    public void RemoveBoid(CBoid _boid)
    {
        // TODO: Hay que cambiar las referencias del anterior y el siguiente
        boids.Remove(_boid);
        numberOfMembers--;
        _boid.LinkOut();
    }
}
