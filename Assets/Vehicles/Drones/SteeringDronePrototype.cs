using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringDronePrototype : SteeringDroneQuadrocopter
{
    [Tooltip("Speed of moving wings")]
    public float wingsSpeed = 36f;
    [Tooltip("Power of additional motors")]
    public float turboSpeed = 50f;
    public Vector3 FrontRotationAuto = new Vector3(0f, 0f, 270f);
    public Vector3 FrontRotationManual = new Vector3(0f, 0f, 135f);
    public Vector3 RearRotationAuto = new Vector3(0f, 0f, 0f);
    public Vector3 RearRotationManual = new Vector3(0f, 0f, 315f);
    private Coroutine moveWingsCoroutine;
    private bool isMoving;
    protected int RightFrontTurboMotorIndex = -1;
    protected int RightRearTurboMotorIndex = -1;
    protected int LeftRearTurboMotorIndex = -1;
    protected int LeftFrontTurboMotorIndex = -1;
    protected override void CheckMotors()
    {
        if (motors.Length != 8)
        {
            Debug.LogError("Number of motors doesn't match! Should be 8 is " + motors.Length.ToString());
        }
    }
    protected override void Setup()
    {
        base.Setup();
        SelfLevelingChanged += WingsUp;
        WingsUp(selfLeveling);
    }
    private void WingsUp(bool up)
    {
        if (moveWingsCoroutine != null)
            StopCoroutine(moveWingsCoroutine);
        moveWingsCoroutine = StartCoroutine(MoveWings(!up));
    }
    protected override void AssignMotorsIndexes()
    {
        float[] distances = new float[motors.Length];
        float average_distance = CalcAverageMotorsDistanceFromCenter(distances);
        for (int i = 0; i < motors.Length; i++)
        {
            Vector3 pos = transform.InverseTransformPoint(motors[i].transform.position);
            motors[i].Setup(IsRight(pos) ^ IsFront(pos));
            if (distances[i] < average_distance)
            {
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
            else
            {
                if (IsRight(pos))
                {
                    if (IsFront(pos))
                    {
                        RightFrontTurboMotorIndex = i;
                    }
                    else
                    {
                        RightRearTurboMotorIndex = i;
                    }
                }
                else
                {
                    if (IsFront(pos))
                    {
                        LeftFrontTurboMotorIndex = i;
                    }
                    else
                    {
                        LeftRearTurboMotorIndex = i;
                    }
                }
            }
        }
    }
    protected float CalcAverageMotorsDistanceFromCenter(float[] distances)
    {
        float sum_of_distances = 0;
        for (int i = 0; i < motors.Length; i++)
        {
            distances[i] = Vector3.Distance(motors[i].transform.position, transform.TransformPoint(rigidbody.centerOfMass));
            sum_of_distances += distances[i];
        }
        return sum_of_distances / distances.Length;
    }
    protected override void CheckMotorsIndexes()
    {
        base.CheckMotorsIndexes();
        if (RightFrontTurboMotorIndex < 0 ||
            RightRearTurboMotorIndex < 0 ||
            LeftFrontTurboMotorIndex < 0 ||
            LeftRearTurboMotorIndex < 0)
        {
            Debug.LogError("Some turbo motors indexes weren't found");
        }
    }
    IEnumerator MoveWings(bool up)
    {
        isMoving = true;
        if (up)
        {
            while (CheckRotateWing(LeftFrontTurboMotorIndex, FrontRotationAuto) &&
                   CheckRotateWing(RightFrontTurboMotorIndex, FrontRotationAuto) &&
                   CheckRotateWing(LeftRearTurboMotorIndex, RearRotationAuto) &&
                   CheckRotateWing(RightRearTurboMotorIndex, RearRotationAuto))
            {
                RotateWing(LeftFrontTurboMotorIndex, FrontRotationAuto);
                RotateWing(RightFrontTurboMotorIndex, FrontRotationAuto);
                RotateWing(LeftRearTurboMotorIndex, RearRotationAuto);
                RotateWing(RightRearTurboMotorIndex, RearRotationAuto);
                yield return null;
            }
        }
        else
        {
            while (CheckRotateWing(LeftFrontTurboMotorIndex, FrontRotationManual) &&
                   CheckRotateWing(RightFrontTurboMotorIndex, FrontRotationManual) &&
                   CheckRotateWing(LeftRearTurboMotorIndex, RearRotationManual) &&
                   CheckRotateWing(RightRearTurboMotorIndex, RearRotationManual))
            {
                RotateWing(LeftFrontTurboMotorIndex, FrontRotationManual);
                RotateWing(RightFrontTurboMotorIndex, FrontRotationManual);
                RotateWing(LeftRearTurboMotorIndex, RearRotationManual);
                RotateWing(RightRearTurboMotorIndex, RearRotationManual);
                yield return null;
            }
        }
        isMoving = false;
    }
    private bool CheckRotateWing(int MotorIndex, Vector3 TargetRotation)
    {
        return motors[MotorIndex].transform.parent.localRotation != Quaternion.Euler(TargetRotation);
    }
    private void RotateWing(int MotorIndex, Vector3 TargetRotation)
    {
        motors[MotorIndex].transform.parent.localRotation = Quaternion.RotateTowards(
                    motors[MotorIndex].transform.parent.localRotation,
                    Quaternion.Euler(TargetRotation),
                    Time.deltaTime * wingsSpeed);
    }
    protected override void CalculateMotorsSpeed()
    {
        base.CalculateMotorsSpeed();
        CalcTurbo();
    }
    protected virtual void CalcTurbo()
    {
        if (!isMoving)
        {
            RotTurbo(turbo * turboSpeed);
        }
    }
    protected virtual void RotTurbo(float turbo_val)
    {
        motors[RightFrontTurboMotorIndex].TargetThrust = -turbo_val;
        motors[RightRearTurboMotorIndex].TargetThrust = -turbo_val;
        motors[LeftFrontTurboMotorIndex].TargetThrust = -turbo_val;
        motors[LeftRearTurboMotorIndex].TargetThrust = -turbo_val;
    }
}
