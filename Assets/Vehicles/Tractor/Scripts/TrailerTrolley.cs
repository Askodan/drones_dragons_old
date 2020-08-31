using UnityEngine;
using System.Collections;

public class TrailerTrolley : MonoBehaviour {
	public Transform Floor;
	public Transform SideLeft;
//	public Transform AxisLeft;
	public Transform SideRight;
//	public Transform AxisRight;
	public float speedChange;
	public float state;
	public AnimationCurve SideRot;

	public Vector3 pos_Right= new Vector3(-0.3384521f, 0f, 0.883354f);
	Quaternion rot_Right = Quaternion.Euler(0f, 70.283f, 0f);
	public Vector3 pos_Left= new Vector3(0.3384521f, 0f, 0.883354f);
	Quaternion rot_Left = Quaternion.Euler(0f, -70.283f, 0f);

	float maxState=1.3f;
	public void Steer(float Axis_Trolley){
		if (Axis_Trolley != 0) {
			state += Axis_Trolley*speedChange*Time.deltaTime;
			state = Mathf.Clamp (state, -maxState, maxState); 
		} else {
			state = Mathf.Lerp(state, 0, speedChange*Time.deltaTime);
		}
		UpdateState (state);
	}

	void UpdateState(float newState){
		if (newState > 0) {
			Floor.localPosition = Vector3.Lerp(Vector3.zero, pos_Right, newState);
			Floor.localRotation = Quaternion.Lerp(Quaternion.identity, rot_Right, newState);
			SideLeft.localRotation = Quaternion.Euler(0f, SideRot.Evaluate(state), 0f);
			SideRight.localRotation = Quaternion.identity;
		} else if (newState < 0) {
			Floor.localPosition = Vector3.Lerp(Vector3.zero, pos_Left, -newState);
			Floor.localRotation = Quaternion.Lerp(Quaternion.identity, rot_Left, -newState);
			SideRight.localRotation = Quaternion.Euler(0f, -SideRot.Evaluate(-newState), 0f);
			SideLeft.localRotation = Quaternion.identity;
		} else {
			Floor.localPosition = Vector3.zero;
			Floor.localRotation = Quaternion.identity;
			SideLeft.localRotation = Quaternion.identity;
			SideRight.localRotation = Quaternion.identity;
		}
	}
}
