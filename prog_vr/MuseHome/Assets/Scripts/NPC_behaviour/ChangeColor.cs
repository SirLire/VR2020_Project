using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    private void Awake()
    {
        Color col = Random.ColorHSV();
        this.transform.GetChild(0).GetChild(1).GetComponent<Renderer>().material.color = col;
        float h, s, v;
        Color.RGBToHSV(col, out h, out s, out v);
        col = Color.HSVToRGB(h, s - Random.Range(s/3, s), v);
        this.transform.GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = col;
        this.transform.GetChild(0).GetChild(6).GetComponent<Renderer>().material.color = col;
    }
}
