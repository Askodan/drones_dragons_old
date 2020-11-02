using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringDronePrototype : SteeringDroneQuadrocopter
{
    [Tooltip("Speed of moving wings")]
    public float wingsSpeed = 36f;
    [Tooltip("Power of additional propellers")]
    public float turboSpeed = 50f;

    private Coroutine moveWingsCoroutine;
    private int number = 4;
    // Start is called before the first frame update
    void Awake()
    {
        SetupPrototype();
        zeroThrust = -Physics.gravity.y / 4f * GetComponent<Rigidbody>().mass / forceFactor;
    }
    public void Steer(float axis_Thrust, float axis_Pitch, float axis_Roll, float axis_Yaw, float axis_Turbo,
        bool butDown_Lights, bool butDown_Motors, bool butDown_Stabilize, bool butDown_KeepAltitude, bool butDown_SelfLeveling)
    {
        if (motorsOn)
        {
            thrust = axis_Thrust;
            pitch = axis_Pitch * pitchFactor;
            roll = axis_Roll * rollFactor;
            yaw = axis_Yaw * yawFactor;

            if (keepAltitude)
            {
                thrust = Mathf.Clamp(thrust + zeroThrust, -1, 1);
            }
            for (int i = 0; i < propellers.Length - number; i++)
            {
                propellers[i].CurrentRotationSpeed = thrust;
            }
            propellers[0].CurrentRotationSpeed += -pitch - roll;
            propellers[1].CurrentRotationSpeed += pitch - roll;
            propellers[2].CurrentRotationSpeed += -pitch + roll;
            propellers[3].CurrentRotationSpeed += pitch + roll;

        }
        if (butDown_Lights)
        {
            flashLight.Change();
        }
        if (butDown_Motors)
        {
            motorsOn = !motorsOn;
            if (!motorsOn)
            {
                for (int i = 0; i < propellers.Length; i++)
                {
                    propellers[i].CurrentRotationSpeed = 0f;
                }
            }
        }
        if (butDown_Stabilize)
        {
            stabilize = !stabilize;
        }
        if (butDown_KeepAltitude)
        {
            keepAltitude = !keepAltitude;
        }
        if (butDown_SelfLeveling)
        {
            //if (moveWingsCoroutine != null)
            //    StopCoroutine(moveWingsCoroutine);
            //moveWingsCoroutine = StartCoroutine(MoveWings());
            selfLeveling = !selfLeveling;
        }
        float tempfloat = axis_Turbo * turboSpeed;
        propellers[4].CurrentRotationSpeed = -tempfloat;
        propellers[5].CurrentRotationSpeed = tempfloat;
        propellers[6].CurrentRotationSpeed = -tempfloat;
        propellers[7].CurrentRotationSpeed = tempfloat;

        if (butDown_SelfLeveling)
        {
            if (moveWingsCoroutine != null)
                StopCoroutine(moveWingsCoroutine);
            moveWingsCoroutine = StartCoroutine(MoveWings());
            selfLeveling = !selfLeveling;
        }
    }
    float calcAverageOfPropellersAltitude()
    {
        float averageOfPropellersAltitude = 0f;
        for (int i = 0; i < propellers.Length - number; i++)
        {
            averageOfPropellersAltitude += propellers[i].transform.position.y;
        }
        return averageOfPropellersAltitude / 4;
    }

    public void ApplyPropellersThrust()
    {

        float[] cor = new float[propellers.Length];
        if (selfLeveling)
        {
            float averageOfPropellersAltitude = calcAverageOfPropellersAltitude();

            float[] propellerAngleFromFlat = new float[4];
            for (int i = 0; i < propellers.Length - number; i++)
            {
                propellerAngleFromFlat[i] = Mathf.Asin((averageOfPropellersAltitude - propellers[i].transform.position.y) / propellerDistFromCenter);
            }
            float[] pitchRollCor = new float[4];
            //x+
            pitchRollCor[0] = (propellerAngleFromFlat[0] + propellerAngleFromFlat[1]) / 2f;
            //x-
            pitchRollCor[1] = (propellerAngleFromFlat[2] + propellerAngleFromFlat[3]) / 2f;
            //z+
            pitchRollCor[2] = (propellerAngleFromFlat[0] + propellerAngleFromFlat[2]) / 2f;
            //z-
            pitchRollCor[3] = (propellerAngleFromFlat[1] + propellerAngleFromFlat[3]) / 2f;
            cor[0] = pitchRollCor[0] * pitch + propellerAngleFromFlat[0] * (1 - pitch);
            cor[1] = pitchRollCor[0] * pitch + propellerAngleFromFlat[1] * (1 - pitch);
            cor[2] = pitchRollCor[1] * pitch + propellerAngleFromFlat[2] * (1 - pitch);
            cor[3] = pitchRollCor[1] * pitch + propellerAngleFromFlat[3] * (1 - pitch);
            cor[0] += pitchRollCor[2] * roll + propellerAngleFromFlat[0] * (1 - roll);
            cor[1] += pitchRollCor[3] * roll + propellerAngleFromFlat[1] * (1 - roll);
            cor[2] += pitchRollCor[2] * roll + propellerAngleFromFlat[2] * (1 - roll);
            cor[3] += pitchRollCor[3] * roll + propellerAngleFromFlat[3] * (1 - roll);
            for (int i = 0; i < propellers.Length; i++)
            {
                cor[i] /= 2;
            }
        }
        for (int i = 0; i < propellers.Length; i++)
        {
            Vector3 thrustVec = new Vector3(0, 0, propellers[i].CurrentRotationSpeed * forceFactor + cor[i] * selfLevelFactor);
            rigidbody.AddForceAtPosition(propellers[i].transform.rotation * thrustVec, propellers[i].transform.position);
            //Debug.DrawLine (propellers [i].position, propellers [i].position + propellers [i].rotation * thrustVec);
        }
        if (stabilize)
        {
            rigidbody.AddTorque(rigidbody.angularVelocity * (-Time.deltaTime * stabVelFactor));
        }
        rigidbody.AddTorque(transform.rotation * new Vector3(0, yaw, 0));
    }

    void SetupPrototype()
    {
        //order (x+z+, x+z-, x-z+, x-z-), close +4 
        Propeller[] screws = new Propeller[propellers.Length];
        float[] distances = new float[propellers.Length], dists = new float[2];
        bool[] dist1 = new bool[propellers.Length];
        distances[0] = Vector3.Distance(propellers[0].transform.position, transform.position);
        dist1[0] = true;
        for (int i = 1; i < propellers.Length; i++)
        {
            float distance = Vector3.Distance(propellers[i].transform.position, transform.position);
            distances[i] = distance;
            if (distance > distances[0] * 0.75f && distance < distances[0] * 1.5f)
            {
                dist1[i] = dist1[0];
                dists[0] += distance;
            }
            else
            {
                dist1[i] = !dist1[0];
                dists[1] += distance;
            }
        }
        bool close;
        if (dists[0] > dists[1])
        {
            close = false;
        }
        else
        {
            close = true;
        }
        for (int i = 0; i < propellers.Length; i++)
        {
            int j = 0;
            if (!close)
            {
                if (dist1[i])
                {
                    j += 4;
                }
            }
            else
            {
                if (!dist1[i])
                {
                    j += 4;
                }
            }
            if (transform.InverseTransformPoint(propellers[i].transform.position).x > 0)
            {
                if (transform.InverseTransformPoint(propellers[i].transform.position).z > 0)
                {

                    screws[j + 0] = propellers[i];
                }
                else
                {
                    screws[j + 1] = propellers[i];
                }
            }
            else
            {
                if (transform.InverseTransformPoint(propellers[i].transform.position).z > 0)
                {
                    screws[j + 2] = propellers[i];
                }
                else
                {
                    screws[j + 3] = propellers[i];
                }
            }
        }
        propellers = screws;
    }
    IEnumerator MoveWings()
    {
        if (!selfLeveling)
        {
            while (propellers[0].transform.parent.localRotation != Quaternion.Euler(new Vector3(0f, 0f, 270f)))
            {
                for (int i = number; i < propellers.Length; i++)
                {

                    if (i == 2 + number || i == 0 + number)
                    {
                        propellers[i].transform.parent.localRotation = Quaternion.RotateTowards(propellers[i].transform.parent.localRotation, Quaternion.Euler(new Vector3(0f, 0f, 270f)), Time.deltaTime * wingsSpeed);
                    }
                    else
                    {
                        propellers[i].transform.parent.localRotation = Quaternion.RotateTowards(propellers[i].transform.parent.localRotation, Quaternion.Euler(new Vector3(0f, 0f, 0f)), Time.deltaTime * wingsSpeed);
                    }
                }
                yield return null;
            }
        }
        else
        {
            while (propellers[0].transform.parent.localRotation != Quaternion.Euler(new Vector3(0f, 0f, 135f)))
            {
                for (int i = number; i < propellers.Length; i++)
                {

                    if (i == 2 + number || i == 0 + number)
                    {
                        propellers[i].transform.parent.localRotation = Quaternion.RotateTowards(propellers[i].transform.parent.localRotation, Quaternion.Euler(new Vector3(0f, 0f, 135f)), Time.deltaTime * wingsSpeed);
                    }
                    else
                    {
                        propellers[i].transform.parent.localRotation = Quaternion.RotateTowards(propellers[i].transform.parent.localRotation, Quaternion.Euler(new Vector3(0f, 0f, 315f)), Time.deltaTime * wingsSpeed);
                    }
                }
                yield return null;
            }
        }
    }
}
