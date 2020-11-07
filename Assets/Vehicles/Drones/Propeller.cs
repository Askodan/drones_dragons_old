using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Propeller : MonoBehaviour
{
    public GameObject SmudgePrefab;
    private float currentRotationSpeed;
    public float CurrentRotationSpeed { get { return currentRotationSpeed; } set { currentRotationSpeed = Mathf.Clamp(value, -1f, 1f); } }
    public float Rotation { get { return Direction * CurrentRotationSpeed; } }
    private float Direction { get; set; }
    private SmudgeRotation smudger;
    private float maxVel = 18000f;
    // Start is called before the first frame update
    void Awake()
    {
    }
    public void Rotate()
    {
        transform.Rotate(0, 0, maxVel * Rotation * Time.deltaTime);
        if (smudger)
            smudger.Smudge(CurrentRotationSpeed);
    }
    public void Setup(bool left)
    {
        if (left)
        {
            Direction = -1f;
        }
        else
        {
            Direction = 1f;
        }
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
            smudger.Smudge(CurrentRotationSpeed);
        }
    }
}
