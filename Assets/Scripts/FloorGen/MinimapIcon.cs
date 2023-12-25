using UnityEngine;
using UnityEngine.UI;

namespace FloorGen
{
    [RequireComponent(typeof(Image))]
    public class MinimapIcon : MonoBehaviour
    {
        [SerializeField] private Color unvisitedColor = new(25 / 255f, 25 / 255f, 25 / 255f, 197 / 255f);
        [SerializeField] private Color visitedColor = new(163 / 255f, 163 / 255f, 163 / 255f, 197 / 255f);
        [SerializeField] private Color presentColor = new(225 / 255f, 225 / 255f, 225 / 255f, 197 / 255f);
        
        [SerializeField] private RectTransform upgradeIcon;
        [SerializeField] private RectTransform weaponIcon;
        [SerializeField] private RectTransform bossIcon;
        [SerializeField] private RectTransform newRoomIcon;

        private GameObject _currDisplayed;
        private DisplayedIcon _currIcon;
        private Image _image;

        public DisplayState State
        {
            get
            {
                if (!_image) _image = GetComponent<Image>();
                if (_image.color == presentColor) return DisplayState.Present;
                return _image.color == visitedColor ? DisplayState.Visited : DisplayState.Unvisited;
            }
            set
            {
                if (!_image)
                {
                    Debug.LogWarning("Trying to set display state without image");
                    return;
                }

                switch (value)
                {
                    case DisplayState.Present:
                        _image.color = presentColor;
                        return;
                    case DisplayState.Visited:
                        _image.color = visitedColor;
                        return;
                    case DisplayState.Unvisited:
                    default:
                        _image.color = unvisitedColor;
                        return;
                }
            }
        }

        public DisplayedIcon RoomIcon
        {
            get => _currIcon;
            set {
                if (_currDisplayed)
                {
                    _currDisplayed.SetActive(false);
                    _currDisplayed = null;
                }

                _currDisplayed = value switch
                {
                    DisplayedIcon.Upgrade => upgradeIcon.gameObject,
                    DisplayedIcon.Boss => bossIcon.gameObject,
                    DisplayedIcon.Weapon => weaponIcon.gameObject,
                    DisplayedIcon.NewRoom => newRoomIcon.gameObject,
                    _ => _currDisplayed
                };

                if (_currDisplayed) _currDisplayed.SetActive(true);
            }
        }
        
        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        public void ForcePresent()
        {
            if (!_image) _image = GetComponent<Image>();
            _image.color = presentColor;
        }

        public enum DisplayedIcon
        {
            None, Upgrade, Weapon, Boss, NewRoom
        }

        public enum DisplayState
        {
            Unvisited, Visited, Present
        }
    }
}