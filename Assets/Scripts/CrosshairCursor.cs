using UnityEngine;
using UnityEngine.UI;

public class UICursorController : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite defaultCursor;
    public Sprite crosshairCursor;

    private Image cursorImage;
    private RectTransform rectTransform;

    void Awake()
    {
        DontDestroyOnLoad(transform.parent.gameObject);

        cursorImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        // start with default
        cursorImage.sprite = defaultCursor;

        // hide the system cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        // move image to mouse position
        rectTransform.position = Input.mousePosition;
    }

    public void SetDefaultCursor()
    {
        cursorImage.sprite = defaultCursor;
    }

    public void SetCrosshairCursor()
    {
        cursorImage.sprite = crosshairCursor;
    }
}