using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lights : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] pointLights;
    public GameObject[] lampadine;
    public Material emitMaterial;
    void Start()
    {
        //pointlight tutte spente di default. Vengono accese solo quelle del quadro interessato
        if (this.name == "Light")
        {
            turnOff_PointLights();
            turnOn_emissiveMaterial();
        }
    }

    // Update is called once per frame
    void Update()
    {
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
                p.gameObject.GetComponent<Light>().intensity = 30f;
                p.gameObject.GetComponent<Light>().range = 75f;
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
}
