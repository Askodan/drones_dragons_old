using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour
{
    public GameObject SmudgePrefab;
    private SmudgeRotation smudger;

    public void Rotate(float speed, float smudge_level)
    {
        transform.Rotate(0, 0, speed * Time.deltaTime);
        if (smudger)
            smudger.Smudge(smudge_level);
    }
    public void Setup(bool left)
    {
        if (SmudgePrefab)
        {
            smudger = Instantiate(SmudgePrefab).GetComponent<SmudgeRotation>();
            smudger.transform.SetParent(transform);
            smudger.transform.localPosition = Vector3.zero;
            if (left)
            {
                smudger.transform.localRotation = Quaternion.Euler(0f, 180f, 90f);
            }
            else
            {
                smudger.transform.localRotation = Quaternion.identity;
            }
            smudger.Smudge(0f);
        }
    }
}
