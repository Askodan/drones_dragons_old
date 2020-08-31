using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum DragonActionState{
	Patrol,//flight around, getting bored
	Attack,//flight around, if some enemy get certain interest, go for them
	Nest,//flight around nest, sit in it, if smth gets too close, attack
	Collect,//flight around, if some crystal get certain interest, go for them
	Rest//go to nest in order to rest(low hp), high hp any landing place is ok
}
public enum DragonTargetType{
	Nestpoint,
	Flypoint,
	Sitpoint
}
public class DragonBrain : MonoBehaviour {
	public bool setTarget;

	public Vector3 targetPoint;
	Quaternion targetRotation;

	public float sizeOfTest;
	float sqrSizeOfTest;


	[SerializeField] LayerMask mask;
	[SerializeField] public DragonMover DM;
	DragonSensesAnalizator DSA;
	[SerializeField] DragonAttackTarget DAT;
	[SerializeField] SkinnedMeshRenderer detailedDragonModel;
	[SerializeField] DragonSenses DS;

	[SerializeField] PatrolBehaviour patrol;
	[SerializeField] AttackBehaviour attack;
	[SerializeField] CollectBehaviour collect;
	[SerializeField] NestBehaviour nest;
	[SerializeField] RestBehaviour rest;
	private DragonTargetType targetType;
	public bool firing{ get; private set;}
	BezierCurve curve;
	float progress=-1;
	void Awake(){
		patrol.Awake (this);
		attack.Awake (this);
		collect.Awake (this);
		nest.Awake (this);
		rest.Awake (this);

		sqrSizeOfTest = sizeOfTest * sizeOfTest;

		DAT.dragon = DM.transform;
		DS.transform.SetParent (DM.transform);
		curve = transform.parent.gameObject.AddComponent<BezierCurve> ();

		DSA = DM.GetComponent<DragonSensesAnalizator> ();
	}
	// Use this for initialization
	void Start () {
		targetPoint = transform.position;
		targetRotation = transform.rotation;
		attack.Start ();
		nest.Start ();
	}
	public bool outofrangeaim;
	bool atPoint;
	bool fight;

	DragonActionState prevState;
	void Update(){
		//pierdoly
		if (prevState != DSA.state && prevState == DragonActionState.Attack) {
			fight = false;
		}

		DAT.UpdateFireBreath ();

		if (setTarget) {
			setTarget = false;
			SetNewTarget (targetPoint);
		}


		switch (DSA.state) {
		case DragonActionState.Patrol:
			patrol.Update ();
			break;
		case DragonActionState.Attack:
			attack.Update ();
			break;
		case DragonActionState.Nest:
			//nieskonczone
			//if (path_built) {
			//	targetPoint = DragonManager.Instance.nests [Random.Range (0, DragonManager.Instance.nests.Length)].position;
			//	targetType = DragonTargetType.Nestpoint;
			//	setTarget = true;
			//}
			nest.Update ();
			break;
		case DragonActionState.Collect:
			//	targetPoint = DSA.most_crystal.transform.position;
			//	targetType = DragonTargetType.Nestpoint;
			//	setTarget = true;
			collect.Update ();
			break;
		case DragonActionState.Rest:
			
			//if (path_built) {
			//	targetPoint = DragonManager.Instance.sitpoints [Random.Range (0, DragonManager.Instance.sitpoints.Length)].position;
			//	targetType = DragonTargetType.Sitpoint;
			//}
			rest.Update ();
			break;
		default:
			throw new System.ArgumentOutOfRangeException ();
		}


		//odpowiedz na zaczepke
		if (DSA.state != DragonActionState.Rest && DSA.state != DragonActionState.Attack) {
			if (DS.SensedEnemies.Count > 0) {
				fight = true;
				Vector3 point2attack = DSA.closest_machine.transform.position;//DS.SensedEnemies [0].transform.position;
				DAT.transform.position = point2attack;

				transform.LookAt (point2attack);
				transform.position = point2attack + attack.attackPoint * DAT.averageDist;
			} else {
				fight = false;
			}
		}
		prevState = DSA.state;

	}
	void LateUpdate () {
		firing = DAT.InRange ();
		if ( firing || outofrangeaim) {
			DAT.Aim ();
			DAT.fireOn (detailedDragonModel, 0);
		} else {
			DAT.fireOff (detailedDragonModel, 0);
		}
	}
	void SetNewTarget(Vector3 point){
		targetPoint = point;
		StartCoroutine(BuildPath ());
	}
	bool path_built = true;
	IEnumerator BuildPath(){
		if (path_built) {
			atPoint = false;
			path_built = false;
			while (curve.pointCount > 0) {
				BezierPoint point = curve.GetAnchorPoints () [0]; 
				curve.RemovePoint (point);
				Destroy (point.gameObject);
				yield return null;
			}
			progress = 0f;
			curve.AddPointAt (DM.transform.position);
			curve.AddPointAt (targetPoint);
			curve.SetDirty ();
			int numberOfTests = (int)(curve.length / sizeOfTest);
			for (int i = 0; i < numberOfTests; i++) {
				if (Physics.OverlapSphere (curve.GetPointAt ((float)i / (float)numberOfTests), sizeOfTest, mask).Length > 0) {
					Vector3 midPos = curve.GetPointAt ((float)i / (float)numberOfTests) + Vector3.up * (sizeOfTest);
					while (Physics.OverlapSphere (midPos, sizeOfTest, mask).Length > 0) {
						midPos += Vector3.up * (sizeOfTest);
					}
					curve.AddPointAt (midPos);
					BezierPoint[] points = curve.GetAnchorPoints ();
					points [points.Length - 2].position = points [points.Length - 1].position;
					points [points.Length - 1].position = targetPoint;
					curve.SetDirty ();
					yield return null;
				}
			}
			if (targetType == DragonTargetType.Sitpoint) {
				curve.AddPointAt (targetPoint + Vector3.up * sizeOfTest);
				BezierPoint[] points = curve.GetAnchorPoints ();
				points [points.Length - 2].position = points [points.Length - 1].position;
				points [points.Length - 1].position = targetPoint;
				curve.SetDirty ();
			}
			path_built = true;
		}
	}

