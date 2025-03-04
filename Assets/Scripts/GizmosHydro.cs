using UnityEngine;
using System.Collections;

[ExecuteInEditMode] 

public class GizmosHydro : MonoBehaviour
{
    public Vector3 scale = new Vector3(0.2f, 0.2f, 0.2f);

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position, scale);
    }
}
