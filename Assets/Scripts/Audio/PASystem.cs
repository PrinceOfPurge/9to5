using UnityEngine;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;

public class PASystem : MonoBehaviour
{
    public static PASystem Instance;

    public enum AnnouncementType
    {
        Student = 0,
        FoodFight = 1,
        MopGym = 2,
        CloggedToilet = 3,
        AllComplete = 4
    }

    [Header("Loop Settings")]
    public float delayBetweenAnnouncements = 15f; 
    private bool allTasksDone = false;
    
    [HideInInspector]
    public bool finalAnnouncementFinished = false; 

    private List<EventInstance> activeInstances = new List<EventInstance>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        StartCoroutine(AnnouncementLoop());
    }

    public void CheckForInstantUpdate()
    {
        if (activeInstances.Count == 0)
        {
            StopAllCoroutines();
            StartCoroutine(AnnouncementLoop());
        }
    }

    private IEnumerator AnnouncementLoop()
    {
        while (!allTasksDone)
        {
            List<AnnouncementType> activeTasks = new List<AnnouncementType>();

            // ONLY add tasks to the queue if they are NOT won AND NOT currently active

            if (SinglePlayerModeManager.Instance != null && SinglePlayerModeManager.Instance.BagsRemaining > 0)
                activeTasks.Add(AnnouncementType.Student);

            if (PlungerMiniGame.instance != null && !PlungerMiniGame.instance.isWon && !PlungerMiniGame.instance.isMinigameActive)
                activeTasks.Add(AnnouncementType.CloggedToilet);

            if (PrincipalMinigame.instance != null && !PrincipalMinigame.instance.hasWon && !PrincipalMinigame.instance.isMinigameActive)
                activeTasks.Add(AnnouncementType.FoodFight);

            if (!Nets.IsMinigameWon && !Nets.IsMinigameActive)
                activeTasks.Add(AnnouncementType.MopGym);

            if (activeTasks.Count > 0)
            {
                foreach (AnnouncementType task in activeTasks)
                {
                    // Double-check just in case the player started the game while we were waiting in the loop
                    if (IsTaskStillActive(task))
                    {
                        yield return StartCoroutine(PlayBroadcast(task));
                        
                        // SMART WAIT: Checks if game is won while waiting, cutting the 15s delay short if true
                        float timeWaited = 0f;
                        while (timeWaited < delayBetweenAnnouncements)
                        {
                            if (CheckIfAllTasksComplete()) break; 
                            
                            timeWaited += 1f;
                            yield return new WaitForSeconds(1f);
                        }
                    }
                }
            }
            else
            {
                if (CheckIfAllTasksComplete())
                {
                    allTasksDone = true;
                    yield return StartCoroutine(PlayBroadcast(AnnouncementType.AllComplete));
                    finalAnnouncementFinished = true; 
                    Debug.Log("PA System: Final Announcement Finished.");
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }

    private bool CheckIfAllTasksComplete()
    {
        bool bagsDone = SinglePlayerModeManager.Instance == null || SinglePlayerModeManager.Instance.BagsRemaining <= 0;
        bool plungerDone = PlungerMiniGame.instance == null || PlungerMiniGame.instance.isWon;
        bool principalDone = PrincipalMinigame.instance == null || PrincipalMinigame.instance.hasWon;
        bool gymDone = Nets.IsMinigameWon;

        return bagsDone && plungerDone && gymDone && principalDone;
    }

    private bool IsTaskStillActive(AnnouncementType type)
    {
        switch (type)
        {
            case AnnouncementType.Student:
                return SinglePlayerModeManager.Instance != null && SinglePlayerModeManager.Instance.BagsRemaining > 0;
            
            case AnnouncementType.CloggedToilet:
                return PlungerMiniGame.instance != null && !PlungerMiniGame.instance.isWon && !PlungerMiniGame.instance.isMinigameActive;
            
            case AnnouncementType.FoodFight:
                return PrincipalMinigame.instance != null && !PrincipalMinigame.instance.hasWon && !PrincipalMinigame.instance.isMinigameActive;
            
            case AnnouncementType.MopGym:
                return !Nets.IsMinigameWon && !Nets.IsMinigameActive;
            
            default: return false;
        }
    }

    private IEnumerator PlayBroadcast(AnnouncementType type)
    {
        PASpeakerLocation[] speakers = FindObjectsOfType<PASpeakerLocation>();
        if (speakers.Length == 0) yield break;

        activeInstances.Clear();
        foreach (PASpeakerLocation speaker in speakers)
        {
            EventInstance inst = AudioManager.instance.CreateInstance(FMODEvents.instance.PAannouncement);
            inst.setParameterByName("AnouncementType", (float)type);
            inst.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(speaker.transform.position));
            inst.start();
            activeInstances.Add(inst);
        }

        bool isPlaying = true;
        while (isPlaying)
        {
            isPlaying = false;
            foreach (EventInstance inst in activeInstances)
            {
                inst.getPlaybackState(out PLAYBACK_STATE state);
                if (state != PLAYBACK_STATE.STOPPED) { isPlaying = true; break; }
            }
            yield return new WaitForSeconds(0.1f);
        }

        foreach (EventInstance inst in activeInstances) inst.release();
        activeInstances.Clear();
    }
}