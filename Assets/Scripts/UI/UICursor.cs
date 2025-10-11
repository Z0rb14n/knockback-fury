using UnityEngine;

namespace UI
{
    /// <summary>
    /// Simple class that when enabled sets the cursor to something.
    /// </summary>
    public class UICursor : MonoBehaviour
    {
        [SerializeField] private Texture2D cursorInUI;
        [SerializeField] private Texture2D cursorInCombat;
        
        private void OnEnable()
        {
            Cursor.SetCursor(cursorInUI, Vector2.zero, CursorMode.Auto);
        }

        private void OnDisable()
        {
            Cursor.SetCursor(cursorInCombat, Vector2.zero, CursorMode.Auto);
        }
    }
}
