using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Life : MonoBehaviour {
	public bool vehicle;

	public float lifePoints{ get; private set; }
	[SerializeField] float repairSpeed;
	public float maxLife;
	public bool dead{ get; private set; }
	public float recovery{ get; set; }

	public float fuelPoints{ get; private set; }
	[SerializeField] float refuelSpeed;
	public float maxFuel;
	public bool empty{ get; private set; }
	public float refuely{ get; set; }

	public float minimpactFactor2damage;
	public float impactDamageCoefficient;
	[SerializeField] Transform BarsPrefab;
	[SerializeField] Vector3 BarsPosition;
	Rigidbody rigid;
	public DragonSensesAnalizator DSA{ get; private set;}
	//Transform bars;
	public void Awake(){
		if (BarsPrefab) {
//			bars = 
			Instantiate (BarsPrefab, transform.position + BarsPosition, Quaternion.identity, transform);
		}
		lifePoints = maxLife;
		fuelPoints = maxFuel;
		rigid = GetComponent<Rigidbody> ();
		DSA = GetComponent<DragonSensesAnalizator> ();
		if (rigid == null) {
			Debug.LogError ("There is no rigidbody!");
		}
	}
	void Update(){
		Repair (recovery + 1f);
		Refuel (refuely + 1f);
	}
	void Repair(float speed){
		lifePoints += repairSpeed * speed * Time.deltaTime;
		if (lifePoints > maxLife) {
			lifePoints = maxLife;
		}
	}
	public void Damage(float damage){
		lifePoints -= damage;
		if (lifePoints <= 0) {
			Dead ();
		}
	}
	void Refuel(float speed){
		fuelPoints += refuelSpeed * speed * Time.deltaTime;
		if (fuelPoints > maxFuel) {
			fuelPoints = maxFuel;
		}
	}
	public void UseFuel(float used){
		fuelPoints -= used;
		if (fuelPoints <= 0) {
			empty = true;
		}
	}
	void Dead (){
		dead = true;
		if (vehicle) {
			lifePoints = 0;
			//tu trzeba napisać blokadę sterowania ew. restart
		} else {
			//tu trzeba pewnie zdeaktywowac przeciwnika
		}
	}
	void OnCollisionEnter(Collision col){
		if (rigid.isKinematic)
			return;
		
		float impactFactor = col.relativeVelocity.sqrMagnitude / rigid.mass - minimpactFactor2damage;
		if (impactFactor > 0) {
			Damage (impactFactor * impactDamageCoefficient);
		}
	}
}
