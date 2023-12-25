using System;
using Player;
using UnityEditor;
using UnityEngine;
using Upgrades;

namespace Editor
{
    [CustomEditor(typeof(PlayerUpgradeManager)), CanEditMultipleObjects]
    public class PlayerUpgradeManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (targets.Length != 1) return;
            GUI.enabled = EditorApplication.isPlaying;
            if (GUILayout.Button("Give All Upgrades"))
            {
                PlayerUpgradeManager manager = (PlayerUpgradeManager)target;
                foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
                {
                    if (manager[type] > 0) continue;
                    if (UpgradeManager.Instance.UpgradeMapping.TryGetValue(type, out UpgradePickupData data))
                    {
                        manager.PickupUpgrade(type, data.upgradeData);
                    }
                }
            }
            GUI.enabled = true;
        }
    }
}