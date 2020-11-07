using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class PIDControllerFactory
{
    /// <summary>
    /// The overall amplify.
    /// </summary>
    [Tooltip("Wzmocnienie całego wyjścia")]
    public float overallAmplify = 1f;
    /// <summary>
    /// The proportional gain.
    /// </summary>
    [Tooltip("Wzmocnienie członu P")]
    public float proportionalConst = 1f;
    /// <summary>
    /// The integral  gain.
    /// </summary>
    [Tooltip("Wzmocnienie członu I")]
    public float integralConst = 0.1f;
    /// <summary>
    /// The max absolute value of the integral. Should be positive
    /// </summary>
    [Tooltip("Ograniczenie siły całki")]
    public float maxIntegralAbs = 1f;
    /// <summary>
    /// The derivative gain.
    /// </summary>
    [Tooltip("Wzmocnienie członu D")]
    public float derivativeConst = 0.2f;
    /// <summary>
    /// Makes clamps output to <-overallAmplify, overallAmplify>.
    /// </summary>
    [Tooltip("Ograniczenie wyjścia do [-overallAmplify, overallAmplify]")]
    public bool BIBO;
    public PIDController CreatePID()
    {
        var PID = new PIDController();
        PID.overallAmplify = overallAmplify;
        PID.proportionalConst = proportionalConst;
        PID.integralConst = integralConst;
        PID.maxIntegralAbs = maxIntegralAbs;
        PID.derivativeConst = derivativeConst;
        PID.BIBO = BIBO;
        return PID;
    }
}
