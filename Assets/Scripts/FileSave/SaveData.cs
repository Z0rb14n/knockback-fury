namespace FileSave
{
    /// <summary>
    /// Persistent save data.
    /// </summary>
    /// <remarks>
    /// Is a class rather than a struct to avoid boxing of data type.
    /// </remarks>
    [System.Serializable]
    public class SaveData
    {
        public int cheese;
    }
}