using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightEffectNew : MonoBehaviour
{
    [SerializeField] private Material outlineShader;
    public Color normalColor = Color.black;
    public Color hoverColor = Color.magenta;
    // Start is called before the first frame update
    void Start()
    {
        outlineShader.SetColor("_OutlineColor",normalColor);
    }
    private void OnMouseEnter()
    {
        outlineShader.SetColor("_OutlineColor", hoverColor);
    }
    private void OnMouseExit()
    {
        outlineShader.SetColor("_OutlineColor", normalColor);
    }
}
