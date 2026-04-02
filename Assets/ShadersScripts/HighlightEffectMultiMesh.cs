using UnityEngine;

public class HighlightEffectMultiMesh : MonoBehaviour
{
    [SerializeField] private Renderer[] objectRenderers; 
    [SerializeField] private string colorParam = "_OutlineColor"; 
    [SerializeField] private string widthParam = "_Outline"; 

    public float outlineThickness = 0.1f;
    public Color hoverOverColor = Color.magenta;

    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        // Force highlight OFF immediately on spawn
        ToggleHighlight(false);
    }

    public void ToggleHighlight(bool isOn)
    {
        if (objectRenderers == null) return;

        foreach (Renderer ren in objectRenderers)
        {
            if (ren == null) continue;

            // Get current properties
            ren.GetPropertyBlock(propBlock);

            if (isOn)
            {
                propBlock.SetColor(colorParam, hoverOverColor);
                propBlock.SetFloat(widthParam, outlineThickness);
            }
            else
            {
                // Ensure it is strictly 0
                propBlock.SetFloat(widthParam, 0f);
            }

            // Apply properties back to renderer
            ren.SetPropertyBlock(propBlock);
        }
    }
}