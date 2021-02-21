using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpegniMuroPortale : MonoBehaviour
{
    private Color def;
    private Color now;
    List<IEnumerator> routines;
    // Start is called before the first frame update
    void Start()
    {
        routines = new List<IEnumerator>();
        def = this.GetComponent<Renderer>().material.color;
    }

    public void spegniMuro()
    {
        IEnumerator r;
        foreach (IEnumerator routine in this.routines)
        {
            StopCoroutine(routine);
        }
        routines.Clear();

        r = spegni(this.GetComponent<Renderer>().material.color);
        StartCoroutine(r);
        routines.Add(r);
    }

    IEnumerator spegni(Color attuale)
    {
        float t = 0;

        while (this.GetComponent<Renderer>().material.color.b > 0.01f * def.b)
        {
            now = Color.Lerp(attuale, Color.black, t);

            this.GetComponent<Renderer>().material.SetColor("_Color", now);

            t += 1.0f * Time.deltaTime;

            yield return null;
        }
    }

    public void accendiMuro()
    {
        IEnumerator r;
        foreach (IEnumerator routine in this.routines)
        {
            StopCoroutine(routine);
        }
        routines.Clear();

        r = accendi(this.GetComponent<Renderer>().material.color);
        StartCoroutine(r);
        routines.Add(r);
    }

    IEnumerator accendi(Color attuale)
    {
        float t = 0;

        while (this.GetComponent<Renderer>().material.color.b < 0.99f * def.b)
        {
            now = Color.Lerp(attuale, def, t);

            this.GetComponent<Renderer>().material.SetColor("_Color", now);

            t += 1.0f * Time.deltaTime;

            yield return null;
        }
    }
}
