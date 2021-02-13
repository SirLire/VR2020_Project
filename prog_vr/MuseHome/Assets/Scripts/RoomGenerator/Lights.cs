using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lights : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] pointLights;
    public GameObject[] lampadine;
    public Material emitMaterial;
    public float minIntensity = 5f;
    public float maxIntensity = 20f;
    public float roomH, minRoomH, maxRoomH;
    private bool updated = false;
    void Start()
    {
        minRoomH = 3.49f;
        maxRoomH = 10f;
        //pointlight tutte spente di default. Vengono accese solo quelle del quadro interessato
        if (this.name == "Light")
        {
            turnOff_PointLights();
            turnOn_emissiveMaterial();
        }
        else
        {
            turnOn_PointLights();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(this.transform.parent != null && !this.updated)
        {
            roomH = this.transform.position.y + 0.22f;
            if(this.name == "Lampadario")
                turnOn_PointLights();
            this.updated = true;
        }
    }

    public void turnOff_PointLights()
    {
        foreach (GameObject p in this.pointLights)
        {
            p.gameObject.GetComponent<Light>().intensity = 0f;
        }
    }

    public void turnOn_PointLights()
    {
        foreach (GameObject p in this.pointLights)
        {
            if(this.name == "Light")
                p.gameObject.GetComponent<Light>().intensity = 3f;
            else //lampadario
            {
                p.gameObject.GetComponent<Light>().intensity = 20f;
                p.gameObject.GetComponent<Light>().range = 100f;
                float intensity = Intensity_scaleFactor() * maxIntensity + (1 - Intensity_scaleFactor()) * minIntensity;
                p.gameObject.GetComponent<Light>().intensity = intensity;
            }
        }
    }

    public void turnOff_emissiveMaterial()
    {
        foreach(GameObject l in this.lampadine)
        {
            l.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black);
        }
    }
    public void turnOn_emissiveMaterial()
    {
        foreach (GameObject l in this.lampadine)
        {
            l.GetComponent<Renderer>().material = emitMaterial;
        }
    }

    float Intensity_scaleFactor()
    {
        float scale = 1f - (float)(Mathf.Abs((maxRoomH - roomH) / (maxRoomH - minRoomH))); 
        return Mathf.Abs(scale);
    }
}
