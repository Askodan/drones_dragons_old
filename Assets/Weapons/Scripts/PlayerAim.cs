using UnityEngine;
using System.Collections;

public class PlayerAim : MonoBehaviour {
	public new Transform camera;
	Transform FrontSight;
    float targetDistance = 100f;
	// Use this for initialization
	void Start () {
		FrontSight = transform.parent.GetComponent<Shooter> ().FrontSight;
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = FrontSight.position + camera.forward * targetDistance;
	}
}
