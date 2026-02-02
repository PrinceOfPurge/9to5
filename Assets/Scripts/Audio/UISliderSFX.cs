using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;


public class UISliderSFX : MonoBehaviour
{
    private Slider slider;
    private EventInstance sliderInstance;
    private bool isDragging = false;
    private float dragTimeout = 0.2f; // how long after moving to consider dragging stopped
    private float lastValueChangeTime;

    private void Start()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderChanged);

        // Create one persistent instance of the FMOD event
        sliderInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.sliderMove);
    }

    private void Update()
    {
        // Detect if dragging has stopped
        if (isDragging && Time.time - lastValueChangeTime > dragTimeout)
        {
            sliderInstance.setPaused(true);
            isDragging = false;
        }
    }

    private void OnSliderChanged(float value)
    {
        lastValueChangeTime = Time.time;

        if (!isDragging)
        {
            // Start or resume the audio when dragging begins
            sliderInstance.start();
            sliderInstance.setPaused(false);
            isDragging = true;
        }
    }

    private void OnDestroy()
    {
        sliderInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        sliderInstance.release();
    }
}
