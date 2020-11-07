using UnityEngine;
using System.Collections;

public class AltitudeMeter : MonoBehaviour
{
    public LayerMask mask;
    public float height { get; private set; }
    public float altitude { get { return transform.position.y; } }
    public float offset;
    private void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position - transform.up * offset, Vector3.down, out hit, 1000f, mask))
        {
            height = hit.distance;
        }
    }
}
