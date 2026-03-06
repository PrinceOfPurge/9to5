using UnityEngine;

public class HighlightEffectToilet : MonoBehaviour
{
    [SerializeField] private Renderer toiletRenderer;
    [Header("Shader Property Names")]
    [SerializeField] private string colorParam = "_OutlineColor"; 
    [SerializeField] private string widthParam = "_Outline"; // Often "_OutlineWidth" or "_Thickness"
    
    [Header("Settings")]
    public float outlineThickness = 0.1f; // Adjust this for "Bigger" outline
    public Color hoverColor = Color.magenta;

    private Material mat;

    void Awake()
    {
        if (toiletRenderer != null)
        {
            mat = toiletRenderer.material;
            // Start invisible
            mat.SetFloat(widthParam, 0f);
        }
    }

    public void ToggleHighlight(bool isOn)
    {
        if (mat == null) return;

        if (isOn)
        {
            mat.SetColor(colorParam, hoverColor);
            mat.SetFloat(widthParam, outlineThickness);
        }
        else
        {
            mat.SetFloat(widthParam, 0f);
        }
    }
}