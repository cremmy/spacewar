using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour 
	{
	public GameObject player;
	public float behind;
	public float up;
	public GameObject crosshair;
	
	private Spaceship target;
	
	// Use this for initialization
	void Start () 
		{
		var ships=GameObject.FindObjectsOfType<Spaceship>();
		
		foreach(var s in ships)
			{
			if(player.GetComponent<Spaceship>().playerID==s.playerID)
				continue;
			
			target=s;
			break;
			}
			
		if(crosshair)
			crosshair=(GameObject)Instantiate(crosshair);
		}
	
	// Update is called once per frame
	void LateUpdate () 
		{
		if(!player)
			{
			if(crosshair)
				Destroy(crosshair);
			return;
			}
		
		transform.position=player.transform.position-(player.transform.forward*behind)+(player.transform.up*up);
		transform.rotation=player.transform.rotation;

		if(!target)
			{
			if(crosshair)
				Destroy(crosshair);
			return;
			}
			
		Vector3 dist=target.transform.position-player.transform.position;
		
		if(dist.sqrMagnitude<2)
			return;
						
		crosshair.transform.position=camera.transform.position+dist.normalized;
		crosshair.transform.LookAt(camera.transform.position, camera.transform.up);
		crosshair.transform.Rotate(new Vector3(0, 180, 0));
		
		//Vector3 desiredPosition=player.transform.position-(player.transform.forward*behind)+(player.transform.up*up);
		//transform.position=Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime*0.1f);
		//transform.rotation=player.transform.rotation;
		}
	}
