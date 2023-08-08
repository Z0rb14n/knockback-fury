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
        
        private void Awake()
        {
            Debug.Log("[CrossRunInfo::Awake] Reading save data from " + SaveIO.saveLocation);
            data = SaveIO.Read();
        }

        private void OnApplicationQuit()
        {
            Debug.Log("[CrossRunInfo::OnApplicationQuit] Writing Save Data to " + SaveIO.saveLocation);
            SaveIO.Save(data);
        }
    }
}