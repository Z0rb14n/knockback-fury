using Player;
using UnityEngine;

namespace Util
{
    public static class UIUtil
    {
        public static bool IsUIOpen = false;

        public static void OpenUI()
        {
            Time.timeScale = 0;
            PlayerMovementScript.Instance.enabled = false;
            PlayerWeaponControl.Instance.enabled = false;
            CameraScript.Instance.enabled = false;
            IsUIOpen = true;
        }

        public static void CloseUI()
        {
            Time.timeScale = 1;
            CameraScript.Instance.enabled = true;
            PlayerMovementScript.Instance.enabled = true;
            PlayerWeaponControl.Instance.enabled = true;
            IsUIOpen = false;
        }
    }
}