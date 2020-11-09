using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMotor : MonoBehaviour
{
    private float targetRotationSpeed;
    public float TargetRotationSpeed
    {
        get { return targetRotationSpeed; }
        set
        {
            targetRotationSpeed = Mathf.Clamp(value, -1f, 1f);
        }
    }
    public float CurrentRotationSpeed { get; private set; }
    public float Rotation { get { return Direction * CurrentRotationSpeed; } }
    private float Direction { get; set; }
    public float Speed { get { return maxVel * Rotation; } }

    public float maxVel = 18000f;
    public float acceleration = 100f;
    private Propeller propeller;
    private MotorSounds motorSounds;
    private void Update()
    {
        CurrentRotationSpeed = Mathf.MoveTowards(CurrentRotationSpeed, targetRotationSpeed, acceleration / maxVel * Time.deltaTime);
    }
    public void Rotate()
    {
        propeller.Rotate(Speed, CurrentRotationSpeed);
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
