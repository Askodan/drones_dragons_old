using UnityEngine;
using System.Collections;

public abstract class SteeringDrone : MonoBehaviour
{
    protected Propeller[] propellers;
    //ustawienia elektroniki(wzmocnień)
    public float pitchFactor = 0.03f;
    public float rollFactor = 0.03f;
    public float yawFactor = 0.05f;
    public float stabVelFactor = 1f;
    public float selfLevelFactor = 0.2f;
    //ustawienia fizyki
    public float forceFactor = 3.6f;

    protected float thrust;
    protected float pitch;
    protected float roll;
    protected float yaw;

    protected new Rigidbody rigidbody;
    protected float zeroThrust;

    public bool keepAltitude { get; protected set; }

    public bool stabilize { get; protected set; }

    public bool motorsOn { get; protected set; }

    public bool selfLeveling { get; protected set; }

    [HideInInspector] public VehicleLight flashLight;
    protected float propellerDistFromCenter;
    protected CenterOfMass centerOfMass;

    void Awake()
    {
        FindReferenes();
        CheckPropellers();
        Setup();
    }

    protected void FindReferenes()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.maxAngularVelocity = 100;
        centerOfMass = GetComponentInChildren<CenterOfMass>();
        rigidbody.centerOfMass = centerOfMass.transform.localPosition;
        propellers = GetComponentsInChildren<Propeller>();
        flashLight = GetComponentInChildren<VehicleLight>();
    }

    protected abstract void CheckPropellers();
    protected abstract void Setup();

    void Update()
    {
        RotatePropellers(propellers);
    }

    void RotatePropellers(Propeller[] _propellers)
    {
        for (int i = 0; i < _propellers.Length; i++)
        {
            _propellers[i].Rotate();
        }
    }

    public void Steer(float axis_Thrust, float axis_Pitch, float axis_Roll, float axis_Yaw, float axis_Turbo,
        bool butDown_Lights, bool butDown_Motors, bool butDown_Stabilize, bool butDown_KeepAltitude, bool butDown_SelfLeveling)
    {
        thrust = axis_Thrust;
        pitch = axis_Pitch * pitchFactor;
        roll = axis_Roll * rollFactor;
        yaw = axis_Yaw * yawFactor;
        if (butDown_Lights)
        {
            flashLight.Change();
        }
        if (butDown_Motors)
        {
            motorsOn = !motorsOn;
        }
        if (butDown_Stabilize)
        {
            stabilize = !stabilize;
        }
        if (butDown_KeepAltitude)
        {
            keepAltitude = !keepAltitude;
        }
        if (keepAltitude)
        {
            thrust = Mathf.Clamp(thrust + zeroThrust, -1, 1);
        }
        if (butDown_SelfLeveling)
        {
            selfLeveling = !selfLeveling;
        }
    }

    void FixedUpdate()
    {
        if (motorsOn)
        {
            ApplyPropellersThrust();
        }
    }

    protected abstract void ApplyPropellersThrust();
}

public enum DroneType
{
    quadrocopter,
    heksacopter,
    octacopter,
    prototype
}