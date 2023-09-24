using FileSave;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CrossRunInfo)), CanEditMultipleObjects]
    public class CrossRunInfoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets.Length > 1) return;
            CrossRunInfo info = (CrossRunInfo)target;
            if (GUILayout.Button("Load from Save")) info.ReadFromSave();
            if (GUILayout.Button("Clear Upgrades")) info.ClearUpgrades();
            if (GUILayout.Button("Clear Unlocked Weapons")) info.ClearUnlockedWeapons();
            if (GUILayout.Button("Write To Save")) info.WriteToSave();
        }
    }
}