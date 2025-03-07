using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro

public class SliderValueDisplay : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI valueText; // Use "Text" instead if not using TMP

    void Start()
    {
        slider.onValueChanged.AddListener(UpdateValueText);
        UpdateValueText(slider.value);
    }

    void UpdateValueText(float value)
    {
        valueText.text = Mathf.RoundToInt(value) + "%";
    }
}
