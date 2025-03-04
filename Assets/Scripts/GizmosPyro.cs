using UnityEngine;
using System.Collections;

[ExecuteInEditMode] // Permet d'afficher les Gizmos en mode édition
public class GizmosScript : MonoBehaviour
{
    public Vector3 scale = new Vector3(0.2f, 0.2f, 0.2f);

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, scale);
    }
}
