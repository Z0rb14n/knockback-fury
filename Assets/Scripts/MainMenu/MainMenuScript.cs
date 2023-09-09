using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    /// <summary>
    /// Wrapper to expose LoadScene and Quit to main menu buttons.
    /// </summary>
    public class MainMenuScript : MonoBehaviour
    {
        public void LoadScene(string str)
        {
            SceneManager.LoadScene(str);
        }

        public void Exit()
        {
            Application.Quit();
        }
    }
}