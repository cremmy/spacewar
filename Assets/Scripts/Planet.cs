using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Planet : MonoBehaviour 
	{
	public float mass;
	//public float shadow;	// Dlugosc cienia
	
	private float scaletoradius;
	private ObjectWithRadius owr;
	
	public Planet parent;
	public List<Planet> children;
	
	// Use this for initialization
	void Start () 
		{
		float radius=(transform.localScale.x + transform.localScale.y + transform.localScale.z)/3.0f;
					  
		owr=GetComponent<ObjectWithRadius>();
		
		scaletoradius=radius/owr.radius;
		
		print("Planet aspect: "+scaletoradius);
		
		if(parent)
			parent.children.Add(this);
		}
	
	// Update is called once per frame
	void Update () 
		{
		transform.localScale=new Vector3(1, 1, 1)*owr.radius*scaletoradius;
		}
		
		
	public Vector3 GetForceAt(Vector3 pos)
		{
		const float G=6.67f;
		
		Vector3 dir=transform.position-pos;
		
		if(dir.sqrMagnitude<1.0f)						// Bo planety, zrodla grawitacji, tez sa space objectami
			return new Vector3(0.0f, 0.0f, 0.0f);		// A dzielenie przez zero nie jest fajne.
		
		return dir.normalized * G*mass/dir.sqrMagnitude;
		}
	}
