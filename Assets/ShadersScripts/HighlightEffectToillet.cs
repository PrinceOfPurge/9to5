using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightEffectToilet : MonoBehaviour
{
    [SerializeField] private Material outlineShader;
    [SerializeField] private GameObject ToiletGameObject;
    private bool canSeeHighlight = false;
    public Color normalColor = Color.black;
    public Color hoverColor = Color.magenta;
    // Start is called before the first frame update
    void Start()
    {
        outlineShader.SetColor("_OutlineColor",normalColor);
    }
    private void OnMouseEnter()
    {
        if (canSeeHighlight) { outlineShader.SetColor("_OutlineColor", hoverColor); }
    }
    private void OnMouseExit()
    {
        outlineShader.SetColor("_OutlineColor", normalColor);
    }
    private void OnTriggerEnter(Collider other)
    {
        canSeeHighlight = true;
       Destroy (ToiletGameObject,20);
    }
    private void OnTriggerExit(Collider other)
    {
        canSeeHighlight = false;
    }
}
