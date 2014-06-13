using UnityEngine;
using System.Collections;

public class Spaceship : MonoBehaviour 
	{	
	private const float FUEL_USAGE=				1.0f/10.0f;	// Zyzywa cale paliwo w 10 sekund
	private const float FUEL_REGEN=				1.0f/20.0f;	// Pelny bak w 20 sekund
	private const float THRUST=					0.6f;
	private const float MAX_SPEED=				4.0f;
	
	private const float BULLET_REGEN=			1.0f/0.5f;	// 1 pocisk co pol sekundy
	private const float BULLET_RECOIL=			0.1f;
	private const float BULLET_MAX=				20;
	private const float BULLET_SHOOT_DELAY=		0.1f;
	private const float BULLET_INITIAL_SPEED=	1.0f;
	
	private const float ROCKET_REGEN=			1.0f/5.0f;	// 1 rakieta co 5 sekund
	private const float ROCKET_RECOIL=			0.1f;
	private const float ROCKET_MAX=				10;
	private const float ROCKET_SHOOT_DELAY=		1.5f;
	
	private const float MINE_REGEN=				1.0f/10.0f;	// 1 mina co 10 sekund
	private const float MINE_RECOIL=			1.5f;
	private const float MINE_MAX=				4;
	private const float MINE_SHOOT_DELAY=		4.0f;
	
			
	abstract class Weapon
		{
		public float ammo;
		public Object projectile;
		
		protected Spaceship parent;	// Rodzic
		
		protected float max;		// Maksymalna liczba pociskow
		protected float regen;		// Regeneracja pociskow na sekunde, przy maksymalnej energii
		protected float recoil;		// Odrzut
		protected float delay;
		
		protected float delaycounter;
		
		public Weapon(Spaceship parent, Object projectile, float max, float regen, float recoil, float delay)
			{
			this.projectile=projectile;
			this.parent=parent;
			this.ammo=0.0f;
			this.max=max;
			this.regen=regen;
			this.recoil=recoil;
			this.delay=delay;
			
			this.delaycounter=0.0f;
			}
			
		public virtual void Update(float energy)
			{
			this.ammo=Mathf.Min(max, ammo+regen*energy*Time.deltaTime);
			parent.energyLabel.text = "Energia: " + Mathf.Round(ammo * 100).ToString() + "%";
			if(delaycounter>0.0f)
				delaycounter-=Time.deltaTime;
			}
			
		public bool CanShoot()
			{
			if(!projectile)
				return false;
			if(delaycounter>0.0f)
				return false;
			if(ammo<1.0f)
				return false;
			return true;	
			}
			
		public virtual bool Shoot() {return false;}
		}
		
	class WeaponGun: Weapon
		{
		protected float speed;
		
		public WeaponGun(Spaceship parent, Object projectile, float max, float regen, float recoil, float delay, float speed):
			base(parent, projectile, max, regen, recoil, delay)
			{
			this.speed=speed;
			}
		
		public override void Update(float energy)
			{
			base.Update(energy);
			}
		
		public override bool Shoot()
			{
			if(!CanShoot())
				return false;

			SpaceObject so=parent.so;
									
			ammo-=1.0f;
			so.ApplyForce(-parent.transform.forward*recoil);

			GameObject bgo=(GameObject)Instantiate(projectile, parent.transform.position, parent.transform.rotation);
			SpaceObject bso=bgo.GetComponent<SpaceObject>();
			
			bso.spd=so.spd+parent.transform.forward*speed;
			bso.universe=so.universe;
			bso.GetComponent<Projectile>().parent=parent;
			so.universe.AddObject(bso);
			
			delaycounter=delay;
			
			return true;
			}
		}
	
	class WeaponRPG: Weapon
	{
		public WeaponRPG(Spaceship parent, Object projectile, float max, float regen, float recoil, float delay):
			base(parent, projectile, max, regen, recoil, delay)
		{
			//
		}
		
		public override void Update(float energy)
		{
			base.Update(energy);
		}
		
		public override bool Shoot()
		{
			if(!CanShoot())
				return false;
			
			SpaceObject so=parent.so;
			
			ammo-=1.0f;
			so.ApplyForce(-parent.transform.forward*recoil);
			
			GameObject bgo=(GameObject)Instantiate(projectile, parent.transform.position, parent.transform.rotation);
			SpaceObject bso=bgo.GetComponent<SpaceObject>();
			
			bso.spd=so.spd;
			bso.universe=so.universe;
			bso.GetComponent<Projectile>().parent=parent;
			so.universe.AddObject(bso);
			
			delaycounter=delay;
			
			return true;
		}
	}
	
	class WeaponMine: Weapon
	{		
		public WeaponMine(Spaceship parent, Object projectile, float max, float regen, float recoil, float delay):
			base(parent, projectile, max, regen, recoil, delay)
		{
			//
		}
		
		public override void Update(float energy)
		{
			base.Update(energy);
		}
		
		public override bool Shoot()
		{
			if(!CanShoot())
				return false;
			
			SpaceObject so=parent.so;
			
			ammo-=1.0f;
			so.ApplyForce(parent.transform.forward*recoil);
			
			GameObject bgo=(GameObject)Instantiate(projectile, parent.transform.position, parent.transform.rotation);
			SpaceObject bso=bgo.GetComponent<SpaceObject>();
			
			bso.spd=so.spd-parent.transform.forward*so.spd.magnitude*0.1f;
			bso.universe=so.universe;
			bso.GetComponent<Projectile>().parent=parent;
			so.universe.AddObject(bso);
			
			delaycounter=delay;
			
			return true;
		}
	}
	
	public SpaceObject so;
	public int playerID;
	
	// Zycia, energia
	public float hp;		// Wartosci od 0 do 1 (wlacznie)
	public float energy;	// jw.
	public float fuel;		// jw.
	
	public Object bulletBase;
	public Object rocketBase;
	public Object laserBase;
	
	public float planetalert;
	public float objectalert;

	public GUIText fuelLabel;
	public GUIText energyLabel;
	public GUIText hpLabel;
	
	// Amunicja
	// Calosci oznaczaja pelny pocisk, czesc ulamkowa jest do generowania kolejnych pociskow
	private Weapon gun;
	private Weapon rpg;
	private Weapon mine;

	// Use this for initialization
	void Start () 
		{
		so=GetComponent<SpaceObject>();
		hpLabel.text = "Życie: " + Mathf.Round (hp * 100).ToString () + "%";
		gun=new WeaponGun(this, bulletBase, BULLET_MAX, BULLET_REGEN, BULLET_RECOIL, BULLET_SHOOT_DELAY, BULLET_INITIAL_SPEED);
		rpg=new WeaponRPG(this, rocketBase, ROCKET_MAX, ROCKET_REGEN, ROCKET_RECOIL, ROCKET_SHOOT_DELAY);
		mine=new WeaponMine(this, laserBase, MINE_MAX, MINE_REGEN, MINE_RECOIL, MINE_SHOOT_DELAY);
		}
	
	// Update is called once per frame
	void Update () 
		{
		if(hp<=0.0f)
			{
			// TODO wstawic tutaj piekna animacje wybuchu
			// Oraz, o ile kiedys beda, dodac oslabiajacy wybuch do universe
			
			Destroy(gameObject);
			return;
			}
		
		Debug.DrawLine(transform.position+transform.right*transform.localScale.x,
		               transform.position+transform.right*transform.localScale.x+transform.up*objectalert*transform.localScale.z, Color.green);
		Debug.DrawLine(transform.position-transform.right*transform.localScale.x,
		               transform.position-transform.right*transform.localScale.x+transform.up*planetalert*transform.localScale.z, Color.cyan);
		Debug.DrawLine(transform.position+transform.right*transform.localScale.x*0.25f,
		               transform.position+transform.right*transform.localScale.x*0.25f+transform.up*hp*transform.localScale.z, Color.red);
		Debug.DrawLine(transform.position-transform.right*transform.localScale.x*0.25f,
		               transform.position-transform.right*transform.localScale.x*0.25f+transform.up*energy*transform.localScale.z, Color.magenta);
		
		UpdateMove();
		UpdateEnergy();
		}
		
	private void UpdateMove()
		{
		const float TURN_SPEED=90;
		
		if(playerID==0)
			{
			if(Input.GetKey(KeyCode.A))
				transform.Rotate(0, -TURN_SPEED*Time.deltaTime, 0);
			if(Input.GetKey(KeyCode.D))
				transform.Rotate(0,  TURN_SPEED*Time.deltaTime, 0);
				
			if(Input.GetKey(KeyCode.Q))
				transform.Rotate(0, 0, -TURN_SPEED*Time.deltaTime);
			if(Input.GetKey(KeyCode.E))
				transform.Rotate(0, 0,  TURN_SPEED*Time.deltaTime);
				
			if(Input.GetKey(KeyCode.W))
				transform.Rotate( TURN_SPEED*Time.deltaTime, 0, 0);
			if(Input.GetKey(KeyCode.S))
				transform.Rotate(-TURN_SPEED*Time.deltaTime, 0, 0);
				
			if(Input.GetKey(KeyCode.Z))
				gun.Shoot();
			if(Input.GetKey(KeyCode.X))
				rpg.Shoot();
			if(Input.GetKey(KeyCode.C))
				mine.Shoot();
				
			if(Input.GetKey(KeyCode.LeftControl))
				AddThrust();
			}
		else
			{
			if(Input.GetKey(KeyCode.Keypad4))
				transform.Rotate(0, -TURN_SPEED*Time.deltaTime, 0);
			if(Input.GetKey(KeyCode.Keypad6))
				transform.Rotate(0,  TURN_SPEED*Time.deltaTime, 0);
			
			if(Input.GetKey(KeyCode.Keypad7))
				transform.Rotate(0, 0, -TURN_SPEED*Time.deltaTime);
			if(Input.GetKey(KeyCode.Keypad9))
				transform.Rotate(0, 0,  TURN_SPEED*Time.deltaTime);
			
			if(Input.GetKey(KeyCode.Keypad8))
				transform.Rotate( TURN_SPEED*Time.deltaTime, 0, 0);
			if(Input.GetKey(KeyCode.Keypad5))
				transform.Rotate(-TURN_SPEED*Time.deltaTime, 0, 0);
			
			if(Input.GetKey(KeyCode.Keypad1))
				gun.Shoot();
			if(Input.GetKey(KeyCode.Keypad2))
				rpg.Shoot();
			if(Input.GetKey(KeyCode.Keypad3))
				mine.Shoot();
			
			if(Input.GetKey(KeyCode.KeypadEnter))
				AddThrust();
			}
		}
		
	private void UpdateEnergy()
		{
		// Sprawdzic czy obiekt jest w cieniu
		energy=1.0f;
		
		// Regeneracja paliwa;
		fuel=   Mathf.Min(1.0f, fuel+FUEL_REGEN*Time.deltaTime*energy);
		this.fuelLabel.text = "Paliwo: " + Mathf.Round(fuel * 100).ToString() + "%";

		// Regeneracja amunicji
		gun.Update(energy);
		rpg.Update(energy);
		mine.Update(energy);
		}
	
	public void AddThrust()
		{
		if(fuel<=0.0f)
			return;
		if(Vector3.Dot(so.spd, transform.forward)>=MAX_SPEED)
			return;
		
		fuel-=FUEL_USAGE*Time.deltaTime;
		so.spd+=transform.forward*Time.deltaTime*THRUST;
		this.fuelLabel.text = "Paliwo: " + Mathf.Round(fuel * 100).ToString() + "%";
		}
		
	public int GetAmmoGun() {return (int)gun.ammo;}
	public int GetAmmoRPG() {return (int)rpg.ammo;}
	public int GetAmmoMine() {return (int)mine.ammo;}
	}

