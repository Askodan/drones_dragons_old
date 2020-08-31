using UnityEngine;
using System.Collections;

public class RobotSimpleSmoothHelper : MonoBehaviour {
	public float distz;
	public float height;
	public float maxDegreesY = 180f;
	public float maxDegreesX = 45f;
	float up;
	float left;
	void LateUpdate () {
		transform.position = transform.parent.position - Quaternion.Euler(0f, left*maxDegreesY, up * maxDegreesX)*Vector3.ProjectOnPlane(transform.forward, Vector3.up) * distz + Vector3.up * height;
	}
	public void Steer(float axis_CameraX, float axis_CameraY){
		up = axis_CameraY;
		left = axis_CameraX;
	}
}