    //[System.Serializable]
    public class Behaviour {
        protected DragonBrain DB;
        virtual public void Awake(DragonBrain DB_) {
            DB = DB_;
        }
        virtual public void Update() {
            if (DB.prevState != DB.DSA.state) {
                OnEnterState();
                switch (DB.prevState)
                {
                    case DragonActionState.Patrol:
                        DB.patrol.OnExitState();
                        break;
                    case DragonActionState.Attack:
                        DB.attack.OnExitState();
                        break;
                    case DragonActionState.Nest:
                        DB.nest.OnExitState();
                        break;
                    case DragonActionState.Collect:
                        DB.collect.OnExitState();
                        break;
                    case DragonActionState.Rest:
                        DB.rest.OnExitState();
                        break;
                }
            }
        }
        virtual protected void OnEnterState() {
            print("podstawowa wersja wejścia");
        }
        virtual protected void OnExitState()
        {
            print("podstawowa wersja wyjścia");
        }
    }

    [System.Serializable]
	public class PatrolBehaviour : Behaviour{

		public float dist2Keep;
		float sqrDist2Keep;

		public float dist2KeepSlow;
		float sqrDist2KeepSlow;

		public float dist2Stop;
		float sqrDist2Stop;

		public float ChangePositionIdleInterval;
		public float ChangePositionIdleTolerance;

		bool sit;
		override public void Awake(DragonBrain DB_){
			base.Awake (DB_);

			sqrDist2KeepSlow = dist2KeepSlow * dist2KeepSlow;
			sqrDist2Keep = dist2Keep * dist2Keep;
			sqrDist2Stop = dist2Stop * dist2Stop;
		}

