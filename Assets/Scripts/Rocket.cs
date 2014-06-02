using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour 
	{
	private const float THRUST=					1.2f;
	private const float MAX_SPEED=				6.0f;
	private const float FUEL_USAGE=				1.0f/40.0f;	// 40 sekund napedzania, 60 sekund TTL
	
	private SpaceObject so;
	private Projectile proj;
	private float fuel;
	
	private Spaceship target;

	// Use this for initialization
	void Start () 
		{
		so=GetComponent<SpaceObject>();
		proj=GetComponent<Projectile>();
		fuel=1.0f;
		
		target=null;
		
		var ships=GameObject.FindObjectsOfType<Spaceship>();
		
		foreach(var s in ships)
			{
			if(proj.parent.playerID==s.playerID)
				continue;
				
			target=s;
			break;
			}
		}
	
	// Update is called once per frame
	void Update () 
		{
		// Nakieruj na przeciwnika...
		if(target)
			{
			// TODO
			}
		
		// I wlacz silniki!
		AddThrust();
		}
		
	void AddThrust()
		{
		if(Vector3.Dot(so.spd, transform.forward)>=MAX_SPEED)
			return;
		
		fuel-=FUEL_USAGE*Time.deltaTime;
		so.spd+=transform.forward*Time.deltaTime*THRUST;
		}
	}

