using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FontSetter : EditorWindow
{
    TMP_FontAsset fontToApply;
    bool applyToSceneObjects = true;
    bool applyToPrefabs = false;

    [MenuItem("Tools/Apply Font To UI Texts")]
    public static void ShowWindow()
    {
        GetWindow<FontSetter>("Font Applier");
    }

    void OnGUI()
    {
        GUILayout.Label("Apply Font to UI Texts", EditorStyles.boldLabel);
        fontToApply = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", fontToApply, typeof(TMP_FontAsset), false);

        applyToSceneObjects = EditorGUILayout.ToggleLeft("Apply to Scene Objects", applyToSceneObjects);
        applyToPrefabs = EditorGUILayout.ToggleLeft("Apply to Prefabs", applyToPrefabs);

        if (GUILayout.Button("Apply Font"))
        {
            if (fontToApply == null)
            {
                EditorUtility.DisplayDialog("Missing Font", "Please assign a TMP_FontAsset before applying.", "OK");
                return;
            }

            if (applyToSceneObjects) ApplyToSceneObjects();
            if (applyToPrefabs) ApplyToPrefabs();
        }
    }

    void ApplyToSceneObjects()
    {
        int count = 0;
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in texts)
            {
                if (tmp.font != fontToApply)
                {
                    Undo.RecordObject(tmp, "Apply Font");
                    tmp.font = fontToApply;
                    EditorUtility.SetDirty(tmp);
                    count++;
                }
            }
        }
        Debug.Log($"Applied font to {count} TextMeshProUGUI components in scene.");
    }

    void ApplyToPrefabs()
    {
        string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in prefabPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            bool changed = false;

            foreach (var tmp in texts)
            {
                if (tmp.font != fontToApply)
                {
                    tmp.font = fontToApply;
                    EditorUtility.SetDirty(prefab);
                    changed = true;
                    count++;
                }
            }

            if (changed)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }

        Debug.Log($"Applied font to {count} TextMeshProUGUI components in prefabs.");
    }

}



