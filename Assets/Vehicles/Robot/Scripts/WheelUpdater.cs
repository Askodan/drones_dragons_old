using UnityEngine;
using System.Collections;

public class WheelUpdater : MonoBehaviour {
	//tester obliczania pozycji, zostanie przestawiony w docelową pozycję koła
	//public Transform tester;

	//czy to kolo napedowe
	public bool motorOn;
	public float force;
	//czy jest prawe, czy lewe
	public bool right;
	//czy jest przedie, czy tylne
	public bool front;

	HingeJoint joint;
	Transform parent;
	//GameObject axis;
	HingeJoint axis_joint;
	//Rigidbody axis_rigidbody;
	void Awake(){
		parent = transform.parent;
		Rigidbody wheelbody = GetComponent<Rigidbody> ();

		joint = GetComponent<HingeJoint> ();

		if (motorOn) {
			joint.useMotor = true;
			JointMotor mot = joint.motor;
			mot.freeSpin = false;
			mot.force = force;
			joint.motor = mot;
		}
		wheelbody.maxAngularVelocity = Mathf.Infinity;

		foreach(HingeJoint newJoint in transform.parent.GetComponentsInChildren <HingeJoint>()){
			if (newJoint != joint)
				axis_joint = newJoint;
		}
	}
	//implementacja na zwykłym koliderze
	public void  UpdateWheel(float motor, float steer){
		if (motorOn) {
			JointMotor mot = joint.motor;
			mot.targetVelocity = motor;
			joint.motor = mot;
		}
		axis_joint.connectedAnchor = axis_joint.connectedBody.transform.InverseTransformPoint(parent.position);

		JointLimits lim = axis_joint.limits;
		lim.min = -steer;
		lim.max = steer;
		axis_joint.limits = lim;
		JointMotor mot_axis = axis_joint.motor;
		mot_axis.targetVelocity = steer;
		axis_joint.motor = mot_axis;
	}

	/* implementacja na wheelcolliderach
	WheelCollider wheelCollider;
	public float wheelSteerSpeed=6f;
	// Use this for initialization
	void Start () {
		wheelCollider = GetComponent<WheelCollider> ();
	}
	public void  UpdateWheel(float motor, float steer){
		wheelCollider.motorTorque = motor;
		wheelCollider.steerAngle = steer;
		Vector3 pos;
		Quaternion rot;
		wheelCollider.GetWorldPose (out pos, out rot);
		wheel.position = pos;
		wheel.rotation = rot;
	}
	public void UpdateWheel(float motor, float steer, float breaking, Transform axis, Transform arm, float wheelLevel, bool left){
		wheelCollider.motorTorque = motor;
		wheelCollider.steerAngle = Mathf.Lerp(wheelCollider.steerAngle, steer, Time.deltaTime*wheelSteerSpeed);
		wheelCollider.brakeTorque = breaking;
		Vector3 pos;
		Quaternion rot;
		wheelCollider.GetWorldPose (out pos, out rot);
		float num = (arm.parent.InverseTransformVector(arm.position - pos)).z - wheelLevel;
		num /= wheelCollider.radius;
		float alfa = Mathf.Rad2Deg * Mathf.Asin ( Mathf.Clamp(num,-1f,1f));
		//sssalfa =-alfa; 
		if (!left) {
			arm.localRotation = Quaternion.Euler (-alfa,0f,0f);
			axis.localRotation = Quaternion.Euler (alfa, 0f, 0f);
		} else {
			arm.localRotation = Quaternion.Euler (-alfa,0f,180f);
			axis.localRotation = Quaternion.Euler (-alfa, 0f, 180f);
		}
		//arm.localRotation = Quaternion.Euler (alfa,0f,0f);

		wheel.position = pos;//+Vector3.right*Mathf.Cos(alfa*Mathf.Deg2Rad);
		wheel.rotation = rot;
	}*/
}
