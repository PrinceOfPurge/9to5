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
    private float dragTimeout = 0.2f; 
    private float lastValueChangeTime;

    private void Start()
    {
        // CHANGED: Look in children since script is on the Canvas parent
        slider = GetComponentInChildren<Slider>();

        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderChanged);
        }
        else
        {
            Debug.LogError("UISliderSFX: No Slider found in children of " + gameObject.name);
            return;
        }

        // CHANGED: Added safety check to prevent NullReferenceException on line 20
        if (AudioManager.instance != null && FMODEvents.instance != null)
        {
            sliderInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.sliderMove);
        }
        else
        {
            Debug.LogWarning("UISliderSFX: AudioManager or FMODEvents instance is null. Audio won't play.");
        }
    }

    private void Update()
    {
        if (isDragging && Time.time - lastValueChangeTime > dragTimeout)
        {
            sliderInstance.setPaused(true);
            isDragging = false;
        }
    }

    private void OnSliderChanged(float value)
    {
        lastValueChangeTime = Time.time;

        // Added check to ensure sliderInstance was actually created
        if (!isDragging && sliderInstance.isValid())
        {
            sliderInstance.start();
            sliderInstance.setPaused(false);
            isDragging = true;
        }
    }

    private void OnDestroy()
    {
        if (sliderInstance.isValid())
        {
            sliderInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sliderInstance.release();
        }
    }
}