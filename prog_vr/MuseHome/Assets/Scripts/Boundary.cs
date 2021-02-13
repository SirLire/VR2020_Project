﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Boundary : MonoBehaviour
{
    public GameObject wallMarker;
    public float AreaSize = 0;
    public bool _roomChanged; //true se è avvenuto un cambiamento stanza (per test: checkbox)
    public int player_in_CurrentRoom = 1; //true se il player è nella currentRoom

    [SerializeField] private Vector3 playArea_dimensions;
    private Vector2 minsize, maxsize;
    private float roomH, minroomH, maxRoomH;
    [SerializeField] private bool configured; //true=> guardian system configurato correttamente
    [SerializeField] private GameObject[] roomPrefabs; //prefabs delle stanze
    [SerializeField] private GameObject startRoom;// prefab della stanza di partenza
    [SerializeField] private GameObject curRoom;
    private GameObject newRoom, oldRoom;// gameobject delle stanze presenti ad un certo istante

    private GameObject curRoompref, newRoompref, oldRoompref; //prefabs delle stanze presenti ad un certo istante
    private Vector3[] activeRoom_points; //conterrà i 121 punti ritornati dal metodo getcomponent
    private Vector3[] activeRoom_Corners; //angoli della stanza attiva (rettangolare)
    private Vector3 pos_1, pos_2, pos_3; //posizioni fisse per istanziare le stanze
    private int changes = 0;
    private int seenRoom = 0; //numero di stanze visitate
    private Vector3[] boundaryPoints;
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

        minroomH = 3.5f;
        maxRoomH = 8f;

        _roomChanged = false; //true ogni volta che entro in una nuova stanza, compreso l'inizio dell'esecuzione
        player_in_CurrentRoom = 1; //inizialmente si è nella currentRoom.


        //start room [1 sola in tutto il gioco]: la istanzio e sarà la nostra prima currentRoom
        curRoom = Instantiate(startRoom, pos_1, Quaternion.identity);
        startRoom.tag = "CurrentRoom"; //prefab -> current [ma non tornerà mai nel pool]
        curRoom.name = "CurrentRoom"; //istanza
        curRoompref = startRoom;

        boundaryPoints = null;
        if (configured)
        {
            //Grab all the boundary points. Setting BoundaryType to PlayArea is necessary
            boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
            playArea_dimensions = OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);
            minsize = new Vector2(playArea_dimensions[0] +1f, playArea_dimensions[2]+1f);        
        }
        else
        {
            minsize = new Vector2(5f, 6f);
        }
        newRoom = createRoom(pos_2, boundaryPoints);
        newRoom.name = "NewRoom";

    }

    // Update is called once per frame
    void Update()
    {
        //int new_idx;
        if (_roomChanged == true) //attenzione: se l'utente è tornato indietro, e poi torna nella current Room=>
                                                                   //quel passaggio nel portale NON DEVE modificare le stanze. Solo se ci si trova nella Current Room
                                                                   // il portale cambierà la disposizione!
        {
            
            StartCoroutine(changeRoom());
            
            _roomChanged = false;

        }
    }

    private IEnumerator changeRoom()
    {
        if (player_in_CurrentRoom == 1)
        {
            if (this.seenRoom < 15)
                this.seenRoom++; //visitato una nuova stanza
            switch (changes)
            {
                case 0: //cur pos1->pos2, new pos2->pos3, old = pos1
                    if (oldRoom != null)
                        DestroyImmediate(oldRoom);

                    yield return new WaitForSeconds(0.1f);

                    //la vecchia currRoom diventa old Room
                    oldRoom = curRoom;
                    oldRoom.name = "OldRoom";
                    //la vecchia new Room diventa la current Room
                    curRoom = newRoom;
                    curRoom.name = "CurrentRoom";

                    newRoom = createRoom(pos_3, boundaryPoints);
                    newRoom.name = "NewRoom";

                    changes++;
                    break;

                case 1://cur pos2->pos3, new pos3->pos1, old = pos1-> pos2

                    if (oldRoom != null)
                        DestroyImmediate(oldRoom);

                    yield return new WaitForSeconds(0.1f);

                    //la vecchia currRoom diventa old Room
                    oldRoom = curRoom;
                    oldRoom.name = "OldRoom";
                    //la vecchia new Room diventa la current Room
                    curRoom = newRoom;
                    curRoom.name = "CurrentRoom";
                    //Istanziamo una nuova new Room in pos_1

                    newRoom = createRoom(pos_1, boundaryPoints);
                    newRoom.name = "NewRoom";
                    //next case
                    changes++;
                    break;

                case 2: //cur pos3->pos1, new pos1->pos2, old = pos2-> pos3

                    if (oldRoom != null)
                        DestroyImmediate(oldRoom);

                    yield return new WaitForSeconds(0.1f);


                    //la vecchia currRoom diventa old Room
                    oldRoom = curRoom;
                    oldRoom.name = "OldRoom";
                    //la vecchia new Room diventa la current Room
                    curRoom = newRoom;
                    curRoom.name = "CurrentRoom";

                    //Istanziamo una nuova new Room in pos_2
                    newRoom = createRoom(pos_2, boundaryPoints);
                    newRoom.name = "NewRoom";
                    //next case
                    changes = 0; //riparte il ciclo

                    break;
            }
        }
        yield return new WaitForSeconds(0);
    }

    GameObject createRoom(Vector3 posizione, Vector3[] bpoint = null)
    {
        bool gConfig = configured;
        calculateRoomH();
        calculateMaxMinDimensions();
        Room newRoom = gameObject.GetComponent<GenerateRoom>().createRoom(posizione, this.roomH, this.minsize, this.maxsize, 60f, bpoint, gConfig);
        //GameObject r = gameObject.GetComponent<GenerateRoom>().createRoom(posizione, this.roomH, this.minsize, this.maxsize, 60f, bpoint, gConfig);
        //return r;
        return newRoom.room_GameObj;
    }

    void calculateRoomH()
    {
        float limit = 15; //limite di massimo di stanze: dopo la 15 saranno tutte grandi e alte
        float numR = this.seenRoom;
        if (this.seenRoom >= 15)
            numR = 15;
        float scaleFactor = 1f - Mathf.Abs(limit - numR) / (limit);

        this.roomH = scaleFactor * maxRoomH + (1 - scaleFactor) * minroomH;
    }

    void calculateMaxMinDimensions()
    {
        float limit = 15; //limite di massimo di stanze: dopo la 15 saranno tutte grandi e alte
        Vector2 maxArea = new Vector2(20, 20);
        float xmin_increase = 0f;
        float xmax_increase = 1f;
        float xincrease,zincrease;
        float numR = this.seenRoom;
        if (this.seenRoom >= 15)
            numR = 15;
        float scaleFactor = 1f - Mathf.Abs(limit - numR) / (limit);
        if (this.configured)
        {
            xincrease = scaleFactor * xmax_increase + (1 - scaleFactor) * xmin_increase;
            zincrease = scaleFactor * maxArea.y + (1 - scaleFactor) * playArea_dimensions[2];
            this.minsize = new Vector2(this.minsize.x + 0.5f*xincrease, this.minsize.y); //su asse x non potrà crescere troppo, altrimenti non si vedranno i quadri
            
            this.maxsize = new Vector2(this.minsize.x + xincrease, zincrease);
        }
        else
        {
            
            this.minsize = new Vector2(0.75f*scaleFactor*maxArea.x + (1-scaleFactor)*this.minsize.x,
                0.75f * scaleFactor * maxArea.y + (1 - scaleFactor) * this.minsize.y
                );
            this.maxsize = new Vector2(scaleFactor * maxArea.x + (1 - scaleFactor) * this.minsize.x,
                scaleFactor * maxArea.y + (1 - scaleFactor) * this.minsize.y
                );
        }

    }
}