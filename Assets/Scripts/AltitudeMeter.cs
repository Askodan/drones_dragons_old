using UnityEngine;
using System.Collections;

public class AltitudeMeter : MonoBehaviour {
	public LayerMask mask;
	[HideInInspector]
	public float height;
	public float offset;
	void OnGUI(){
		RaycastHit hit;
		if (Physics.Raycast (transform.position-transform.up*offset, Vector3.down, out hit, 1000f, mask)) {
			height = hit.distance;
		}
	}
}
