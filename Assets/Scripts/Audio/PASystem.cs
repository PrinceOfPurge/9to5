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

    [Header("Loop Settings")]
    public float delayBetweenAnnouncements = 15f; 
    private bool allTasksDone = false;

    private List<EventInstance> activeInstances = new List<EventInstance>();

    private void Start()
    {
        StartCoroutine(AnnouncementLoop());
    }

    // --- THIS IS THE FIXED METHOD ---
    public void CheckForInstantUpdate()
    {
        StopAllCoroutines();
        StartCoroutine(AnnouncementLoop());
    }
    // --------------------------------

    private IEnumerator AnnouncementLoop()
    {
        while (!allTasksDone)
        {
            List<AnnouncementType> activeTasks = new List<AnnouncementType>();

            if (PlungerMiniGame.instance != null && !PlungerMiniGame.instance.isWon)
                activeTasks.Add(AnnouncementType.CloggedToilet);

            if (PrincipalMinigame.instance != null && !PrincipalMinigame.instance.hasWon)
                activeTasks.Add(AnnouncementType.FoodFight);

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
                bool plungerDone = PlungerMiniGame.instance == null || PlungerMiniGame.instance.isWon;
                bool principalDone = PrincipalMinigame.instance == null || PrincipalMinigame.instance.hasWon;
                bool gymDone = Nets.instance == null || Nets.instance.isWon;

                if (plungerDone && principalDone && gymDone)
                {
                    allTasksDone = true;
                    yield return StartCoroutine(PlayBroadcast(AnnouncementType.AllComplete));
                }
            }
            yield return new WaitForSeconds(2f);
        }
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

    private IEnumerator PlayBroadcast(AnnouncementType type)
    {
        PASpeakerLocation[] speakers = FindObjectsOfType<PASpeakerLocation>();
        activeInstances.Clear();

        if (speakers.Length == 0) yield break;

        foreach (PASpeakerLocation speaker in speakers)
        {
            EventInstance inst = AudioManager.instance.CreateInstance(FMODEvents.instance.PAannouncement);
            inst.setParameterByName("AnouncementType", (float)type);
            inst.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(speaker.gameObject));
            inst.start();
            activeInstances.Add(inst);
        }

        if (activeInstances.Count > 0)
        {
            PLAYBACK_STATE state;
            activeInstances[0].getPlaybackState(out state);
            while (state != PLAYBACK_STATE.STOPPED)
            {
                activeInstances[0].getPlaybackState(out state);
                yield return null;
            }
        }

        foreach (EventInstance inst in activeInstances)
        {
            inst.release();
        }
    }
}