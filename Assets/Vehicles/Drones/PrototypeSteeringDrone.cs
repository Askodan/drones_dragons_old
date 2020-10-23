using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrototypeSteeringDrone : SteeringDrone
{
    [Tooltip("Speed of moving wings")]
    public float wingsSpeed = 36f;
    [Tooltip("Power of additional propellers")]
    public float turboSpeed = 50f;
    
    private Coroutine moveWingsCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        
        number = 4;
        SetupPrototype();
        zeroThrust = -Physics.gravity.y / 4f * GetComponent<Rigidbody>().mass / forceFactor;
    }
    public void Steer(float axis_Thrust, float axis_Pitch, float axis_Roll, float axis_Yaw, float axis_Turbo,
        bool butDown_Lights, bool butDown_Motors, bool butDown_Stabilize, bool butDown_KeepAltitude, bool butDown_SelfLeveling) {
        float tempfloat = axis_Turbo * turboSpeed;
        propellers[4].CurrentRotationSpeed = -tempfloat;
        propellers[5].CurrentRotationSpeed = tempfloat;
        propellers[6].CurrentRotationSpeed = -tempfloat;
        propellers[7].CurrentRotationSpeed = tempfloat;
        
        if (butDown_SelfLeveling) {
            if (moveWingsCoroutine != null)
                StopCoroutine(moveWingsCoroutine);
            moveWingsCoroutine = StartCoroutine(MoveWings());
            selfLeveling = !selfLeveling;
        }
    }
    void SetupPrototype ()
	{
		//order (x+z+, x+z-, x-z+, x-z-), close +4 
		Propeller[] screws = new Propeller[propellers.Length];
		float[] distances = new float[propellers.Length], dists = new float[2];
		bool[] dist1 = new bool[propellers.Length];
		distances [0] = Vector3.Distance (propellers [0].transform.position, transform.position);
		dist1 [0] = true;
		for (int i = 1; i < propellers.Length; i++) {
			float distance = Vector3.Distance (propellers [i].transform.position, transform.position);
			distances [i] = distance;
			if (distance > distances [0] * 0.75f && distance < distances [0] * 1.5f) {
				dist1 [i] = dist1 [0];
				dists [0] += distance;
			} else {
				dist1 [i] = !dist1 [0];
				dists [1] += distance;
			}
		}
		bool close;
		if (dists [0] > dists [1]) {
			close = false;
		} else {
			close = true;
		}
		for (int i = 0; i < propellers.Length; i++) {
			int j = 0;
			if (!close) {
				if (dist1 [i]) {
					j += 4;
				}
			} else {
				if (!dist1 [i]) {
					j += 4;
				}
			}
			if (transform.InverseTransformPoint (propellers [i].transform.position).x > 0) {
				if (transform.InverseTransformPoint (propellers [i].transform.position).z > 0) {

					screws [j + 0] = propellers [i];
				} else {
					screws [j + 1] = propellers [i];
				}
			} else {
				if (transform.InverseTransformPoint (propellers [i].transform.position).z > 0) {
					screws [j + 2] = propellers [i];
				} else {
					screws [j + 3] = propellers [i];
				}
			}
		}
		propellers = screws;
	}
    IEnumerator MoveWings(){
        if (!selfLeveling) {
            while (propellers [0].transform.parent.localRotation != Quaternion.Euler (new Vector3 (0f, 0f, 270f))) {
                for (int i = number; i < propellers.Length; i++) {

                    if (i == 2+number || i == 0+number) {
                        propellers [i].transform.parent.localRotation = Quaternion.RotateTowards (propellers [i].transform.parent.localRotation, Quaternion.Euler (new Vector3 (0f, 0f, 270f)), Time.deltaTime * wingsSpeed);
                    } else {
                        propellers [i].transform.parent.localRotation = Quaternion.RotateTowards (propellers [i].transform.parent.localRotation, Quaternion.Euler (new Vector3 (0f, 0f, 0f)), Time.deltaTime * wingsSpeed);
                    }
                }
                yield return null;
            }
        } else {
            while (propellers [0].transform.parent.localRotation != Quaternion.Euler (new Vector3 (0f, 0f, 135f))) {
                for (int i = number; i < propellers.Length; i++) {

                    if (i == 2+number || i == 0+number) {
                        propellers [i].transform.parent.localRotation = Quaternion.RotateTowards (propellers [i].transform.parent.localRotation, Quaternion.Euler (new Vector3 (0f, 0f, 135f)), Time.deltaTime * wingsSpeed);
                    } else {
                        propellers [i].transform.parent.localRotation = Quaternion.RotateTowards (propellers [i].transform.parent.localRotation, Quaternion.Euler (new Vector3 (0f, 0f, 315f)), Time.deltaTime * wingsSpeed);
                    }
                }
                yield return null;
            }
        }
    }
}
