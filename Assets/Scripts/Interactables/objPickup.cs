using System.Collections;
using UnityEngine;

public class objPickup : MonoBehaviour, IInteractable
{
    [Header("UI Elements")]
    public GameObject crosshair1, crosshair2;
    public GameObject worldPrompt, throwPrompt;

    [Header("Highlighting")]
    public HighlightEffectBananaAndGarbage highlightScript;

    [Header("References")]
    public Transform objTransform;
    public Transform cameraTrans;
    public Rigidbody objRigidbody;
    public Collider playerCollider;
    public Animator playerAnimator; 

    [Header("Animations")]
    public string pickupTrigger = "PickUp";
    public string throwTrigger = "Throw";

    [Header("Settings")]
    public float throwAmount = 25f;
    public float promptCooldown = 1.0f;
    public float holdSmoothness = 15f; 
    public float holdDistance = 2.0f;

    [Header("View Offsets")]
    public float heightOffset = -0.6f;
    public float sideOffset = 0.4f;
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    [Header("Trajectory Prediction")]
    public LineRenderer trajectoryRenderer;
    public int predictionSteps = 40;
    public float timestep = 0.04f;
    public LayerMask trajectoryCollisionMask;
    public float predictionSmooth = 20f;
    public Vector3 trajectoryVisualOffset = new Vector3(0.25f, -0.35f, 0f);
    
    private Vector3 smoothedStartPos;
    private Vector3 smoothedStartVel;
    private Vector3[] splineBuffer = new Vector3[128];
    public int splineResolution = 10;

    [Header("GarbageCan Hit")]
    public GameObject successEffect;

    [HideInInspector] public bool pickedup;
    private bool canShowPrompt = true;
    private Collider[] allColliders;

    void Start()
    {
        allColliders = GetComponentsInChildren<Collider>();
        if (worldPrompt) worldPrompt.SetActive(false);
        if (throwPrompt) throwPrompt.SetActive(false);
        objRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }
    
    public void OnFocus()
    {
        if (pickedup || !canShowPrompt) return;
        
        if (highlightScript != null) highlightScript.ToggleHighlight(true);

        if(crosshair1) crosshair1.SetActive(false);
        if(crosshair2) crosshair2.SetActive(true);

        if (worldPrompt) worldPrompt.SetActive(true);
    }

    public void OnLoseFocus()
    {
        if (highlightScript != null) highlightScript.ToggleHighlight(false);
        
        if (!pickedup)
        {
            if(crosshair1) crosshair1.SetActive(true);
            if(crosshair2) crosshair2.SetActive(false);
        }

        if (worldPrompt) worldPrompt.SetActive(false);
    }

    public void OnInteract()
    {
        if (!pickedup) PickUpObject();
    }

    void Update()
    {
        if (pickedup)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ThrowObject();
                return;
            }

            Vector3 targetPos = cameraTrans.position + 
                               (cameraTrans.forward * holdDistance) + 
                               (cameraTrans.up * heightOffset) +
                               (cameraTrans.right * sideOffset);

            objTransform.position = Vector3.Lerp(objTransform.position, targetPos, Time.deltaTime * holdSmoothness);
            Quaternion targetRot = cameraTrans.rotation * Quaternion.Euler(rotationOffset);
            objTransform.rotation = Quaternion.Lerp(objTransform.rotation, targetRot, Time.deltaTime * holdSmoothness);

            ShowTrajectory();
        }
        else
        {
            if(trajectoryRenderer) trajectoryRenderer.positionCount = 0;
        }
    }

    void PickUpObject()
    {
        pickedup = true;
        
        if (playerAnimator != null) 
        {
            playerAnimator.SetTrigger(pickupTrigger);
        }
        
        if (crosshair1) crosshair1.SetActive(false);
        if (crosshair2) crosshair2.SetActive(false);

        if (highlightScript != null) highlightScript.ToggleHighlight(false);

        objRigidbody.useGravity = false;
        objRigidbody.isKinematic = true; 
        objTransform.parent = cameraTrans;

        foreach (Collider col in allColliders) col.enabled = false;

        if (worldPrompt) worldPrompt.SetActive(false);
        StartCoroutine(ShowThrowPromptAfterDelay(0.4f));
    }

    void ThrowObject()
    {
        pickedup = false;
        
        if (playerAnimator != null) 
        {
            playerAnimator.SetTrigger(throwTrigger);
        }
        
        if (crosshair1) crosshair1.SetActive(true);
        if (crosshair2) crosshair2.SetActive(false);
        
        if (highlightScript != null) highlightScript.ToggleHighlight(false);
        objTransform.parent = null;
        objRigidbody.useGravity = true;
        objRigidbody.isKinematic = false;

        foreach (Collider col in allColliders) col.enabled = true;
        
        objRigidbody.velocity = cameraTrans.forward * throwAmount;

        if (throwPrompt) throwPrompt.SetActive(false);
        StartCoroutine(PromptCooldownRoutine());
    }

    IEnumerator ShowThrowPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (throwPrompt && pickedup) throwPrompt.SetActive(true);
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
        if (pickedup) return;
        if (collision.gameObject.CompareTag("GarbageCan")) HandleGarbageCanCollision(collision);
    }

    void HandleGarbageCanCollision(Collision collision)
    {
        if(AudioManager.instance && FMODEvents.instance)
            AudioManager.instance.PlayOneShot(FMODEvents.instance.Done, transform.position);

        if (successEffect != null)
        {
            GameObject vfxObj = Instantiate(successEffect, transform.position, Quaternion.identity);
            Destroy(vfxObj, 2f);
        }
        Destroy(gameObject);
    }

    void ShowTrajectory()
    {
        if (trajectoryRenderer == null) return;

        Vector3 rawStartPos = objTransform.position + cameraTrans.TransformVector(trajectoryVisualOffset);
        Vector3 rawStartVel = cameraTrans.forward * throwAmount;

        smoothedStartPos = Vector3.Lerp(smoothedStartPos, rawStartPos, Time.deltaTime * predictionSmooth);
        smoothedStartVel = Vector3.Lerp(smoothedStartVel, rawStartVel, Time.deltaTime * predictionSmooth);

        Vector3 pos = smoothedStartPos;
        Vector3 vel = smoothedStartVel;

        int count = 0;
        for (int i = 0; i < predictionSteps; i++)
        {
            if (count < splineBuffer.Length) splineBuffer[count] = pos;
            count++;

            vel += Physics.gravity * timestep;
            Vector3 newPos = pos + vel * timestep;

            if (Physics.Raycast(pos, newPos - pos, out RaycastHit hit, (newPos - pos).magnitude, trajectoryCollisionMask))
            {
                if (count < splineBuffer.Length) splineBuffer[count] = hit.point;
                count++;
                break;
            }
            pos = newPos;
        }

        int outCount = (count - 1) * splineResolution;
        trajectoryRenderer.positionCount = Mathf.Max(0, outCount);

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

                if (idx < outCount) trajectoryRenderer.SetPosition(idx, p);
                idx++;
            }
        }
    }
}