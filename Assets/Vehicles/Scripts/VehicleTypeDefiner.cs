using UnityEngine;
using System.Collections;
public enum VehicleType
{
    Drone,
    Tractor,
    Robot,
    Unknown,
}
public class VehicleTypeDefiner : MonoBehaviour
{
    public VehicleType vehicleType;
    public Color color;
    public bool changeColor;
    public IndexAndMat[] ColorChanging;
    void Awake()
    {
        if (changeColor)
        {
            for (int i = 0; i < ColorChanging.Length; i++)
            {
                for (int j = 0; j < ColorChanging[i].renderer.Length; j++)
                {
                    ColorChanging[i].renderer[j].materials[ColorChanging[i].index].color = color;
                    //ColorChanging [i].renderer[j].materials [ColorChanging [i].index] = ColorChanging [i].material;
                }
            }
        }
    }
}
[System.Serializable]
public struct IndexAndMat
{
    public int index;
    public Renderer[] renderer;
}
