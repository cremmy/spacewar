using UnityEngine;
using System.Collections;

public class SpaceObject : MonoBehaviour 
	{
	public Universe universe;
	public Vector3 spd;
	
	// Use this for initialization
	void Start () 
		{		
		//
		}
	
	// Update is called once per frame
	void Update() 
		{
		Vector3 f=universe.GetForceAt(transform.position);
		ApplyForce(f);
		
		transform.position+=spd*Time.deltaTime;
		}

	public void ApplyForce(Vector3 f)
		{
		spd+=f*Time.deltaTime;
		}
	}
