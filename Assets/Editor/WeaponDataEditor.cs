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
            if (script.rightClickAction != WeaponRightClickAction.Melee) excluded.Add("meleeInfo");
            if (script.fireMode != FireMode.Burst && script.altFireMode != FireMode.Burst) excluded.Add("burstInfo");
            if (script.rightClickAction != WeaponRightClickAction.FireModeToggle) excluded.Add("altFireMode");
            DrawPropertiesExcluding(serializedObject, excluded.ToArray());
            
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
