using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridScroller : MonoBehaviour
{
    public RawImage rawImage; // optional if using RawImage
    public Image image;        // for tiled UI Image
    public float scrollSpeedX = 0.05f;
    public float scrollSpeedY = 0.0f;

    private Material runtimeMat;
    private Vector2 offset;

    void Start()
    {
        // duplicate material so we don't edit the shared one
        runtimeMat = Instantiate(image.material);
        image.material = runtimeMat;
    }

    void Update()
    {
        offset.x += scrollSpeedX * Time.deltaTime;
        offset.y += scrollSpeedY * Time.deltaTime;
        runtimeMat.mainTextureOffset = offset;
    }
}