using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour 
	{
	public GameObject player;
	public float behind;
	public float up;
	
	// Use this for initialization
	void Start () 
		{
		//
		}
	
	// Update is called once per frame
	void LateUpdate () 
		{
		if(!player)
			{
			return;
			}
		
		transform.position=player.transform.position-(player.transform.forward*behind)+(player.transform.up*up);
		transform.rotation=player.transform.rotation;
		
		//Vector3 desiredPosition=player.transform.position-(player.transform.forward*behind)+(player.transform.up*up);
		//transform.position=Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime*0.1f);
		//transform.rotation=player.transform.rotation;
		}
	}
