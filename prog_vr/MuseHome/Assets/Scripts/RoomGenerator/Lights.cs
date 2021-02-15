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
    public float maxIntensity = 10f;
    public float roomH, minRoomH, maxRoomH;
    private bool updated = false;

    private List<IEnumerator> routinesAccensione = new List<IEnumerator>();
    private List<IEnumerator> routinesSpegnimento = new List<IEnumerator>();

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
        foreach (IEnumerator r in this.routinesAccensione)
        {
            StopCoroutine(r);
        }
        routinesAccensione.Clear();
        foreach (GameObject p in this.pointLights)
        {
            IEnumerator r = spegni(p, p.gameObject.GetComponent<Light>().intensity);
            routinesSpegnimento.Add(r);
            StartCoroutine(r);
        }
    }

    IEnumerator spegni(GameObject p, float curIntensity)
    {
        float t = 0;

        while (p.gameObject.GetComponent<Light>().intensity > 0.05f)
        {
            p.gameObject.GetComponent<Light>().intensity = Mathf.Lerp(curIntensity, 0, t);

            t += 1.0f * Time.deltaTime;

            yield return null;
        }
    }

    public void turnOn_PointLights()
    {
        foreach (IEnumerator r in this.routinesSpegnimento)
        {
            StopCoroutine(r);
        }
        routinesSpegnimento.Clear();
        foreach (GameObject p in this.pointLights)
        {
            IEnumerator r;
            if (this.name == "Light")
            {
                //p.gameObject.GetComponent<Light>().intensity = 3f;
                r = accendi(p, p.gameObject.GetComponent<Light>().intensity, 3.0f, 0.0f, 0.0f, "Light");
                routinesAccensione.Add(r);
                StartCoroutine(r);
            }

            else //lampadario
            {
                r = accendi(p, p.gameObject.GetComponent<Light>().intensity, maxIntensity, p.gameObject.GetComponent<Light>().range, 100.0f, "Lampadario");
                routinesAccensione.Add(r);
                StartCoroutine(r);
            }
        }
    }

    IEnumerator accendi(GameObject p, float curIntensity, float maxIntensity, float curRange, float maxRange, string tipo)
    {
        float t = 0;

        while (p.gameObject.GetComponent<Light>().intensity < 0.95f*maxIntensity)
        {
            if(tipo == "Light")
            {
                p.gameObject.GetComponent<Light>().intensity = Mathf.Lerp(curIntensity, maxIntensity, t);
            }
            else if(tipo == "Lampadario")
            {
                //p.gameObject.GetComponent<Light>().intensity = 20f;
                p.gameObject.GetComponent<Light>().range = Mathf.Lerp(curRange, maxRange, t); ;
                float intensity = Intensity_scaleFactor() * maxIntensity + (1 - Intensity_scaleFactor()) * minIntensity;
                p.gameObject.GetComponent<Light>().intensity = Mathf.Lerp(curIntensity, intensity, t); ;
            }
            
            
            t += 1.0f * Time.deltaTime;

            yield return null;
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
