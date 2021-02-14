using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemaMode : MonoBehaviour
{
    public Camera cam;
    public GameObject smallHUD, bigHUD;
    public GameObject quadro;
    public bool cinemaMode = false;
    public float angoloUscitaModCinema = 0.9f;
    public GameObject roomGenerator = null;

    // Start is called before the first frame update
    void Start()
    {
        smallHUD.SetActive(false);
        bigHUD.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
        CheckInterfaces();
        
    }

    private void CheckInterfaces()
    {
        RaycastHit hit;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));


        if (cinemaMode == false)
        {
            bigHUD.gameObject.SetActive(false);

            int layerMask = -1;
            if (Physics.Raycast(ray, out hit, 15.0f, layerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != null )
                {
                    if (hit.transform.tag == "Quadro")
                        smallHUD.SetActive(true);
                    else
                        smallHUD.SetActive(false);
                }
            }
            else
            {
                smallHUD.SetActive(false);
            }
        }
        else
        {
            smallHUD.gameObject.SetActive(false);
        }
    }

    public void CheckCinemaMode()
    {
        float divergenza = Vector3.Dot(quadro.transform.forward, cam.transform.forward);

        if (divergenza > angoloUscitaModCinema && !bigHUD.activeSelf)
        {
            Focus();
        }
        else if (divergenza < angoloUscitaModCinema && bigHUD.activeSelf)
        {
            Defocus();
        }
    }

    public void Focus()
    {
        Room stanzaCorrente;

        bigHUD.SetActive(true);
        if (roomGenerator != null)
        {
            if (roomGenerator.GetComponent<Boundary>().player_in_CurrentRoom >= 0)
            {
                stanzaCorrente = roomGenerator.GetComponent<Boundary>().getCurRoom();
            }
            else
            {
                stanzaCorrente = roomGenerator.GetComponent<Boundary>().getOldRoom();
            }

            roomGenerator.GetComponent<GenerateRoom>().turnOff_Lights(cam.transform.position, stanzaCorrente);
        }
    }

    public void Defocus()
    {
        Room stanzaCorrente;

        bigHUD.SetActive(false);
        if (roomGenerator != null)
        {
            if (roomGenerator.GetComponent<Boundary>().player_in_CurrentRoom >= 0)
            {
                stanzaCorrente = roomGenerator.GetComponent<Boundary>().getCurRoom();
            }
            else
            {
                stanzaCorrente = roomGenerator.GetComponent<Boundary>().getOldRoom();
            }

            roomGenerator.GetComponent<GenerateRoom>().turnOn_Lights(stanzaCorrente);
        }
    }
}
