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
    private GameObject _luceAmbiente;
    private float _luceAmbienteIntensity;

    private List<IEnumerator> routinesAccensione = new List<IEnumerator>();
    private List<IEnumerator> routinesSpegnimento = new List<IEnumerator>();

    void Start()
    {
        _luceAmbiente = GameObject.Find("Directional Light");
        _luceAmbiente.GetComponent<Light>().intensity = 1f;
        _luceAmbienteIntensity = _luceAmbiente.gameObject.GetComponent<Light>().intensity;
        minRoomH = 3.49f;
        maxRoomH = 10f;
        //pointlight tutte spente di default. Vengono accese solo quelle del quadro interessato
        if (this.name == "Light")
        {
            foreach (GameObject l in this.pointLights)
                l.GetComponent<Light>().enabled = false;
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
        IEnumerator r;
        foreach (IEnumerator routine in this.routinesAccensione)
        {
            StopCoroutine(routine);
        }
        routinesAccensione.Clear();
        foreach (GameObject p in this.pointLights)
        {
            r = spegni(p, p.gameObject.GetComponent<Light>().intensity);
            routinesSpegnimento.Add(r);
            StartCoroutine(r);
        }
        /*r = spegni(_luceAmbiente, _luceAmbiente.gameObject.GetComponent<Light>().intensity);
        routinesSpegnimento.Add(r);
        StartCoroutine(r);*/
    }
    public void turnOff_DirLight()
    {
        IEnumerator r = spegni(_luceAmbiente, _luceAmbiente.gameObject.GetComponent<Light>().intensity);
        routinesSpegnimento.Add(r);
        StartCoroutine(r);
    }
    public void turnOn_DirLight()
    {
        IEnumerator r = accendi(_luceAmbiente, _luceAmbiente.gameObject.GetComponent<Light>().intensity, _luceAmbienteIntensity, 0.0f, 0.0f, "Light");
        routinesAccensione.Add(r);
        StartCoroutine(r);
    }

    IEnumerator spegni(GameObject p, float curIntensity)
    {
        float t = 0;

        while (p.gameObject.GetComponent<Light>().intensity> 0.05f)
        {
            p.gameObject.GetComponent<Light>().intensity = Mathf.Lerp(curIntensity, 0, t);

            t += 1.0f * Time.deltaTime;

            if (p.gameObject.GetComponent<Light>().intensity <= 0.05)
            {
                p.gameObject.GetComponent<Light>().intensity = 0;
            }

            yield return null;
        }

        p.gameObject.GetComponent<Light>().enabled = false;
    }

    public void turnOn_PointLights()
    {
        IEnumerator r;
        foreach (IEnumerator routine in this.routinesSpegnimento)
        {
            StopCoroutine(routine);
        }
        routinesSpegnimento.Clear();
        foreach (GameObject p in this.pointLights)
        {
            if (this.name == "Light")
            {
                //p.gameObject.GetComponent<Light>().intensity = 3f;
                r = accendi(p, p.gameObject.GetComponent<Light>().intensity, 1.0f, 0.0f, 0.0f, "Light");
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
        /*r = accendi(_luceAmbiente, _luceAmbiente.gameObject.GetComponent<Light>().intensity, _luceAmbienteIntensity, 0.0f, 0.0f, "Light");
        routinesAccensione.Add(r);
        StartCoroutine(r);*/
    }

    public void setLights()
    {
        minRoomH = 3.49f;
        maxRoomH = 10f;
        _luceAmbiente = GameObject.Find("Directional Light");
        _luceAmbienteIntensity = _luceAmbiente.gameObject.GetComponent<Light>().intensity;
        //float startIntensity = Intensity_scaleFactor() * 10 + (1 - Intensity_scaleFactor()) * 5;
        if (this.transform.parent != null)
        {
            roomH = this.transform.position.y + 0.22f;
           
        }
        float startIntensity = Intensity_scaleFactor() * maxIntensity + (1 - Intensity_scaleFactor()) * minIntensity;
        this.transform.Find("Point Light").gameObject.GetComponent<Light>().intensity = startIntensity;
        this.transform.Find("Point Light").gameObject.GetComponent<Light>().enabled = false;

    }
    IEnumerator accendi(GameObject p, float curIntensity, float maxIntensity, float curRange, float maxRange, string tipo)
    {
        float t = 0;

        while (p.gameObject.GetComponent<Light>().intensity < 0.95f*maxIntensity)
        {
            p.gameObject.GetComponent<Light>().enabled = true;
            if (tipo == "Light")
            {
                p.gameObject.GetComponent<Light>().intensity = Mathf.Lerp(curIntensity, maxIntensity, t);
            }
            else if(tipo == "Lampadario")
            {
                //p.gameObject.GetComponent<Light>().intensity = 20f;
                p.gameObject.GetComponent<Light>().range = Mathf.Lerp(curRange, maxRange, t);
                float intensity = Intensity_scaleFactor() * maxIntensity + (1 - Intensity_scaleFactor()) * minIntensity;
                p.gameObject.GetComponent<Light>().intensity = Mathf.Lerp(curIntensity, intensity, t);
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