		override public void Update(){
			base.Update ();

			float sqrDist2target = (DB.targetPoint - DB.transform.position).sqrMagnitude;
			if (DB.path_built&&!DB.fight) {
				if (sqrDist2target < sqrDist2Stop) {
					DB.transform.position = DB.targetPoint;
					DB.transform.rotation = DB.targetRotation;
					if (!DB.atPoint) {
						DB.StartCoroutine (WaitAndFly ());
						if (sit) {
							DB.DM.flying = false;
						}
					}
					DB.atPoint = true;
				} else {
					float dist2body = (DB.DM.transform.position - DB.transform.position).sqrMagnitude;
					bool slow = sqrDist2target < DB.sqrSizeOfTest;
					if ( dist2body<(slow?sqrDist2KeepSlow:sqrDist2Keep)) {
						DB.progress += (slow?dist2KeepSlow:dist2Keep) / DB.curve.length*Time.deltaTime;
						Vector3 dir = DB.curve.GetPointAt (DB.progress + Time.deltaTime) - DB.curve.GetPointAt (DB.progress);
						dir.y = 0;
						if(dir!=Vector3.zero)
							DB.transform.rotation = Quaternion.LookRotation(dir);
						DB.transform.position = DB.curve.GetPointAt (DB.progress);
						DB.DM.flying = true;
					}
				}
			}
		}
		IEnumerator WaitAndFly(){
			yield return new WaitForSeconds(ChangePositionIdleInterval+Random.Range(-ChangePositionIdleTolerance,ChangePositionIdleTolerance));
			switch(RandomExtension.WeightedRandom(new float[]{DragonManager.Instance.waypoints.Length, DragonManager.Instance.nests.Length, DragonManager.Instance.sitpoints.Length})){
			case 0:	
				DB.targetPoint = DragonManager.Instance.waypoints [Random.Range (0, DragonManager.Instance.waypoints.Length)].position;
				DB.targetType = DragonTargetType.Flypoint;
				sit = false;
				break;
			case 1:
				DB.targetPoint = DragonManager.Instance.nests [Random.Range (0, DragonManager.Instance.nests.Length)].position;
				DB.targetType = DragonTargetType.Nestpoint;
				sit = true;
				break;
			case 2:
				Transform tar = DragonManager.Instance.sitpoints [Random.Range (0, DragonManager.Instance.sitpoints.Length)];
				DB.targetPoint = tar.position;
				DB.targetRotation = tar.rotation;
				DB.targetType = DragonTargetType.Sitpoint;
				sit = true;
				break;
			}
			DB.setTarget = true;
		}
		override protected void OnEnterState(){
			//print ("patrolowa wersja");
			DB.SetNewTarget (DB.DM.transform.position);
		}
	}
	[System.Serializable]
	public class AttackBehaviour : Behaviour{
		public float height2land;
		[HideInInspector] public Vector3 attackPoint;

		public float dist2Attack;
		float sqrDist2Attack;

		public float dist2Keep;
		float sqrDist2Keep;

		public float ChangePositionFightInterval;
		public float ChangePositionFightTolerance;

		public bool land{ get; private set;}
		public bool land_closest{ get; private set;}
		override public void Awake(DragonBrain DB_){
			base.Awake (DB_);

			sqrDist2Keep = dist2Keep * dist2Keep;
			sqrDist2Attack = dist2Attack * dist2Attack;
		}
		public void Start(){

			DB.StartCoroutine (ChangeAttack ());
		}
		override public void Update(){
			base.Update ();
			if (DB.DSA.most_machine) {
				float sqrDist2target = (DB.targetPoint - DB.DM.transform.position).sqrMagnitude;
				if (sqrDist2target < sqrDist2Attack) {
					DB.fight = true;
					DB.DAT.transform.position = DB.DSA.most_machine.transform.position;

					DB.transform.LookAt (DB.DSA.most_machine.transform.position);
					DB.transform.position = DB.DSA.most_machine.transform.position + attackPoint * DB.DAT.averageDist;
					DB.DM.flying = !land;
				} else {
					DB.fight = false;
					if (DB.path_built) {
						if ((DB.targetPoint - DB.DSA.most_machine.transform.position).sqrMagnitude > sqrDist2Attack / 4f) {
							DB.SetNewTarget(DB.DSA.most_machine.transform.position);
							return;
						}
						float dist2body = (DB.DM.transform.position - DB.transform.position).sqrMagnitude;
						if (dist2body < sqrDist2Keep) {
							DB.progress += (dist2Keep) / DB.curve.length * Time.deltaTime;
							Vector3 dir = DB.curve.GetPointAt (DB.progress + Time.deltaTime) - DB.curve.GetPointAt (DB.progress);
							dir.y = 0;
							if (dir != Vector3.zero)
								DB.transform.rotation = Quaternion.LookRotation (dir);
							DB.transform.position = DB.curve.GetPointAt (DB.progress);
							DB.DM.flying = true;
						}
					}
				}
			}
		}
		IEnumerator ChangeAttack(){
			while (true) {
				yield return new WaitForSeconds (ChangePositionFightInterval + Random.Range (-ChangePositionFightTolerance, ChangePositionFightTolerance));
				attackPoint = Random.onUnitSphere;
				attackPoint.y = Mathf.Abs (attackPoint.y);
				if (DB.DSA.most_machine) {
					bool landDragon = Physics.Raycast (DB.DM.transform.position, Vector3.down, height2land, DB.mask);
					bool landEnemy = Physics.Raycast (DB.DSA.most_machine.transform.position + attackPoint * DB.DAT.averageDist, Vector3.down, height2land, DB.mask);
					bool landEnemyClosest = Physics.Raycast (DB.DSA.closest_machine.transform.position + attackPoint * DB.DAT.averageDist, Vector3.down, height2land, DB.mask);
					land = landDragon&&landEnemy;
					land_closest = landDragon && landEnemyClosest;
				}
			}
		}
		override protected void OnEnterState(){
			//print (DB.DSA.interestID+"atakowa wersja");
			if (DB.DSA.most_machine) {
				DB.SetNewTarget(DB.DSA.most_machine.transform.position);
			}
		}
	}

