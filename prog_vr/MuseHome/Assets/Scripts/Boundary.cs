using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Boundary : MonoBehaviour
{
    public GameObject wallMarker;
    public float AreaSize = 0;
    public bool _roomChanged; //true se è avvenuto un cambiamento stanza (per test: checkbox)
    public int player_in_CurrentRoom = 1; //true se il player è nella currentRoom

    [SerializeField] private Vector3 playArea_dimensions;
    [SerializeField] private bool configured; //true=> guardian system configurato correttamente
    [SerializeField] private GameObject[] roomPrefabs; //prefabs delle stanze
    [SerializeField] private GameObject startRoom;// prefab della stanza di partenza
    [SerializeField] private GameObject curRoom, newRoom, oldRoom;// gameobject delle stanze presenti ad un certo istante

    private GameObject curRoompref, newRoompref, oldRoompref; //prefabs delle stanze presenti ad un certo istante
    private Vector3[] activeRoom_points; //conterrà i 121 punti ritornati dal metodo getcomponent
    private Vector3[] activeRoom_Corners; //angoli della stanza attiva (rettangolare)
    private Vector3 pos_1, pos_2, pos_3; //posizioni fisse per istanziare le stanze
    private int changes = 0;

    private GameObject door1, door2, door3;

    // Start is called before the first frame update
    void Start()
    {
        //configurazione del guardian system:
        configured = OVRManager.boundary.GetConfigured();
        //posizioni fisse delle 3 stanze presenti in game
        pos_1 = new Vector3(-60, 0, 0);
        pos_2 = new Vector3(0, 0, 0);
        pos_3 = new Vector3(60 ,0 ,0); // all'inizio non vi è alcuna stanza precedente

        _roomChanged = false; //true ogni volta che entro in una nuova stanza, compreso l'inizio dell'esecuzione
        player_in_CurrentRoom = 1; //inizialmente si è nella currentRoom.

        //trovo e mi salvo i gameobject delle porte usate per tornare indietro nei corridoi
        GameObject corridoio1 = GameObject.Find("Corridoio_1");
        GameObject corridoio2 = GameObject.Find("Corridoio_2");
        GameObject corridoio3 = GameObject.Find("Corridoio_3");

        door1 = FindChildObject(corridoio1, "Door_2_OldRoom");
        door2 = FindChildObject(corridoio2, "Door_2_OldRoom");
        door3 = FindChildObject(corridoio3, "Door_2_OldRoom");

        //start room [1 sola in tutto il gioco]: la istanzio e sarà la nostra prima currentRoom
        curRoom = Instantiate(startRoom, pos_1, Quaternion.identity);
        startRoom.tag = "CurrentRoom"; //prefab -> current [ma non tornerà mai nel pool]
        curRoom.tag = "CurrentRoom"; //istanza
        curRoompref = startRoom;
        //porta per tornare indietro (corrodoio1) : chiusa
        door1.SetActive(true);

        //imposto tutte le stanze nel pool a "poolRoom"
        foreach (GameObject room in roomPrefabs)
            room.tag = "PoolRoom";

        //estraggo casualmente una delle stanze nel pool
        int new_idx = Random.Range(0, roomPrefabs.Length - 1);
        newRoompref = roomPrefabs[new_idx];
        newRoom = Instantiate(newRoompref, pos_2, Quaternion.identity); //aggiorniamo il tag anche al prefab
        newRoompref.tag = "NewRoom"; //prefab
        newRoom.tag = "NewRoom"; //diventa new [in futuro tornerà nel pool]

        if (configured)
        {
            //Grab all the boundary points. Setting BoundaryType to PlayArea is necessary
            Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);

            // marker vengono instanziati in corrispondenza di tutti i portali (Fissi) => solo le stanze cambiano
            foreach (Vector3 pos in boundaryPoints)
            {
                Instantiate(wallMarker, pos + pos_1, Quaternion.identity); 
                Instantiate(wallMarker, pos + pos_2, Quaternion.identity);
                Instantiate(wallMarker, pos + pos_3, Quaternion.identity); 
            }

            //TODO: get Area size and check if it's smaller than room size
            playArea_dimensions = OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);
            //OVRBoundary.GetDimensions() returns a Vector3 containing the width, height, and depth in tracking space units, with height always returning 0.
            AreaSize = playArea_dimensions[0] * playArea_dimensions[2]; //m^2


        }
        else
        {
            //guardian non configurato: area di gioco predefinita=> il giocatore sarà seduto e si muoverà coi joystick.

        }

    }

    // Update is called once per frame
    void Update()
    {
        int new_idx;
        if (_roomChanged == true) //attenzione: se l'utente è tornato indietro, e poi torna nella current Room=>
                                                                   //quel passaggio nel portale NON DEVE modificare le stanze. Solo se ci si trova nella Current Room
                                                                   // il portale cambierà la disposizione!
        {
            if (player_in_CurrentRoom == 1)
            {
                switch (changes)
                {
                    case 0: //cur pos1->pos2, new pos2->pos3, old = pos1
                        freeRoom(oldRoom, oldRoompref);

                        //la vecchia currRoom diventa old Room
                        oldRoom = curRoom;
                        oldRoompref = curRoompref;
                        oldRoom.tag = "OldRoom";
                        oldRoompref.tag = "OldRoom";
                        //la vecchia new Room diventa la current Room
                        curRoom = newRoom;
                        oldRoompref = newRoompref;
                        curRoom.tag = "CurrentRoom";
                        curRoompref.tag = "CurrentRoom";

                        new_idx = Random.Range(0, roomPrefabs.Length - 1); //al momento prende anche quelli non "poolroom"
                        newRoompref = roomPrefabs[new_idx];
                        newRoom = Instantiate(newRoompref, pos_3, Quaternion.identity); //aggiorniamo il tag anche al prefab
                        newRoom.tag = "NewRoom"; //diventa new [in futuro tornerà nel pool]
                        newRoompref.tag = "NewRoom";

                        door1.SetActive(true); //porta 1 chiusa
                        door2.SetActive(false);//porta 2 aperta
                        door3.SetActive(false);//porta 3 aperta

                        changes++;
                        break;

                    case 1://cur pos2->pos3, new pos3->pos1, old = pos1-> pos2

                        //elimino la vecchia old Room e la rimetto nel pool
                        freeRoom(oldRoom, oldRoompref);

                        //la vecchia currRoom diventa old Room
                        oldRoom = curRoom;
                        oldRoompref = curRoompref;
                        oldRoom.tag = "OldRoom";
                        oldRoompref.tag = "OldRoom";
                        //la vecchia new Room diventa la current Room
                        curRoom = newRoom;
                        oldRoompref = newRoompref;
                        curRoom.tag = "CurrentRoom";
                        curRoompref.tag = "CurrentRoom";
                        //Istanziamo una nuova new Room in pos_3
                        new_idx = Random.Range(0, roomPrefabs.Length - 1); //al momento prende anche quelli non "poolroom"
                        newRoompref = roomPrefabs[new_idx];
                        newRoom = Instantiate(newRoompref, pos_1, Quaternion.identity); //aggiorniamo il tag anche al prefab
                        newRoom.tag = "NewRoom"; //diventa new [in futuro tornerà nel pool]
                        newRoompref.tag = "NewRoom";

                        door1.SetActive(false); //porta 1 aperta
                        door2.SetActive(true);//porta 2 chiusa
                        door3.SetActive(false);//porta 3 aperta
                        //next case
                        changes++;
                        break;

                    case 2: //cur pos3->pos1, new pos1->pos2, old = pos2-> pos3

                        //elimino la vecchia old Room e la rimetto nel pool
                        freeRoom(oldRoom, oldRoompref);

                        //la vecchia currRoom diventa old Room
                        oldRoom = curRoom;
                        oldRoompref = curRoompref;
                        oldRoom.tag = "OldRoom";
                        oldRoompref.tag = "OldRoom";
                        //la vecchia new Room diventa la current Room
                        curRoom = newRoom;
                        oldRoompref = newRoompref;
                        curRoom.tag = "CurrentRoom";
                        curRoompref.tag = "CurrentRoom";

                        //Istanziamo una nuova new Room in pos_1
                        new_idx = Random.Range(0, roomPrefabs.Length - 1); //al momento prende anche quelli non "poolroom"
                        newRoompref = roomPrefabs[new_idx];
                        newRoom = Instantiate(newRoompref, pos_2, Quaternion.identity); //aggiorniamo il tag anche al prefab
                        newRoom.tag = "NewRoom"; //diventa new [in futuro tornerà nel pool]
                        newRoompref.tag = "NewRoom";

                        door1.SetActive(false); //porta 1 aperta
                        door2.SetActive(false);//porta 2 aperta
                        door3.SetActive(true);//porta 3 chiusa
                        //next case
                        changes = 0; //riparte il ciclo

                        break;
                }
            }
            _roomChanged = false;

        }
    }

    Vector3[] getRoomCorners(GameObject floor) //floor sarà un piano, che costituisce il pavimento
    {

        Vector3[] activeRoom_points = floor.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;
        //il metodo per i vertici torna 121 punti del piano. In posizione: 0,10,110,120 abbiamo 
        //gli angoli del nostro piano (stanza
        Vector3[] corners = new Vector3[4];
        //transformPoint per averli in coordinate globali
        corners[0] = floor.transform.TransformPoint(activeRoom_points[0]);
        corners[1] = floor.transform.TransformPoint(activeRoom_points[10]);
        corners[2] = floor.transform.TransformPoint(activeRoom_points[110]);
        corners[3] = floor.transform.TransformPoint(activeRoom_points[120]);

        return corners; //ritorniamo i 4 angoli della faccia superiore (visibile) del pavimento
    }

    void freeRoom(GameObject room, GameObject roomPref)
    {
        if (room != null)
        {
            room.tag = "PoolRoom";
            roomPref.tag = "PoolRoom";
            Destroy(oldRoom);
        }
    }

    //find child object
    public GameObject FindChildObject(GameObject parent, string name)
    {
        Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                return t.gameObject;
            }
        }
        return null;
    }

    //metodo che dati pavimento, soffitto e muri crea una stanza
    public GameObject createNewRoom(GameObject pavimento, GameObject muri, GameObject soffitto)
    {
        return null;
    }
}