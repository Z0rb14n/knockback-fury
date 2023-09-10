using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Weapons;

namespace Editor
{
    [CustomEditor(typeof(WeaponData))]
    public class WeaponDataEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            WeaponData script = (WeaponData)target;
            GUI.enabled = false;
            GUILayout.Label("Current DPS: " + script.DPS);
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();

            List<string> excluded = new() { "m_Script" };
            if (script.rightClickAction != WeaponRightClickAction.Melee) excluded.Add("meleeInfo");
            if (script.fireMode != FireMode.Burst && script.altFireMode != FireMode.Burst) excluded.Add("burstInfo");
            if (script.rightClickAction != WeaponRightClickAction.FireModeToggle) excluded.Add("altFireMode");
            DrawPropertiesExcluding(serializedObject, excluded.ToArray());
            
            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }
    }
}
