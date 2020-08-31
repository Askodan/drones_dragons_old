using UnityEngine;
using System.Collections;

public class LightsPanel : MonoBehaviour {
	public UnityEngine.UI.Image[] lights;
	bool[] states;
	public Color On;
	public Color Off;
	void Awake(){
		states = new bool[lights.Length];
	}
	//use always
	public void ChangeState(bool[] newStates){
		if (newStates.Length != states.Length) {
			Debug.LogError ("Number of set states "+newStates.Length+" differs from available states "+ states.Length);
		} else {
			for (int i = 0; i < newStates.Length; i++) {
				if (states != newStates) {
					if (newStates [i]) {
						lights [i].color = On;
					} else { 
						lights [i].color = Off;
					}
					states [i] = newStates [i];
				}
			}
		}
	}
	//use at start
	public void SetState(bool[] newStates){
		if (newStates.Length != states.Length) {
			Debug.LogError ("Number of set states differs from available states");
		} else {
			for (int i = 0; i < newStates.Length; i++) {
				if (newStates [i]) {
					lights [i].color = On;
				} else { 
					lights [i].color = Off;
				}
				states [i] = newStates [i];
			}
		}
	}
}
