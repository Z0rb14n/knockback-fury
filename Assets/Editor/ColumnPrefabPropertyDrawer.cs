using ColumnMode;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomPropertyDrawer(typeof(ColumnModeGenerator.ColumnPrefab))]
    public class ColumnPrefabPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            Rect goRect = new(position.x, position.y, position.width/2, position.height);
            Rect weightRect = new(position.x + position.width/2+5, position.y, position.width/2, position.height);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(goRect, property.FindPropertyRelative(nameof(ColumnModeGenerator.ColumnPrefab.go)), GUIContent.none);
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative(nameof(ColumnModeGenerator.ColumnPrefab.weight)), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}