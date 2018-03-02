using UnityEngine;
using System.Collections;

public class CBox : MonoBehaviour
{
	public Bounds boxBound;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(boxBound.center,boxBound.extents);	
	}
	
	#region GENERAL METHODS
	
	public float GetBoxLength()
	{
		return boxBound.extents.z;
	}
	
	public float GetBoxWidth()
	{
		return boxBound.extents.x;
	}
	
	public float GetBoxHeight()
	{
		return boxBound.extents.y;	
	}
	
	#endregion
}
