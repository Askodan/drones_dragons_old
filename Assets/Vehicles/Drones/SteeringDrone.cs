using UnityEngine;
using System.Collections;

public abstract class SteeringDrone : MonoBehaviour
{
    // main amplification
    public float pitchFactor = 0.03f;
    public float rollFactor = 0.03f;
    public float yawFactor = 0.05f;
    public float stabVelFactor = 1f;
    public float selfLevelFactor = 0.2f;
    public float forceFactor = 3.6f;
    // modes
    public bool motorsOn { get; protected set; }
    private bool _stabilize;
    public bool stabilize
    {
        get { return _stabilize; }
        protected set
        {
            _stabilize = value;
            if (_stabilize)
            {
                keepAltitude = true;
                selfLeveling = true;
            }
        }
    }
    private bool _keepAltitude;
    public bool keepAltitude
    {
        get { return _keepAltitude; }
        protected set
        {
            _keepAltitude = value;
            if (!_keepAltitude)
            {
                stabilize = false;
            }
        }
    }
    private bool _selfLeveling;
    public bool selfLeveling
    {
        get { return _selfLeveling; }
        protected set
        {
            _selfLeveling = value;
            if (!_selfLeveling)
            {
                stabilize = false;
            }
        }
    }
    // steering values
    protected float thrust { get; set; }
    protected float pitch { get; set; }
    protected float roll { get; set; }
    protected float yaw { get; set; }
    // one time calculated
    protected float zeroThrust { get; set; }
    protected float propellerDistFromCenter { get; set; }
    // references
    public VehicleLight flashLight { get; protected set; }
    protected new Rigidbody rigidbody { get; set; }
    protected Propeller[] propellers { get; set; }
    protected CenterOfMass centerOfMass { get; set; }

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
        if (motorsOn)
        {
            RotatePropellers(propellers);
        }
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
        if (butDown_SelfLeveling)
        {
            selfLeveling = !selfLeveling;
        }
        if (keepAltitude)
        {
            thrust = Mathf.Clamp(thrust + zeroThrust, -1, 1);
        }
    }

    void FixedUpdate()
    {
        if (motorsOn)
        {
            CalculatePropellersSpeed();
            ApplyPropellersThrust();
        }
    }

    protected abstract void CalculatePropellersSpeed();

    protected void ApplyPropellersThrust()
    {
        for (int i = 0; i < propellers.Length; i++)
        {
            Vector3 thrustVec = new Vector3(0, 0, propellers[i].CurrentRotationSpeed * forceFactor);
            rigidbody.AddForceAtPosition(propellers[i].transform.rotation * thrustVec, propellers[i].transform.position);
            rigidbody.AddTorque(transform.rotation * new Vector3(0, propellers[i].Rotation, 0));
            //Debug.DrawLine (propellers [i].position, propellers [i].position + propellers [i].rotation * thrustVec);
        }
    }
}

public enum DroneType
{
    quadrocopter,
    heksacopter,
    octacopter,
    prototype
}