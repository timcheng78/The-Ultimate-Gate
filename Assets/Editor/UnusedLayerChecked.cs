using UnityEditor;
using UnityEngine;

public class UnusedLayerChecked : EditorWindow
{
    [MenuItem("Tools/Check Unused Layers")]
    public static void ShowWindow()
    {
        GetWindow<UnusedLayerChecked>("Check Unused Layers");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Check Unused Layers"))
        {
            CheckUnusedLayers();
        }

        GUILayout.Space(20); // 分隔區塊

        GUILayout.Label("MeshCollider Checker", EditorStyles.boldLabel);

        if (GUILayout.Button("Find GameObjects with MeshCollider"))
        {
            FindMeshColliderObjects();
        }
    }

    private static void CheckUnusedLayers()
    {
        int layersCount = 32; // Unity支援32個Layer
        bool[] layerUsed = new bool[layersCount];

        // 檢查場景中所有的GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            layerUsed[obj.layer] = true;
        }

        // 列出未使用的Layer
        for (int i = 0; i < layersCount; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName) && !layerUsed[i])
            {
                Debug.Log($"Layer '{layerName}' (Layer {i}) is not used.");
            }
        }
    }

    private static void FindMeshColliderObjects()
    {
        // 找到場景中所有的GameObjects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // 檢查每個物件是否有MeshCollider
            MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                Debug.Log($"GameObject '{obj.name}' uses a MeshCollider.", obj);
            }
        }
    }
}
