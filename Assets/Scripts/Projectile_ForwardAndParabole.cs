using UnityEngine;
using System.Collections;
public enum Shapes{
	ray,
	sphere,
}
public class Projectile_ForwardAndParabole : MonoBehaviour {
	public DragonInterest shooter;
	public Shapes shape=Shapes.ray;
	[Tooltip("Only in sphere")]
	public float radius;
	public LayerMask mask;
	public float length;
	public float speed;
	public float maxLifeTime;
	public float fallSpeed;
	public float damage;
	public float minDamage;
	float myDamage;
	RaycastHit hit;
	void Start(){
	
	}
	// Update is called once per frame
	void Update () {
		myDamage -= (damage - minDamage) / maxLifeTime * Time.deltaTime;
		Debug.DrawRay (transform.position, length * transform.forward);

		bool smthHit=false;
		switch (shape) {
		case  Shapes.ray:
			smthHit = Physics.Raycast (transform.position, transform.forward, out hit, length, mask);
			break;
		case Shapes.sphere:
			smthHit = Physics.SphereCast (transform.position, radius, transform.forward, out hit, length, mask);
			break;
		}
		if (smthHit) {
			//Debug.Log("Trafiono "+ hit.collider.name);
			if (hit.collider.attachedRigidbody) {
				Life life = hit.collider.attachedRigidbody.GetComponent<Life> ();
				if (life) {
					life.Damage (myDamage);
					if (shooter) {
						shooter.Dragons [life.DSA.interestID].PumpInterest (life.DSA.damage2interest_factor * myDamage);
					}
				}
			}
			ObjectPool.Instance.PoolObject (gameObject);
		}
		Move ();
	}
	void Move(){
		transform.position += transform.forward * speed* Time.deltaTime;

		float angle = Vector3.Angle (Vector3.up, transform.forward);
		transform.RotateAround (transform.position, Vector3.Cross (transform.forward, Vector3.down), Mathf.Sin (angle*Mathf.Deg2Rad)*  Time.deltaTime * fallSpeed);
	}
	void OnEnable(){
		StartCoroutine (DieOnTime());
		myDamage = damage;
	}
	IEnumerator DieOnTime(){
		yield return new WaitForSeconds (maxLifeTime);
		ObjectPool.Instance.PoolObject (gameObject);
	}
}
