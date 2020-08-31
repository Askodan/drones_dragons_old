using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonSenses : MonoBehaviour {
	[SerializeField] float Range;
	SphereCollider Sensor;
	[SerializeField] Vector3 Offset;
	public List<DragonInterest> SensedEnemies;
	public List<DragonInterest> SensedCrystals;
	void Awake(){
		GameObject obj = new GameObject ();
		Sensor = obj.AddComponent<SphereCollider> ();
		Sensor.transform.SetParent (transform);
		Sensor.transform.localPosition = Offset;
		Sensor.transform.localRotation = Quaternion.identity;
		Sensor.transform.localScale = Vector3.one;
		Sensor.radius = Range;
		Sensor.isTrigger = true;
	}
	void Start(){
		SensedEnemies = new List<DragonInterest> ();
		SensedCrystals = new List<DragonInterest> ();
	}
	void OnTriggerEnter(Collider col){
		DragonInterest DI = col.GetComponentInParent<DragonInterest> ();
		if (DI)	{
			if (DI.tag == "Pickable") {
				SensedCrystals.Add (DI);
			}else if (col.tag == "Vehicle") {
					SensedEnemies.Add (DI);
				}
		}
	}
	void OnTriggerExit(Collider col){
		DragonInterest DI = col.GetComponentInParent<DragonInterest> ();
		if (DI)	{
			if (DI.tag == "Pickable") {
				RemoveFromSensed (DI, true);
			}else if (col.tag == "Vehicle") {
				RemoveFromSensed (DI, false);
			}
		}
	}

	public void RemoveFromSensed(DragonInterest item, bool crystal){
		if (crystal) {
			if (SensedCrystals.Contains (item)) {
				SensedCrystals.Remove (item);
			}
		}else{
			if (SensedEnemies.Contains (item)) {
				SensedEnemies.Remove (item);
			}
		}
	}
	public void SetNewParams(float range, Vector3 offset){
		Range = range;
		Offset = offset;
		Sensor.radius = range;
		Sensor.transform.localPosition = offset;
	}
	public void TurnOn(){
		Sensor.enabled = true;
		SensedEnemies.Clear ();
		SensedCrystals.Clear ();
	}
	public void TurnOff(){
		Sensor.enabled = false;
	}
}
