using UnityEngine;
using System.Collections;

public class HerdFactory : MonoBehaviour
{
    public CBoid boidTemplate;
    public int flockSize = 50;
    public CBox worldBounds;
    public float emitDelay = 0f;

    void Awake()
    {
        GameObject flockGO      = new GameObject("Flock");
        var newFlock            = flockGO.AddComponent<CFlock>();
        newFlock.worldBounds    = worldBounds;

        if (emitDelay != 0f)
        {
            StartCoroutine(EmitWithDelay(newFlock, emitDelay));
        }
        else
        {
            for (int i = 0; i < flockSize; i++)
            {
                CBoid newBoid   = Instantiate<CBoid>(boidTemplate);
                newBoid.name    = string.Format("Boid {0}",i);
                newFlock.AddTo(newBoid);
            }
        }
    }

    IEnumerator EmitWithDelay(CFlock _flock,float _delay)
    {
        for (int i = 0; i < flockSize; i++)
        {
            CBoid newBoid   = Instantiate<CBoid>(boidTemplate);
            newBoid.name    = string.Format("Boid {0}",i);
            _flock.AddTo(newBoid);

            yield return new WaitForSeconds(emitDelay);
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
