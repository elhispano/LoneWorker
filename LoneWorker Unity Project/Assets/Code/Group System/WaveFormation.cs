using UnityEngine;
using System.Collections;

public class WaveFormation : GroupFormation 
{
    public float amplitude = 2f;

    public float frequency = 2f;

    public float separation = 1f;

    [Range(0,360)]
    public int outOfPhaseDegrees = 0;

    public override Vector3 GetMemberPosition(int _index, Vector3 _forward,Vector3 _right, Vector3 _up)
    {
        // Note: Maybe we want to cache som calculous, the thing is we can't lose flexibility if we change thins in realtime
        float yOffset = amplitude * Mathf.Sin((2 * Mathf.PI * frequency * Time.time) + (_index * outOfPhaseDegrees * Mathf.Deg2Rad));

        return (_up * yOffset) + (_forward * -separation * _index);
    }	
}
