using UnityEngine;
using System.Collections;

// Tak, dobrze widzisz. Tylko tyle.

public class ObjectWithRadius : MonoBehaviour 
	{
	public float radius;
	
	void Start() 
		{
		//
		}
	
	void Update () 
		{
		//
		}
		
	void OnDrawGizmos()
		{
		if(radius>1)
			Gizmos.color=Color.yellow;
		else
			Gizmos.color=Color.red;
		Gizmos.DrawWireSphere(transform.position, radius);
		}
	}
