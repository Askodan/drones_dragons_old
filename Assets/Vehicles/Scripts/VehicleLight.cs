using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleLight : MonoBehaviour
{
    private Light[] lights;
    void Awake()
    {
        lights = GetComponents<Light>();
    }
    public void Change()
    {
        foreach (var light in lights)
        {
            light.enabled = !light.enabled;
        }
    }
    public void TurnOn()
    {
        Turn(true);
    }
    public void TurnOff()
    {
        Turn(false);
    }
    private void Turn(bool on)
    {
        foreach (var light in lights)
        {
            light.enabled = on;
        }
    }
}
