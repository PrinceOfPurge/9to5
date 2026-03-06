using UnityEngine;

public class HighlightEffectMultiMesh : MonoBehaviour
{
    [SerializeField] private Renderer[] objectRenderers; // Drag both Head and Plane here
    [SerializeField] private string colorParam = "_OutlineColor"; 
    [SerializeField] private string widthParam = "_Outline"; 

    public float outlineThickness = 0.1f;
    public Color hoverOverColor = Color.magenta;

    private Material[] mats;

    void Awake()
    {
        if (objectRenderers != null && objectRenderers.Length > 0)
        {
            mats = new Material[objectRenderers.Length];
            for (int i = 0; i < objectRenderers.Length; i++)
            {
                // Create unique material instances for every part
                mats[i] = objectRenderers[i].material;
                mats[i].SetFloat(widthParam, 0f);
            }
        }
    }

    public void ToggleHighlight(bool isOn)
    {
        if (mats == null) return;

        foreach (Material m in mats)
        {
            if (m == null) continue;
            
            if (isOn)
            {
                m.SetColor(colorParam, hoverOverColor);
                m.SetFloat(widthParam, outlineThickness);
            }
            else
            {
                m.SetFloat(widthParam, 0f);
            }
        }
    }
}