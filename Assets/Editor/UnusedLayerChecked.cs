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

        GUILayout.Space(20); // ���j�϶�

        GUILayout.Label("MeshCollider Checker", EditorStyles.boldLabel);

        if (GUILayout.Button("Find GameObjects with MeshCollider"))
        {
            FindMeshColliderObjects();
        }
    }

    private static void CheckUnusedLayers()
    {
        int layersCount = 32; // Unity�䴩32��Layer
        bool[] layerUsed = new bool[layersCount];

        // �ˬd�������Ҧ���GameObject
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            layerUsed[obj.layer] = true;
        }

        // �C�X���ϥΪ�Layer
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
        // ���������Ҧ���GameObjects
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // �ˬd�C�Ӫ���O�_��MeshCollider
            MeshCollider meshCollider = obj.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                Debug.Log($"GameObject '{obj.name}' uses a MeshCollider.", obj);
            }
        }
    }
}
