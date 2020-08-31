using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonAttackTarget : MonoBehaviour {
	[SerializeField] GameObject projectilePref;
	[SerializeField] float time2shoot;
	[SerializeField] ParticleSystem attackEffect;
	[SerializeField] Transform neckStart;
	[SerializeField] Transform head;
	List<Transform> neck;

	[SerializeField] float sqrMinDist;
	[SerializeField] float sqrMaxDist;
	public float averageDist;
	[SerializeField] float maxAngle;

	[HideInInspector] public Transform dragon;
	bool _allright = true;
	bool AllRight{ get { return _allright; } }
	void Start(){
		averageDist = (Mathf.Sqrt (sqrMinDist) + Mathf.Sqrt (sqrMaxDist))/2;
		if (!neckStart) {
			_allright = false;
			Debug.LogError ("Dragon hasn't assgined start of a neck.");
		}
		if (!head) {
			_allright = false;
			Debug.LogError ("Dragon hasn't assgined a head.");
		}
		if (!attackEffect) {
			_allright = false;
			Debug.LogError ("Dragon hasn't assgined an attack effect.");
		}
		if (sqrMaxDist <= sqrMinDist) {
			_allright = false;
			Debug.LogError ("Max is smaller or equal to min range");
		}
		if (maxAngle<=0) {
			_allright = false;
			Debug.LogError ("Maximum angle is 0 or smaller.");
		}
		if (_allright) {
			neck = new List<Transform> ();
			neck.Add (head);
			do{
				neck.Add(neck[neck.Count-1].parent);
			}while(neck[neck.Count-1]!=neckStart && neck[neck.Count-1].parent);
		}
	}

	//last checked ones
	float angle;
	Vector3 diff;

	public bool InRange(){
		bool result;
		diff = transform.position - neckStart.transform.position;
		float sqrDist = diff.sqrMagnitude;
		if (sqrDist > sqrMinDist && sqrDist < sqrMaxDist) {
			angle = Vector3.Angle(dragon.forward, diff);	
			if(angle>maxAngle){
				result = false;
			}else{
				result = true;
			}
		} else {
			result = false;
		}
		return result;
  	}
	[SerializeField] Vector3 offset;
	bool canShoot=true;
	public void Aim(){
		Quaternion targetRotation = Quaternion.LookRotation (diff)*Quaternion.Euler(offset);
		float deltaAngle = Quaternion.Angle(targetRotation, neck[neck.Count-1].parent.rotation)/neck.Count;
		for (int i = neck.Count-1; i >= 0; i--) {
			neck [i].rotation = Quaternion.RotateTowards (neck[i].parent.rotation, targetRotation, deltaAngle);
		}
		if(canShoot){
			GameObject projectile = ObjectPool.Instance.GetObjectForType (projectilePref.name, false);
			projectile.transform.position = FirePoint.position;
			projectile.transform.rotation = FirePoint.rotation;
			Projectile_ForwardAndParabole flyProjectile = projectile.GetComponent<Projectile_ForwardAndParabole> ();
			flyProjectile.maxLifeTime = (averageDist+5)/flyProjectile.speed;
			//to jest niezbędne, żeby automatyczne ograniczenie czasu życia działało od pierwszej iteracji
			flyProjectile.enabled = false;
			flyProjectile.enabled = true;
			StartCoroutine(waitAgain ());
			//neck [0].rotation = targetRotation;
		}
	}
	IEnumerator waitAgain(){
		canShoot = false;
		yield return new WaitForSeconds (time2shoot);
		canShoot = true;
	}
	[SerializeField] Transform FirePoint;
	public void UpdateFireBreath(){
		attackEffect.transform.position = FirePoint.transform.position;
		attackEffect.transform.rotation = FirePoint.transform.rotation;
	}
	bool isFire;
	public void fireOn(SkinnedMeshRenderer meshRenderer, int blendShape){
		if (!isFire) {
			meshRenderer.SetBlendShapeWeight (blendShape, 0f);
			attackEffect.Play ();
			isFire = true;
		}
	}
	public void fireOff(SkinnedMeshRenderer meshRenderer, int blendShape){
		if (isFire) {
			meshRenderer.SetBlendShapeWeight (blendShape, 100f);
			attackEffect.Stop ();
			isFire = false;
		}
	}
}
