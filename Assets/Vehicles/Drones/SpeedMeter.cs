using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedMeter : MonoBehaviour
{
    
    private new Rigidbody rigidbody { get; set; }
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public Vector3 GetSpeed()
    {
        return transform.InverseTransformDirection(rigidbody.velocity);
    }
    public Vector3 GetSpeedGlobal()
    {
        return rigidbody.velocity;
    }
    public Vector3 GetSpeedFlat()
    {
        return transform.InverseTransformDirection(new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z));
    }
    public static float Mps2Kmph(float speed){
        return speed*3.6f;
    }
    public static float Kmph2Mps(float speed){
        return speed/3.6f;
    }
}
