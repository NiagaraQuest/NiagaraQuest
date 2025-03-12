using UnityEngine;
using System.Collections.Generic;

public class Board : MonoBehaviour
{
    private Dictionary<string, List<GameObject>> Paths = new Dictionary<string, List<GameObject>>();

    void Start()
    {
        AddPath("PyroPath");
        AddPath("AnemoPath");
        AddPath("HydroPath");
        AddPath("GeoPath");
    }

    private void AddPath(string pathName)
    {
        GameObject pathObject = GameObject.Find(pathName);

        if (pathObject != null)
        {
            List<GameObject> waypoints = new List<GameObject>();

            for (int i = 0; i < pathObject.transform.childCount; i++)
            {
                GameObject waypoint = pathObject.transform.GetChild(i).gameObject;
                waypoints.Add(waypoint);
            }

            Paths[pathName] = waypoints;
        }
        else
        {
            Debug.LogWarning($"❌ Chemin {pathName} introuvable !");
        }
    }

    //  Retourne un waypoint spécifique
    public GameObject GetTile(string pathName, int index)
    {
        if (Paths.ContainsKey(pathName))
        {
            List<GameObject> waypoints = Paths[pathName];

            if (index >= 0 && index < waypoints.Count)
            {
                return waypoints[index];
            }
            else
            {
                Debug.LogWarning($"⚠️ Index {index} hors limites pour le chemin {pathName} !");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Chemin {pathName} inconnu !");
        }

        return null;
    }

    //  Retourne l'index d'un waypoint dans son chemin
    public int GetWaypointIndex(string pathName, GameObject waypoint)
    {
        if (Paths.ContainsKey(pathName))
        {
            return Paths[pathName].IndexOf(waypoint);
        }
        return -1;
    }


    //  Retourne un chemin entier (utile pour vérifier s'il existe)
    public List<GameObject> GetPath(string pathName)
    {
        if (Paths.ContainsKey(pathName))
        {
            return Paths[pathName];
        }
        else
        {
            Debug.LogWarning($"❌ Chemin {pathName} non enregistré dans Board !");
            return null;
        }
    }

    //  Vérifie si un chemin existe
    public bool PathExists(string pathName)
    {
        return Paths.ContainsKey(pathName);
    }

}


