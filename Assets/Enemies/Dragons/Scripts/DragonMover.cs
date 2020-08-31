using UnityEngine;
using System.Collections;

public class DragonMover : MonoBehaviour {
	//pierwsza proba na rigidbody
	bool Rotate2Target;
	public Transform target;
	public bool flying;//flying czy jest w trybie lotu

	//w osi z
	float speed_act;
	//some max speeds
	public FlyBehaviour flight;
	public GroundBehaviour ground;
	public GroundBehaviour2 ground2;
	public GroundBehaviour3 ground3;
	//public Speeds speedMaxRun;

	bool addForce;
	private Animator anim;
	private Rigidbody body;
	private Transform dummy;
	void Awake(){
		flight.DM = this;
		ground.DM = this;
		ground2.DM = this;
		ground3.DM = this;
		dummy = new GameObject ().transform;
	}
	void Start () {
		anim = GetComponentInChildren<Animator> ();
		anim.SetBool("Flap", true);
		body = GetComponent<Rigidbody> ();
		body.maxAngularVelocity = 100f;
		last_pos = transform.position;
	}
	// Update is called once per frame
	void FixedUpdate () {
		//speed_act = (Quaternion.Inverse(body.transform.rotation)*body.velocity).z;
		if (flying) {
			flight.FixedUpdate ();
		} else {
			ground3.FixedUpdate ();
		}
	}
	Vector3 last_pos;
	void LateUpdate(){
		speed_act = (Quaternion.Inverse(body.transform.rotation)*(last_pos-transform.position)/Time.deltaTime).z;
		last_pos = transform.position;
		if (flying) {
			flight.LateUpdate ();
		} else {
			ground3.LateUpdate ();
		}
	}
	//public void ForceOn(){
	//	addForce = true;
	//}
	//public void ForceOff(){
	//	addForce = false;
	//}

	[System.Serializable]
	public class FlyBehaviour{
		[HideInInspector]
		public DragonMover DM;
		//wings
		[SerializeField] Transform wings;
		public Vector3 wingsOffset;
		//animation stuff
		[SerializeField] AnimationCurve flapForceByAnimation;
		[SerializeField] float animationSpeedFactorHorizontal;
		[SerializeField] float animationSpeedFactorVertical;
		[SerializeField] float maxWingRot;

		//speeds and pids
		[SerializeField] AnimationCurve wingSpeedByHeight;
		[SerializeField] PIDController pidVertical;

		[SerializeField] AnimationCurve speed_profile;
		[SerializeField] PIDController pidHorizontalX;
		[SerializeField] PIDController pidHorizontalZ;

		[SerializeField] AnimationCurve RotSpeed_profile;
		[SerializeField] PIDController pidRotAngle;

		//drags
		[SerializeField] Vector2 minMaxDrag;
		[SerializeField] AnimationCurve AngularDrag_Angle;

		//slopes
		[SerializeField] float divingSlope;//angle at which it should dive
		[SerializeField] float rotatingSlope;//angle at which it should not rotate

		//speeds and square distances
		[SerializeField] float sqrDist2stopRot;
		[SerializeField] float sqrDist2stopDive;
		[SerializeField] float speed2Dive;
		[SerializeField] float speed2Glide;

		//force factors
		[SerializeField] float liftForceBySpeed;
		[Tooltip("How much of force when not swinging")]
		[SerializeField] float forceFactor;

		float diveAngle;
		Quaternion wingRotQuat;

