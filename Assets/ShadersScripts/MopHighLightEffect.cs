using UnityEngine;

public class MopHighLighEffect : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private SpriteRenderer objectRenderer;

    [Header("Highlight Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    private void Awake()
    {
        if (objectRenderer == null)
            objectRenderer = GetComponentInChildren<SpriteRenderer>();

        if (objectRenderer == null)
        {
            Debug.LogWarning("[Highlight] No SpriteRenderer found on " + gameObject.name);
            return;
        }

        objectRenderer.color = normalColor;
    }

    public void ToggleHighlight(bool isOn)
    {
        if (objectRenderer == null) return;

        objectRenderer.color = isOn ? highlightColor : normalColor;
    }
}