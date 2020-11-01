using UnityEngine;
using System.Collections;

public class SteeringDrone : MonoBehaviour
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

    public bool keepAltitude { get; set; }

    public bool stabilize { get; set; }

    public bool motorsOn { get; set; }

    public bool selfLeveling { get; set; }

    [HideInInspector] public VehicleLight flashLight;
    protected float propellerDistFromCenter;
    protected CenterOfMass centerOfMass;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.maxAngularVelocity = 100;
        centerOfMass = GetComponentInChildren<CenterOfMass>();
        rigidbody.centerOfMass = centerOfMass.transform.localPosition;
        propellers = GetComponentsInChildren<Propeller>();
        flashLight = GetComponentInChildren<VehicleLight>();
        CheckPropellers();
        Setup();
    }

    void Update()
    {
        RotatePropellers(propellers);
    }

    public void Steer(float axis_Thrust, float axis_Pitch, float axis_Roll, float axis_Yaw, float axis_Turbo,
        bool butDown_Lights, bool butDown_Motors, bool butDown_Stabilize, bool butDown_KeepAltitude, bool butDown_SelfLeveling)
    {
        if (motorsOn)
        {
            thrust = axis_Thrust;
            pitch = axis_Pitch * pitchFactor;
            roll = axis_Roll * rollFactor;
            yaw = axis_Yaw * yawFactor;

            if (keepAltitude)
            {
                thrust = Mathf.Clamp(thrust + zeroThrust, -1, 1);
            }
            for (int i = 0; i < propellers.Length; i++)
            {
                propellers[i].CurrentRotationSpeed = thrust;
            }
            propellers[0].CurrentRotationSpeed += -pitch - roll;
            propellers[1].CurrentRotationSpeed += pitch - roll;
            propellers[2].CurrentRotationSpeed += -pitch + roll;
            propellers[3].CurrentRotationSpeed += pitch + roll;

        }
        if (butDown_Lights)
        {
            flashLight.Change();
        }
        if (butDown_Motors)
        {
            motorsOn = !motorsOn;
            if (!motorsOn)
            {
                for (int i = 0; i < propellers.Length; i++)
                {
                    propellers[i].CurrentRotationSpeed = 0f;
                }
            }
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
            //if (moveWingsCoroutine != null)
            //    StopCoroutine(moveWingsCoroutine);
            //moveWingsCoroutine = StartCoroutine(MoveWings());
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

    public void ApplyPropellersThrust()
    {

        float[] cor = new float[propellers.Length];
        if (selfLeveling)
        {
            float averageOfPropellersAltitude = calcAverageOfPropellersAltitude();

            float[] propellerAngleFromFlat = new float[4];
            for (int i = 0; i < propellers.Length; i++)
            {
                propellerAngleFromFlat[i] = Mathf.Asin((averageOfPropellersAltitude - propellers[i].transform.position.y) / propellerDistFromCenter);
            }
            float[] pitchRollCor = new float[4];
            //x+
            pitchRollCor[0] = (propellerAngleFromFlat[0] + propellerAngleFromFlat[1]) / 2f;
            //x-
            pitchRollCor[1] = (propellerAngleFromFlat[2] + propellerAngleFromFlat[3]) / 2f;
            //z+
            pitchRollCor[2] = (propellerAngleFromFlat[0] + propellerAngleFromFlat[2]) / 2f;
            //z-
            pitchRollCor[3] = (propellerAngleFromFlat[1] + propellerAngleFromFlat[3]) / 2f;
            cor[0] = pitchRollCor[0] * pitch + propellerAngleFromFlat[0] * (1 - pitch);
            cor[1] = pitchRollCor[0] * pitch + propellerAngleFromFlat[1] * (1 - pitch);
            cor[2] = pitchRollCor[1] * pitch + propellerAngleFromFlat[2] * (1 - pitch);
            cor[3] = pitchRollCor[1] * pitch + propellerAngleFromFlat[3] * (1 - pitch);
            cor[0] += pitchRollCor[2] * roll + propellerAngleFromFlat[0] * (1 - roll);
            cor[1] += pitchRollCor[3] * roll + propellerAngleFromFlat[1] * (1 - roll);
            cor[2] += pitchRollCor[2] * roll + propellerAngleFromFlat[2] * (1 - roll);
            cor[3] += pitchRollCor[3] * roll + propellerAngleFromFlat[3] * (1 - roll);
            for (int i = 0; i < propellers.Length; i++)
            {
                cor[i] /= 2;
            }
        }
        for (int i = 0; i < propellers.Length; i++)
        {
            Vector3 thrustVec = new Vector3(0, 0, propellers[i].CurrentRotationSpeed * forceFactor + cor[i] * selfLevelFactor);
            rigidbody.AddForceAtPosition(propellers[i].transform.rotation * thrustVec, propellers[i].transform.position);
            //Debug.DrawLine (propellers [i].position, propellers [i].position + propellers [i].rotation * thrustVec);
        }
        if (stabilize)
        {
            rigidbody.AddTorque(rigidbody.angularVelocity * (-Time.deltaTime * stabVelFactor));
        }
        rigidbody.AddTorque(transform.rotation * new Vector3(0, yaw, 0));
    }

    float calcAverageOfPropellersAltitude()
    {
        float averageOfPropellersAltitude = 0f;
        for (int i = 0; i < propellers.Length; i++)
        {
            averageOfPropellersAltitude += propellers[i].transform.position.y;
        }
        return averageOfPropellersAltitude / 4;
    }

    void RotatePropellers(Propeller[] _propellers)
    {
        for (int i = 0; i < _propellers.Length; i++)
        {
            _propellers[i].Rotate();
        }
    }

    protected void Setup()
    {
        //order x+z+, x+z-, x-z+, x-z-
        Propeller[] screws = new Propeller[propellers.Length];
        for (int i = 0; i < propellers.Length; i++)
        {
            if (transform.InverseTransformPoint(propellers[i].transform.position).x > 0)
            {
                if (transform.InverseTransformPoint(propellers[i].transform.position).z > 0)
                {
                    screws[0] = propellers[i];
                    propellers[i].Setup(true, 0);
                }
                else
                {
                    screws[1] = propellers[i];
                    propellers[i].Setup(false, 1);
                }
            }
            else
            {
                if (transform.InverseTransformPoint(propellers[i].transform.position).z > 0)
                {
                    screws[2] = propellers[i];
                    propellers[i].Setup(false, 2);
                }
                else
                {
                    screws[3] = propellers[i];
                    propellers[i].Setup(true, 3);
                }
            }
        }
        propellers = screws;

        zeroThrust = -Physics.gravity.y / propellers.Length * rigidbody.mass / forceFactor;
        // calc propellers dist from center
        Vector3 propellersCenter = Vector3.zero;
        foreach (var propeller in propellers)
        {
            propellersCenter += propeller.transform.position;
        }
        propellersCenter /= 4f;
        propellerDistFromCenter = Vector3.Distance(propellersCenter, propellers[0].transform.position);
    }

    protected void CheckPropellers()
    {
        if (propellers.Length != 4)
        {
            Debug.LogError("Number of propellers doesn't match! Should be 4 is " + propellers.Length.ToString());
        }
        float[] dists = new float[4];
        dists[0] = Vector3.Distance(propellers[0].transform.position, centerOfMass.transform.position);
        dists[1] = Vector3.Distance(propellers[1].transform.position, centerOfMass.transform.position);
        dists[2] = Vector3.Distance(propellers[2].transform.position, centerOfMass.transform.position);
        dists[3] = Vector3.Distance(propellers[3].transform.position, centerOfMass.transform.position);
        for (int i = 0; i < dists.Length; i++)
        {
            for (int j = i; j < dists.Length; j++)
            {
                if (Mathf.Abs(dists[i] - dists[j]) > 0.0001)
                {
                    Debug.LogError("Distance from center is different for " + i + " " + j);
                }
            }
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