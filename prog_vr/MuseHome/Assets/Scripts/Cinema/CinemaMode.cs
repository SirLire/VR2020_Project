using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemaMode : MonoBehaviour
{
    public Camera cam;
    public GameObject smallHUD, bigHUD;
    public GameObject smallHUD_text, bigHUD_text;
    private GameObject quadro;
    private int idQuadro;
    public bool cinemaMode = false;
    public float angoloUscitaModCinema = 0.9f;
    public GameObject roomGenerator = null;
    public bool inFocus = false;
    // Start is called before the first frame update
    void Start()
    {
        bigHUD_text = bigHUD.transform.GetChild(1).gameObject/*.transform.GetChild(0).gameObject*/;
        smallHUD_text = smallHUD.transform.GetChild(1).gameObject/*.transform.GetChild(0).gameObject*/;
        smallHUD.SetActive(false);
        bigHUD.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (cinemaMode)
        {
            writeTitle(smallHUD_text);
            CheckInterfaces();
            turnOff_otherRoomLights();
            if (!inFocus)
                Defocus();
        }
        
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            inFocus = !inFocus;
        }
    }
    private void turnOff_otherRoomLights()//quando si sale su una piattaforma, spegne le luci nelle altre due stanze 
    {
        Room stanzaCorrente, oldRoom, newRoom;
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
            newRoom.turnOffRoomLight();
            if (oldRoom != null)
                oldRoom.turnOffRoomLight();

        }
    }
    private void CheckInterfaces()
    {
        if (!inFocus)
        {
            RaycastHit hit;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));

            int layerMask = -5;

            if (Physics.Raycast(ray, out hit, 15.0f, layerMask, QueryTriggerInteraction.Ignore))
            {
                int idHIT = hit.transform.gameObject.GetInstanceID();
                if (idHIT == idQuadro)
                    smallHUD.SetActive(true);
                else
                    smallHUD.SetActive(false);
            }
            else
            {
                smallHUD.SetActive(false);
            }
        }
        else
        {
            CheckCinemaMode();
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
        writeText(bigHUD_text);
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
            roomGenerator.GetComponent<GenerateRoom>().turnOff_Lights(cam.transform.position, stanzaCorrente, true);
            roomGenerator.GetComponent<GenerateRoom>().turnOff_Lights(cam.transform.position, newRoom, false);
            if (oldRoom != null)
                roomGenerator.GetComponent<GenerateRoom>().turnOff_Lights(cam.transform.position, oldRoom, false);
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
            if (!cinemaMode)
            {
                roomGenerator.GetComponent<GenerateRoom>().turnOn_Lights(newRoom);
                if (oldRoom != null)
                    roomGenerator.GetComponent<GenerateRoom>().turnOn_Lights(oldRoom);
            }
            stanzaCorrente.room_lights[0].gameObject.GetComponent<Lights>().turnOn_DirLight();
        }
        inFocus = false;
    }

    public void setQuadro (GameObject q)
    {
        this.quadro = q;
        this.idQuadro = q.transform.GetChild(0).gameObject.GetInstanceID();
    }
    private void writeText(GameObject gui_text)
    {
        string text, fileName;
        fileName = quadro.GetComponent<Display_Image>().nomeQuadro + ".txt";
        int len = fileName.Length;
        Debug.Log("filename  " + fileName);
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            text = www.text;
        }
        else
        {
            text = System.IO.File.ReadAllText(filePath);
        }
        gui_text.GetComponent<TextMesh>().text = text;
    }
    private void writeTitle(GameObject gui_text)
    {
        string text = quadro.GetComponent<Display_Image>().nomeQuadro;
        gui_text.GetComponent<TextMesh>().text = text;
        Debug.Log("small text:  " + text);
    }

}
