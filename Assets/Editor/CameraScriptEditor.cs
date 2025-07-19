using System.Collections.Generic;
using Player;
using UnityEditor;

namespace Editor
{
    [CustomEditor(typeof(CameraScript)), CanEditMultipleObjects]
    public class CameraScriptEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (targets.Length > 1)
            {
                base.OnInspectorGUI();
                return;
            }

            CameraScript cam = (CameraScript)target;
            EditorGUI.BeginChangeCheck();
            List<string> excluded = new() { "m_Script" };
            if (!cam.columnMode)
            {
                excluded.Add(nameof(CameraScript.columnMaxX));
                excluded.Add(nameof(CameraScript.columnMinX));
            }
            DrawPropertiesExcluding(serializedObject, excluded.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}