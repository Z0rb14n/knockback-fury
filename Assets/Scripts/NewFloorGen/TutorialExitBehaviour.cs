using UnityEngine;
using UnityEngine.SceneManagement;

namespace NewFloorGen
{
    public class TutorialExitBehaviour : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad;

        public void SwitchScene()
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}