using UnityEngine;
using System.Collections;

[ExecuteInEditMode] // Permet d'afficher les Gizmos en mode édition

public class GizmosGeo : MonoBehaviour
{
  
    public Vector3 scale = new Vector3(0.2f, 0.2f, 0.2f);

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.6f, 0.3f, 0.1f); // Marron approximatif
        Gizmos.DrawCube(transform.position, scale);
    }


}
