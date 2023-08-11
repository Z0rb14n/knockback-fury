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
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
        }
    }
}