using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierPathFinding : MonoBehaviour {
	//kiedyś ukryj
	public BezierCurve curve;
	public Transform target;
	//public Transform prefab;
	public float sizeOfTest;
	// Use this for initialization
	void Start () {
		curve = gameObject.AddComponent<BezierCurve> ();
		curve.AddPointAt (transform.position);
		curve.AddPointAt (target.position);
	}

	public IEnumerator check(){
		curve.SetDirty ();
		int numberOfTests = (int)(curve.length / sizeOfTest);
		for (int i = 0; i < numberOfTests; i++) {
			if (Physics.OverlapSphere (curve.GetPointAt ((float)i / (float)numberOfTests), sizeOfTest).Length>0) {
				Vector3 midPos = curve.GetPointAt ((float)i / (float)numberOfTests) + Vector3.up * (sizeOfTest);
				while(Physics.OverlapSphere (midPos, sizeOfTest).Length>0){
					midPos += Vector3.up * (sizeOfTest);
				}
				curve.AddPointAt (midPos);
				BezierPoint[] points = curve.GetAnchorPoints();
				points [points.Length - 2].position = points [points.Length-1].position;
				points [points.Length - 1].position = target.position;
				curve.SetDirty ();
				//Transform prf = Instantiate (prefab, midPos, Quaternion.identity, transform);
				//prf.localScale = Vector3.one * sizeOfTest;
				yield return null;
			}
		}
	}
}
