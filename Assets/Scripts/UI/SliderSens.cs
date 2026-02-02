using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderSens : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;

    [Header("Animation Settings")]
    [SerializeField] private float smoothSpeed = 10f;

    private float displayedValue;

    private void Awake()
    {
        // Try to auto-assign if null
        if (slider == null)
            slider = GetComponentInChildren<Slider>();

        if (slider == null)
            Debug.LogError("Slider not found in children of " + gameObject.name);

        if (valueText == null)
            valueText = GetComponentInChildren<TextMeshProUGUI>();

        if (valueText == null)
            Debug.LogError("TMP_Text not found in children of " + gameObject.name);

        // Initialize
        if (slider != null)
            displayedValue = slider.value;
    }

    private void OnEnable()
    {
        if (slider != null)
            slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void Update()
    {
        if (slider == null || valueText == null) return;

        // Smoothly animate towards slider value
        displayedValue = Mathf.Lerp(displayedValue, slider.value, Time.deltaTime * smoothSpeed);

        int percentage = Mathf.RoundToInt(displayedValue * 100f);
        valueText.text = percentage + "";
    }

    // Optional: Instant update when slider is dragged
    private void OnSliderValueChanged(float value)
    {
        displayedValue = value;
    }
}