using UnityEngine;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;

public class PASystem : MonoBehaviour
{
    public enum AnnouncementType
    {
        Student = 0,
        FoodFight = 1,
        MopGym = 2,
        CloggedToilet = 3,
        AllComplete = 4
    }

    public static PASystem instance { get; private set; }

    [Header("Loop Settings")]
    public float delayBetweenAnnouncements = 15f; 
    private bool allTasksDone = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(AnnouncementLoop());
    }

    private IEnumerator AnnouncementLoop()
    {
        while (!allTasksDone)
        {
            List<AnnouncementType> activeTasks = new List<AnnouncementType>();

            // 1. Check PlungerMiniGame
            if (PlungerMiniGame.instance != null && !PlungerMiniGame.instance.isWon)
                activeTasks.Add(AnnouncementType.CloggedToilet);

            // 2. Check PrincipalMinigame
            if (PrincipalMinigame.instance != null && !PrincipalMinigame.instance.hasWon)
                activeTasks.Add(AnnouncementType.FoodFight);

            // 3. Check GymMinigame
            if (Nets.instance != null && !Nets.instance.isWon)
                activeTasks.Add(AnnouncementType.MopGym);

            // Play
            if (activeTasks.Count > 0)
            {
                foreach (AnnouncementType task in activeTasks)
                {
                    if (IsTaskStillActive(task))
                    {
                        yield return StartCoroutine(PlayVoiceLine(task));
                        yield return new WaitForSeconds(delayBetweenAnnouncements);
                    }
                }
            }
            else
            {
                // Check if everything is finished
                bool plungerDone = PlungerMiniGame.instance == null || PlungerMiniGame.instance.isWon;
                bool principalDone = PrincipalMinigame.instance == null || PrincipalMinigame.instance.hasWon;
                bool gymDone = Nets.instance == null || Nets.instance.isWon;

                if (plungerDone && principalDone && gymDone)
                {
                    allTasksDone = true;
                    yield return StartCoroutine(PlayVoiceLine(AnnouncementType.AllComplete));
                }
            }
            yield return new WaitForSeconds(2f);
        }
    }

    // This allows a game script to check if it's done immediately
    public void CheckForInstantUpdate()
    {
        StopAllCoroutines();
        StartCoroutine(AnnouncementLoop());
    }

    private bool IsTaskStillActive(AnnouncementType type)
    {
        switch (type)
        {
            case AnnouncementType.CloggedToilet:
                return PlungerMiniGame.instance != null && !PlungerMiniGame.instance.isWon;
            case AnnouncementType.FoodFight:
                return PrincipalMinigame.instance != null && !PrincipalMinigame.instance.hasWon;
            case AnnouncementType.MopGym:
                return Nets.instance != null && !Nets.instance.isWon;
            default:
                return false;
        }
    }

    private IEnumerator PlayVoiceLine(AnnouncementType type)
    {
        EventInstance paInstance = AudioManager.instance.CreateInstance(FMODEvents.instance.PAannouncement);
        paInstance.setParameterByName("AnouncementType", (float)type);
        paInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        
        paInstance.start();

        PLAYBACK_STATE state;
        paInstance.getPlaybackState(out state);
        while (state != PLAYBACK_STATE.STOPPED)
        {
            paInstance.getPlaybackState(out state);
            yield return null;
        }
        paInstance.release();
    }
}