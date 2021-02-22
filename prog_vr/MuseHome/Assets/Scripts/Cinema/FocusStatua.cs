using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusStatua : MonoBehaviour
{
    public int indice;
    private GameObject statua;
    private CinemaMode cm = null;

    private void Start()
    {
        statua = this.gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (cm == null)
            {
                cm = other.GetComponentInChildren<CinemaMode>();
            }
            cm.setStatua(statua);
            cm.cinemaMode = true;
            cm.caricaTesto("statua");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            cm.cinemaMode = false;
            cm.shortText = false;
            cm.Defocus();
        }
    }
}