		public void FixedUpdate(){
			if (!DM.Rotate2Target) {
				DM.body.AddRelativeTorque (rotate (DM.target.rotation), ForceMode.Acceleration);
			} else {
				DM.body.AddRelativeTorque (rotate (Quaternion.LookRotation (DM.target.position - DM.transform.position)), ForceMode.Acceleration);
			}
			if (DM.anim.GetBool ("Dive")) {
				float angle = Mathf.Atan2 (DM.transform.forward.y, (new Vector2 (DM.transform.forward.x, DM.transform.forward.z)).magnitude);
				DM.body.AddForce ((DM.transform.forward * Mathf.Sin (-angle) + DM.transform.up * Mathf.Cos (angle)) * 9.81f, ForceMode.Acceleration);
			} else {
				float Factor = forceFactor;
				AnimatorStateInfo asi = DM.anim.GetCurrentAnimatorStateInfo (0);
				//if (DM.addForce)
				//	Factor = 1f;
				if (asi.IsName ("Flap"))
					Factor = flapForceByAnimation.Evaluate (asi.normalizedTime % 1);
				else {
					Factor = 0f;
				}

				DM.Rotate2Target = (DM.target.transform.position - DM.transform.position).sqrMagnitude > sqrDist2stopRot && diveAngle<rotatingSlope;
				Vector3 m = (wingRotQuat * Vector3.up) * move (DM.target.transform.position);
				DM.body.AddForce (m * Factor, ForceMode.Acceleration);
				DM.body.AddForce(Vector3.up * liftForceBySpeed * DM.speed_act, ForceMode.Acceleration);
			}
		}

		public void LateUpdate(){
			DM.anim.SetFloat ("DragonVertical", Mathf.Clamp01 ((Mathf.Abs (DM.body.velocity.x) + Mathf.Abs (DM.body.velocity.z)) * animationSpeedFactorVertical));
			DM.anim.SetFloat ("DragonHorizontal", Mathf.Clamp (DM.body.angularVelocity.y * animationSpeedFactorHorizontal, -1f, 1f) + 1);

			if(DM.anim.GetBool ("Grounded"))
				DM.anim.SetBool ("Grounded", false);

			diveAngle = Mathf.Atan2 (DM.target.position.y - DM.transform.position.y, (new Vector2 (DM.target.position.x - DM.transform.position.x, DM.target.position.z - DM.transform.position.z)).magnitude);
			if (DM.Rotate2Target && divingSlope > diveAngle && DM.speed_act > speed2Dive && (DM.target.transform.position - DM.transform.position).sqrMagnitude > sqrDist2stopDive) {
				zeroWingRot (DM.transform.rotation);
				if (!DM.anim.GetBool ("Dive"))
					DM.anim.SetBool ("Dive", true);
			} else {
				if (0 > diveAngle && DM.anim.GetBool ("Flap") && DM.speed_act < speed2Glide) {
					DM.anim.SetBool ("Flap", false);
				} else {
					if (!DM.anim.GetBool ("Flap")) {
						DM.anim.SetBool ("Flap", true);
					}
				}
				if (DM.anim.GetBool ("Dive"))
					DM.anim.SetBool ("Dive", false);
			}

			wings.rotation = wingRotQuat * Quaternion.Euler (wingsOffset);
		}
		public void zeroWingRot(Quaternion rot){
			wingRotQuat = rot;
   		}
		float move(Vector3 targetPoint){
			float dist_v = targetPoint.y - DM.body.transform.position.y;
			float dir = Mathf.Atan2 (DM.body.transform.forward.x, DM.body.transform.forward.z);
			Vector2 dist_h = new Vector2 (targetPoint.x - DM.body.transform.position.x, targetPoint.z - DM.body.transform.position.z);
			float dist_z = -Mathf.Sin (dir)*dist_h.y + Mathf.Cos (dir)*dist_h.x;
			float dist_x = Mathf.Cos (dir)*dist_h.y + Mathf.Sin (dir)*dist_h.x;
			float force;
			DM.body.drag = Mathf.Clamp(Mathf.Abs(dist_v/dist_x), minMaxDrag.x, minMaxDrag.y);
			if (dist_v < 0){
				float num = dist_x / dist_v;
				force = wingSpeedByHeight.Evaluate (float.IsNaN(num)?0:num);
			}else
				force = wingSpeedByHeight.Evaluate (float.IsNaN(dist_v)?0:dist_v);
			DM.anim.SetFloat("FlapSpeed", force);
			Vector3 speeds = new Vector3 (Mathf.Sign(dist_x)*speed_profile.Evaluate(Mathf.Abs(dist_x)), Mathf.Sign(dist_v)*speed_profile.Evaluate(Mathf.Abs(dist_v)), Mathf.Sign(dist_z)*speed_profile.Evaluate(Mathf.Abs(dist_z)));
			Vector3 velo = Quaternion.Inverse (Quaternion.LookRotation(new Vector3(DM.body.transform.forward.x, 0, DM.body.transform.forward.z)))* DM.body.velocity;

			Vector3 wingRot = new Vector3(pidHorizontalX.Regulate(speeds.x-velo.z), 0, pidHorizontalZ.Regulate(velo.x - speeds.z));
			if (float.IsNaN (wingRot.x) || float.IsNaN (wingRot.y) || float.IsNaN (wingRot.z)) {
				wingRot = Vector3.zero;
			}

			wingRotQuat = (Quaternion.Euler (0f, DM.body.transform.rotation.eulerAngles.y, 0f) * Quaternion.Euler (wingRot));
			//clap wing rotation
			wingRotQuat = Quaternion.RotateTowards(DM.body.transform.rotation, wingRotQuat, maxWingRot);

			return Mathf.Clamp (pidVertical.Regulate (speeds.y-DM.body.velocity.y), 0f, pidVertical.overallAmplify) * force;
		}

