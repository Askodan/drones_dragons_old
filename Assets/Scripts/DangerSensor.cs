﻿using UnityEngine;
using System.Collections;

public class DangerSensor : MonoBehaviour {
	public LayerMask mask;
	void OnTriggerEnter(Collider entered){
		if (((1<<entered.gameObject.layer) & mask) != 0) {
			StageManager.Instance.DragonFlewIntoDangerZone (entered.transform);
		}
	}
	void OnTriggerExit(Collider exited){
		if (((1<<exited.gameObject.layer) & mask) != 0) {
			StageManager.Instance.DragonFlewOutOfDangerZone (exited.transform);
		}
	}
}
