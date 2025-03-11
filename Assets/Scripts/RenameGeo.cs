using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RenameGeo : MonoBehaviour
{
    [ContextMenu("Rename Children Permanently")]
    void RenameAllChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).name = "Anemo" + i.ToString("00"); // "Pyro00", "Pyro01", ..., "Pyro49"
        }

        #if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject); // Mark changes to be saved
        #endif
    }
}