		Vector3 rotate(Quaternion targetRot){
			
			Quaternion tar = targetRot;
			Quaternion act = DM.body.transform.rotation;

			Quaternion diff_tar;
			Vector3 axis_tar;
			float angle_tar;
			GiveAxisAngle (act, tar, out diff_tar, out angle_tar, out axis_tar);
			float targetSpeed_tar = Mathf.Sign(angle_tar)*RotSpeed_profile.Evaluate (Mathf.Abs(angle_tar));
			if (float.IsInfinity (axis_tar.x))
				axis_tar = Vector3.right;
			DM.body.angularDrag = AngularDrag_Angle.Evaluate (angle_tar);
			return axis_tar * pidRotAngle.Regulate (targetSpeed_tar);
		}

	}
	static void GiveAxisAngle(Quaternion act, Quaternion tar, out Quaternion diff, out float angle, out Vector3 axis){
		diff = Quaternion.Inverse (act) * tar;
		diff.ToAngleAxis (out angle, out axis);
		if (angle > 180)
			angle -= 360;
	}
	[System.Serializable]
	public class GroundBehaviour{
		[HideInInspector]
		public DragonMover DM;
		public int GroundedState;
		[SerializeField] float groundForce;
		[SerializeField] AnimationCurve Speed_profile;
		[SerializeField] PIDController pidRunSpeed;
		[SerializeField] AnimationCurve Drag;

		[SerializeField] DragonGroundSensor sensorHead;
		[SerializeField] DragonGroundSensor sensorLeft;
		[SerializeField] DragonGroundSensor sensorRight;

