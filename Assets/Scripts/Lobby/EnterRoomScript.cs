using System;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Weapons;

namespace Lobby
{
    [RequireComponent(typeof(Collider2D))]
    public class EnterRoomScript : TriggerTextScript
    {
        [SerializeField] private string sceneToLoad;

        protected override void OnPlayerInteraction()
        {
            WeaponData[] data = PlayerWeaponControl.Instance.Inventory;
            SceneManager.LoadScene(sceneToLoad);
            GameObject go = new("Temporary Weapon Adder", typeof(TemporaryAdditionScript));
            TemporaryAdditionScript tas = go.GetComponent<TemporaryAdditionScript>();
            DontDestroyOnLoad(go);
            tas.data = data;
        }

        private class TemporaryAdditionScript : MonoBehaviour
        {
            [NonSerialized]
            public WeaponData[] data;

            private void FixedUpdate()
            {
                PlayerWeaponControl.Instance.Inventory = data;
                Destroy(gameObject);
            }
        }
    }
}