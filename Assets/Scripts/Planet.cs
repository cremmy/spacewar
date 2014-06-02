using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour 
	{
	public float mass;
	//public float shadow;	// Dlugosc cienia
	
	private float scaletoradius;
	
	ObjectWithRadius owr;
	
	// Use this for initialization
	void Start () 
		{
		float radius=(transform.localScale.x + transform.localScale.y + transform.localScale.z)/3.0f;
					  
		owr=GetComponent<ObjectWithRadius>();
		
		scaletoradius=radius/owr.radius;
		
		print("Planet aspect: "+scaletoradius);
		}
	
	// Update is called once per frame
	void Update () 
		{
		transform.localScale=new Vector3(1, 1, 1)*owr.radius*scaletoradius;
		}
		
	void OnCollisionEnter(Collision collision)
		{
		GameObject o=collision.gameObject;
		Spaceship ship=o.GetComponent<Spaceship>();
		//SpaceObject so=o.GetComponent<SpaceObject>();
		
		if(ship!=null)
			{
			ship.hp=0.0f;
			}
		else
			{
			Destroy(o);
			}
		}
	}
