using UnityEngine;
using System.Collections;

public class LeanCamera : MonoBehaviour {
	public Transform target;
	public float dist;
	public bool alwaysBack;
	void Start(){
	}
	void Update () {
		if (alwaysBack) {
			Vector3 offset = target.forward;
			offset.y = 0;
			transform.position = target.position - offset * dist;
			transform.rotation = Quaternion.LookRotation (offset);
		} else {
			transform.position = target.position - Vector3.forward * dist;
			transform.rotation = Quaternion.identity;
		}
	}
}
