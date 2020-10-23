using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour
{
    private float powerMultiplier; 
    public GameObject SmudgePrefab;
    private SmudgeRotation smudger;
    public float CurrentRotationSpeed{get;set;}
    private float maxVel = 18000f;
    // Start is called before the first frame update
    void Awake(){
    }
    public void Rotate(){
        transform.Rotate(0, 0, maxVel * CurrentRotationSpeed * powerMultiplier * Time.deltaTime);
        if(smudger)
            smudger.Smudge (CurrentRotationSpeed);
    }
	public void AddSmudger(Transform prop, bool left, int i){
		if (SmudgePrefab) {
			smudger = Instantiate (SmudgePrefab).GetComponent<SmudgeRotation>();
			smudger.transform.SetParent (prop);
			smudger.transform.localPosition = Vector3.zero;
			if (left)
				smudger.transform.localRotation = Quaternion.Euler(0f, 180f, 80f);
			else
				smudger.transform.localRotation = Quaternion.identity;
		}
	}
}
