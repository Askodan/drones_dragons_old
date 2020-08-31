using UnityEngine;
using System.Collections;

public class DigitsDisplayer : MonoBehaviour {
	[HideInInspector] public float value;
	void Update(){
		Light (value);
	}
	int digitsNumber = 3;
	Transform[,] digits_Lights;
	Transform panel;
	static bool[][] digits;
	void Awake(){
		digits = new bool[10][];
		digits [0] = new bool[]{true, true, true, false, true, true, true};
		digits [1] = new bool[]{false, false, true, false, false, false, true};
		digits [2] = new bool[]{false, true, true, true, true, true, false};
		digits [3] = new bool[]{false, true, true, true, false, true, true};
		digits [4] = new bool[]{true, false, true, true, false, false, true};
		digits [5] = new bool[]{true, true, false, true, false, true, true};
		digits [6] = new bool[]{true, true, false, true, true, true, true};
		digits [7] = new bool[]{false, true, true, false, false, false, true};
		digits [8] = new bool[]{true, true, true, true, true, true, true};
		digits [9] = new bool[]{true, true, true, true, false, true, true};
		digits_Lights = new Transform[digitsNumber, 7];
		Transform[] all_transforms = GetComponentsInChildren<Transform> ();
		foreach (Transform current in all_transforms) {
			if (current.name == "Panel") {
				panel = current;
			} else {
				string[] current_tab = current.name.Split("_".ToCharArray(), 2);
				if (current_tab.Length == 2) {
					digits_Lights[int.Parse(current_tab[0])-1, int.Parse(current_tab[1])-1] = current;
				}
			}
		}

	}
	//public 
	void Light(float speed){
		int[] numbers = to3Digits (speed);
		panel.SetAsLastSibling ();
		for (int i0 = 0; i0 < digits_Lights.GetLength (0); i0++) {
			for (int i1 = 0; i1 < digits_Lights.GetLength (1); i1++) {
				if (digits [numbers [i0]] [i1]) {
					digits_Lights [digitsNumber-1-i0, i1].SetAsLastSibling ();
				}
			}	
		}
	}
	int[] to3Digits(float value){
		int value_i = Mathf.RoundToInt (value);
		string value_s = value_i.ToString ();
		int[] result = new int[value_s.Length];
		for (int i = 0; i < value_s.Length; i++) {
			result [i] = int.Parse (value_s [i].ToString());
		}

		if (result.Length > digitsNumber) {
			Debug.LogWarning ("Za dużo cyfr dla wyświetlacza");
		} else if (result.Length < digitsNumber) {
			int[] new_result = new int[digitsNumber];
			for (int i = 0; i < result.Length; i++) {
				new_result [i + digitsNumber - result.Length] = result [i];
			}
			result = new_result;
		}
		return result;
	}
}
