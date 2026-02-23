using UnityEngine;
using FMODUnity;

public class GymTeacherGreetingTrigger : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float greetRadius = 5f;
    [SerializeField] private float greetCooldown = 5f;

    [SerializeField] private GymTeacherVO teacherVO; // assign in inspector

    private float lastGreetTime;

    private void Update()
    {
        if (teacherVO == null) return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        float distance = Vector3.Distance(player.transform.position, transform.position);

        if (distance <= greetRadius &&
            Time.time >= lastGreetTime + greetCooldown &&
            NPCVoiceManager.instance.CanPlay())
        {
            lastGreetTime = Time.time;
            teacherVO.TriggerGreeting();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, greetRadius);
    }
}