using UnityEngine;
using System.Collections;

public class GUIDrone : MonoBehaviour {
	//[HideInInspector] 
	public AltitudeMeter altmeter;
	public DigitsDisplayer altDisplayer;
	public LightsPanel lightPanel;
    public RotateInfo rotateInfo;
	void OnGUI(){
		altDisplayer.value = altmeter.height;
	}
}
