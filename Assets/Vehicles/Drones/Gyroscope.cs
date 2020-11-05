using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gyroscope : MonoBehaviour
{
    private new Rigidbody rigidbody { get; set; }
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public Vector3 GetRotation()
    {
        return transform.rotation.eulerAngles;
    }

    public Vector3 GetLocalAngularVelocity()
    {
        return transform.InverseTransformDirection(rigidbody.angularVelocity);
    }
    static public float Angle2OneMinusOne(float angle)
    {
        return Mathf.Repeat(angle / 180f + 1f, 2f) - 1f;
    }
}
