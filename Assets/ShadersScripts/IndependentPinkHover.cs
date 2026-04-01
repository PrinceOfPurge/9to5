using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class IndependentPinkHoverMesh : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Camera targetCamera;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hoverMaterial;

    [Header("Hover Detection")]
    [SerializeField] private bool useChildRendererIfMissing = true;

    private bool isHovered = false;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            targetRenderer = useChildRendererIfMissing
                ? GetComponentInChildren<Renderer>()
                : GetComponent<Renderer>();
        }

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetRenderer == null)
        {
            Debug.LogWarning("[IndependentPinkHoverMesh] No Renderer found on " + gameObject.name);
            return;
        }

        if (normalMaterial == null)
        {
            normalMaterial = targetRenderer.sharedMaterial;
        }

        if (hoverMaterial == null)
        {
            Debug.LogWarning("[IndependentPinkHoverMesh] Hover Material is not assigned on " + gameObject.name);
        }

        SetNormal();
    }

    private void Update()
    {
        if (targetRenderer == null || targetCamera == null || hoverMaterial == null)
            return;

        bool currentlyHovered = IsMouseOverRendererBounds();

        if (currentlyHovered && !isHovered)
        {
            SetHover();
        }
        else if (!currentlyHovered && isHovered)
        {
            SetNormal();
        }
    }

    private bool IsMouseOverRendererBounds()
    {
        Bounds bounds = targetRenderer.bounds;

        Vector3[] corners = new Vector3[8];
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        corners[0] = targetCamera.WorldToScreenPoint(c + new Vector3(-e.x, -e.y, -e.z));
        corners[1] = targetCamera.WorldToScreenPoint(c + new Vector3(-e.x, -e.y, e.z));
        corners[2] = targetCamera.WorldToScreenPoint(c + new Vector3(-e.x, e.y, -e.z));
        corners[3] = targetCamera.WorldToScreenPoint(c + new Vector3(-e.x, e.y, e.z));
        corners[4] = targetCamera.WorldToScreenPoint(c + new Vector3(e.x, -e.y, -e.z));
        corners[5] = targetCamera.WorldToScreenPoint(c + new Vector3(e.x, -e.y, e.z));
        corners[6] = targetCamera.WorldToScreenPoint(c + new Vector3(e.x, e.y, -e.z));
        corners[7] = targetCamera.WorldToScreenPoint(c + new Vector3(e.x, e.y, e.z));

        float minX = corners[0].x;
        float maxX = corners[0].x;
        float minY = corners[0].y;
        float maxY = corners[0].y;

        foreach (Vector3 corner in corners)
        {
            minX = Mathf.Min(minX, corner.x);
            maxX = Mathf.Max(maxX, corner.x);
            minY = Mathf.Min(minY, corner.y);
            maxY = Mathf.Max(maxY, corner.y);
        }

        Vector3 mousePos = Input.mousePosition;

        return mousePos.x >= minX && mousePos.x <= maxX &&
               mousePos.y >= minY && mousePos.y <= maxY;
    }

    private void SetHover()
    {
        isHovered = true;
        targetRenderer.material = hoverMaterial;
    }

    private void SetNormal()
    {
        isHovered = false;
        targetRenderer.material = normalMaterial;
    }
}