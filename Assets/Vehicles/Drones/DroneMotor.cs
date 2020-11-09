using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMotor : MonoBehaviour
{
    private float targetThrust;
    public float TargetThrust
    {
        get { return targetThrust; }
        set
        {
            targetThrust = Mathf.Clamp(value, -1f, 1f);
        }
    }
    public AnimationCurve RotationSpeedByThrust;
    public float CurrentThrust { get; private set; }

    public float Rotation { get { return Direction * CurrentThrust; } }
    private float Direction { get; set; }
    public float Speed { get { return maxVel * Direction * RotationSpeedByThrust.Evaluate(Mathf.Abs(CurrentThrust)); } }

    public float maxVel = 5000f;
    public float acceleration = 20000f;
    private Propeller propeller;
    private MotorSounds motorSounds;
    private void Update()
    {
        CurrentThrust = Mathf.MoveTowards(CurrentThrust, targetThrust, acceleration / maxVel * Time.deltaTime);
    }
    public void Rotate()
    {
        propeller.Rotate(Speed, CurrentThrust);
        motorSounds?.MakeSound(Speed);
    }

    public void Setup(bool left)
    {
        propeller = GetComponentInChildren<Propeller>();
        motorSounds = GetComponent<MotorSounds>();
        if (left)
        {
            Direction = -1f;
        }
        else
        {
            Direction = 1f;
        }
        propeller.Setup(left);
    }
}
