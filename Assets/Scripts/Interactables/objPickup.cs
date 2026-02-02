using System.Collections;
using UnityEngine;
using FMODUnity;


public class objPickup : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject crosshair1, crosshair2;
    public GameObject TutPrompt;
    public GameObject worldPrompt;
    public GameObject throwPrompt;

    [Header("References")]
    public Transform objTransform;
    public Transform cameraTrans;
    public Rigidbody objRigidbody;
    public Collider playerCollider;

    [Header("Settings")]
    public float throwAmount = 2000f;
    public float promptCooldown = 1.0f;
    public float holdSmoothness = 1000f;
    public float holdDistance = 8f;

    [Header("Trajectory Prediction")]
    public LineRenderer trajectoryRenderer;
    public int predictionSteps = 40;
    public float timestep = 0.04f;
    public LayerMask trajectoryCollisionMask;
    private Vector3 smoothedStartPos;
    private Vector3 smoothedStartVel;
    public float predictionSmooth = 20f;
    private bool hasLastHit;
    private Vector3 lastHitPoint;
    public Vector3 trajectoryVisualOffset = new Vector3(0.25f, -0.35f, 0f);
    private Vector3[] splineBuffer = new Vector3[128];
    public int splineResolution = 10;

    [Header("Rotation Offset")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    [Header("Position Offset")]
    public float heightOffset = -1.5f;

    [Header("GarbageCan Hit (optional)")]
    public GameObject successEffect;
    public int scoreValue = 1;

    private bool interactable;
    public bool pickedup;
    private bool canShowPrompt = true;
    private Collider objCollider;

    void Start()
    {
        if (worldPrompt) worldPrompt.SetActive(false);
        if (throwPrompt) throwPrompt.SetActive(false);
        if (TutPrompt) TutPrompt.SetActive(true);
        

        objCollider = GetComponent<Collider>();
        objRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            crosshair1.SetActive(false);
            crosshair2.SetActive(true);
            interactable = true;

            if (!pickedup && worldPrompt && canShowPrompt)
                worldPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            crosshair1.SetActive(true);
            crosshair2.SetActive(false);
            interactable = false;

            if (!pickedup && worldPrompt)
                worldPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (interactable)
        {
            if (Input.GetKeyDown(KeyCode.F) && !pickedup)
                PickUpObject();

            if (pickedup && Input.GetMouseButtonDown(0))
                ThrowObject();
        }

        if (pickedup)
        {
            Vector3 targetPos = cameraTrans.position
                                + cameraTrans.forward * holdDistance
                                + cameraTrans.up * heightOffset;

            objTransform.position = Vector3.Lerp(objTransform.position, targetPos, Time.deltaTime * holdSmoothness);

            Quaternion targetRot = cameraTrans.rotation * Quaternion.Euler(rotationOffset);
            objTransform.rotation = Quaternion.Lerp(objTransform.rotation, targetRot, Time.deltaTime * holdSmoothness);

            ShowTrajectory();
        }
        else
        {
            trajectoryRenderer.positionCount = 0;
        }
    }

    void PickUpObject()
    {
        objRigidbody.useGravity = false;
        objRigidbody.velocity = Vector3.zero;
        objRigidbody.angularVelocity = Vector3.zero;
        objTransform.parent = cameraTrans;
        pickedup = true;

        if (playerCollider && objCollider)
            Physics.IgnoreCollision(objCollider, playerCollider, true);

        if (worldPrompt) worldPrompt.SetActive(false);
        if (TutPrompt) TutPrompt.SetActive(false);
        StartCoroutine(ShowThrowPromptAfterDelay(0.4f));
    }

    void ThrowObject()
    {
        objTransform.parent = null;
        objRigidbody.useGravity = true;
        objRigidbody.velocity = cameraTrans.forward * throwAmount;
        pickedup = false;

        if (playerCollider && objCollider)
            Physics.IgnoreCollision(objCollider, playerCollider, false);

        if (throwPrompt) throwPrompt.SetActive(false);
        StartCoroutine(PromptCooldownRoutine());
    }

    IEnumerator ShowThrowPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (throwPrompt && pickedup)
            throwPrompt.SetActive(true);
    }

    IEnumerator PromptCooldownRoutine()
    {
        canShowPrompt = false;
        if (worldPrompt) worldPrompt.SetActive(false);
        yield return new WaitForSeconds(promptCooldown);
        canShowPrompt = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (pickedup)
            return;

        if (collision.gameObject.CompareTag("GarbageCan"))
        {
            HandleGarbageCanCollision(collision);
        }
    }

    void HandleGarbageCanCollision(Collision collision)
    {
        AudioManager.instance.PlayOneShot(FMODEvents.instance.Done, transform.position);

        if (successEffect != null)
        {
            GameObject vfxObj = Instantiate(successEffect, transform.position, Quaternion.identity);
            ParticleSystem vfx = vfxObj.GetComponent<ParticleSystem>();

            if (vfx != null)
            {
                vfx.Play();
                Destroy(vfxObj, vfx.main.duration + vfx.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(vfxObj, 2f);
            }
        }

        Destroy(gameObject);
        /*Causing game to crash at the moment since Mode manager isnt being used
        SinglePlayerModeManager.Instance.BagsRemaining -=1;
        SinglePlayerStats.Instance.money += 500;
        */
        
    }

    void ShowTrajectory()
    {
        hasLastHit = false;

        Vector3 rawStartPos = objTransform.position + cameraTrans.TransformVector(trajectoryVisualOffset);
        Vector3 rawStartVel = cameraTrans.forward * throwAmount;

        smoothedStartPos = Vector3.Lerp(smoothedStartPos, rawStartPos, Time.deltaTime * predictionSmooth);
        smoothedStartVel = Vector3.Lerp(smoothedStartVel, rawStartVel, Time.deltaTime * predictionSmooth);

        Vector3 pos = smoothedStartPos;
        Vector3 vel = smoothedStartVel;

        int count = 0;

        for (int i = 0; i < predictionSteps; i++)
        {
            if (count < splineBuffer.Length)
                splineBuffer[count] = pos;

            count++;

            vel += Physics.gravity * timestep;
            Vector3 newPos = pos + vel * timestep;

            if (Physics.Raycast(pos, newPos - pos, out RaycastHit hit, (newPos - pos).magnitude, trajectoryCollisionMask))
            {
                if (count < splineBuffer.Length)
                    splineBuffer[count] = hit.point;

                count++;
                break;
            }

            pos = newPos;
        }

        int outCount = (count - 1) * splineResolution;
        trajectoryRenderer.positionCount = outCount;

        int idx = 0;

        for (int i = 0; i < count - 1; i++)
        {
            Vector3 p0 = i == 0 ? splineBuffer[i] : splineBuffer[i - 1];
            Vector3 p1 = splineBuffer[i];
            Vector3 p2 = splineBuffer[i + 1];
            Vector3 p3 = (i + 2 < count) ? splineBuffer[i + 2] : splineBuffer[i + 1];

            for (int j = 0; j < splineResolution; j++)
            {
                float t = j / (float)splineResolution;
                Vector3 a = 2f * p1;
                Vector3 b = p2 - p0;
                Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
                Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

                Vector3 p = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

                if (idx < outCount)
                    trajectoryRenderer.SetPosition(idx, p);

                idx++;
            }
        }
    }
}
