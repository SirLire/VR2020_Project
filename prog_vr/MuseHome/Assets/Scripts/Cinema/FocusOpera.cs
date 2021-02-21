using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusOpera : MonoBehaviour
{
    public GameObject quadro;
    private CinemaMode cm = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (cm == null)
            {
                cm = other.GetComponentInChildren<CinemaMode>();
            }
            cm.setQuadro(quadro);
            cm.cinemaMode = true;
            cm.caricaTesto();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            cm.cinemaMode = false;
            //cm.smallHUD.SetActive(false);
            //cm.tablet.SetActive(false);
            cm.shortText = false;
            cm.Defocus();
        }
    }
}



/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusOpera : MonoBehaviour
{
    public GameObject quadro;
    private CinemaMode cm = null;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if(cm == null)
            {
                cm = other.GetComponentInChildren<CinemaMode>();
            }
            cm.setQuadro(quadro);
            cm.cinemaMode = true;
            cm.caricaTesto();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            cm.cinemaMode = false;
            cm.smallHUD.SetActive(false);
            cm.Defocus();
        }
    }
}*/
