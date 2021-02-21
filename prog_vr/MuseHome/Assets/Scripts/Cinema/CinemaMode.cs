using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; // per leggere da streaming assets

public class CinemaMode : MonoBehaviour
{
    public Camera cam;
    //public GameObject smallHUD, bigHUD;
    //public GameObject smallHUD_text, bigHUD_text;
    public GameObject tablet, tablet_text;
    private GameObject quadro;
    private int idQuadro;
    public bool cinemaMode = false;
    public float angoloUscitaModCinema = 0.9f;
    public GameObject roomGenerator = null;
    public bool inFocus = false;
    private string descUIPiccola, descUIGrossa;
    public TextAsset[] descrizioni;
    public bool trasparente = false;
    private bool longText;
    public bool shortText;

    public Material schermoNormale, schermoCinema;
    public GameObject schermo;
    // Start is called before the first frame update
    void Start()
    {
        //smallHUD.SetActive(false);
        //bigHUD.SetActive(false);
        tablet.SetActive(false);
        longText = false;
        shortText = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (cinemaMode)
        {
            schermo.GetComponent<Renderer>().material = schermoCinema;
            //writeTitle(smallHUD_text);
            if (!longText)
                writeTitle(tablet_text);
            CheckInterfaces();
            turnOff_otherRoomLights();
            if (!inFocus)
                Defocus();
        }
        else
        {
            tablet_text.GetComponent<TextMesh>().text = "MuseHome\nComandi:\n-premi A per ottenere più\ninformazioni\n- premi Y per nascondere o\nvisualizzare il tablet";
            tablet_text.GetComponent<TextMesh>().characterSize = 30;
            schermo.GetComponent<Renderer>().material = schermoNormale;
        }
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            inFocus = !inFocus;
        }

        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            trasparente = !trasparente;
        }
        //bigHUD.GetComponent<Canvas>().enabled = !trasparente;
        //smallHUD.GetComponent<Canvas>().enabled = !trasparente;
        tablet.SetActive(!trasparente);
        //text
        //bigHUD.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = !trasparente;
        //smallHUD.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = !trasparente;
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
                {
                    //smallHUD.SetActive(true);
                    shortText = true;
                }
                else
                {
                    //smallHUD.SetActive(false);
                    shortText = false;
                }
            }
            else
            {
                //smallHUD.SetActive(false);
                shortText = false;
            }
        }
        else
        {
            //smallHUD.SetActive(false);
            shortText = false;
            CheckCinemaMode();
        }
    }

    public void CheckCinemaMode()
    {
        float divergenza = Vector3.Dot(quadro.transform.forward, cam.transform.forward);
        if (quadro.name == "FrontPainting")
        {
            divergenza = Vector3.Dot(quadro.transform.right, cam.transform.forward);
        }


        if (/*divergenza > angoloUscitaModCinema && */!longText/*!bigHUD.activeSelf*/)
        {
            Focus();
        }
        //else if (/*divergenza < angoloUscitaModCinema &&*/ longText/* && bigHUD.activeSelf*/)
        {
           // Defocus();
        }
    }

    public void Focus()
    {
        Room stanzaCorrente;
        Room oldRoom, newRoom;
        //bigHUD.SetActive(true);
        longText = true;
        //tablet.SetActive(true);
        //writeText(bigHUD_text);
        Debug.Log("Focus");
        writeText(tablet_text);
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
        //bigHUD.SetActive(false);
        longText = false;
        Debug.Log("Defocus");
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

    public void setQuadro(GameObject q)
    {
        this.quadro = q;
        this.idQuadro = q.transform.GetChild(0).gameObject.GetInstanceID();
    }
    private void writeText(GameObject gui_text)
    {
        gui_text.GetComponent<TextMesh>().text = descUIGrossa;
        gui_text.GetComponent<TextMesh>().characterSize = 25;
    }
    private void writeTitle(GameObject gui_text)
    {
        gui_text.GetComponent<TextMesh>().text = descUIPiccola;
        gui_text.GetComponent<TextMesh>().characterSize = 30;
    }
    public void caricaTesto()
    {
        int indice = quadro.GetComponent<Display_Image>().indiceOpera;

        string text;
        text = descrizioni[indice].text;
        string[] str = text.Split('\n');
        descUIPiccola = str[0];
        descUIGrossa = str[1];
        str = descUIPiccola.Split('/');
        descUIPiccola = formattaTestoReturn(str[0].Split(' '), 35);
        descUIPiccola = descUIPiccola + '\n' + str[1] + '\n' + str[2];
        print(descUIPiccola);
        str = descUIGrossa.Split(' ');
        descUIGrossa = formattaTestoReturn(str, 35);
    }

    private string formattaTestoReturn(string[] str, int length)
    {
        string ret = "";
        int len = 0;
        foreach (string s in str)
        {
            len += (s.Length + 1);
            if (len > length)
            {
                ret = ret + "\n";
                len = s.Length + 1;
            }
            else if (ret.Length > 0)
            {
                ret = ret + " ";
            }
            ret = ret + s;
        }

        return ret;
    }
}




/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking; // per leggere da streaming assets

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
    private string descUIPiccola, descUIGrossa;
    public TextAsset[] descrizioni;
    public bool trasparente = false;
    // Start is called before the first frame update
    void Start()
    {
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
        
        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            trasparente = !trasparente;
        }
        bigHUD.GetComponent<Canvas>().enabled = !trasparente;
        smallHUD.GetComponent<Canvas>().enabled = !trasparente;
        //text
        bigHUD.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = !trasparente;
        smallHUD.transform.GetChild(1).gameObject.GetComponent<MeshRenderer>().enabled = !trasparente;
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
            smallHUD.SetActive(false);
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
        gui_text.GetComponent<TextMesh>().text = descUIGrossa;
    }
    private void writeTitle(GameObject gui_text)
    {
        gui_text.GetComponent<TextMesh>().text = descUIPiccola;
    }
    public void caricaTesto()
    {
        int indice = quadro.GetComponent<Display_Image>().indiceOpera;

        string text;
        text = descrizioni[indice].text;
        string[] str = text.Split('\n');
        descUIPiccola = str[0];
        descUIGrossa = str[1];
        str = descUIPiccola.Split('/');
        descUIPiccola = formattaTestoReturn(str[0].Split(' '), 35);
        descUIPiccola = descUIPiccola + '\n' + str[1] + '\n' + str[2];
        print(descUIPiccola);
        str = descUIGrossa.Split(' ');
        descUIGrossa = formattaTestoReturn(str, 35);
    }

    private string formattaTestoReturn(string[] str, int length)
    {
        string ret = "";
        int len = 0;
        foreach (string s in str)
        {
            len += (s.Length + 1);
            if (len > length)
            {
                ret = ret + "\n";
                len = s.Length + 1;
            }
            else if (ret.Length > 0)
            {
                ret = ret + " ";
            }
            ret = ret + s;
        }

        return ret;
    }
}*/
