using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour
{
    private float PowerMultiplier { get; set; }
    public GameObject SmudgePrefab;
    private SmudgeRotation smudger;
    public float CurrentRotationSpeed { get; set; }
    private float maxVel = 18000f;
    private int id;
    public int ID { get; }
    // Start is called before the first frame update
    void Awake()
    {
    }
    public void Rotate()
    {
        transform.Rotate(0, 0, maxVel * CurrentRotationSpeed * PowerMultiplier * Time.deltaTime);
        if (smudger)
            smudger.Smudge(CurrentRotationSpeed);
    }
    public void Setup(bool left, int i)
    {
        id = i;
        if (left)
        {
            PowerMultiplier = -1f;
        }
        else
        {
            PowerMultiplier = 1f;
        }
        if (SmudgePrefab)
        {
            smudger = Instantiate(SmudgePrefab).GetComponent<SmudgeRotation>();
            smudger.transform.SetParent(transform);
            smudger.transform.localPosition = Vector3.zero;
            if (left)
            {
                smudger.transform.localRotation = Quaternion.Euler(0f, 180f, 80f);
            }
            else
            {
                smudger.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
