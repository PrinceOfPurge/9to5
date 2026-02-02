using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPreview : MonoBehaviour
{
    [Header("References")]
    public objPickup pickupScript;    // Assign your pickup script
    public LineRenderer lineRenderer; // Assign in Inspector

    [Header("Trajectory Settings")]
    public int points = 30;
    public float timeStep = 0.05f;

    private void Start()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (pickupScript == null || lineRenderer == null)
            return;

        // Only show when object is being held
        if (pickupScript.pickedup)
        {
            lineRenderer.enabled = true;
            DrawTrajectory();
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    void DrawTrajectory()
    {
        Vector3 startPos = pickupScript.objTransform.position;

        // Initial throw velocity using your exact system
        Vector3 startVel = pickupScript.cameraTrans.forward * pickupScript.throwAmount * Time.deltaTime;

        lineRenderer.positionCount = points;

        for (int i = 0; i < points; i++)
        {
            float t = i * timeStep;

            // Physics formula: P(t) = P0 + V0*t + 1/2 * g * t²
            Vector3 point = startPos 
                            + startVel * t 
                            + 0.5f * Physics.gravity * (t * t);

            lineRenderer.SetPosition(i, point);
        }
    }
}
