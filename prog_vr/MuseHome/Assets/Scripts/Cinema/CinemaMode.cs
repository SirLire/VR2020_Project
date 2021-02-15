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

            //int layerMask = LayerMask.NameToLayer("Default");
            //int layerMask = 2 << 8;
            //layerMask = ~layerMask;

            int layerMask = -5;

            //int layerMask = LayerMask.NameToLayer("Ignore Raycast");
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
        if(quadro.name == "FrontPainting")
        {
            divergenza = Vector3.Dot(quadro.transform.right, cam.transform.forward);
        }


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
        Room oldRoom, newRoom;
        bigHUD.SetActive(true);
        if (roomGenerator != null)
        {
            if (roomGenerator.GetComponent<Boundary>().player_in_CurrentRoom >= 0)
            {
                stanzaCorrente = roomGenerator.GetComponent<Boundary>().getCurRoom();
                oldRoom = roomGenerator.GetComponent<Boundary>().getOldRoom();
            }
            else
            {
                stanzaCorrente = roomGenerator.GetComponent<Boundary>().getOldRoom();
                oldRoom = roomGenerator.GetComponent<Boundary>().getCurRoom();
            }
            newRoom = roomGenerator.GetComponent<Boundary>().getNewRoom();
            roomGenerator.GetComponent<GenerateRoom>().turnOff_Lights(cam.transform.position, stanzaCorrente);
            roomGenerator.GetComponent<GenerateRoom>().turnOff_Lights(cam.transform.position, newRoom);
            if (oldRoom != null)
                roomGenerator.GetComponent<GenerateRoom>().turnOff_Lights(cam.transform.position, oldRoom);
            stanzaCorrente.room_lights[0].gameObject.GetComponent<Lights>().turnOff_DirLight();
        }
    }

    public void Defocus()
    {
        Room stanzaCorrente;
        Room newRoom, oldRoom;
        bigHUD.SetActive(false);
        if (roomGenerator != null)
        {
            if (roomGenerator.GetComponent<Boundary>().player_in_CurrentRoom >= 0)
            {
                stanzaCorrente = roomGenerator.GetComponent<Boundary>().getCurRoom();
                oldRoom = roomGenerator.GetComponent<Boundary>().getOldRoom();
            }
            else
            {
                oldRoom = roomGenerator.GetComponent<Boundary>().getCurRoom();
                stanzaCorrente = roomGenerator.GetComponent<Boundary>().getOldRoom();
            }
            newRoom = roomGenerator.GetComponent<Boundary>().getNewRoom();
            roomGenerator.GetComponent<GenerateRoom>().turnOn_Lights(stanzaCorrente);
            roomGenerator.GetComponent<GenerateRoom>().turnOn_Lights(newRoom);
            if (oldRoom != null)
                roomGenerator.GetComponent<GenerateRoom>().turnOn_Lights(oldRoom);
            stanzaCorrente.room_lights[0].gameObject.GetComponent<Lights>().turnOn_DirLight();
        }
    }
}
