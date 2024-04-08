using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Weapons;

namespace Editor
{
    [CustomEditor(typeof(WeaponData)), CanEditMultipleObjects]
    public class WeaponDataEditor : UnityEditor.Editor
    {
        private const string DefaultFont = "Roboto Mono";
        private static Font usedFont;

        private static void InitializeFont()
        {
            usedFont = Font.CreateDynamicFontFromOSFont(DefaultFont, 12);
            if (usedFont != null) return;
            string[] names = Font.GetOSInstalledFontNames();
            string first = names.FirstOrDefault(str => str.Contains("Mono"));
            Debug.Log($"Warning: can't find {DefaultFont}; using {first}");
            if (first != null) usedFont = Font.CreateDynamicFontFromOSFont(first, 12);
        }

        public override void OnInspectorGUI()
        {
            if (targets.Length > 1)
            {
                base.OnInspectorGUI();
                return;
            }
            WeaponData script = (WeaponData)target;
            GUI.enabled = false;
            GUILayout.Label("Current DPS: " + script.DPS);
            if (script.rightClickAction == WeaponRightClickAction.FireModeToggle) 
                GUILayout.Label("Alt DPS: " + script.AltDPS);
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();

            List<string> excluded = new() { "m_Script", nameof(WeaponData.displayText) };
            if (script.rightClickAction != WeaponRightClickAction.Melee) excluded.Add("meleeInfo");
            if (script.fireMode != FireMode.Burst && script.altFireMode != FireMode.Burst) excluded.Add("burstInfo");
            if (script.rightClickAction != WeaponRightClickAction.FireModeToggle) excluded.Add("altFireMode");
            if (script.pierceMode == PierceMode.None) excluded.Add(nameof(script.pierceInfo));

            GUIStyle style = GUI.skin.GetStyle("TextArea");
            if (usedFont == null) InitializeFont();
            style.font = usedFont;
            script.displayText = EditorGUILayout.TextArea(script.displayText, style, GUILayout.MinHeight(39), GUILayout.MaxHeight(130), GUILayout.ExpandHeight(true));
            
            DrawPropertiesExcluding(serializedObject, excluded.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
