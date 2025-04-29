using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro; // Include this if you're using TextMeshPro

public class ButtonTextColorChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Button component reference
    private Button button;

    // Text component reference (using TextMeshPro)
    public TMP_Text buttonText;

    // Colors for the text
    public Color normalTextColor = Color.black;
    public Color hoverTextColor = new Color(0.98f, 0.89f, 0.78f); // #FAE3C6
    public Color clickedTextColor = new Color(0.98f, 0.89f, 0.78f); // #FAE3C6

    // Keep track of current state
    private bool isClicked = false;
    private bool isHovering = false;

    void Start()
    {
        // Reset state on start
        ResetState();

        // Get the Button component
        button = GetComponent<Button>();

        // If buttonText wasn't assigned in the inspector, try to find it as a child
        if (buttonText == null)
        {
            // For TextMeshPro:
            buttonText = GetComponentInChildren<TMP_Text>();
        }

        // Add click listener
        button.onClick.AddListener(OnButtonClick);

        // Set initial text color
        SetTextColor(normalTextColor);
    }

    // Reset state when object is enabled (like when panel is shown)
    void OnEnable()
    {
        ResetState();
        // Reset text color to normal when re-enabled
        SetTextColor(normalTextColor);
    }

    // Method to reset the button state
    public void ResetState()
    {
        isClicked = false;
        isHovering = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        // Only change color on hover if the button isn't in clicked state
        if (!isClicked)
        {
            SetTextColor(hoverTextColor);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // Return to normal color if not clicked
        if (!isClicked)
        {
            SetTextColor(normalTextColor);
        }
    }

    void OnButtonClick()
    {
        // Toggle the clicked state
        isClicked = !isClicked;

        if (isClicked)
        {
            // Change to clicked text color
            SetTextColor(clickedTextColor);
        }
        else
        {
            // If we're still hovering when unclicked, use hover text color
            if (isHovering)
            {
                SetTextColor(hoverTextColor);
            }
            else
            {
                // Otherwise return to normal text color
                SetTextColor(normalTextColor);
            }
        }
    }

    // Public method to force reset the button color and state
    public void ForceReset()
    {
        ResetState();
        SetTextColor(normalTextColor);
    }

    void SetTextColor(Color color)
    {
        if (buttonText != null)
        {
            buttonText.color = color;
        }
    }
}