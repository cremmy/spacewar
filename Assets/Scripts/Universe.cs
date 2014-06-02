using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Universe : MonoBehaviour 
	{
	// Normalnie jest *10^-11, ale nie ma sensu zostawiac tak malej liczby
	// Pamietajcie jednak, aby masy obiektow robic odpowiednio mniejsze		
	private const float G=6.67f;	
							
	
	private List<Planet> planets;
	private List<SpaceObject> objects;
	private List<Spaceship> ships;
	
	public int asteroids;
	public float fieldradius;
	public Vector3 fieldcenter;
	public GameObject asteroid;
	
	// Use this for initialization
	void Start ()
		{
		planets=new List<Planet>();
		objects=new List<SpaceObject>();
		ships=new List<Spaceship>();
		
		// Znajdz wszystkie planety aktualnie w grze
		var ps=GameObject.FindObjectsOfType<Planet>();
		
		print("Planets: "+ps.Length);
		
		foreach(var p in ps)
			{			
			planets.Add(p);
			}
			
		// Znajdz wszystkie obiekty aktualnie w grze
		var os=GameObject.FindObjectsOfType<SpaceObject>();
		
		print("Objects: "+os.Length);
		
		foreach(var o in os)
			{
			o.universe=this;
			
			objects.Add(o);
			}
			
		// Ustaw poczatkowa szybkosc graczy
		var ss=GameObject.FindObjectsOfType<Spaceship>();
		
		print("Players: "+ss.Length);
		
		foreach(var s in ss)
			{
			Planet closest=GetClosest(s.transform.position);
			
			Vector3 dir=s.transform.position-closest.transform.position;
			float dist=dir.magnitude;
			
			Vector3 f=dir.normalized * G*closest.mass/dir.sqrMagnitude;
			Matrix4x4 m=Matrix4x4.TRS(new Vector3(), Quaternion.Euler(0, 90, 0), new Vector3(1, 1, 1));
			
			float v=Mathf.Sqrt(f.magnitude*dist/closest.mass);
			
			SpaceObject so=s.GetComponent<SpaceObject>();
			
			so.spd+=m.MultiplyPoint3x4(dir.normalized)*v;
			so.transform.LookAt(closest.transform.position);
			
			ships.Add(s);
			}
			
		if(asteroids>0 && asteroid)
			{
			//print("Tworzenie asteroid...");
			
			CreateAsteridField(asteroids, fieldradius, asteroid);
			}
		}
	
	// Update is called once per frame
	void Update () 
		{
		planets.RemoveAll(item => item == null);
		objects.RemoveAll(item => item == null);
		ships.RemoveAll(item => item == null);
		
		CheckAlerts();
		CheckEnergy();
		CheckCollisions();
		}
		
	public Vector3 GetForceAt(Vector3 pos)
		{
		Vector3 ret=new Vector3(0, 0, 0);
		
		foreach(var p in planets)
			{
			if(!p)
				continue;
			
			Vector3 dir=p.transform.position-pos;
			
			if(dir.sqrMagnitude<1.0f)	// Bo planety, zrodla grawitacji, tez sa space objectami
				continue;				// A dzielenie przez zero nie jest fajne.
			
			ret+=dir.normalized * G*p.mass/dir.sqrMagnitude;	// sqrMag bo r^2
			}
			
		//print("count "+planets.Count+"; force "+ret);
		
		return ret;
		}
		
	public Planet GetClosest(Vector3 pos)
		{
		float min=Mathf.Infinity;
		Planet ret=null;
		
		foreach(var p in planets)
			{
			if(!p)
				continue;
			
			float dist=(p.transform.position-pos).magnitude;
			
			if(dist>min)
				continue;
				
			min=dist;
			ret=p;	
			}
			
		return ret;
		}
		
	public Vector3 GetCenterOfMass()
		{
		Vector3 ret=new Vector3();
		float mass=GetMass();
		
		foreach(var p in planets)
			ret+=p.mass/mass*p.transform.position;
		
		return ret;
		}
		
	public float GetMass()
		{
		float ret=0.0f;
		
		foreach(var p in planets)
			ret+=p.mass;
			
		return ret;
		}
		
	public void AddObject(SpaceObject o)
		{
		objects.Add(o);
		}
	
	private void CheckAlerts()
		{
		const float ALERT_MULTIPLIER_PLANET=2.0f;
		const float ALERT_MULTIPLIER_OBJECT=30.0f;
		
		foreach(var s in ships)
			{
			s.objectalert=0.0f;
			s.planetalert=0.0f;
			
			foreach(var obj in objects)
				{
				if(s.gameObject==obj.gameObject)	// Nie sprawdzaj z samym soba
					continue;
				if(obj.GetComponent<Planet>())
					continue;
					
				float sr=s.GetComponent<ObjectWithRadius>().radius;
				float objr=obj.GetComponent<ObjectWithRadius>().radius;
					
				float dist=(s.transform.position-obj.transform.position).magnitude-sr;
				
				if(dist>objr*ALERT_MULTIPLIER_OBJECT)
					continue;
				
				s.objectalert=Mathf.Max(1.0f-(dist-objr)/(objr*(ALERT_MULTIPLIER_OBJECT-1.0f)), s.objectalert);
				}
			}
			
		foreach(var s in ships)
			{
			foreach(var obj in planets)
				{				
				float sr=s.GetComponent<ObjectWithRadius>().radius;
				float objr=obj.GetComponent<ObjectWithRadius>().radius;
				
				float dist=(s.transform.position-obj.transform.position).magnitude-sr;
				
				if(dist>objr*ALERT_MULTIPLIER_PLANET)
					continue;
				
				s.planetalert=Mathf.Max(1.0f-(dist-objr)/(objr*(ALERT_MULTIPLIER_PLANET-1.0f)), s.planetalert);
				}
			}
		}
		
	private void CheckEnergy()
		{
		Vector3 lightdir=GetComponent<Light>().transform.forward;
		
		foreach(var ship in ships)
			{
			ship.energy=1.0f;
			
			float sr=ship.GetComponent<ObjectWithRadius>().radius;
			
			foreach(var planet in planets)
				{				
				Vector3 dir=ship.transform.position-planet.transform.position;
				float dot=Vector3.Dot(dir, lightdir);
				
				if(dot<0.0f)
					continue;
					
				Vector3 n=(dir-lightdir*dot).normalized;
				
				float spos=Vector3.Dot(ship.transform.position, n);
				
				float pr=planet.GetComponent<ObjectWithRadius>().radius;
				float ppos=Vector3.Dot(planet.transform.position, n);
				
				float dist=Mathf.Abs(ppos-spos);
				
				if(dist>sr+pr)
					ship.energy=Mathf.Min(ship.energy, 1.0f);
				else if(dist<pr-sr)
					ship.energy=0.0f;
				else
					ship.energy=Mathf.Min(ship.energy, (dist-pr+sr)/(2.0f*sr));
				}
			}
		}
	
	private void CheckCollisions()
		{
		var objectsarr=objects.ToArray();
			
		for(int ia=0; ia<objectsarr.Length; ++ia)
			{
			var a=objectsarr[ia];
			
			for(int ib=ia+1; ib<objectsarr.Length; ++ib)
				{
				var b=objectsarr[ib];
				
				float ar=a.GetComponent<ObjectWithRadius>().radius;
				float br=b.GetComponent<ObjectWithRadius>().radius;
				float dist=(a.transform.position-b.transform.position).magnitude;
				
				if(dist>ar+br)
					continue;
					
				Planet aplanet=a.GetComponent<Planet>();
				Spaceship aspaceship=a.GetComponent<Spaceship>();
				Projectile aprojectile=a.GetComponent<Projectile>();
				
				Planet bplanet=b.GetComponent<Planet>();
				Spaceship bspaceship=b.GetComponent<Spaceship>();
				Projectile bprojectile=b.GetComponent<Projectile>();
				
				if(aplanet)
					{
					if(bplanet)
						Collide(aplanet, bplanet);
					else if(bspaceship)
						Collide(aplanet, bspaceship);
					else
						Collide(aplanet, b);
					}
				else if(bplanet)
					{
					if(aspaceship)
						Collide(bplanet, aspaceship);
					else
						Collide(bplanet, a);
					}
				else if(aspaceship)
					{
					if(bprojectile)
						Collide(aspaceship, bprojectile);
					else
						Collide(aspaceship, b);
					}
				else if(bspaceship)
					{
					if(aprojectile)
						Collide(bspaceship, aprojectile);
					else
						Collide(bspaceship, a);
					}
				else if(!aprojectile && !bprojectile)
					Collide(a, b);
					
				}
			}
		}
		
	private void Collide(Planet p1, Planet p2)
		{		
		if(p1.GetHashCode()==p2.GetHashCode())
			return;
		
		if(p1.mass<p2.mass)
			{
			Planet tmp=p1;
			p1=p2;
			p2=tmp;
			}
		else if(p1.mass==p2.mass)
			{
			p2.mass-=p2.mass*0.001f;
			}
		
		ObjectWithRadius p1r=p1.GetComponent<ObjectWithRadius>();
		ObjectWithRadius p2r=p2.GetComponent<ObjectWithRadius>();
		
		SpaceObject p1so=p1.GetComponent<SpaceObject>();
		SpaceObject p2so=p2.GetComponent<SpaceObject>();
					
		float dist=(p1.transform.position-p2.transform.position).magnitude;
		float pct=(1.0f-dist/(p1r.radius+p2r.radius))*Time.deltaTime;
		float msum=p1.mass+p2.mass;
		
		if(dist>p1r.radius*0.1f)
			{
			p1.mass+=p2.mass*pct;
			p2.mass-=p2.mass*pct;
			
			float p2vol=4.0f/3.0f*Mathf.PI*Mathf.Pow(p2r.radius, 3.0f)*pct;
			float p1vol=4.0f/3.0f*Mathf.PI*Mathf.Pow(p1r.radius, 3.0f);
			float p1rad=Mathf.Pow((p1vol+p2vol)*3.0f/4.0f/Mathf.PI, 1.0f/3.0f);
			
			p1r.radius+=p1rad-p1r.radius;
			p2r.radius-=p2r.radius*pct;
			
			p1so.spd+=p2so.spd*pct*p2.mass/msum;
			//p2so.spd-=p2so.spd*pct*p2.mass/msum-p1so.spd*pct*p1.mass/msum;
			}
		else
			{
			//p1.mass+=p2.mass;
			//p1r.radius+=p2r.radius;
			//p1so.spd+=p2so.spd*p2.mass/msum;
			
			Destroy(p2.gameObject);
			return;
			}
		}
		
	private void Collide(Planet planet, SpaceObject obj)
		{
		float dist=(planet.transform.position-obj.transform.position).magnitude;
		float pr=planet.GetComponent<ObjectWithRadius>().radius;
		
		if(dist>pr*0.7f)
			return;
		
		Destroy(obj.gameObject);
		}
		
	private void Collide(Planet planet, Spaceship ship)
		{
		float pr=planet.GetComponent<ObjectWithRadius>().radius;
		float dist=(planet.transform.position-ship.transform.position).magnitude;
		
		if(dist>pr*0.7f)
			ship.hp-=(1.0f-(dist-pr*0.7f)/(pr*0.7f))*Time.deltaTime;
		else
			ship.hp=0.0f;
		}
		
	private void Collide(Spaceship ship, Projectile projectile)
		{
		if(projectile.parent.playerID==ship.playerID)
			return;
		
		SpaceObject sso=ship.GetComponent<SpaceObject>();
		SpaceObject pso=projectile.GetComponent<SpaceObject>();
		
		Vector3 shipv=sso.spd;
		Vector3 projectilev=pso.spd;
		
		float pv=projectilev.magnitude;
		float vrel=(projectilev-shipv).magnitude;
		
		if(vrel<pv)
			{
			ship.hp-=vrel*projectile.damage;
			}
		else
			{
			ship.hp-=(vrel-1.0f)*0.2f*projectile.damage + projectile.damage;
			}
		
		Collide(sso, pso);
		
		Destroy(projectile.gameObject);
		}
		
	private void Collide(Spaceship ship, SpaceObject obj)
		{
		SpaceObject sso=ship.GetComponent<SpaceObject>();
		
		Vector3 shipv=sso.spd;
		Vector3 objv=obj.spd;
		Vector3 vrel=objv-shipv;
		
		ship.hp-=0.1f*vrel.magnitude;
		
		Collide(sso, obj);
		}
		
	private void Collide(SpaceObject a, SpaceObject b)
		{
		// Odbicie
		Vector3 mtv=b.transform.position-a.transform.position;	// Minimum Translation Vector
		Vector3 vrel=b.spd-a.spd;
		
		float vn=Vector3.Dot(vrel, mtv.normalized);
		
		if(vn<0.0f)
			return;
			
		const float RESTITUTION=0.6f;
			
		float i=-((1.0f+RESTITUTION)*vn)/2.0f;
		
		a.spd+=i*mtv.normalized;
		b.spd-=i*mtv.normalized;
		}
		
	private void CreateAsteridField(int asteroids, float radius, GameObject asteroid)
		{
		//Vector3 center=GetCenterOfMass();
		
		//if(asteroid)
		//	print("Asteroida nie jest NULL'em");
		print("Tworzenie "+asteroids+" obiektow wokol "+fieldcenter+", promien "+radius);
		
		for(int i=0; i<asteroids; ++i)
			{			
			Vector3 dir=Random.insideUnitSphere.normalized*radius;
			
			GameObject obj=(GameObject)Instantiate(asteroid);
			SpaceObject so=obj.GetComponent<SpaceObject>();
			
			so.transform.position=fieldcenter+dir;
			so.universe=this;
			
			AddObject(so);
			}
		}
	}
	
	
	
