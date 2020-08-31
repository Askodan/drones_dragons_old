using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonNestManager : MonoBehaviour {
	public Sensor Given{ get; private set;}

	void Awake(){
		Given = GetComponentInChildren<Sensor> ();

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