		[SerializeField] AnimationCurve RotSpeed_profile;
		[SerializeField] PIDController pidRotAngle;
		[SerializeField] AnimationCurve AngularDrag_Angle;
		[SerializeField] float sqrDist2stopRot;
		[SerializeField] float angle2Crawl;
		[SerializeField] float CrawlAnimationSpeedMultiplayer;
		[SerializeField] float RunAnimationSpeedMultiplayer;
		Vector3 up;
		public void FixedUpdate(){
			if (!DM.Rotate2Target) {
				DM.body.AddRelativeTorque (rotate (DM.target.rotation), ForceMode.Acceleration);
			} else {
				up = CalculateUp ();
				Vector3 forward = (DM.target.position - DM.transform.position);
				Vector3.OrthoNormalize (ref up, ref forward);
				DM.body.AddRelativeTorque (rotate (Quaternion.LookRotation (forward, up)), ForceMode.Acceleration);
			}
			DM.Rotate2Target = (DM.target.transform.position - DM.transform.position).sqrMagnitude > sqrDist2stopRot;
			DM.body.AddRelativeForce (Vector3.down * groundForce+ move(DM.target.position), ForceMode.Acceleration);
			//DM.body.AddRelativeForce (Vector3.forward*groundForce, ForceMode.Acceleration);
		}
		public void LateUpdate(){
			if(DM.anim.GetInteger("GroundedState")!= GroundedState)
				DM.anim.SetInteger ("GroundedState", GroundedState);
			if(!DM.anim.GetBool ("Grounded"))
				DM.anim.SetBool ("Grounded", true);

			if (dist2target * dist2target < sqrDist2stopRot) {
				GroundedState = 0;
				DM.Rotate2Target = false;
			} else {
				DM.Rotate2Target = true;
				float angle = Vector3.Angle(Vector3.up, up);
				//Debug.Log (angle);
				//Debug.DrawRay (DM.transform.position, 10*up);
				if (angle>angle2Crawl) {
					DM.anim.SetFloat("FlapSpeed", DM.speed_act*CrawlAnimationSpeedMultiplayer);
					GroundedState = 1;
				} else {
					DM.anim.SetFloat("FlapSpeed", DM.speed_act*RunAnimationSpeedMultiplayer);
					GroundedState = 2;
				}
			}
		}
		Vector3 CalculateUp(){
			Vector3 headPoint = sensorHead.Sense (-DM.transform.up);
			Vector3 leftPoint = sensorLeft.Sense (-DM.transform.up);
			Vector3 rightPoint = sensorRight.Sense (-DM.transform.up);
			if (!float.IsNaN (headPoint.x) && !float.IsNaN (leftPoint.x) && !float.IsNaN (rightPoint.x)) {
				return Vector3.Cross (headPoint - leftPoint, rightPoint - leftPoint);
			}else {
				//return DM.transform.up;
				return Vector3.up;
			}
		}
		float dist2target;
		Vector3 move(Vector3 targetPoint){

			float dir = Mathf.Atan2 (DM.body.transform.forward.x, DM.body.transform.forward.z);
			Vector2 dist_h = new Vector2 (targetPoint.x - DM.body.transform.position.x, targetPoint.z - DM.body.transform.position.z);
			float dist_z = -Mathf.Sin (dir)*dist_h.y + Mathf.Cos (dir)*dist_h.x;
			float dist_x = Mathf.Cos (dir)*dist_h.y + Mathf.Sin (dir)*dist_h.x;
			dist2target = dist_x;
			DM.body.drag = Drag.Evaluate(dist_z);
			float force = pidRunSpeed.Regulate(Speed_profile.Evaluate(dist_x)-DM.speed_act);
			return Vector3.forward * force;
		}
		Vector3 rotate(Quaternion targetRot){
			Quaternion tar = targetRot;
			Quaternion act = DM.body.transform.rotation;

			Quaternion diff_tar;
			Vector3 axis_tar;
			float angle_tar;
			GiveAxisAngle (act, tar, out diff_tar, out angle_tar, out axis_tar);
			float targetSpeed_tar = Mathf.Sign(angle_tar)*RotSpeed_profile.Evaluate (Mathf.Abs(angle_tar));
			if (float.IsInfinity (axis_tar.x))
				axis_tar = Vector3.right;
			DM.body.angularDrag = AngularDrag_Angle.Evaluate (angle_tar);
			return axis_tar * pidRotAngle.Regulate (targetSpeed_tar);
		}
	}
	[System.Serializable]
	public class GroundBehaviour2{
		[HideInInspector]
		public DragonMover DM;
		public int GroundedState;
		[SerializeField] float groundForce;
		[SerializeField] AnimationCurve Speed_profile;
		[SerializeField] AnimationCurve RotSpeed_profile;

		[SerializeField] DragonGroundSensor sensorHead;
		[SerializeField] DragonGroundSensor sensorLeft;
		[SerializeField] DragonGroundSensor sensorRight;

