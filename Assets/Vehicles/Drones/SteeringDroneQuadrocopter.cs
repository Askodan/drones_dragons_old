using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringDroneQuadrocopter : SteeringDrone
{
    public PIDController stabilizator;
    public PIDController selfLeveler;
    public PIDController stopper;

    protected PIDController[] stabilizators;
    protected PIDController[] selfLevelers;
    protected PIDController[] stoppers;

    protected override void CheckPropellers()
    {
        float acceptableDistanceDiff = 0.0001f;
        if (propellers.Length != 4)
        {
            Debug.LogError("Number of propellers doesn't match! Should be 4 is " + propellers.Length.ToString());
        }
        float[] dists = new float[4];
        dists[0] = Vector3.Distance(propellers[0].transform.position, centerOfMass.transform.position);
        dists[1] = Vector3.Distance(propellers[1].transform.position, centerOfMass.transform.position);
        dists[2] = Vector3.Distance(propellers[2].transform.position, centerOfMass.transform.position);
        dists[3] = Vector3.Distance(propellers[3].transform.position, centerOfMass.transform.position);
        for (int i = 0; i < dists.Length; i++)
        {
            for (int j = i; j < dists.Length; j++)
            {
                if (Mathf.Abs(dists[i] - dists[j]) > acceptableDistanceDiff)
                {
                    Debug.LogError("Distance from center of mass is different for " + i + " " + j);
                }
            }
        }
    }

    protected override void Setup()
    {
        //order x+z+, x+z-, x-z+, x-z-
        Propeller[] screws = new Propeller[propellers.Length];
        for (int i = 0; i < propellers.Length; i++)
        {
            Vector3 pos = transform.InverseTransformPoint(propellers[i].transform.position);
            if (pos.x > 0)
            {
                if (pos.z > 0)
                {
                    screws[0] = propellers[i];
                    propellers[i].Setup(true, 0);
                }
                else
                {
                    screws[1] = propellers[i];
                    propellers[i].Setup(false, 1);
                }
            }
            else
            {
                if (pos.z > 0)
                {
                    screws[2] = propellers[i];
                    propellers[i].Setup(false, 2);
                }
                else
                {
                    screws[3] = propellers[i];
                    propellers[i].Setup(true, 3);
                }
            }
        }
        propellers = screws;

        zeroThrust = -Physics.gravity.y / propellers.Length * rigidbody.mass / forceFactor;
        // calc propellers dist from center
        Vector3 propellersCenter = Vector3.zero;
        foreach (var propeller in propellers)
        {
            propellersCenter += propeller.transform.position;
        }
        propellersCenter /= 4f;
        propellerDistFromCenter = Vector3.Distance(propellersCenter, propellers[0].transform.position);

        // create pid controllers
        selfLevelers = new PIDController[4];
        for (int i = 0; i < selfLevelers.Length; i++)
        {
            selfLevelers[i] = new PIDController();
            selfLevelers[i].CopySettings(selfLeveler);
        }
        stabilizators = new PIDController[3];
        for (int i = 0; i < stabilizators.Length; i++)
        {
            stabilizators[i] = new PIDController();
            stabilizators[i].CopySettings(stabilizator);
        }
        stoppers = new PIDController[3];
        for (int i = 0; i < stoppers.Length; i++)
        {
            stoppers[i] = new PIDController();
            stoppers[i].CopySettings(stopper);
        }
    }

    protected override void ApplyPropellersThrust()
    {
        calcBaseSteeringRotationSpeedChange();
        calcRotationStabilizeRotationSpeedChange();
        if (selfLeveling)
        {
            calcSelfLevelRotationSpeedChange();
        }
        if (stabilize)
        {
            calcStabilizeRotationSpeedChange();
        }
        for (int i = 0; i < propellers.Length; i++)
        {
            Vector3 thrustVec = new Vector3(0, 0, propellers[i].CurrentRotationSpeed * forceFactor);
            rigidbody.AddForceAtPosition(propellers[i].transform.rotation * thrustVec, propellers[i].transform.position);
            rigidbody.AddTorque(transform.rotation * new Vector3(0, propellers[i].Rotation, 0));
            //Debug.DrawLine (propellers [i].position, propellers [i].position + propellers [i].rotation * thrustVec);
        }
    }
    void calcBaseSteeringRotationSpeedChange()
    {
        ClearMotors();
        AddThrust(thrust);
        RotPitch(pitch);
        RotYaw(yaw);
        RotRoll(roll);
    }
    void ClearMotors()
    {
        for (int i = 0; i < propellers.Length; i++)
        {
            propellers[i].CurrentRotationSpeed = 0;
        }
    }
    void AddThrust(float thrust_val)
    {
        propellers[0].CurrentRotationSpeed += thrust_val;
        propellers[1].CurrentRotationSpeed += thrust_val;
        propellers[2].CurrentRotationSpeed += thrust_val;
        propellers[3].CurrentRotationSpeed += thrust_val;
    }
    void RotPitch(float pitch_val)
    {
        propellers[0].CurrentRotationSpeed += -pitch_val;
        propellers[1].CurrentRotationSpeed += pitch_val;
        propellers[2].CurrentRotationSpeed += -pitch_val;
        propellers[3].CurrentRotationSpeed += pitch_val;
    }
    void RotYaw(float yaw_val)
    {
        propellers[0].CurrentRotationSpeed += -yaw_val;
        propellers[1].CurrentRotationSpeed += yaw_val;
        propellers[2].CurrentRotationSpeed += yaw_val;
        propellers[3].CurrentRotationSpeed += -yaw_val;
    }
    void RotRoll(float roll_val)
    {
        propellers[0].CurrentRotationSpeed += -roll_val;
        propellers[1].CurrentRotationSpeed += -roll_val;
        propellers[2].CurrentRotationSpeed += roll_val;
        propellers[3].CurrentRotationSpeed += roll_val;
    }

    void calcRotationStabilizeRotationSpeedChange()
    {
        if (Application.isEditor)
        {
            for (int i = 0; i < stabilizators.Length; i++)
            {
                stabilizators[i].CopySettings(stabilizator);
            }
        }
        Vector3 localangularvelocity = transform.InverseTransformDirection(rigidbody.angularVelocity);
        RotPitch(stabilizators[0].Regulate(-localangularvelocity.x));
        RotYaw(stabilizators[1].Regulate(-localangularvelocity.y));
        RotRoll(stabilizators[2].Regulate(localangularvelocity.z));
    }

    void calcSelfLevelRotationSpeedChange()
    {
        float averageOfPropellersAltitude = calcAverageOfPropellersAltitude();

        float[] propellerAngleFromFlat = new float[4];
        for (int i = 0; i < propellers.Length; i++)
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
        float[] cor = new float[4];
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
            if (Application.isEditor)
            {
                selfLevelers[i].CopySettings(selfLeveler);
            }
            propellers[i].CurrentRotationSpeed += selfLevelers[i].Regulate(cor[i] / 2f);
        }
    }

    float calcAverageOfPropellersAltitude()
    {
        float averageOfPropellersAltitude = 0f;
        for (int i = 0; i < propellers.Length; i++)
        {
            averageOfPropellersAltitude += propellers[i].transform.position.y;
        }
        return averageOfPropellersAltitude / 4;
    }

    void calcStabilizeRotationSpeedChange()
    {
        if (Application.isEditor)
        {
            for (int i = 0; i < stoppers.Length; i++)
            {
                stoppers[i].CopySettings(stopper);
            }
        }
        Vector3 localVelocity = transform.InverseTransformDirection(new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z));

        RotRoll(stoppers[0].Regulate(-localVelocity.x));
        AddThrust(stoppers[1].Regulate(-rigidbody.velocity.y));
        RotPitch(stoppers[2].Regulate(-localVelocity.z));
    }
}
