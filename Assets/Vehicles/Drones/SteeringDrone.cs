using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Gyroscope), typeof(SpeedMeter), typeof(AltitudeMeter))]
public abstract class SteeringDrone : MonoBehaviour
{
    // main amplification
    public float pitchFactor = 0.3f;
    public float rollFactor = 0.3f;
    public float yawFactor = 0.2f;
    public float forceFactor = 3.6f;

    public float autoYawPower = 100f;
    public PIDController yawKeeper; // A = 1, P = 0.01, I = 0, MI = 1, D = 0.001, BIBO = true

    public float autoAltitudePower = 30f;
    public PIDController altitudeKeeper; // A = 1, P = 1, I = 0.1, MI = 1, D = 0.2, BIBO = true

    public float autoAnglePower = 50f;
    public PIDControllerFactory selfLevelerFactory; // A = 1, P = 0.01, I = 0, MI = 1, D = 0.001, BIBO = true
    protected PIDController[] selfLevelers;

    public PIDControllerFactory stabilizatorFactory; // A = 1, P = 0.03, I = 0.001, MI = 0.1, D = 0.001, BIBO = true
    protected PIDController[] stabilizators;

    public float maxAngle = 50f;
    public float autoSpeedPower = 60f;
    public PIDControllerFactory stopperFactory; // A = 1, P = 0.1, I = 0, MI = 0.5, D = 0, BIBO = true
    protected PIDController[] stoppers;

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
            if (_keepAltitude)
            {
                targetAltitude = altitudeMeter.altitude;
            }
            else
            {
                stabilize = false;
            }
        }
    }
    private bool _selfLeveling;
    public delegate void SelfLevelingChangedHandler(bool selfLeveling);
    public event SelfLevelingChangedHandler SelfLevelingChanged;
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
            else
            {
                targetYaw = gyroscope.GetRotation().y;
            }
            SelfLevelingChanged.Invoke(_selfLeveling);
        }
    }
    // steering values
    protected float thrust { get; set; }
    protected float pitch { get; set; }
    protected float roll { get; set; }
    protected float yaw { get; set; }
    protected float turbo { get; set; }
    protected float targetAltitude { get; set; }
    protected float targetYaw { get; set; }
    // references
    public VehicleLight flashLight { get; protected set; }
    protected new Rigidbody rigidbody { get; set; }
    protected Propeller[] propellers { get; set; }
    protected CenterOfMass centerOfMass { get; set; }
    protected Gyroscope gyroscope { get; set; }
    protected SpeedMeter speedMeter { get; set; }
    protected AltitudeMeter altitudeMeter { get; set; }

    void Awake()
    {
        FindReferenes();
        CheckPropellers();
        Setup();

        PreparePIDControllers(stopperFactory, ref stoppers, 2);
        PreparePIDControllers(selfLevelerFactory, ref selfLevelers, 2);
        PreparePIDControllers(stabilizatorFactory, ref stabilizators, 3);
    }
    protected void FindReferenes()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.maxAngularVelocity = 100;
        centerOfMass = GetComponentInChildren<CenterOfMass>();
        rigidbody.centerOfMass = centerOfMass.transform.localPosition;
        propellers = GetComponentsInChildren<Propeller>();
        flashLight = GetComponentInChildren<VehicleLight>();
        gyroscope = GetComponentInChildren<Gyroscope>();
        speedMeter = GetComponentInChildren<SpeedMeter>();
        altitudeMeter = GetComponentInChildren<AltitudeMeter>();
    }

    protected abstract void CheckPropellers();
    protected abstract void Setup();

    protected void PreparePIDControllers(PIDControllerFactory PID_factory, ref PIDController[] PID_array, int PID_num)
    {
        PID_array = new PIDController[PID_num];
        for (int i = 0; i < PID_array.Length; i++)
        {
            PID_array[i] = PID_factory.CreatePID();
        }
    }

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
        pitch = axis_Pitch;
        roll = axis_Roll;
        yaw = axis_Yaw;
        turbo = axis_Turbo;
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
    }

    void FixedUpdate()
    {
        if (motorsOn)
        {
            CalculatePropellersSpeed();
            ApplyPropellersThrust();
        }
    }

    protected virtual void CalculatePropellersSpeed()
    {
        ClearMotors();
        CalcPitchRoll();
        CalcYaw();
        CalcThrust();
        RegulateAngularSpeed();
    }
    protected virtual void ClearMotors()
    {
        for (int i = 0; i < propellers.Length; i++)
        {
            propellers[i].CurrentRotationSpeed = 0;
        }
    }
    protected abstract void AddThrust(float thrust_val);
    protected abstract void RotPitch(float pitch_val);
    protected abstract void RotYaw(float yaw_val);
    protected abstract void RotRoll(float roll_val);
    protected void CalcPitchRoll()
    {
        if (!selfLeveling && !stabilize)
        {
            RotPitch(pitch * pitchFactor);
            RotRoll(roll * rollFactor);
        }
        if (selfLeveling)
        {
            if (!stabilize)
            {
                RegulateAngle(pitch * autoAnglePower, roll * autoAnglePower);
            }
            else
            {
                RegulateSpeed(pitch * SpeedMeter.Kmph2Mps(autoSpeedPower), roll * SpeedMeter.Kmph2Mps(autoSpeedPower));
            }
        }
    }
    protected void CalcYaw()
    {
        if (selfLeveling)
        {
            targetYaw += autoYawPower * yaw * Time.deltaTime;
            targetYaw = Mathf.Repeat(targetYaw, 360f);
            float current_yaw = gyroscope.GetRotation().y;
            float delta = targetYaw - current_yaw;
            delta = Mathf.Repeat(delta + 180f, 360f) - 180f;
            RotYaw(yawKeeper.Regulate(delta));
        }
        else
        {
            RotYaw(yaw * yawFactor);
        }
    }
    protected void CalcThrust()
    {
        if (keepAltitude)
        {
            targetAltitude += SpeedMeter.Kmph2Mps(autoAltitudePower) * thrust * Time.deltaTime;
            AddThrust(altitudeKeeper.Regulate(targetAltitude - altitudeMeter.altitude));
        }
        else
        {
            AddThrust(thrust);
        }
    }
    protected void RegulateSpeed(float speed_pitch, float speed_roll)
    {
        if (Application.isEditor)
        {
            for (int i = 0; i < stoppers.Length; i++)
            {
                stoppers[i].CopySettings(stopperFactory);
            }
        }
        Vector3 localVelocity = speedMeter.GetSpeedFlat();
        float str = stoppers[0].Regulate(speed_roll - localVelocity.x);
        float stp = stoppers[1].Regulate(speed_pitch - localVelocity.z);
        RegulateAngle(stp * maxAngle, str * maxAngle);
    }
    protected void RegulateAngle(float target_pitch_angle, float target_roll_angle)
    {
        if (Application.isEditor)
        {
            for (int i = 0; i < 2; i++)
            {
                selfLevelers[i].CopySettings(selfLevelerFactory);
            }
        }
        float pitch_val = Gyroscope.Angle2OneMinusOne(gyroscope.GetRotation().x) * 180f;
        float slp = selfLevelers[0].Regulate(target_pitch_angle - pitch_val);
        float roll_val = Gyroscope.Angle2OneMinusOne(gyroscope.GetRotation().z) * 180f;
        float slr = selfLevelers[1].Regulate(target_roll_angle + roll_val);

        RotPitch(slp);
        RotRoll(slr);
    }
    protected void RegulateAngularSpeed()
    {
        if (Application.isEditor)
        {
            for (int i = 0; i < stabilizators.Length; i++)
            {
                stabilizators[i].CopySettings(stabilizatorFactory);
            }
        }
        Vector3 localangularvelocity = gyroscope.GetLocalAngularVelocity();
        RotPitch(stabilizators[0].Regulate(-localangularvelocity.x));
        RotYaw(stabilizators[1].Regulate(-localangularvelocity.y));
        RotRoll(stabilizators[2].Regulate(localangularvelocity.z));
    }
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
