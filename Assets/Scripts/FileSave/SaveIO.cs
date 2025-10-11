using System;
using System.IO;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace FileSave
{
    /// <summary>
    /// Utility class to handle File IO of save data.
    /// </summary>
    public static class SaveIO
    {
        public static string saveLocation = Path.Combine(Application.persistentDataPath, "save.json");
        
        public static void Save(SaveData data)
        {
            File.WriteAllText(saveLocation, JsonUtility.ToJson(data, true));
        }

        public static SaveData Read()
        {
            if (!File.Exists(saveLocation))
            {
                Debug.Log("No save file found, using defaults.");
                return new SaveData
                {
                    cheese = 0,
                    unlockedPermanentUpgrades = Array.Empty<int>(),
                    unlockedWeapons = Array.Empty<string>()
                };
            }
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
        }
    }
}