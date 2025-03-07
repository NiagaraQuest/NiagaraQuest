using UnityEngine;
using UnityEngine.UI;

public class ToggleImageSwap : MonoBehaviour
{
    public Image toggleImage; // Assign the toggle's background image
    public Sprite selectedSprite;
    public Sprite unselectedSprite;
    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();

        // Set initial image
        UpdateToggleImage(toggle.isOn);

        // Listen for value change
        toggle.onValueChanged.AddListener(UpdateToggleImage);
    }

    void UpdateToggleImage(bool isOn)
    {
        toggleImage.sprite = isOn ? selectedSprite : unselectedSprite;
    }

    void OnDestroy()
    {
        // Remove listener to prevent memory leaks
        toggle.onValueChanged.RemoveListener(UpdateToggleImage);
    }
}
