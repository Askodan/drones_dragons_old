using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringDroneQuadrocopter : SteeringDrone
{
    protected int RightFrontMotorIndex = -1;
    protected int RightRearMotorIndex = -1;
    protected int LeftRearMotorIndex = -1;
    protected int LeftFrontMotorIndex = -1;

    protected override void CheckMotors()
    {
        if (motors.Length != 4)
        {
            Debug.LogError("Number of motors doesn't match! Should be 4 is " + motors.Length.ToString());
        }
        float acceptableDistanceDiff = 0.0001f;
        float[] dists = new float[4];
        for (int i = 0; i < dists.Length; i++)
        {
            dists[i] = Vector3.Distance(motors[i].transform.position, centerOfMass.transform.position);
        }
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
        AssignMotorsIndexes();
        CheckMotorsIndexes();
    }
    protected virtual void AssignMotorsIndexes()
    {
        // Sort motors in order 0=x+z+, 1=x+z-, 2=x-z+, 3=x-z-
        for (int i = 0; i < motors.Length; i++)
        {
            Vector3 pos = transform.InverseTransformPoint(motors[i].transform.position);
            motors[i].Setup(IsRight(pos) ^ IsFront(pos));
            if (IsRight(pos))
            {
                if (IsFront(pos))
                {
                    RightFrontMotorIndex = i;
                }
                else
                {
                    RightRearMotorIndex = i;
                }
            }
            else
            {
                if (IsFront(pos))
                {
                    LeftFrontMotorIndex = i;
                }
                else
                {
                    LeftRearMotorIndex = i;
                }
            }
        }
    }
    protected virtual void CheckMotorsIndexes()
    {
        if (RightFrontMotorIndex < 0 ||
            RightRearMotorIndex < 0 ||
            LeftFrontMotorIndex < 0 ||
            LeftRearMotorIndex < 0)
        {
            Debug.LogError("Some motors indexes weren't found");
        }
    }
    protected bool IsRight(Vector3 pos)
    {
        return pos.x > 0;
    }
    protected bool IsFront(Vector3 pos)
    {
        return pos.z > 0;
    }
    protected override void AddThrust(float thrust_val)
    {
        motors[RightFrontMotorIndex].TargetThrust += thrust_val;
        motors[RightRearMotorIndex].TargetThrust += thrust_val;
        motors[LeftFrontMotorIndex].TargetThrust += thrust_val;
        motors[LeftRearMotorIndex].TargetThrust += thrust_val;
    }
    protected override void RotPitch(float pitch_val)
    {
        motors[RightFrontMotorIndex].TargetThrust += -pitch_val;
        motors[RightRearMotorIndex].TargetThrust += pitch_val;
        motors[LeftFrontMotorIndex].TargetThrust += -pitch_val;
        motors[LeftRearMotorIndex].TargetThrust += pitch_val;
    }
    protected override void RotYaw(float yaw_val)
    {
        motors[RightFrontMotorIndex].TargetThrust += yaw_val;
        motors[RightRearMotorIndex].TargetThrust += -yaw_val;
        motors[LeftFrontMotorIndex].TargetThrust += -yaw_val;
        motors[LeftRearMotorIndex].TargetThrust += yaw_val;
    }
    protected override void RotRoll(float roll_val)
    {
        motors[RightFrontMotorIndex].TargetThrust += -roll_val;
        motors[RightRearMotorIndex].TargetThrust += -roll_val;
        motors[LeftFrontMotorIndex].TargetThrust += roll_val;
        motors[LeftRearMotorIndex].TargetThrust += roll_val;
    }
}