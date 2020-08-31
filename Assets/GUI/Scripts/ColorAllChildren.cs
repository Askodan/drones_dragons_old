using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ColorAllChildren : MonoBehaviour
{
    public Color color;
    public System.Type[] typesOfTargets;
    // Start is called before the first frame update
    void Start()
    {
        var texts = GetComponentsInChildren<Text>();
        foreach(var element in texts)
        {
            element.color = color;
        }
        var images = GetComponentsInChildren<Image>();
        foreach (var element in images)
        {
            element.color = color;
        }
        
    }
    
}
