using UnityEngine;
using System.Collections;

public class DragonBrainRotator : MonoBehaviour {
	public Transform body;
	public float speed;
	public bool rotate;
	// Use this for initialization
	Rigidbody rigid;
	void Start () {
		rigid = body.GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		//zabawa z targetem
		if (rotate)
		transform.rotation = Quaternion.LookRotation(new Vector3(transform.position.x-body.transform.position.x, 0 , transform.position.z-body.transform.position.z));
		speed = rigid.velocity.magnitude*3.6f;
	}
}
