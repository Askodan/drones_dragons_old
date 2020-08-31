using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Life))]
[System.Serializable]
public class DragonPersonalityParam{
	public float value{ get; set;}
	public float minValue=0f;
	public float maxValue=1f;
	public float toleranceUp;
	public float toleranceDown;
	public float baseValue;
	public void Init(){
		value = Mathf.Clamp(baseValue + (Random.Range(0,2)==0?-RandomGaussianTolerance (toleranceDown):RandomGaussianTolerance(toleranceUp)), minValue, maxValue);
	}
	float RandomGaussianTolerance(float tolerance){
		float result = Mathf.Abs(RandomExtension.NextGaussianFloat () * tolerance / 3f);
		if (result > tolerance) {
			result = tolerance;
		}
		return result;
	}
}
[System.Serializable]
public class DragonTendencyParam{
	public float reactionCounter;
	public float reactionStrength;
	private float tempValue;
	public bool isState{get{return tempValue > 0.9f;}}
	public void Init(){
		tempValue = 0f;
	}
	public void TimeChange(float change){
		if (change > 0) {
			tempValue += reactionStrength * change * Time.deltaTime;
		} else {
			tempValue = Mathf.MoveTowards (tempValue, 0f, Time.deltaTime * reactionCounter);
		}
		tempValue = Mathf.Clamp01 (tempValue);
	}
}
public class DragonSensesAnalizator : MonoBehaviour {
	[HideInInspector] public int interestID;
	public DragonTendencyParam boredom;//do smth when bored
	public DragonTendencyParam apathy;//after longer activity go, rest somewhere
	public DragonTendencyParam panic;//when low on hp try to escape

	public DragonPersonalityParam aggressiveness;//tend to be aggressive
	public DragonPersonalityParam collectiveness;//like to collect crystals
	public DragonPersonalityParam defensiveness;//the most important is to watch over a nest 

	public DragonActionState state;
	[SerializeField] float damage2interestFactor;
	public float damage2interest_factor{ get{ return damage2interestFactor;} }
	[SerializeField] float interest2triggerAttack;
	private Life life;
	DragonBrain DB;
	//private float sum_crystals;
	//private float sum_machines;
	public DragonInterest most_machine;
	public DragonInterest most_crystal;
	public DragonInterest closest_machine;
	public DragonInterest closest_drone;
	public DragonInterest closest_robot;
	public DragonInterest closest_tractor;

	private float[] weigths;
	//public List<KeyValuePair<GameObject, float> > damage_done{ get; set;}
	//[SerializeField] DragonSenses prefab;
	//[SerializeField] DragonSensesParams[] dragonSensesParams;
	//DragonSenses[] senses;

	//public List<DragonInterest> SensedEnemies;
	//public List<DragonInterest> SensedCrystals;
	//public List<int> SensedEnemiesPower;
	//public List<int> SensedCrystalsPower;

	void Awake(){
		DB = transform.parent.GetComponentInChildren<DragonBrain> ();
		boredom.Init ();
		apathy.Init ();
		panic.Init ();
	
		aggressiveness.Init ();
		collectiveness.Init ();
		defensiveness.Init ();
		weigths = new float[]{aggressiveness.value, collectiveness.value, defensiveness.value};
		state = DragonActionState.Patrol;
		life = GetComponent<Life> ();
		/*senses=new DragonSenses[dragonSensesParams.Length];
		int i = 0;
		foreach (DragonSensesParams pars in dragonSensesParams) {
			DragonSenses newsense = Instantiate (prefab, transform.position, transform.rotation, transform).GetComponent<DragonSenses>();
			newsense.SetNewParams (pars.Range, pars.offset);
			senses [i++] = newsense;
		}
		StartCoroutine (checkWorld ());*/
	}
	void Start(){

		StartCoroutine (Sense ());
	}

