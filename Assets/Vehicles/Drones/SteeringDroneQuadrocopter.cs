using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringDroneQuadrocopter : SteeringDrone
{
    public float autoAltitudePower;
    public PIDController altitudeKeeper;

    public float autoAnglePower;
    public PIDController selfLeveler;
    protected PIDController[] selfLevelers;

    public PIDController stabilizator;
    protected PIDController[] stabilizators;
    
    public float maxAngle;
    public float autoSpeedPower;
    public PIDController stopper;
    protected PIDController[] stoppers;

    public SteeringModeNormal modeNormal = new SteeringModeNormal();
    public SteeringModeSelfLeveling modeSelfLeveling = new SteeringModeSelfLeveling();
    public SteeringModeStabilize modeStabilize = new SteeringModeStabilize();

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
        // Sort propellers in order x+z+, x+z-, x-z+, x-z-
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
        stabilizators = new PIDController[3];
        for (int i = 0; i < stabilizators.Length; i++)
        {
            stabilizators[i] = new PIDController();
            stabilizators[i].CopySettings(stabilizator);
        }
        modeNormal.Setup(ClearMotors, AddThrust, RotPitch, RotYaw, RotRoll);
        modeSelfLeveling.Setup(gyroscope, ClearMotors, AddThrust, RotPitch, RotYaw, RotRoll);
        modeStabilize.Setup(speedMeter, ClearMotors, AddThrust, RotPitch, RotYaw, RotRoll);
        
        selfLevelers = new PIDController[2];
        for (int i = 0; i < selfLevelers.Length; i++)
        {
            selfLevelers[i] = new PIDController();
            selfLevelers[i].CopySettings(selfLeveler);
        }
        
        stoppers = new PIDController[2];
        for (int i = 0; i < stoppers.Length; i++)
        {
            stoppers[i] = new PIDController();
            stoppers[i].CopySettings(stopper);
        }
    }

    protected override void CalculatePropellersSpeed()
    {
        ClearMotors();
        RotYaw(yaw * yawFactor);
        if (!selfLeveling && !stabilize)
        {
            RotPitch(pitch * pitchFactor);
            RotRoll(roll * rollFactor);
        }
        if(keepAltitude){
            targetAltitude += SpeedMeter.Kmph2Mps(autoAltitudePower) * thrust * Time.deltaTime;
            AddThrust(altitudeKeeper.Regulate(targetAltitude - altitudeMeter.altitude));
        }else{
            AddThrust(thrust);
        }
        if (selfLeveling)
        {
            if (!stabilize)
            {
                selfLevel(pitch * autoAnglePower, roll * autoAnglePower);
            }
            else
            {
                if (Application.isEditor)
                {
                    for (int i = 0; i < stoppers.Length; i++)
                    {
                        stoppers[i].CopySettings(stopper);
                    }
                }
                Vector3 localVelocity = speedMeter.GetSpeedFlat();
                float str = stoppers[0].Regulate(roll * SpeedMeter.Kmph2Mps(autoSpeedPower) - localVelocity.x);
                float stp = stoppers[1].Regulate(pitch * SpeedMeter.Kmph2Mps(autoSpeedPower) - localVelocity.z);
                selfLevel(stp * maxAngle, str * maxAngle);
            }
        }
        
        calcRotationStabilizeRotationSpeedChange();
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
        Vector3 localangularvelocity = gyroscope.GetLocalAngularVelocity();
        RotPitch(stabilizators[0].Regulate(-localangularvelocity.x));
        RotYaw(stabilizators[1].Regulate(-localangularvelocity.y));
        RotRoll(stabilizators[2].Regulate(localangularvelocity.z));
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

    void selfLevel(float target_pitch, float target_roll)
    {
        if (Application.isEditor)
        {
            for (int i = 0; i < 2; i++)
            {
                selfLevelers[i].CopySettings(selfLeveler);
            }
        }
        float pitch_val = Gyroscope.Angle2OneMinusOne(gyroscope.GetRotation().x)*180f;
        float slp = selfLevelers[0].Regulate(target_pitch - pitch_val);
        float roll_val = Gyroscope.Angle2OneMinusOne(gyroscope.GetRotation().z)*180f;
        float slr = selfLevelers[1].Regulate(target_roll + roll_val);
        
        RotPitch(slp);
        RotRoll(slr);
    }
}
