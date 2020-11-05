using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringModeNormal
{
    public delegate void CM();
    protected CM ClearMotors;
    public delegate void AT(float thrust);
    protected AT AddThrust;
    public delegate void RP(float pitch);
    protected RP RotPitch;
    public delegate void RY(float yaw);
    protected RY RotYaw;
    public delegate void RR(float roll);
    protected RR RotRoll;

    public virtual void Setup(CM _clearMotors, AT _addThrust, RP _rotPitch, RY _rotYaw, RR _rotRoll)
    {
        ClearMotors = _clearMotors;
        AddThrust = _addThrust;
        RotPitch = _rotPitch;
        RotYaw = _rotYaw;
        RotRoll = _rotRoll;
    }
    public virtual void Setup(Gyroscope _gyroscope, CM _clearMotors, AT _addThrust, RP _rotPitch, RY _rotYaw, RR _rotRoll)
    {
        Setup(_clearMotors, _addThrust, _rotPitch, _rotYaw, _rotRoll);
    }
    
    public virtual void Setup(SpeedMeter _speedMeter, CM _clearMotors, AT _addThrust, RP _rotPitch, RY _rotYaw, RR _rotRoll){
        Setup(_clearMotors, _addThrust, _rotPitch, _rotYaw, _rotRoll);
    }
    public virtual void CalcSteeringRotationSpeedChange(float thrust, float pitch, float roll, float yaw)
    {
        ClearMotors();
        AddThrust(thrust);
        RotPitch(pitch);
        RotYaw(yaw);
        RotRoll(roll);
    }
}
[System.Serializable]
public class SteeringModeSelfLeveling : SteeringModeNormal
{
    public PIDController selfLeveler;
    protected PIDController[] selfLevelers;
    protected Gyroscope gyroscope;
    public override void Setup(Gyroscope _gyroscope, CM _clearMotors, AT _addThrust, RP _rotPitch, RY _rotYaw, RR _rotRoll)
    {
        base.Setup(_clearMotors, _addThrust, _rotPitch, _rotYaw, _rotRoll);
        gyroscope = _gyroscope;
        selfLevelers = new PIDController[2];
        for (int i = 0; i < selfLevelers.Length; i++)
        {
            selfLevelers[i] = new PIDController();
            selfLevelers[i].CopySettings(selfLeveler);
        }
    }

    public override void CalcSteeringRotationSpeedChange(float thrust, float pitch, float roll, float yaw)
    {
        if (Application.isEditor)
        {
            for (int i = 0; i < 2; i++)
            {
                selfLevelers[i].CopySettings(selfLeveler);
            }
        }
        ClearMotors();
        AddThrust(thrust);
        RotYaw(yaw);
        float pitch_val = Gyroscope.Angle2OneMinusOne(gyroscope.GetRotation().x);
        RotPitch(selfLevelers[0].Regulate(pitch - pitch_val));
        float roll_val = Gyroscope.Angle2OneMinusOne(gyroscope.GetRotation().z);
        RotRoll(selfLevelers[1].Regulate(roll + roll_val));
    }
}


[System.Serializable]
public class SteeringModeStabilize : SteeringModeNormal
{
    public PIDController stopper;
    protected PIDController[] stoppers;
    protected SpeedMeter speedMeter;
    public override void Setup(SpeedMeter _speedMeter, CM _clearMotors, AT _addThrust, RP _rotPitch, RY _rotYaw, RR _rotRoll)
    {
        base.Setup(_clearMotors, _addThrust, _rotPitch, _rotYaw, _rotRoll);
        speedMeter = _speedMeter;
        stoppers = new PIDController[3];
        for (int i = 0; i < stoppers.Length; i++)
        {
            stoppers[i] = new PIDController();
            stoppers[i].CopySettings(stopper);
        }
    }
    public override void CalcSteeringRotationSpeedChange(float thrust, float pitch, float roll, float yaw)
    {
        if (Application.isEditor)
        {
            for (int i = 0; i < stoppers.Length; i++)
            {
                stoppers[i].CopySettings(stopper);
            }
        }
        RotYaw(yaw);
        AddThrust(stoppers[1].Regulate(thrust-speedMeter.GetSpeedGlobal().y));
        Vector3 localVelocity = speedMeter.GetSpeedFlat();
        RotRoll(stoppers[0].Regulate(roll-localVelocity.x));
        RotPitch(stoppers[2].Regulate(pitch-localVelocity.z));
    }
}