	void Update(){
		float boredom_strength=0f;
		float apathy_strength=1f;
		float panic_strength=1f-life.lifePoints/life.maxLife;
		switch (state) {
		case DragonActionState.Patrol:
			boredom_strength = 1f;
			apathy_strength = 0f;
			break;
		case DragonActionState.Attack:
			apathy_strength = Mathf.Clamp( 1f - aggressiveness.value - (most_machine!=null?most_machine.Dragons[interestID].interest:0f), 0.1f, 0.9f);
			break;
		case DragonActionState.Nest:
			apathy_strength = Mathf.Clamp( 1f - defensiveness.value - (most_machine!=null?most_machine.Dragons[interestID].interest:0f), 0.1f, 0.9f);
			panic_strength = 0f;
			break;
		case DragonActionState.Collect:
			apathy_strength = Mathf.Clamp( 1f - collectiveness.value - (most_crystal!=null?most_crystal.Dragons[interestID].interest:0f), 0.1f, 0.9f);
			break;
		case DragonActionState.Rest:
			boredom_strength = 1f - panic_strength;
			apathy_strength = 0f;
			break;
		default:
			throw new System.ArgumentOutOfRangeException ();
		}
		//boredom calculation
		boredom.TimeChange (boredom_strength);
		//apathy calculation
		apathy.TimeChange (apathy_strength);
		//panic calculation
		panic.TimeChange (panic_strength);


		if (apathy.isState) {
			state = DragonActionState.Patrol;
		}else if (panic.isState) {
			state = DragonActionState.Rest;
		}else if (boredom.isState) {
			switch (RandomExtension.WeightedRandom (weigths)) {
			case 0:
				state = DragonActionState.Attack;
				break;
			case 1:
				state = DragonActionState.Collect;
				break;
			case 2:
				state = DragonActionState.Nest;
				break;
			}
		}
		
	}
	IEnumerator Sense(){
		while (true) {
			//float sum_crystals_temp=0f;
			//float sum_machines_temp=0f;
			float max_crystals_temp=-0.1f;
			DragonInterest most_machine_temp=null;

			float max_machines_temp=-0.1f;
			DragonInterest most_crystal_temp=null;

			float min_sqrDist=float.PositiveInfinity;
			DragonInterest closest_machine_temp=null;

			float min_sqrDist_drone=float.PositiveInfinity;
			DragonInterest closest_drone_temp=null;

			float min_sqrDist_robot=float.PositiveInfinity;
			DragonInterest closest_robot_temp=null;

			float min_sqrDist_tractor=float.PositiveInfinity;
			DragonInterest closest_tractor_temp=null;

			foreach (DragonInterest interested in DragonManager.Instance.sensable) {
				interested.Dragons [interestID].sqrDist = (interested.transform.position - transform.position).sqrMagnitude;
				if (interested.gameObject.tag == "Pickable") {
					//sum_crystals_temp += interested.Dragons [interestID].interest;
					if (interested.Dragons [interestID].interest > max_crystals_temp) {
						max_crystals_temp = interested.Dragons [interestID].interest;
						most_crystal_temp = interested;
					}
				} else {
					//sum_machines_temp += interested.Dragons [interestID].interest;
					if (interested.Dragons [interestID].interest > max_machines_temp) {
						max_machines_temp = interested.Dragons [interestID].interest;
						most_machine_temp = interested;
					}
					if (interested.Dragons [interestID].sqrDist < min_sqrDist) {
						min_sqrDist = interested.Dragons [interestID].sqrDist;
						closest_machine_temp = interested;
					}
					switch (interested.Type) {
					case VehicleType.Drone:
						if (interested.Dragons [interestID].sqrDist < min_sqrDist_drone) {
							min_sqrDist_drone = interested.Dragons [interestID].sqrDist;
							closest_drone_temp = interested;
						}
						break;
					case VehicleType.Tractor:
						if (interested.Dragons [interestID].sqrDist < min_sqrDist_tractor) {
							min_sqrDist_tractor = interested.Dragons [interestID].sqrDist;
							closest_tractor_temp = interested;
						}
						break;
					case VehicleType.Robot:
						if (interested.Dragons [interestID].sqrDist < min_sqrDist_robot) {
							min_sqrDist_robot = interested.Dragons [interestID].sqrDist;
							closest_robot_temp = interested;
						}
						break;
					default:
						throw new System.ArgumentOutOfRangeException ();
					}
				}
				//foreach(KeyValuePair<GameObject, float> k in damage_done){
				//	if (interested.gameObject == k.Key) {
				//		interested.Dragons [interestID].interest += k.Value*damage2interest_factor;
				//	}
				//}
				//interested.Dragons [interestID].interested = 1f / interested.Dragons [interestID].sqrDist;
				yield return null;
			}
			//sum_crystals = sum_crystals_temp;
			//sum_machines = sum_machines_temp;
//			if (closest_machine != closest_machine_temp) {
//				print ("closest machine changed in "+interestID+" from "+(closest_machine?closest_machine.name:"null") +" to "+(closest_machine_temp?closest_machine_temp.name:"null"));
//			}
			closest_machine = closest_machine_temp;
			closest_drone = closest_drone_temp;
			closest_tractor = closest_tractor_temp;
			closest_robot = closest_robot_temp;

			most_crystal = most_crystal_temp;
//			if (most_machine != most_machine_temp) {
//				print ("machine changed in "+interestID+" from "+(most_machine?most_machine.name:"null") +" to "+(most_machine_temp?most_machine_temp.name:"null"));
//			}
			if ((state == DragonActionState.Collect || state == DragonActionState.Patrol || state == DragonActionState.Nest) && max_machines_temp > interest2triggerAttack) {
				state = DragonActionState.Attack;
				boredom.Init ();
				apathy.Init ();
				panic.Init ();
			}
			most_machine = most_machine_temp;
			yield return null;
		}
	}
	/*[System.Serializable]
	class DragonSensesParams{
		public float Range;
		public Vector3 offset;
	}
	IEnumerator checkWorld(){
		while (true) {
			//yield return new WaitForSeconds (2f);
			StartCoroutine(UpdateSensed ());
			yield return null;
		}
	}
	IEnumerator UpdateSensed(){
		SensedEnemies.Clear ();
		SensedCrystals.Clear ();
		SensedEnemiesPower.Clear ();
		SensedCrystalsPower.Clear ();
		foreach (DragonSenses sense in senses) {
			//sense.TurnOn();
			int limit = Random.Range (1, 5);
			for(int i=0;i<limit;i++)
				yield return null;
			foreach (DragonInterest interest in sense.SensedEnemies) {
				if (!SensedEnemies.Contains (interest)) {
					SensedEnemies.Add (interest);
					SensedEnemiesPower.Add (1);
				} else {
					SensedEnemiesPower [SensedEnemies.IndexOf (interest)]++;
				}
			}
			foreach (DragonInterest interest in sense.SensedCrystals) {
				if (!SensedCrystals.Contains (interest)) {
					SensedCrystals.Add (interest);
					SensedCrystalsPower.Add (1);
				} else {
					SensedCrystalsPower [SensedCrystals.IndexOf (interest)]++;
				}
			}
			//sense.TurnOff();
		}
	}*/
}
static class RandomExtension{

	public static float NextGaussianFloat()
	{
		float u, v, S;

		do
		{
			u = 2.0f * Random.value - 1.0f;
			v = 2.0f * Random.value - 1.0f;
			S = u * u + v * v;
		}
		while (S >= 1.0f);

		float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
		return u * fac;
	}
	public static int WeightedRandom (float[] data){
		float max = 0f;
		foreach (float f in data) {
			max += f;
		}
		float rand = Random.Range (0f, max);
		max = 0f;
		for (int i = 0; i < data.Length; i++) {
			max += data [i];
			if (rand < max) {
				return i;
			}
		}
		return -1;
	}
}