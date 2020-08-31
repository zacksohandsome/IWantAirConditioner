using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int startIndex;
    public int shadowLength;
    public bool isTranslucent = false;

    public Renderer[] getImages;
    Color translucentColor = new Color(1, 1, 1, 0.1f);
    Color opaqueColor = new Color(1, 1, 1, 1f);

    private void Start()
    {
        getImages = GetComponentsInChildren<Renderer>();
        //Debug.Log("getImages.length = " + getImages.Length);
        if(isTranslucent)
        {
            SetImageTranslucent();
        }
        else
        {
            SetImageOpaque();
        }
    }

    //設定透明度
    public void SetImageTranslucent()
    {
        //Debug.Log("SetImageTranslucent getImages.length = " + getImages.Length);
        foreach (Renderer render in getImages)
        {
            render.material.color = translucentColor;
        }
    }

    public void SetImageOpaque()
    {
        //Debug.Log("SetImageOpaque getImages.length = " + getImages.Length);
        foreach (Renderer render in getImages)
        {
            render.material.color = opaqueColor;
        }
    }

}
