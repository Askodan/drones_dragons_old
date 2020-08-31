using UnityEngine;
using System.Collections;

public class CopyTransform : MonoBehaviour {
	public Transform toCopy;
	public bool pos = true;
	public bool rot = true;
	public bool scale = true;
	private bool always;
	void Start(){
		always = !(pos && scale && rot);
		if (!always) {
			if (pos)
				transform.position = toCopy.position;
			if (rot) {
				transform.rotation = toCopy.rotation;
			}
			if (scale) {
				transform.localScale = toCopy.localScale;
			}
			transform.SetParent (toCopy);
		}
	}
	void Update () {
		if (always) {
			if (pos)
				transform.position = toCopy.position;
			if (rot) {
				transform.rotation = toCopy.rotation;
			}
			if (scale) {
				transform.localScale = toCopy.localScale;
			}
		}
	}
}
