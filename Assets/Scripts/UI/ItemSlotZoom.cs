using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using DentedPixel;

public class ItemSlotZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse Hovered Over the Image");
        LeanTween.scale(gameObject, new Vector2(4.5f, 4f), 0.2f) .setEase( LeanTweenType.easeInOutBack );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse Left the Image");
        LeanTween.scale(gameObject, new Vector2(4, 3.5f), 0.2f) .setEase( LeanTweenType.easeInOutBack );

    }

}
