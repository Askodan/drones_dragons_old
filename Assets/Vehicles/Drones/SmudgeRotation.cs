using UnityEngine;
using System.Collections;

public class SmudgeRotation : MonoBehaviour {
	Renderer[] Renderers;
	public float effectMultiplier=1f;
	public float maxTintValue=0.75f;
	void Start(){
		Renderers = GetComponentsInChildren<Renderer> ();
	}
	public void Smudge(float speed){
		float degree = Mathf.Clamp(Mathf.Abs(speed)*effectMultiplier, 0f, maxTintValue);
		for (int i = 0; i < Renderers.Length; i++) {
			Renderers [i].material.SetColor ("_TintColor", new Color (degree, degree, degree, degree));
		}
	}
}
