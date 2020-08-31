using UnityEngine;
using System.Collections;

public class Shooter_Balista: Shooter{
	//hierarchia:
	//-RotatorHorizontal
	//--RotatorVertical
    //---FrontSight
	public float rotSpeed;
	public float AngleLimes;

	public Transform VerticalRotator;
	public Transform HorizontalRotator;
    [Tooltip("Projectile in Ballista used solely for animation")]
	public GameObject projectileAnim;
	public Transform WinchWheel;
    public Vector3 WinchWheelRotationAxis;
	private bool canShoot = false;

    private DragonInterest shooter;

    private Vector2 rotation_offset;
    private Vector3 translation_offset;
    private Vector2 previous_angles;

	void Start(){
		BlendShape = GetComponentInChildren<SkinnedMeshRenderer> ();
		StartCoroutine (getReady ());
        shooter = GetComponentInParent<DragonInterest> ();

        ApplyAngles(Vector2.zero);
        CalculateOffsets();
        previous_angles = rotation_offset;
    }

	void Update(){
		if(shoot){
			Shoot (projectilePrefab, FrontSight.position, Power);
		}
		Aim (aimer.position);
	}

	override public void Shoot (GameObject projectile, Vector3 projectileSpawnPoint, float force){
		if (canShoot) {
			StartCoroutine (animShoot ());
			projectile = ObjectPool.Instance.GetObjectForType (projectile.name, false);
			projectile.transform.position = projectileSpawnPoint;
            projectile.transform.rotation = FrontSight.rotation;
			Projectile_ForwardAndParabole p = projectile.GetComponent<Projectile_ForwardAndParabole> ();
			p.speed = force;
			p.shooter = shooter;
		}
    }

    public override void Aim(Vector3 target_pos)
    {
        target_pos -= translation_offset;
        var angles = CalculateAnglesFromDirection(target_pos - transform.position);
        angles = ClampAngles(angles);
        previous_angles = Vector2.MoveTowards(previous_angles, angles - rotation_offset, rotSpeed * Time.deltaTime);
        ApplyAngles(previous_angles);
    }

    void CalculateOffsets()
    {
        rotation_offset = CalculateAnglesFromDirection(FrontSight.forward);
        translation_offset = new Vector3(0f, (transform.position - HorizontalRotator.position).magnitude -
                            (HorizontalRotator.position - VerticalRotator.position).magnitude +
                            (VerticalRotator.position - FrontSight.position).magnitude,
                            0f);
    }

    Vector2 CalculateAnglesFromDirection(Vector3 point)
    {
        Vector3 local_pos = transform.InverseTransformDirection(point);
        var local_pos_y0 = new Vector3(local_pos.x, 0f, local_pos.z);
        var local_angle_x = Mathf.Sign(-local_pos.y) * Vector3.Angle(local_pos_y0, local_pos);
        var local_angle_y = Vector3.SignedAngle(Vector3.forward, local_pos_y0, Vector3.up);
        return new Vector2(local_angle_x, local_angle_y);
    }

    Vector2 ClampAngles(Vector2 angles)
    {
        return new Vector2(Mathf.Clamp(angles.x, -AngleLimes, AngleLimes), angles.y);
    }

    void ApplyAngles(Vector2 angles)
    {
        HorizontalRotator.rotation = Quaternion.AngleAxis(angles.y, transform.up) * transform.rotation;
        VerticalRotator.rotation = Quaternion.AngleAxis(angles.y, transform.up) * Quaternion.AngleAxis(angles.x, transform.right) * transform.rotation;
    }

    IEnumerator animShoot(){
		canShoot = false;
		float time = 0;
		while(time<shootTime){
			time += Time.deltaTime;
            if(BlendShape)
			    BlendShape.SetBlendShapeWeight(0, 100f - time/shootTime*100f);
			projectileAnim.SetActive (false);
			yield return null;
		}
        if (BlendShape)
            BlendShape.SetBlendShapeWeight (0, 0f);
		StartCoroutine (getReady ());
	}

	IEnumerator getReady(){
		float time = 0;
		while(time<loadTime){
			time += Time.deltaTime;
            if (BlendShape)
                BlendShape.SetBlendShapeWeight(0, time/loadTime*100f);
			WinchWheel.Rotate (WinchWheelRotationAxis*400f * Time.deltaTime, Space.Self);
			yield return null;
        }
        if (BlendShape)
            BlendShape.SetBlendShapeWeight (0, 100f);
		projectileAnim.SetActive (true);
		canShoot = true;
	}
}
