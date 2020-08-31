using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInfo : MonoBehaviour
{
    public Transform Roll;
    public Transform Pitch;
    
    // Update is called once per frame
    public void UpdateData(Transform target)
    {
        Pitch.localPosition = new Vector3(0f, Mathf.DeltaAngle(0, target.rotation.eulerAngles.x) * 5f, 0f);
        Roll.localRotation = Quaternion.Euler(0f, 0f, -target.rotation.eulerAngles.z);
    }
}
