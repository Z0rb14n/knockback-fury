using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Weapons;

namespace Editor
{
    [CustomEditor(typeof(WeaponData)), CanEditMultipleObjects]
    public class WeaponDataEditor : UnityEditor.Editor
    {
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

            List<string> excluded = new() { "m_Script" };
            if (script.rightClickAction != WeaponRightClickAction.Melee) excluded.Add("meleeInfo");
            if (script.fireMode != FireMode.Burst && script.altFireMode != FireMode.Burst) excluded.Add("burstInfo");
            if (script.rightClickAction != WeaponRightClickAction.FireModeToggle) excluded.Add("altFireMode");
            if (script.pierceMode == PierceMode.None) excluded.Add(nameof(script.pierceInfo));
            DrawPropertiesExcluding(serializedObject, excluded.ToArray());

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            WeaponData data = (WeaponData)target;

            if (data == null || data.sprite == null)
                return null;

            Texture2D tex = new(width, height);
            
            EditorUtility.CopySerialized (data.sprite.texture, tex);
            return tex;
        }
    }
}
