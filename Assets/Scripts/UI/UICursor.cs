using UnityEngine;

namespace UI
{
    /// <summary>
    /// Simple class that when enabled sets the cursor to something.
    /// </summary>
    public class UICursor : MonoBehaviour
    {
        [SerializeField] private Texture2D cursorInUI;
        [SerializeField]
        private Vector2 cursorCenterInUI = Vector2.zero;
        [SerializeField] private Texture2D cursorInCombat;
        [SerializeField]
        private Vector2 cursorCenterInCombat = new Vector2(16, 16);
        
        private void OnEnable()
        {
            Cursor.SetCursor(cursorInUI, cursorCenterInUI, CursorMode.Auto);
        }

        private void OnDisable()
        {
            Cursor.SetCursor(cursorInCombat, cursorCenterInCombat, CursorMode.Auto);
        }
    }
}
