using UnityEngine;
using System.Collections;
[System.Serializable]
public class PIDController
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
    float integral;
    /// <summary>
    /// The derivative gain.
    /// </summary>
    [Tooltip("Wzmocnienie członu D")]
    public float derivativeConst = 0.2f;
    /// <summary>
    /// Makes clamps output to <-1, 1>.
    /// </summary>
    [Tooltip("Ograniczenie wyjścia do -1 - 1")]
    public bool BIBO;
    float prevError;
    /// <summary>
    /// Regulate the specified errorValue.
    /// </summary>
    /// <param name="errorValue">Error value.</param>
    public float Regulate(float errorValue)
    {
        integral += errorValue * Time.deltaTime;
        if (Mathf.Abs(integral) > maxIntegralAbs)
        {
            integral = Mathf.Sign(integral) * maxIntegralAbs;
        }
        float derivative = (errorValue - prevError) / Time.deltaTime;
        prevError = errorValue;
        float result = (proportionalConst * errorValue + integralConst * integral + derivativeConst * derivative);
        if (BIBO)
        {
            result = Mathf.Clamp(result, -1f, 1f);
        }
        return result * overallAmplify;

    }
    /// <summary>
    /// Resets the intergral to value 0.
    /// </summary>
    public void ResetIntergral()
    {
        integral = 0f;
    }
    /// <summary>
    /// Copy settings from another controller
    /// </summary>
    public void CopySettings(PIDController controller)
    {
        overallAmplify = controller.overallAmplify;
        proportionalConst = controller.proportionalConst;
        integralConst = controller.integralConst;
        maxIntegralAbs = controller.maxIntegralAbs;
        derivativeConst = controller.derivativeConst;
        BIBO = controller.BIBO;
    }
}
