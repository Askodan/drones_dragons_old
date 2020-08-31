using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonManager : Singleton<DragonManager> {
	public Transform unitsParent;
	Transform[] units_;
	public Transform[] units{ get { return units_; } }

	public Transform nestsParent;
	Transform[] nests_;
	public Transform[] nests{ get { return nests_; } }

	public Transform waypointsParent;
	Transform[] waypoints_;
	public Transform[] waypoints{ get { return waypoints_; } }

	public Transform sitpointsParent;
	Transform[] sitpoints_;
	public Transform[] sitpoints{ get { return sitpoints_; } }

	public int num_crystals{ get; private set; }
	public int num_machines{ get; private set; }

	[HideInInspector] public DragonInterest[] sensable;
	// Use this for initialization
	void Awake () {
		waypoints_ = GetChildren (waypointsParent);
		nests_ = GetChildren (nestsParent);
		units_ = GetChildren (unitsParent);
		sitpoints_ = GetChildren (sitpointsParent);
		sensable = FindObjectsOfType<DragonInterest> ();
		CountSensable ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	static public Transform[] GetChildren(Transform parent){
		Transform[] result = new Transform[parent.childCount];
		int i=0;
		foreach (Transform t in parent) {
			result[i] = t;
			i++;
		}
		return result;
	}
	public void CountSensable(){
		num_crystals = 0;
		num_machines = 0;
		foreach(DragonInterest DI in sensable){
			if (DI.gameObject.tag == "Pickable") {
				num_crystals++;
			} else {
				num_machines++;
			}
		}
	}
}
