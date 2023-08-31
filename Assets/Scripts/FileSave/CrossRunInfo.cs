using UnityEngine;

namespace FileSave
{
    /// <summary>
    /// MonoBehaviour to save/store data on application start/exit.
    /// </summary>
    [DisallowMultipleComponent]
    public class CrossRunInfo : MonoBehaviour
    {
        public SaveData data;

        #region Singleton
        public static CrossRunInfo Instance
        {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            get
            {
                if (_instance == null) _instance = FindObjectOfType<CrossRunInfo>();
                return _instance;
            }
        }
        private static CrossRunInfo _instance;
        #endregion
        

        public void ReadFromSave() => data = SaveIO.Read();
        public void WriteToSave() => SaveIO.Save(data);
        
        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Debug.LogWarning("Found two copies of CrossRunInfo: deleting this.");
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Debug.Log("[CrossRunInfo::Awake] Reading save data from " + SaveIO.saveLocation);
            ReadFromSave();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("[CrossRunInfo::OnApplicationQuit] Writing Save Data to " + SaveIO.saveLocation);
            WriteToSave();
        }
    }
}