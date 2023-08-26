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
            if (GUILayout.Button("Generate")) ((TestUpgradeGenerator) target).Generate();
        }
    }
}