using UnityEngine;
using System.Collections;

public class SimpleSmooth : MonoBehaviour {
	public float speed;
	public float rotSpeed;
	public Transform target;
	public Transform lookTarget;
	// Update is called once per frame
	void LateUpdate () {
		transform.position = Vector3.Slerp (transform.position, target.position, speed * Time.deltaTime);
		Quaternion targetRotation = Quaternion.LookRotation(lookTarget.position-transform.position);
		transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
	}
}
