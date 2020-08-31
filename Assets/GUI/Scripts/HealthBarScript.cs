using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class HealthBarScript : MonoBehaviour {
	public Transform bar;
	[HideInInspector]
	public Life life;
	void OnGUI(){
		if (life != null) {
			bar.localScale = new Vector3 (life.lifePoints / life.maxLife, 1f, 1f);
		}
	}
}