		[SerializeField] float sqrDist2stopRot;
		[SerializeField] float angle2Crawl;
		[SerializeField] float force_m;
		[SerializeField] float torque_m;
		[SerializeField] float CrawlAnimationSpeedMultiplayer;
		[SerializeField] float RunAnimationSpeedMultiplayer;
		Vector3 up;
		public void FixedUpdate(){
			if (!DM.Rotate2Target) {
				DM.body.AddRelativeTorque (rotate (DM.target.rotation), ForceMode.VelocityChange);
			} else {
				up = CalculateUp ();
				Vector3 forward = (DM.target.position - DM.transform.position);
				Vector3.OrthoNormalize (ref up, ref forward);
				DM.body.AddRelativeTorque (rotate (Quaternion.LookRotation (forward, up)), ForceMode.VelocityChange);
			}
			DM.Rotate2Target = (DM.target.transform.position - DM.transform.position).sqrMagnitude > sqrDist2stopRot;
			DM.body.AddRelativeForce (Vector3.down * groundForce+ move(DM.target.position), ForceMode.VelocityChange);
			//DM.body.AddRelativeForce (Vector3.forward*groundForce, ForceMode.Acceleration);
		}
		public void LateUpdate(){
				
			if(DM.anim.GetInteger("GroundedState")!= GroundedState)
				DM.anim.SetInteger ("GroundedState", GroundedState);
			if(!DM.anim.GetBool ("Grounded"))
				DM.anim.SetBool ("Grounded", true);

			if (dist2target * dist2target < sqrDist2stopRot) {
				GroundedState = 0;
				DM.Rotate2Target = false;
			} else {
				DM.Rotate2Target = true;
				float angle = Vector3.Angle(Vector3.up, up);
				//Debug.Log (angle);
				//Debug.DrawRay (DM.transform.position, 10*up);
				if (angle>angle2Crawl) {
					DM.anim.SetFloat("FlapSpeed", DM.speed_act*CrawlAnimationSpeedMultiplayer);
					GroundedState = 1;
				} else {
					DM.anim.SetFloat("FlapSpeed", DM.speed_act*RunAnimationSpeedMultiplayer);
					GroundedState = 2;
				}
			}
		}
		Vector3 CalculateUp(){
			Vector3 headPoint = sensorHead.Sense (-DM.transform.up);
			Vector3 leftPoint = sensorLeft.Sense (-DM.transform.up);
			Vector3 rightPoint = sensorRight.Sense (-DM.transform.up);
			if (!float.IsNaN (headPoint.x) && !float.IsNaN (leftPoint.x) && !float.IsNaN (rightPoint.x)) {
				return Vector3.Cross (headPoint - leftPoint, rightPoint - leftPoint);
			}else {
				//return DM.transform.up;
				return Vector3.up;
			}
		}
		float dist2target;
		Vector3 move(Vector3 targetPoint){
			float dir = Mathf.Atan2 (DM.body.transform.forward.x, DM.body.transform.forward.z);
			Vector2 dist_h = new Vector2 (targetPoint.x - DM.body.transform.position.x, targetPoint.z - DM.body.transform.position.z);
			//float dist_z = -Mathf.Sin (dir)*dist_h.y + Mathf.Cos (dir)*dist_h.x; unnused
			float dist_x = Mathf.Cos (dir)*dist_h.y + Mathf.Sin (dir)*dist_h.x;
			dist2target = dist_x;
			float force = Speed_profile.Evaluate(dist_x);
			return Vector3.forward * force*Time.deltaTime*force_m;
		}
		Vector3 rotate(Quaternion targetRot){
			Quaternion tar = targetRot;
			Quaternion act = DM.body.transform.rotation;

			Quaternion diff_tar;
			Vector3 axis_tar;
			float angle_tar;
			GiveAxisAngle (act, tar, out diff_tar, out angle_tar, out axis_tar);
			float targetSpeed_tar = Mathf.Sign(angle_tar)*RotSpeed_profile.Evaluate (Mathf.Abs(angle_tar));
			if (float.IsInfinity (axis_tar.x))
				axis_tar = Vector3.right;
			
			return axis_tar * targetSpeed_tar*Time.deltaTime*torque_m;
		}
	}
	[System.Serializable]
	public class GroundBehaviour3{
		[HideInInspector]
		public DragonMover DM;
		[HideInInspector] public int GroundedState;
		bool grounded;
		//[SerializeField] float groundForce;
		[SerializeField] float SpeedRun_multiplier;
		[SerializeField] float SpeedCrawl_multiplayer;
		[SerializeField] float SpeedCorrection_multiplayer;
		[SerializeField] AnimationCurve Speed_profile;

