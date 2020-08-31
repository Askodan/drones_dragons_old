using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Input2 {
	UnityLikeInput[] inputs;
	Dictionary<string, int> inputs_Dic;
	public Input2(UnityLikeInput[] Inputs){
		inputs_Dic = new Dictionary<string, int> ();
		inputs = Inputs;
		for (int i = 0; i < inputs.Length; i++) {
			inputs_Dic.Add (inputs [i].Name, i);
			inputs [i].Init ();
		}
	}
	public bool GetButtonDown(string name){
		int i;
		if (inputs_Dic.TryGetValue (name, out i)) {
			return inputs [i].GetDown ();
		} else {
			Debug.LogError ("There's no " + name + "!");
			return false;
		}
	}
	public bool GetButtonUp(string name){
		int i;
		if (inputs_Dic.TryGetValue (name, out i)) {
			return inputs [i].GetUp ();
		} else {
			Debug.LogError ("There's no " + name + "!");
			return false;
		}
	}
	public bool GetButton(string name){
		int i;
		if (inputs_Dic.TryGetValue (name, out i)) {
			return inputs [i].Get ();
		} else {
			Debug.LogError ("There's no " + name + "!");
			return false;
		}
	}
	public float GetAxis(string name){
		int i;
		if (inputs_Dic.TryGetValue (name, out i)) {
			return inputs [i].GetAxis ();
		} else {
			Debug.LogError ("There's no " + name + "!");
			return 0f;
		}
	}
}
