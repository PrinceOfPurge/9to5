using UnityEngine;

public class PlayerInteractions : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private LayerMask interactableLayer;

    private IInteractable currentTarget;

    void Update()
    {
        PerformRaycast();

        if (Input.GetKeyDown(KeyCode.E) && currentTarget != null)
        {
            currentTarget.OnInteract();
        }
    }

    private void PerformRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                if (interactable != currentTarget)
                {
                    // Looked at a new interactable
                    if (currentTarget != null) currentTarget.OnLoseFocus();
                    currentTarget = interactable;
                    currentTarget.OnFocus();
                }
            }
            else
            {
                // Hit something on the layer that isn't interactable
                ClearTarget();
            }
        }
        else
        {
            // Hit nothing
            ClearTarget();
        }
    }

    private void ClearTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.OnLoseFocus();
            currentTarget = null;
        }
    }
}