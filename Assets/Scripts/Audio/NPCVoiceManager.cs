using UnityEngine;
using System.Collections;

public class NPCVoiceManager : MonoBehaviour
{
    public static NPCVoiceManager instance { get; private set; }
    private bool isPlaying = false;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Returns true if a VO clip can play
    public bool CanPlay()
    {
        return !isPlaying;
    }

    // Lock the VO for the given duration
    public void PlayForDuration(float duration)
    {
        if (isPlaying) return;
        StartCoroutine(PlayCoroutine(duration));
    }

    private IEnumerator PlayCoroutine(float duration)
    {
        isPlaying = true;
        yield return new WaitForSeconds(duration);
        isPlaying = false;
    }
}