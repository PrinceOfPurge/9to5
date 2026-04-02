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
    public bool finalAnnouncementFinished = false; // Required for SinglePlayerModeManager

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

    // --- THIS IS THE FUNCTION YOUR ERRORS WERE MISSING ---
    public void CheckForInstantUpdate()
    {
        // If we aren't currently playing a broadcast, restart the loop to check tasks
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

            // Check Student Messes
            if (SinglePlayerModeManager.Instance != null && SinglePlayerModeManager.Instance.BagsRemaining > 0)
                activeTasks.Add(AnnouncementType.Student);

            // Check Clogged Toilets
            if (PlungerMiniGame.instance != null && !PlungerMiniGame.instance.isWon)
                activeTasks.Add(AnnouncementType.CloggedToilet);

            // Check Food Fight
            if (PrincipalMinigame.instance != null && !PrincipalMinigame.instance.hasWon)
                activeTasks.Add(AnnouncementType.FoodFight);

            // Check Gym Mop
            if (Nets.instance != null && !Nets.instance.isWon)
                activeTasks.Add(AnnouncementType.MopGym);

            if (activeTasks.Count > 0)
            {
                foreach (AnnouncementType task in activeTasks)
                {
                    if (IsTaskStillActive(task))
                    {
                        yield return StartCoroutine(PlayBroadcast(task));
                        yield return new WaitForSeconds(delayBetweenAnnouncements);
                    }
                }
            }
            else
            {
                // Check if absolutely everything is finished
                bool bagsDone = SinglePlayerModeManager.Instance == null || SinglePlayerModeManager.Instance.BagsRemaining <= 0;
                bool plungerDone = PlungerMiniGame.instance == null || PlungerMiniGame.instance.isWon;
                bool principalDone = PrincipalMinigame.instance == null || PrincipalMinigame.instance.hasWon;
                bool gymDone = Nets.instance == null || Nets.instance.isWon;

                if (bagsDone && plungerDone && gymDone && principalDone)
                {
                    allTasksDone = true;
                    yield return StartCoroutine(PlayBroadcast(AnnouncementType.AllComplete));
                    
                    // This tells the Game Manager it is safe to load the Shop scene
                    finalAnnouncementFinished = true; 
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }

    private bool IsTaskStillActive(AnnouncementType type)
    {
        switch (type)
        {
            case AnnouncementType.Student:
                return SinglePlayerModeManager.Instance != null && SinglePlayerModeManager.Instance.BagsRemaining > 0;
            case AnnouncementType.CloggedToilet:
                return PlungerMiniGame.instance != null && !PlungerMiniGame.instance.isWon;
            case AnnouncementType.FoodFight:
                return PrincipalMinigame.instance != null && !PrincipalMinigame.instance.hasWon;
            case AnnouncementType.MopGym:
                return Nets.instance != null && !Nets.instance.isWon;
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

        // Wait for audio to finish playing fully
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

        // Cleanup
        foreach (EventInstance inst in activeInstances) inst.release();
        activeInstances.Clear();
    }
}