	[System.Serializable]
	public class CollectBehaviour : Behaviour{
		public float dist2Keep;
		float sqrDist2Keep;
		public float dist2Land;
		float sqrDist2Land;
		bool hasCrystal;
		public Transform HoldingPoint;
		DragonInterest found = null;
		override public void Awake(DragonBrain DB_){
			base.Awake (DB_);
			sqrDist2Keep = dist2Keep * dist2Keep;
			sqrDist2Land = dist2Land * dist2Land;
		}

		override public void Update(){
			base.Update ();
			if (DB.atPoint) {
				if (hasCrystal) {
					//put crystal
					DropItem();
				} else {
					//take crystal
					foreach (DragonInterest sensed in DB.DS.SensedCrystals) {
						if (sensed == DB.DSA.most_crystal) {
							found = sensed;
							break;
						}
					}
					if (found) {
						RobotGrabber.SetCollidersInItem (found.transform, false);
						found.transform.SetParent (HoldingPoint);
						found.transform.localPosition = Vector3.zero;
						DB.SetNewTarget (DragonManager.Instance.nests [Random.Range (0, DragonManager.Instance.nests.Length)].position);
					} else {
						print ("smth wrong with grabbing crystal");
					}
				}
			} else {
				if (DB.path_built) {
					float sqrDist2target = (DB.targetPoint - DB.transform.position).sqrMagnitude;
					if (sqrDist2target < sqrDist2Land) {
						DB.atPoint = true;

						DB.DM.flying = false;
					} else {
						float dist2body = (DB.DM.transform.position - DB.transform.position).sqrMagnitude;
						if (dist2body < sqrDist2Keep) {
							DB.progress += (dist2Keep) / DB.curve.length * Time.deltaTime;
							Vector3 dir = DB.curve.GetPointAt (DB.progress + Time.deltaTime) - DB.curve.GetPointAt (DB.progress);
							dir.y = 0;
							if (dir != Vector3.zero)
								DB.transform.rotation = Quaternion.LookRotation (dir);
							DB.transform.position = DB.curve.GetPointAt (DB.progress);
							DB.DM.flying = true;
						}
					}
				}
			}
		}
		public void DropItem(){
			found.transform.SetParent (null);
			RobotGrabber.SetCollidersInItem (found.transform, true);
			found = null;
		}
		override protected void OnEnterState(){
			//print ("collectowa wersja");
			DB.SetNewTarget(DB.DSA.most_crystal.transform.position);
		}
	}

