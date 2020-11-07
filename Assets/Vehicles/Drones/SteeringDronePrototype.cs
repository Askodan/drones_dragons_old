using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringDronePrototype : SteeringDroneQuadrocopter
{
    [Tooltip("Speed of moving wings")]
    public float wingsSpeed = 36f;
    [Tooltip("Power of additional propellers")]
    public float turboSpeed = 50f;
    public Vector3 FrontRotationAuto = new Vector3(0f, 0f, 270f);
    public Vector3 FrontRotationManual = new Vector3(0f, 0f, 135f);
    public Vector3 RearRotationAuto = new Vector3(0f, 0f, 0f);
    public Vector3 RearRotationManual = new Vector3(0f, 0f, 315f);
    private Coroutine moveWingsCoroutine;
    private bool isMoving;
    protected int RightFrontTurboPropellersIndex = -1;
    protected int RightRearTurboPropellersIndex = -1;
    protected int LeftRearTurboPropellersIndex = -1;
    protected int LeftFrontTurboPropellersIndex = -1;
    protected override void CheckPropellers()
    {
        if (propellers.Length != 8)
        {
            Debug.LogError("Number of propellers doesn't match! Should be 8 is " + propellers.Length.ToString());
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
    protected override void AssignPropellersIndexes()
    {
        float[] distances = new float[propellers.Length];
        float average_distance = CalcAveragePropellersDistanceFromCenter(distances);
        for (int i = 0; i < propellers.Length; i++)
        {
            Vector3 pos = transform.InverseTransformPoint(propellers[i].transform.position);
            propellers[i].Setup(IsRight(pos) ^ IsFront(pos));
            if (distances[i] < average_distance)
            {
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
            else
            {
                if (IsRight(pos))
                {
                    if (IsFront(pos))
                    {
                        RightFrontTurboPropellersIndex = i;
                    }
                    else
                    {
                        RightRearTurboPropellersIndex = i;
                    }
                }
                else
                {
                    if (IsFront(pos))
                    {
                        LeftFrontTurboPropellersIndex = i;
                    }
                    else
                    {
                        LeftRearTurboPropellersIndex = i;
                    }
                }
            }
        }
    }
    protected float CalcAveragePropellersDistanceFromCenter(float[] distances)
    {
        float sum_of_distances = 0;
        for (int i = 0; i < propellers.Length; i++)
        {
            distances[i] = Vector3.Distance(propellers[i].transform.position, transform.TransformPoint(rigidbody.centerOfMass));
            sum_of_distances += distances[i];
        }
        return sum_of_distances / distances.Length;
    }
    protected override void CheckPropellerIndexes()
    {
        base.CheckPropellerIndexes();
        if (RightFrontTurboPropellersIndex < 0 ||
            RightRearTurboPropellersIndex < 0 ||
            LeftFrontTurboPropellersIndex < 0 ||
            LeftRearTurboPropellersIndex < 0)
        {
            Debug.LogError("Some turbo propellers indexes weren't found");
        }
    }
    IEnumerator MoveWings(bool up)
    {
        isMoving = true;
        if (up)
        {
            while (CheckRotateWing(LeftFrontTurboPropellersIndex, FrontRotationAuto) &&
                   CheckRotateWing(RightFrontTurboPropellersIndex, FrontRotationAuto) &&
                   CheckRotateWing(LeftRearTurboPropellersIndex, RearRotationAuto) &&
                   CheckRotateWing(RightRearTurboPropellersIndex, RearRotationAuto))
            {
                RotateWing(LeftFrontTurboPropellersIndex, FrontRotationAuto);
                RotateWing(RightFrontTurboPropellersIndex, FrontRotationAuto);
                RotateWing(LeftRearTurboPropellersIndex, RearRotationAuto);
                RotateWing(RightRearTurboPropellersIndex, RearRotationAuto);
                yield return null;
            }
        }
        else
        {
            while (CheckRotateWing(LeftFrontTurboPropellersIndex, FrontRotationManual) &&
                   CheckRotateWing(RightFrontTurboPropellersIndex, FrontRotationManual) &&
                   CheckRotateWing(LeftRearTurboPropellersIndex, RearRotationManual) &&
                   CheckRotateWing(RightRearTurboPropellersIndex, RearRotationManual))
            {
                RotateWing(LeftFrontTurboPropellersIndex, FrontRotationManual);
                RotateWing(RightFrontTurboPropellersIndex, FrontRotationManual);
                RotateWing(LeftRearTurboPropellersIndex, RearRotationManual);
                RotateWing(RightRearTurboPropellersIndex, RearRotationManual);
                yield return null;
            }
        }
        isMoving = false;
    }
    private bool CheckRotateWing(int PropellerIndex, Vector3 TargetRotation)
    {
        return propellers[PropellerIndex].transform.parent.localRotation != Quaternion.Euler(TargetRotation);
    }
    private void RotateWing(int PropellerIndex, Vector3 TargetRotation)
    {
        propellers[PropellerIndex].transform.parent.localRotation = Quaternion.RotateTowards(
                    propellers[PropellerIndex].transform.parent.localRotation,
                    Quaternion.Euler(TargetRotation),
                    Time.deltaTime * wingsSpeed);
    }
    protected override void CalculatePropellersSpeed()
    {
        base.CalculatePropellersSpeed();
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
        propellers[RightFrontTurboPropellersIndex].CurrentRotationSpeed = -turbo_val;
        propellers[RightRearTurboPropellersIndex].CurrentRotationSpeed = -turbo_val;
        propellers[LeftFrontTurboPropellersIndex].CurrentRotationSpeed = -turbo_val;
        propellers[LeftRearTurboPropellersIndex].CurrentRotationSpeed = -turbo_val;
    }
}
