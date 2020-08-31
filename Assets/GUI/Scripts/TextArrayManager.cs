using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextArrayManager : ArrayManager
{
    public string[] texts;
    override public void Setup()
    {
        cumulative_offset = startOffset;
        int n = 0;
        foreach (var element in elements)
        {
            var newOne = Instantiate(element, transform);
            newOne.transform.localPosition = cumulative_offset;
            newOne.GetComponentInChildren<Text>().text = texts[n];
            n += 1;
            cumulative_offset += offset;
        }
    }
}
