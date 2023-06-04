using System.Collections.Generic;
using UnityEditor;
using Weapons;

namespace Editor
{
    [CustomEditor(typeof(WeaponData))]
    public class WeaponDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            WeaponData script = (WeaponData)target;

            List<string> excluded = new() { "m_Script" };
            if (!script.hasMelee) excluded.Add("meleeInfo");
            if (script.fireMode != FireMode.Burst) excluded.Add("burstInfo");
            DrawPropertiesExcluding(serializedObject, excluded.ToArray());
            
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
