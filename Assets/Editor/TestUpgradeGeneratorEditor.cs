using UnityEditor;
using UnityEngine;
using Upgrades;

namespace Editor
{
    [CustomEditor(typeof(TestUpgradeGenerator))]
    public class TestUpgradeGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Generate")) ((TestUpgradeGenerator) target).Generate();
            GUI.enabled = true;
        }
    }
}