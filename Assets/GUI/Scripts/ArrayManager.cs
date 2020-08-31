using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrayManager : MonoBehaviour
{
    public GameObject[] elements;
    public Vector2 offset;
    public Vector2 startOffset;
    protected Vector2 cumulative_offset;
    void Start()
    {
        Setup();   
    }

    public virtual void Setup()
    {
        cumulative_offset = startOffset;
        foreach (var element in elements)
        {
            var newOne = Instantiate(element, transform);
            newOne.transform.localPosition = cumulative_offset;
            cumulative_offset += offset;
        }
    }
}
