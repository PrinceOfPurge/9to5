using UnityEngine;

public class HighlightEffectBananaAndGarbage : MonoBehaviour
{
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private string colorParam = "_OutlineColor"; 
    [SerializeField] private string widthParam = "_Outline"; // Adjust based on your shader

    public float outlineThickness = 0.1f;
    public Color regularColor = Color.black;
    public Color hoverOverColor = Color.magenta;

    private Material mat;

    void Awake()
    {
        if (objectRenderer != null)
        {
            // .material creates a local instance just for this object
            mat = objectRenderer.material;
            mat.SetColor(colorParam, regularColor);
            mat.SetFloat(widthParam, 0f); // Start invisible
        }
    }

    public void ToggleHighlight(bool isOn)
    {
        if (mat == null) return;

        if (isOn)
        {
            mat.SetColor(colorParam, hoverOverColor);
            mat.SetFloat(widthParam, outlineThickness);
        }
        else
        {
            mat.SetFloat(widthParam, 0f);
        }
    }
}