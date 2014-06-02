using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
	{
	public float ttl;	// time to live
	public float damage;
	public Spaceship parent;
	
	private ObjectWithRadius owr;
	
	// Use this for initialization
	void Start () 
		{
		owr=GetComponent<ObjectWithRadius>();
		
		if(damage<=0.0f)
			damage=0.1f;
		if(ttl<=0.0f)
			ttl=10.0f;
		}
	
	// Update is called once per frame
	void Update () 
		{
		ttl-=Time.deltaTime;
		
		if(ttl<=0.0f)
			{
			if(owr.radius>0.0f)
				owr.radius-=Time.deltaTime/2.0f;
			else
				Destroy(gameObject);
			}
		}
		
	/*virtual public void Hit()
		{
		Destroy(gameObject);
		}*/
	}
