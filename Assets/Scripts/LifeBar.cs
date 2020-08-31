using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//object with this script has to be child of object with Life script;
public class LifeBar : MonoBehaviour {
	[SerializeField] Transform HPBar;
	[SerializeField] Transform FuelBar;
	Transform targetCamera;
	Life life;
	// Use this for initialization
	void Start () {
		targetCamera = Camera.main.transform;
		life = GetComponentInParent<Life> ();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.LookAt (targetCamera);
		HPBar.localScale = new Vector3 (life.lifePoints/life.maxLife, 1f, 1f);
		FuelBar.localScale = new Vector3 (life.fuelPoints/life.maxFuel, 1f, 1f);

	}
}
