using UnityEngine;
using System.Collections;

public class MiniMapObject : MonoBehaviour {
	static public Transform universalParent;
	public GameObject symbol;
	public bool rot;
	public bool terrain;
	Vector3 scale;
	// Use this for initialization
	void Awake () {
		symbol = Instantiate (symbol);
		scale = Vector3.one;
		if (symbol.transform.localScale != Vector3.one) {
			Debug.LogWarning ("Minimap symbol's scale wasn't (1, 1, 1), but "+symbol.transform.localScale+" "+symbol.name);
		}
	}
	void Start(){
		symbol.transform.parent = universalParent;
	}
	// Update is called once per frame
	void LateUpdate () {
		symbol.transform.position = transform.position;
		if (rot) {
			symbol.transform.rotation = transform.rotation;//*Quaternion.Euler(90f, 0f, 0f);
			//symbol.transform.localRotation =  Quaternion.Euler(90f, transform.localRotation.eulerAngles.y, 0f);
		}
	}
	public void ChangeSize(float size){
		if (terrain) {
			return;
		}
		symbol.transform.localScale = scale * size;
	}
}