	[System.Serializable]
	public class NestBehaviour : Behaviour{
		public float dist2Keep;
		float sqrDist2Keep;
		public float dist2Watch;
		float sqrDist2Watch;
		public float ChangePositionNestInterval;
		public float ChangePositionNestTolerance;
		public float height2Land;
		Vector3 watchPoint;
		bool land;
		Vector3 NestPoint;
		DragonSenses nestSense;
		override public void Awake(DragonBrain DB_){
			base.Awake (DB_);
			sqrDist2Keep = dist2Keep * dist2Keep;
			sqrDist2Watch = dist2Watch * dist2Watch;
		}
		public void Start(){
			DB.StartCoroutine (ChangePosition ());
		}
		override public void Update(){
			base.Update ();
			if (DB.atPoint) {
				if (nestSense.SensedEnemies.Count > 0) {
					DB.fight = true;
					DB.DM.flying = DB.attack.land_closest;
					Vector3 point2attack = DB.DSA.closest_machine.transform.position;//DS.SensedEnemies [0].transform.position;
					DB.DAT.transform.position = point2attack;

					DB.transform.LookAt (point2attack);
					DB.transform.position = point2attack + DB.attack.attackPoint * DB.DAT.averageDist;
				}else {
					DB.fight = false;
					DB.DM.flying = land;
					DB.transform.position = watchPoint;
				}
			} else {
				if (DB.path_built) {
					float sqrDist2target = (DB.targetPoint - DB.transform.position).sqrMagnitude;
					if (sqrDist2target < sqrDist2Watch) {
						DB.atPoint = true;
					} else {
						float dist2body = (DB.DM.transform.position - DB.transform.position).sqrMagnitude;
						if (dist2body < sqrDist2Keep) {
							DB.progress += (dist2Keep) / DB.curve.length * Time.deltaTime;
							Vector3 dir = DB.curve.GetPointAt (DB.progress + Time.deltaTime) - DB.curve.GetPointAt (DB.progress);
							dir.y = 0;
							if (dir != Vector3.zero)
								DB.transform.rotation = Quaternion.LookRotation (dir);
							DB.transform.position = DB.curve.GetPointAt (DB.progress);
							DB.DM.flying = true;
						}
					}
				}
			}

		}
		override protected void OnEnterState(){
			//print ("nestowa wersja");
			DB.targetType = DragonTargetType.Sitpoint;
			int nest2Flee = Random.Range (0, DragonManager.Instance.nests.Length);
			DB.SetNewTarget (DragonManager.Instance.nests [nest2Flee].position);
			NestPoint = DB.targetPoint;
			nestSense = DragonManager.Instance.nests [nest2Flee].GetComponent<DragonSenses> ();
		}
		IEnumerator ChangePosition(){
			while (true) {
				yield return new WaitForSeconds (ChangePositionNestInterval + Random.Range (-ChangePositionNestTolerance, ChangePositionNestTolerance));
				watchPoint = Random.onUnitSphere;
				watchPoint.y = Mathf.Abs (watchPoint.y);
				watchPoint += NestPoint;
				land = Physics.Raycast (watchPoint * DB.DAT.averageDist, Vector3.down, height2Land, DB.mask)&&Physics.Raycast (DB.DM.transform.position, Vector3.down, height2Land, DB.mask);;

			}
		}
	}
	//rest jest mylące, chodzi o zwiewanie przy uszkodzeniu, żeby zregenerować siły (rest)
	//ucieka w kierunku (najbliższego) gniazda, ale odpoczywać moze wcześniej, jeżeli wrogowie są dość daleko,
	//jeżeli w gnieździe go dogonią to walczy do śmierci
	[System.Serializable]
	public class RestBehaviour : Behaviour{
		public float dist2Keep;
		float sqrDist2Keep;

		public float dist2Rest;//jeszcze nie używane
		float sqrDist2Rest;

		int nest2Flee;
		DragonSenses nestSense;
		override public void Awake(DragonBrain DB_){
			base.Awake (DB_);
			sqrDist2Rest = dist2Rest * dist2Rest;
			sqrDist2Keep = dist2Keep * dist2Keep;
		}
		override public void Update(){
			base.Update ();

			if ((DB.DSA.closest_drone.transform.position-DB.DM.transform.position).sqrMagnitude > sqrDist2Rest) {
				DB.DM.flying = false;
				DB.transform.position = DB.DM.transform.position;
			} else {
				if (DB.path_built) {
					if (!DB.atPoint) {
						if ((DB.transform.position - DB.targetPoint).sqrMagnitude < sqrDist2Keep) {
							DB.transform.position = DB.targetPoint;
							DB.transform.rotation = DB.targetRotation;
							DB.atPoint = true;
						}
						float dist2body = (DB.DM.transform.position - DB.transform.position).sqrMagnitude;
						if (dist2body < sqrDist2Keep) {
							DB.progress += (dist2Keep) / DB.curve.length * Time.deltaTime;
							Vector3 dir = DB.curve.GetPointAt (DB.progress + Time.deltaTime) - DB.curve.GetPointAt (DB.progress);
							dir.y = 0;
							if (dir != Vector3.zero)
								DB.transform.rotation = Quaternion.LookRotation (dir);
							DB.transform.position = DB.curve.GetPointAt (DB.progress);
							DB.DM.flying = true;
						}
					} else {
						if (nestSense.SensedEnemies.Count > 0) {
							DB.fight = true;
							Vector3 point2attack = DB.DSA.closest_machine.transform.position;//DS.SensedEnemies [0].transform.position;
							DB.DAT.transform.position = point2attack;

							DB.transform.LookAt (point2attack);
							DB.transform.position = point2attack + DB.attack.attackPoint * DB.DAT.averageDist;
							DB.DM.flying = DB.attack.land_closest;
						}else {
							DB.fight = false;
							DB.DM.flying = false;
							DB.transform.position = DB.DM.transform.position;
						}
					}
				}
			}
		}
		override protected void OnEnterState(){
			//print ("restowa wersja");
			DB.targetType = DragonTargetType.Sitpoint;
			nest2Flee = Random.Range (0, DragonManager.Instance.nests.Length);
			DB.SetNewTarget (DragonManager.Instance.nests [nest2Flee].position);
			nestSense = DragonManager.Instance.nests [nest2Flee].GetComponent<DragonSenses> ();
		}
	}
}
