using UnityEngine;
using System.Collections;

[ExecuteInEditMode]

public class GizmosAnemo : MonoBehaviour
{
    public Vector3 scale = new Vector3(0.2f, 0.2f, 0.2f);

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.7f, 0.9f, 1.0f); // Light Cyan for Wind
        Gizmos.DrawCube(transform.position, scale);
    }
}
