using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringDroneQuadrocopter : SteeringDrone
{
    protected int RightFrontPropellersIndex = -1;
    protected int RightRearPropellersIndex = -1;
    protected int LeftRearPropellersIndex = -1;
    protected int LeftFrontPropellersIndex = -1;

    protected override void CheckPropellers()
    {
        if (propellers.Length != 4)
        {
            Debug.LogError("Number of propellers doesn't match! Should be 4 is " + propellers.Length.ToString());
        }
        float acceptableDistanceDiff = 0.0001f;
        float[] dists = new float[4];
        for (int i = 0; i < dists.Length; i++)
        {
            dists[i] = Vector3.Distance(propellers[i].transform.position, centerOfMass.transform.position);
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
        AssignPropellersIndexes();
        CheckPropellerIndexes();
    }
    protected virtual void AssignPropellersIndexes()
    {
        // Sort propellers in order 0=x+z+, 1=x+z-, 2=x-z+, 3=x-z-
        for (int i = 0; i < propellers.Length; i++)
        {
            Vector3 pos = transform.InverseTransformPoint(propellers[i].transform.position);
            propellers[i].Setup(IsRight(pos) ^ IsFront(pos));
            if (IsRight(pos))
            {
                if (IsFront(pos))
                {
                    RightFrontPropellersIndex = i;
                }
                else
                {
                    RightRearPropellersIndex = i;
                }
            }
            else
            {
                if (IsFront(pos))
                {
                    LeftFrontPropellersIndex = i;
                }
                else
                {
                    LeftRearPropellersIndex = i;
                }
            }
        }
    }
    protected virtual void CheckPropellerIndexes()
    {
        if (RightFrontPropellersIndex < 0 ||
            RightRearPropellersIndex < 0 ||
            LeftFrontPropellersIndex < 0 ||
            LeftRearPropellersIndex < 0)
        {
            Debug.LogError("Some propellers indexes weren't found");
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
        propellers[RightFrontPropellersIndex].CurrentRotationSpeed += thrust_val;
        propellers[RightRearPropellersIndex].CurrentRotationSpeed += thrust_val;
        propellers[LeftFrontPropellersIndex].CurrentRotationSpeed += thrust_val;
        propellers[LeftRearPropellersIndex].CurrentRotationSpeed += thrust_val;
    }
    protected override void RotPitch(float pitch_val)
    {
        propellers[RightFrontPropellersIndex].CurrentRotationSpeed += -pitch_val;
        propellers[RightRearPropellersIndex].CurrentRotationSpeed += pitch_val;
        propellers[LeftFrontPropellersIndex].CurrentRotationSpeed += -pitch_val;
        propellers[LeftRearPropellersIndex].CurrentRotationSpeed += pitch_val;
    }
    protected override void RotYaw(float yaw_val)
    {
        propellers[RightFrontPropellersIndex].CurrentRotationSpeed += yaw_val;
        propellers[RightRearPropellersIndex].CurrentRotationSpeed += -yaw_val;
        propellers[LeftFrontPropellersIndex].CurrentRotationSpeed += -yaw_val;
        propellers[LeftRearPropellersIndex].CurrentRotationSpeed += yaw_val;
    }
    protected override void RotRoll(float roll_val)
    {
        propellers[RightFrontPropellersIndex].CurrentRotationSpeed += -roll_val;
        propellers[RightRearPropellersIndex].CurrentRotationSpeed += -roll_val;
        propellers[LeftFrontPropellersIndex].CurrentRotationSpeed += roll_val;
        propellers[LeftRearPropellersIndex].CurrentRotationSpeed += roll_val;
    }
}