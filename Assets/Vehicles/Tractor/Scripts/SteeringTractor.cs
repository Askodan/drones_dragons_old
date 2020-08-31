using UnityEngine;
using System.Collections;

public class SteeringTractor : MonoBehaviour {
	public Transform CenterOfMass;
	public Rigidbody[] kolat;
	public HingeJoint[] breaksInTrail;
	public Rigidbody[] kolap;
	public Rigidbody os;
	public float power;
	public float curve;
	public int gear;
	public float[] gears;
	HingeJoint[] joints;
	void Awake () {
		GetComponent<Rigidbody> ().centerOfMass = CenterOfMass.localPosition;
		joints = new HingeJoint[kolat.Length];
		for (int i = 0; i < kolat.Length; i++) {
			joints[i] = kolat [i].GetComponent<HingeJoint> ();
		}
		getBreaksReady (joints);
		getBreaksReady (breaksInTrail);
	}
	void getBreaksReady(HingeJoint[] joint){
		for (int i = 0; i < joint.Length; i++) {
			JointMotor mot= joint [i].motor;
			mot.force =100* power;
			mot.targetVelocity = 0;
			joint [i].motor = mot;
		}
	}
	public float throttleLerpSpeed;
	public float MotorSpeed;
	void UseMotors(HingeJoint[] joint, float throttle){
		MotorSpeed = Mathf.Lerp (MotorSpeed, throttle, Time.deltaTime/ (gear+1));
		MotorSpeed = Mathf.Lerp (MotorSpeed, joint[0].velocity/gears[gear], throttleLerpSpeed* Time.deltaTime);
		for (int i = 0; i < joint.Length; i++) {
			joints [i].useMotor = true;
			JointMotor mot= joint [i].motor;
			mot.force = Mathf.Abs(MotorSpeed*power/gears[gear]);
			mot.targetVelocity = MotorSpeed*gears[gear];
			joint [i].motor = mot;
		}
	}

	bool breaking = false;
	public void Steer(bool butDown_Break, bool butUp_Break, float axis_Vertical, float axis_Horizontal, bool butDown_GearDown, bool butDown_GearUp){
		if (butDown_Break) {				
			breaking = true;
			getBreaksReady (joints);
			for (int i = 0; i < breaksInTrail.Length; i++) {
				breaksInTrail [i].useMotor = true;
			}
		}
		if (butUp_Break) {
			breaking = false;
			for (int i = 0; i < breaksInTrail.Length; i++) {
				breaksInTrail [i].useMotor = false;
			}
		}
		if(!breaking){
			UseMotors (joints, axis_Vertical);
		}
		if (butDown_GearDown) {
			gear--;
			gear = Mathf.Clamp (gear, 0, gears.Length - 1);
		}
		if (butDown_GearUp) {
			gear++;
			gear = Mathf.Clamp (gear, 0, gears.Length - 1);
		}
		for (int i = 0; i < kolap.Length; i++) {
			kolap [i].MoveRotation (os.rotation * Quaternion.Euler (0f, axis_Horizontal * curve, 0f));
		}
	}
	void OnDisable(){
		for (int i = 0; i < kolap.Length; i++) {
			kolap [i].GetComponent<HingeJoint>().useLimits = true;
			kolap [i].GetComponent<HingeJoint>().limits = new JointLimits();
		}
	}
	void OnEnable(){
		for (int i = 0; i < kolap.Length; i++) {
			kolap [i].GetComponent<HingeJoint>().useLimits = false;
		}
	}
}
