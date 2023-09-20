using UnityEngine;
using UnityEngine.SceneManagement;
using Util;

namespace Lobby
{
    [RequireComponent(typeof(Collider2D))]
    public class EnterRoomScript : TriggerTextScript
    {
        [SerializeField] private string sceneToLoad;

        protected override void OnPlayerInteraction()
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}