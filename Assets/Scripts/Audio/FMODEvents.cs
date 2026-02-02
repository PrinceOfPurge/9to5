using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity; 

public class FMODEvents : MonoBehaviour
{
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience { get; private set; }
    
    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }
    
    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }
    [field: SerializeField] public EventReference Scub { get; private set; } 
    [field: SerializeField] public EventReference Splash { get; private set; } 
    [field: SerializeField] public EventReference SprayBottle { get; private set; } 
    [field: SerializeField] public EventReference Done { get; private set; } 
    [field: SerializeField] public EventReference Swish { get; private set; } 
    
    [field: Header("UI Sounds")]
    [field: SerializeField] public EventReference buttonClick { get; private set; }
    [field: SerializeField] public EventReference sliderMove { get; private set; } 
    
    public static FMODEvents instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one AudioManager in scene.");
        }
        instance = this;
    }
}
