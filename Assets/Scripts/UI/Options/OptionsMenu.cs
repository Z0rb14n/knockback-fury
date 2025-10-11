using UnityEngine;

namespace UI.Options
{
    public class OptionsMenu : MonoBehaviour
    {
        public void OnOptionsMenuButtonClicked()
        {
            
        }
        
        public void OnBackButtonClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
