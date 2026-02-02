using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderHandleColor : MonoBehaviour
{
    public Slider slider;
    public Image handleImage;

    public Color leftColor = new Color(0.42f, 0.75f, 0.55f);  // Between yellow & blue (aqua green)
    public Color rightColor = new Color(0.96f, 0.82f, 0.36f);

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        // Listen to value changes
        slider.onValueChanged.AddListener(UpdateHandleColor);

        // Initialize color
        UpdateHandleColor(slider.value);
    }

    void UpdateHandleColor(float value)
    {
        // Get normalized value (0–1)
        float t = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);

        // Lerp between two colors
        handleImage.color = Color.Lerp(leftColor, rightColor, t);
    }
}