		[SerializeField] float RotSpeed_multiplier;
		[SerializeField] AnimationCurve RotSpeed_profile;

		[SerializeField] DragonGroundSensor sensorHead;
		[SerializeField] DragonGroundSensor sensorLeft;
		[SerializeField] DragonGroundSensor sensorRight;

		[SerializeField] float sqrDist2stopRot;
		[SerializeField] float angle2Crawl;
		[SerializeField] float CrawlAnimationSpeedMultiplayer;
		[SerializeField] float RunAnimationSpeedMultiplayer;
		Vector3 up;
		float dist2target;
		bool rot2target;

		public void FixedUpdate(){
			//ważne obliczenia do lateUpdate
			Vector3 target_pos_zero = DM.target.position, body_pos_zero=DM.body.transform.position, axis_tar;
			target_pos_zero.y = 0;body_pos_zero.y = 0;
			Quaternion target_rotation = Quaternion.identity;
			dist2target = (body_pos_zero-target_pos_zero).sqrMagnitude;
			up = CalculateUp ();;

			//rotacja
			if (!DM.Rotate2Target) {
				Vector3 tempfor = DM.target.forward;
				Vector3.OrthoNormalize (ref up, ref tempfor);
				target_rotation = Quaternion.LookRotation (tempfor, up);
			} else {
				Vector3 tempfor = target_pos_zero - body_pos_zero;
				Vector3.OrthoNormalize (ref up, ref tempfor);
				target_rotation = Quaternion.LookRotation (tempfor, up);
			}
			float angle_tar;
			target_rotation.ToAngleAxis (out angle_tar,out axis_tar);
			DM.body.MoveRotation (Quaternion.RotateTowards(DM.body.transform.rotation, target_rotation, RotSpeed_multiplier*RotSpeed_profile.Evaluate(angle_tar)*Time.deltaTime));


			float speed;
			Vector3 direction;
			float Speed_multiplier = SpeedCorrection_multiplayer;
			if (grounded) {
				switch (GroundedState) {
				case 0:
					Speed_multiplier = SpeedCorrection_multiplayer;
					break;
				case 1:
					Speed_multiplier = SpeedCrawl_multiplayer;
					break;
				case 2:
					Speed_multiplier = SpeedRun_multiplier;
					break;
				}
			}
			if (!DM.Rotate2Target) {
				speed = Speed_multiplier * Speed_profile.Evaluate (dist2target);
				direction = target_pos_zero-body_pos_zero;//DM.target.position - DM.body.transform.position;
				Vector3.OrthoNormalize (ref up, ref direction);
			} else {
				//translacja
				float dist;
				DM.dummy.position = body_pos_zero;
				Vector3 forward_zero = DM.body.transform.forward;
				forward_zero.y = 0;
				DM.dummy.rotation = Quaternion.LookRotation (forward_zero);
				dist = DM.dummy.InverseTransformPoint (target_pos_zero).z;
				speed = Speed_multiplier * Speed_profile.Evaluate (dist);
				direction = DM.body.transform.forward;
			}
			DM.body.MovePosition (DM.body.transform.position + direction  * speed * Time.deltaTime);
			//DM.body.AddForce (Vector3.down*groundForce, ForceMode.Acceleration);
			/* w dupie z tym wszystkim
			//rotacja
			Quaternion actual_rotation = DM.transform.rotation;
			Quaternion target_rotation = DM.target.rotation;
			Quaternion diff_tar;
			Vector3 axis_tar;
			float angle_tar;
			if (!DM.Rotate2Target) {
				GiveAxisAngle (actual_rotation, target_rotation, out diff_tar, out angle_tar, out axis_tar);
			} else {
				up = CalculateUp ();
				Vector3 forward = (DM.target.position - DM.transform.position);
				Vector3.OrthoNormalize (ref up, ref forward);

				target_rotation = Quaternion.LookRotation (forward, up);
				GiveAxisAngle (actual_rotation, target_rotation, out diff_tar, out angle_tar, out axis_tar);
			}
			DM.body.MoveRotation (Quaternion.RotateTowards(DM.body.transform.rotation, target_rotation, RotSpeed_multiplier*RotSpeed_profile.Evaluate(angle_tar)*Time.deltaTime));
			//translacja
			float dir = Mathf.Atan2 (DM.body.transform.forward.x, DM.body.transform.forward.z);
			Vector2 dist_h = new Vector2 (DM.target.position.x - DM.body.transform.position.x, DM.target.position.z - DM.body.transform.position.z);
			float dist_z = -Mathf.Sin (dir)*dist_h.y + Mathf.Cos (dir)*dist_h.x;
			float dist_x = Mathf.Cos (dir)*dist_h.y + Mathf.Sin (dir)*dist_h.x;
			Vector3 dist = DM.body.transform.position - DM.target.position;
			dist.y = 0;
			dist2target = (dist).sqrMagnitude;
			DM.body.MovePosition (DM.body.transform.position + DM.body.transform.forward*Speed_multiplier*Speed_profile.Evaluate (dist_x) * Time.deltaTime);
			ghost.position = DM.body.transform.position + DM.body.transform.forward * Speed_multiplier * Speed_profile.Evaluate (dist_x) * Time.deltaTime;
			ghost.rotation = Quaternion.RotateTowards (DM.body.transform.rotation, target_rotation, RotSpeed_multiplier * RotSpeed_profile.Evaluate (angle_tar) * Time.deltaTime);
			//DM.body.AddRelativeForce (Vector3.forward*groundForce, ForceMode.Acceleration);*/
		}
		public void LateUpdate(){

			if(DM.anim.GetInteger("GroundedState")!= GroundedState)
				DM.anim.SetInteger ("GroundedState", GroundedState);
			if(!DM.anim.GetBool ("Grounded"))
				DM.anim.SetBool ("Grounded", true);

			if (dist2target < sqrDist2stopRot) {
				GroundedState = 0;
				DM.Rotate2Target = false;
			} else {
				DM.Rotate2Target = true;
				float angle = Vector3.Angle(Vector3.up, up);
				if (angle>angle2Crawl) {
					DM.anim.SetFloat("FlapSpeed", DM.speed_act*CrawlAnimationSpeedMultiplayer);
					GroundedState = 1;
				} else {
					DM.anim.SetFloat("FlapSpeed", DM.speed_act*RunAnimationSpeedMultiplayer);
					GroundedState = 2;
				}
			}
		}
		Vector3 CalculateUp(){
			Vector3 headPoint = sensorHead.Sense (-DM.transform.up);
			Vector3 leftPoint = sensorLeft.Sense (-DM.transform.up);
			Vector3 rightPoint = sensorRight.Sense (-DM.transform.up);
			if (!float.IsNaN (headPoint.x) && !float.IsNaN (leftPoint.x) && !float.IsNaN (rightPoint.x)) {
				grounded = true;
				return Vector3.Cross (headPoint - leftPoint, rightPoint - leftPoint);
			}else {
				grounded = false;
				return Vector3.up;
			}
		}
	}
}