using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonGroundSensor : MonoBehaviour {
	public float range;
	public LayerMask mask;
	public Vector3 Sense(Vector3 direction){
		Vector3 result;
		RaycastHit hit;
		if (Physics.Raycast (transform.position, direction, out hit, range, mask)) {
			result = hit.point;
		} else {
			result = new Vector3(float.NaN, float.NaN, float.NaN);
		}
		return result;
	}
}
