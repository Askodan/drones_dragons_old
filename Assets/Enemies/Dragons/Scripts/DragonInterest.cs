using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonInterest : MonoBehaviour {
	public List<Interest> Dragons;
	VehicleTypeDefiner typeDefiner;
	public VehicleType Type{get{ return (typeDefiner?typeDefiner.vehicleType:VehicleType.Unknown); }}
	[SerializeField] float baseGainSpeed=0.3f;
	[SerializeField] float baseDecreaseSpeed=1f;
	[SerializeField] float sqrDist2Care=250000f;
	[SerializeField] static float minInterest=0f;
	[SerializeField] static float maxInterest=100f;
	void Awake(){
		typeDefiner = GetComponent<VehicleTypeDefiner> ();
		Dragons = new List<Interest> ();
		int i = 0;
		foreach (Transform dragon in DragonManager.Instance.units) {
			Dragons.Add (new Interest ());
			dragon.GetComponentInChildren<DragonSensesAnalizator> ().interestID = i;
			i++;
		}
	}

	void Update(){
		foreach (Interest interest in Dragons) {
			interest.Update (this);
		}
	}

	[System.Serializable]
	public class Interest{
		float interest_;
		public float interest{ get { return interest_/100f; } }
		public float sqrDist{ get{ return sqr_dist;} set{ sqr_dist = value; inv_sqr_dist = 1f / value;}}
		float sqr_dist;
		float inv_sqr_dist;
		public void Update (DragonInterest DI)
		{
			if (sqrDist < DI.sqrDist2Care) {
				interest_ += DI.baseGainSpeed * Time.deltaTime * (inv_sqr_dist);
			} else {
				interest_ = Mathf.MoveTowards(interest_, minInterest, DI.baseDecreaseSpeed * Time.deltaTime);
			}
			interest_ = Mathf.Clamp (interest_, minInterest, maxInterest);
		}
		public void PumpInterest(float value){
			interest_ += value;
			interest_ = Mathf.Clamp (interest_, minInterest, maxInterest);
		}
	}
}
