using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MenuManager : MonoBehaviour
{
    // Title Screen -------------------------------

    public void OnBackToTitleButtonPressed()
    {
        Debug.Log("Title (Scene) Loaded");
        SceneManager.LoadScene("MainMenu");
    }

    public void OnModeSelectButtonPressed()
    {
        Debug.Log("Mode Select (Scene) Loaded");
        SceneManager.LoadScene("ModeSelect");
    }

    public void OnOptionsButtonPressed()
    {
        Debug.Log("Options (Scene) Loaded");
        SceneManager.LoadScene("Options");
    }
    
    public void OnVolumeButtonPressed()
    {
        Debug.Log("Sound (Scene) Loaded");
        SceneManager.LoadScene("Sound");
    }

    // Mode Select -------------------------------

    public void OnTutorialButtonPressed()
    {
        GameManager.Instance.LoadTutorial();
    }

    public void OnSinglePlayerButtonPressed()
    {
        GameManager.Instance.LoadSinglePlayer();
    }

    public void OnLANMultiPlayerButtonPressed()
    {
        // Assumes current Player is the Host
        GameManager.Instance.LoadLANMultiplayer(true);
    }

    public void OnTestMultiPlayerButtonPressed()
    {
        GameManager.Instance.LoadTestMultiplayer();
    }
    
    public void OnQuitButtonPressed()
    {
        Debug.Log("Quit Game Pressed");
        Application.Quit();

#if UNITY_EDITOR
        // If running in the Unity Editor, stop play mode
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
