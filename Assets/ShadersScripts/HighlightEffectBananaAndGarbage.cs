using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightEffectBananaAndGarbage : MonoBehaviour
{
    [SerializeField] private Material outlineShaderPink;
    private bool cannotSeeHighlight = true;
    public Color regularColor = Color.black;
    public Color hoverOverColor = Color.magenta;
    // Start is called before the first frame update
    void Start()
    {
        outlineShaderPink.SetColor("_OutlineColor", regularColor);
    }
    private void OnMouseEnter()
    {
        if (!cannotSeeHighlight) { outlineShaderPink.SetColor("_OutlineColor", hoverOverColor); }
    }
    private void OnMouseExit()
    {
        outlineShaderPink.SetColor("_OutlineColor", regularColor);
    }
    private void OnTriggerEnter(Collider other)
    {
        cannotSeeHighlight = false;

    }
    private void OnTriggerExit(Collider other)
    {
        cannotSeeHighlight = true;
    }
}
