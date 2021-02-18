using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenerateRoom : MonoBehaviour
{
    public GameObject sidewall, portalwall, frontwall, roof, floor, roomParent,bench;
    public GameObject emitter;
    public GameObject Chandelier;
    public GameObject audioEmitter;
    public GameObject capitello, corpoColonna, baseColonna;
    public GameObject ornament;
    public GameObject torch;
    public GameObject paint_marker; //paint di test: cubo nero
    private GameObject room;
    public GameObject borderMarker, borderDelimiter; //marker degli angoli e nastro tra i singoli marker intorno all'area di gioco
    public GameObject empty_4_paintBench; //empty padre di una coppia quadro/panca
    public Material[] wall_materials, floor_materials, roof_materials;//vettori di materiali per pavimento, soffitto e muri
    public Texture portalWall_tex;
    public GameObject[] statues; //elenco di asset di statue
    private int wmaterial_Num, wmat_indx, fmaterial_Num, fmat_indx, rmaterial_Num, rmat_indx;//indici e lunghezze dei vettori di sopra
    public Vector2 minsize, maxsize; //dimensioni massima e minima della stanza
    //in caso di guardian system, la larghezza delle stanze (asse x) non varia troppo, ma varia la lunghezza

    public Vector3 position1; //posizione in cui allocare la stanza
    private bool enoughSpace; 
    public float roomHeight; //altezza della stanza
    private Vector2 randomSize; //dimensione della stanza calcolata random tra min e maxsize
    public List<GameObject> paintBenchCouples, Lights, paintings, benches; //liste contenenti empty parents, luci, quadri e panche

    private float distance_wall_border_x, distance_wall_border_z; //usate per istanziare le panche a corretta distanza dal border
    private Vector3[] playArea_positions; //coordinate degli angoli del guardianSystem
    private bool guardian_border_singleton = false; //dopo aver istanziato 3 volte i bordi del guardian (STATICI), diventa true => non se ne istanziano altri
    private float distance_between_rooms;
    private float xmin, xmax, zmin, zmax; //per istanziare i cordoni
    private GameObject[] emitters;
    //NPC
    private List<GameObject> NPCs = new List<GameObject>();
    public int minNPCs = 1, maxNPCs = 6; //probabilmente dovrebbe dipendere dalla dimensione della stanza
    public GameObject NPC;
    private Vector3 newPosition;
    public float trackTimePos = 0f;
    public AudioClip music;

    public Shader wallWithWindows;
    public GameObject window;
    public GameObject latoFinestra;
    public GameObject angoloFinestra;

    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        trackTimePos = (trackTimePos+ Time.deltaTime)% music.length;
    }
    
    //METODO GENERICO PER CREARE STANZE!
    public Room createRoom(Vector3 pos, float roomH, Vector2 Minsize, Vector2 Maxsize ,float distance_bet_rooms = 60, Vector3[] guardianCorners = null, bool guardianConfigured = false)
    {
        this.roomHeight = roomH;
        this.minsize = Minsize;
        this.maxsize = Maxsize;
        float t_attraversamentoPortale = 1f; //tempo in secondi per attraversare il portale
        paintBenchCouples = new List<GameObject>();
        Lights = new List<GameObject>();
        paintings = new List<GameObject>();
        benches = new List<GameObject>(); //liste contenenti empty parents, luci, quadri e panche resettate ad ogni stanza creata

        Room newRoom = new Room();
        if (roomH < 3.5f)//altezza minima: 3.5f!
            this.roomHeight = 3.5f;
        if (roomH > 10f)
            this.roomHeight = 10f; //altezza massima=10f!
        Vector3[] positions = null;
        if (guardianConfigured)
        {
            positions = guardianCorners;
        }
        
        GameObject r = generateRoom(sidewall, portalwall, frontwall, roof,
                            floor, minsize, maxsize, roomHeight, pos,
                            roomParent, paint_marker, bench, positions, newRoom);
        newRoom.room_GameObj = r;
        if(!guardianConfigured)
            newRoom.resumeTrack(trackTimePos + t_attraversamentoPortale);
        generateNPCs(pos, minNPCs, maxNPCs, newRoom.room_GameObj, newRoom, newRoom.Area);

        //return r;
        return newRoom;
    }
    void generateNPCs(Vector3 position, int minNPCs, int maxNPCs, GameObject room, Room newRoom, Vector2 Area)
    {
        this.GetComponent<NavMeshSurface>().BuildNavMesh();
        int maxNPCNum = 1;
        float maxArea = 20 * 20;
        float minA = Area.x * Area.y;

        float unit = maxArea / maxNPCs;

        float ratio = minA/unit; //per ogni unit, aumentiamo di 1 il numero massimo di NPC
        maxNPCNum = (int)ratio;
        int minNPCNum = Mathf.Max(1, maxNPCNum - 1);

        int N_NPC = Random.Range(minNPCNum, maxNPCNum);
        if (Area.x < 6 || Area.y < 6)
            N_NPC = 0;
        List<Vector3> TargetsLocations = new List<Vector3>();
        List<GameObject> g_benches = newRoom.room_benches;
        for (int j = 0; j < g_benches.Count; j++)
        {
            TargetsLocations.Add(g_benches[j].transform.position);
        }
        if(TargetsLocations.Count > 0)
        {
            for (int i = 0; i < N_NPC; i++)
            {
                GameObject new_NPC = Instantiate(NPC, position, Quaternion.identity);
                NPC_NavController controller = new_NPC.GetComponent<NPC_NavController>();
                List<Vector3> TargetsPaintings = new List<Vector3>();
                List<GameObject> g_paintings = newRoom.room_paintings;
                for (int j = 0; j < g_paintings.Count; j++)
                {
                    TargetsPaintings.Add(g_paintings[j].transform.position);
                }
                controller.Targets = TargetsLocations;
                controller.Paintings = TargetsPaintings;
                NPCs.Add(new_NPC);
                new_NPC.transform.parent = room.gameObject.transform;
            }
        }
    }


    

    /*Parameters:
     * (Prefabs)
     * -sidewall: muri destro e sinistro della stanza
     * -portalWall: muro con buco per portale (cube. Interno su asse x locale)=> muri statici già presenti in scena. Cambiarne solo materiale e altezza.
     * -frontwall: muro frontale. Può essere uguale o diverso da sidewall (es: muro con finestra) (cube, interno su asse x locale)
     * -roof: soffitto, potrebbe avere varie texture o forme (plane)
     * -floor: essenzialmente, piano con diverse texture (plane)
     * -roomParent: empty padre di tutti gli elementi della stanza (empy)
     * -bench: modello 3D delle panche dei quadri
     * (Vector2: x,y)
     * -min_size, max_size: dimensioni min e max di una stanza [min_size= (playarea+1.5, playarea+1.5) se Guardian configurato, altrimenti (2.5,2.5)]
     *                                                         [maxsize aumenta col procedere dell'esecuzione, per avere stanze sempre più grandi fino ad un limite fissato]
     * (float)
     * -roomHeight: altezza della stanza [da aumentare esternamente all'avanzare dell'esecuzione]
     * -distance_bet_room: distanza tra i centri (asse x) delle stanze [usata per istanziare i border del guardian in ogni stanza]
     * (Vector3)
     * -position: posizione in cui istanziare la stanza
     * (Vector3[])
     * -guardianCorners: se il guardian è configurato, qui ci sono le sue coordinate (4 angoli)
     * (bool)
     * -guardianConfigured= true se è stato configurato il guardian System 
     
     */
    public GameObject generateRoom(GameObject sidewall, GameObject portalWall, GameObject frontWall, GameObject roof, GameObject floor, 
                            Vector2 minsize, Vector2 maxsize, float roomHeight, Vector3 pos, GameObject roomParent,
                            GameObject painting, GameObject bench,Vector3[] guardianCorners, Room newRoom, float distance_bet_rooms = 60, bool guardianConfigured = false)
    {

        //si salva le coordinate del guardianSystem se è configurato
        if (guardianConfigured)
        {
            playArea_positions = guardianCorners;
            distance_between_rooms = distance_bet_rooms;

        }
        randomSize = new Vector2(
            Random.Range(minsize.x, maxsize.x), //asse x
            Random.Range(minsize.y, maxsize.y) //asse y
        );
        newRoom.Area = randomSize;
        Vector3 position = fix_roomPosition(pos, randomSize.y, 1.5f);
        newPosition = position;
        //lista quadri allocati
        paintBenchCouples = new List<GameObject>();
        //istanzio l'empty in position
        GameObject empty = Instantiate(roomParent, position, Quaternion.identity);
        empty.name = "generatedRoom";
        //1)pavimento

        //istanzio il floor in position, come child di empty
        GameObject newFloor = Instantiate(floor, position, Quaternion.identity);
        newFloor.transform.parent = empty.transform;
        newFloor.name = "Floor";
        
        if (randomSize.x > 5f)
            enoughSpace = true;
        else
            enoughSpace = false;
        setMaterial(newFloor);
        //modifico le dimensioni del floor tra (minsize,maxsize)
        //NB!! II plane hanno dimensione default 10x10, quindi le scale vanno divise per 10 per averle in metri!
        newFloor.transform.localScale = new Vector3(randomSize.x/10f, newFloor.transform.localScale.y, randomSize.y/10f); //il piano mantiene y uguale (up vector), e cambia x,z (larghezza e lunghezza)
        instantiateBorders(guardianConfigured, newFloor, newRoom);
        //2) muri laterali: cubi con asse +x locale => verso l'interno della stanza. y=altezza, z=larghezza

        //istanzio i 2 sidewall a distanza +floor.localScale.x/2 (e -floor.LocalScale.x/2) dal centro del floor

        float floorWidth = randomSize.x;
        float floorLength = randomSize.y;
        //posizione del centro del muro: a distanza x/2 dal centro della stanza (pavimento), z invariato, ad altezza position.y + roomHeight/2 (i muri saranno alti roomHeight, il centro sarà a metà altezza)
        // y - 0.01f per rendere attaccati per bene muri e pavimento
        Vector3 wall_sx_pos = new Vector3(position.x + floorWidth / 2f, position.y + roomHeight / 2f - 0.01f, position.z);
        Vector3 wall_dx_pos = new Vector3(position.x - floorWidth / 2f, position.y + roomHeight / 2f - 0.01f, position.z);

        GameObject sidewall_dx = Instantiate(sidewall, wall_dx_pos, Quaternion.identity);
        GameObject sidewall_sx = Instantiate(sidewall, wall_sx_pos, Quaternion.identity);
        sidewall_dx.name = "sidewall_DX";
        sidewall_sx.name = "sidewall_SX";


        //scalo i muri in modo coerente alle dimensioni del pavimento (spessore muri:x= 0.25f)
        Vector3 sideWall_size = new Vector3(0.25f, roomHeight, floorLength);
        sidewall_dx.transform.localScale = sideWall_size;
        sidewall_sx.transform.localScale = sideWall_size;

        //materiale dei muri
        setMaterial(sidewall_dx);
        setMaterial(sidewall_sx, false);

        Vector3 fw_direction = new Vector3(0, 0, 1);
        //5) Frontwall: diverso perchè potrebbe avere materiali diversi, essere una finestra. in base a dimensione stanza, può avere o meno quadri (se troppo lontano da playarea. NO!)
        Vector3 frontwallPos = new Vector3(position.x, position.y + roomHeight / 2f - 0.01f, position.z - floorLength / 2f); // si trova in direzione -z
        GameObject fwall = Instantiate(frontWall, frontwallPos, Quaternion.identity);
        fwall.name = "frontWall";
        setMaterial(fwall, false);

        //col = getRandomColorHSV();
        //setColor(fwall, col);

        //spessore muri su x
        Vector3 frontwall_size = new Vector3(0.25f, roomHeight, floorWidth);
        fwall.transform.localScale = frontwall_size;
        fwall.transform.rotation = Quaternion.AngleAxis(90, Vector3.up); //per mantenere sull'asse z locale la larghezza del muro
        instantiateTorches(fwall);

        /*Color col = getRandomColorHSV();
        setColor(sidewall_dx, col);
        setColor(sidewall_sx, col);
        */
        /*3 casi distinti:
         * 1) direction=(1,0,0) -> asse +x => muro DX
         * 2) direction=(-1,0,0) -> asse-x => muro SX
         * 3) direction=(0,0,1) -> asse +z => frontWall (solo se esplicitamente richiesto)
         */
        Vector3 dx_direction = new Vector3(1, 0, 0);
        Vector3 sx_direction = new Vector3(-1, 0, 0);
        instantiatePaintings(sidewall_sx, newFloor, painting, sx_direction, bench, empty,
                             floorLength, guardianConfigured, newRoom, 0.5f, 6);
        instantiateOrnaments(sidewall_sx, ornament);

        if (randomSize.y > 9f && !guardianConfigured)
        {
            instantiatePaintings(fwall, newFloor, painting, fw_direction, bench, empty,
                floorLength, guardianConfigured, newRoom, 0.75f /** distance*/, 1);
        }
        fwall.transform.parent = empty.transform;

        if (enoughSpace) {// istanzia dipinti e panche solo se c'è abbastanza spazio per entrambi i lati
            instantiatePaintings(sidewall_dx, newFloor, painting, dx_direction, bench, empty,
                         floorLength, guardianConfigured, newRoom, 0.5f, 6);
            instantiateOrnaments(sidewall_dx, ornament);
        }
        //muri figli dell'empty
        sidewall_dx.transform.parent = empty.transform;
        sidewall_sx.transform.parent = empty.transform;



        //3)Portal wall: 
        Vector3 portalWallPos = new Vector3(position.x, position.y + roomHeight / 2f - 0.01f, position.z + floorLength / 2f); // si trova in direzione +z
        GameObject pwall = Instantiate(portalWall, portalWallPos, Quaternion.identity);
        pwall.name = "portalWall";
        //materiale muro
        setMaterial(pwall, true);
        InstantiateWindows(pwall, floorWidth, empty.transform);

        //setColor(pwall, col);

        //spessore muri su x
        Vector3 portalWall_size = new Vector3(0.25f, roomHeight, floorWidth);
        pwall.transform.localScale = portalWall_size;
        pwall.transform.rotation = Quaternion.AngleAxis(90, Vector3.up); //per mantenere sull'asse z locale la larghezza del muro

        pwall.transform.parent = empty.transform;

        //4) Roof: simile a floor, ma può avere materiali diversi

        //istanzio il floor in position, come child di empty
        Vector3 roofPosition = new Vector3(position.x, position.y + roomHeight - 0.02f, position.z);
        GameObject newRoof = Instantiate(roof, roofPosition, Quaternion.identity);
        newRoof.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward); //per avere la normale del piano verso l'interno
        newRoof.transform.parent = empty.transform;
        newRoof.name = "Roof";
        setMaterial(newRoof);
        newRoof.transform.localScale = new Vector3(floorWidth / 10f, newFloor.transform.localScale.y, floorLength / 10f); //il piano mantiene y uguale (up vector), e cambia x,z (larghezza e lunghezza)
        instantiateChandelier(Chandelier, newRoof, newRoom);
        
        instantiateColumns(newFloor, newRoof, roomHeight);
        //Per evitare panche che si compenetrano, idea semplice: pochi quadri centrati nel muro
        instantiateStatue(newFloor, randomSize, roomHeight);
        float distance = newFloor.transform.localScale.x*10 / 2f;
        //se ho abbastanza spazio per una bench centrale (3=totale distanza da muri, + 2 bench)



        //ritorna un gameobject empty che ha come figli tutti gli elementi della stanza
        return empty;
    }







    //instanzia i quadri e ne ritorna il numero

    /*Parametri:
     * (GameObject)
     * - wall: muro preso in considerazione
     * - marker: TEST => viene usato per fare i quadri ---> da cambiare con i quadri veri e propri
     * - floor: centro della stanza, per riferimenti
     * 
     * (float)
     * - space_between_paint: distanza tra due quadri e tra quadri e angoli stanza- > DEFAULT:0.5f
     * (int)
     * - paintNum: numero massimo di quadri che si voglio allocare
     * (Vector3):
     * - direction: versore della direzione della faccia interna del muro (+x, -x ecc)
     * (???)
     * - TODO: serve un parametro per prendere le effettive texture e mesh dei quadri (per test si usano quadri RANDOM!)
     */
    private void instantiatePaintings(GameObject wall, GameObject floor, GameObject marker, Vector3 direction, 
                             GameObject bench, GameObject room, float playarea_len, bool guardianConfigured, Room newRoom,
                             float space_between_paint = 0.75f, int paintNum = 5)
    {
        float used_space =0;
        float paint_max_h = 3f; //altezza massima dei quadri
        float portal_length = 1.6f; //lunghezza (asse z) del portale: i quadri partono solo dopo il portale
        float usable_space;
        if (guardianConfigured)
        {
            usable_space = playarea_len; //i quadri possono arrivare solo ai limiti della playarea
        }
        else
        {
            usable_space = wall.transform.localScale.z -1; //posso usare tutto il muro
        }

        Vector3 center;
        Vector2 size;//TEST: dimensioni dei singoli quadri random (da cambiare)
        int paint_instances=0;
        //dimensioni dei quadri(TEST)
        size.y = 2;
        size.x = Random.Range(1f, (usable_space / 4f));

        //quadri
        paint_instances = 0;
   


        //wall: su asse x spessore muro (x positivo per interno del muro!), su asse y altezza del muro, su z larghezza
        Vector3 floor_center = floor.transform.position;

        //singolo muro:

        center = wall.transform.position; //centro del muro
 
        Vector3 posx;

        //posizione del centro del muro, sulla faccia +x e sulla faccia -x (usato per capire quale di queste si affaccia al centro della stanza)
        Vector3 pos_x_1 = center + wall.transform.right * wall.transform.localScale.x / 2;
        Vector3 pos_x_2 = center - wall.transform.right * wall.transform.localScale.x / 2;


        //trovo il lato a minor distanza dal centro della stanza => si salva la posizione del centro della faccia più vicina al centro stanza (posx)
        if (Vector3.Distance(pos_x_1, floor_center) < Vector3.Distance(pos_x_2, floor_center))
            posx = pos_x_1;
        else
            posx = pos_x_2;

        if (roomHeight >= 4f)
        {
            posx.y = 1.75f; //per mantenere i quadri ad altezza osservabile
        }
        //posizione di partenza da cui allocare i quadri (da dx a sx, partendo da vicino il portale)
        Vector3 start_pos;
        if (wall.name == "sidewall_DX")
        {
            start_pos = posx - wall.transform.forward * (usable_space / 2f + space_between_paint - size.x / 2f - portal_length);
        }
        else
        {
            start_pos = posx + wall.transform.forward * (usable_space / 2f - space_between_paint - size.x / 2f - portal_length);
        }
        //per evitare che compenetri nel muro
        float offset_from_wall = 0.01f;
        int num = paintNum;
        switch (wall.name)
        {
            case ("sidewall_DX"):
                start_pos.x += offset_from_wall;
                break;
            case ("sidewall_SX"):
                start_pos.x -= offset_from_wall;
                break;
            case ("frontWall"):
                start_pos.z += offset_from_wall;
                start_pos.x = wall.transform.localPosition.x;
                num = 1;
                break;
        }

        //spazio del muro usato: per controllare di non eccedere
        used_space = space_between_paint + size.x+ portal_length;


        //allocazione dei quadri:
        for (int i = 0; i < num && used_space <= usable_space - space_between_paint; i++)
        {
            //if (used_space <= usable_space - space_between_paint)
            //{
            if (i != 0)
            {
                if (wall.name == "sidewall_DX")
                {
                    start_pos += wall.transform.forward * (size.x / 2f + 1f * space_between_paint); //avanza la posizione
                }
                else
                {
                    start_pos -= wall.transform.forward * (size.x / 2f + 1f * space_between_paint); //avanza la posizione
                }
            }
            //istanziamo l'empty padre della coppia
            GameObject coupleParent = Instantiate(empty_4_paintBench, start_pos, Quaternion.identity);
            coupleParent.name = "Painting&Bench";
            coupleParent.transform.parent = room.transform;
            //istanziamo un singolo quadro
            GameObject mark = Instantiate(marker, start_pos, wall.transform.rotation);
            mark.name = "Painting";
            float trueSize = mark.GetComponentInChildren<Display_Image>().InstantiateImage(0, size.x);
            if(size.x != trueSize)
            {
                if (wall.name == "sidewall_DX")
                {
                    start_pos -= wall.transform.forward * (size.x / 2f);
                    start_pos += wall.transform.forward * (trueSize / 2f);
                }
                else
                {
                    start_pos += wall.transform.forward * (size.x / 2f);
                    start_pos -= wall.transform.forward * (trueSize / 2f);
                }

                mark.transform.position = start_pos;
                used_space += trueSize - size.x;
            }

            if(wall.name == "sidewall_DX")
                mark.transform.Rotate(0.0f, 270f, 0.0f, Space.Self);
            else if(wall.name == "sidewall_SX")
                mark.transform.Rotate(0.0f, 90f, 0.0f, Space.Self);
            else if(wall.name == "frontWall")
            {
                mark.transform.Find("Plane").Rotate(0f, -180f, 0f, Space.World);
                mark.transform.Find("Plane").Rotate(0f, 270f, 0f, Space.World);
                Vector3 Scale = mark.transform.Find("Plane").localScale;
                Scale.x *= trueSize;
                mark.transform.Find("Plane").localScale = Scale;
                mark.name = "FrontPainting";
            }
            mark.transform.parent = coupleParent.transform;
            //istanzio la panca associata
            instantiateBench(bench, start_pos, direction, coupleParent.transform, newRoom);
            instantiateLights(emitter, start_pos, direction, paint_max_h + 0.25f, coupleParent.transform, newRoom);
            newRoom.room_paintings.Add(mark);
            newRoom.room_lights_benches_paints.Add(coupleParent);
            paint_instances++; //calcoliamo il numero di quadri istanziati
            if(wall.name == "sidewall_DX")
            {
                start_pos += wall.transform.forward * trueSize / 2f; //avanziamo la posizione
            }
            else
            {
                start_pos -= wall.transform.forward * trueSize / 2f; //avanziamo la posizione
            }
            used_space += 1f * space_between_paint;
            //calcoliamo la dimensione massima di un quadro come min(spazio ancora disponibile e 1/4 del muro stesso)
            float maxsize = Mathf.Min((usable_space - used_space) / 1f, usable_space / 4f);
            size.x = Random.Range(1f, maxsize); //size per il prossimo quadro
            used_space += size.x;

            coupleParent.transform.GetComponentInChildren<FocusOpera>().quadro = mark;


            //}
        }
        used_space += space_between_paint; //per lasciare spazio finale

        //return paint_instances;
    }

    /*Metodo che istanzia una panca (per test: piccolo cubo), di fronte ai quadri, 
     * Parametri:
     * (GameObject):
     * - bench: Modello 3D della panca/sedia da usare per entrare in modalità cinema di un quadro
     * (Vector3):
     * - position: (x,y,z) del quadro di fronte al quale istanziare la panca (a distanza di 1.5 dal quadro) 
     *                          [NB: i quadri avranno un "cordone" invalicabile posto a 1m dai quadri. Quindi la panca
     *                           deve lasciare spazio di 50cm dal cordone {quindi 0.5 + larghezzaPanca/2}]
     * - direction: versore che indica il vettore frontale della panca (che deve essere messa in direzione del quadro)
     * (Transform):
     * - paintTransform: transform del quadro corrente, usato per rendere panca child del quadro
     */

    private void instantiateBench(GameObject bench, Vector3 position, Vector3 direction, Transform parent, Room newRoom)
    {
        /*3 casi distinti:
         * 1) direction=(1,0,0) -> asse +x => muro DX
         * 2) direction=(-1,0,0) -> asse-x => muro SX
         * 3) direction=(0,0,1) -> asse +z => frontWall (solo se esplicitamente richiesto)
         */
        Vector3 benchPos = position;
        GameObject paintingBench = null;
        //bench: asse_y=up, asse_x = forward, asse_z= larghezza
        float benchWidth = bench.transform.localScale.z;
        switch (direction.x)
        {
            case 1:
                //muro DX => la panchina si troverà a posx+1.5+width/2 (interno stanza)
                benchPos = new Vector3(position.x + distance_wall_border_x+ 0.25f + benchWidth/2f, 0.001f, position.z);
                break;
            case -1:
                //muro SX => panca a posx-1.5-width/2 (interno stanza)
                benchPos = new Vector3(position.x - distance_wall_border_x -0.25f - benchWidth / 2f, 0.001f, position.z);
                break;
            case 0:
                if(direction.z == 1 && direction.y == 0) //frontwall => panca a posz+1.5+width/2 (interno)
                {
                    benchPos = new Vector3(position.x, 0.001f, position.z + distance_wall_border_z +0.25f + benchWidth / 2f);
                }
                break;
        }
        paintingBench = Instantiate(bench, benchPos, Quaternion.identity);
        paintingBench.name = "Bench";
        //ruotiamo in direzione del quadro
        paintingBench.transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
        paintingBench.transform.localScale = new Vector3(0.85f, 0.08f, 0.85f);
        paintingBench.transform.parent = parent;
        if (paintingBench != null)
            paintingBench.SetActive(true);

        newRoom.room_benches.Add(paintingBench);
    }

    private void instantiateLights(GameObject light, Vector3 position, Vector3 direction, float paint_height, Transform parent, Room newRoom)
    {
        /*3 casi distinti:
         * 1) direction=(1,0,0) -> asse +x => muro DX
         * 2) direction=(-1,0,0) -> asse-x => muro SX
         * 3) direction=(0,0,1) -> asse +z => frontWall (solo se esplicitamente richiesto)
         */
        Vector3 lightPosition = position;
        Vector3 Euler = new Vector3(0,0,0);
        float paint_max_h = 3f; //altezza massima dipinto
        GameObject emitter = null;
        //bench: asse_y=up, asse_x = forward, asse_z= larghezza
        float lightWidth = light.transform.localScale.z;
        switch (direction.x)
        {
            case 1:
                //muro DX
                lightPosition = new Vector3(position.x, paint_max_h +0.4f, position.z);
                Euler = new Vector3(0, 90, 0);
                break;
            case -1:
                //muro SX => panca a posx-1.5-width/2 (interno stanza)
                lightPosition = new Vector3(position.x, paint_max_h + 0.4f, position.z);
                Euler = new Vector3(0, -90, 0);
                break;
            case 0:
                if (direction.z == 1 && direction.y == 0) //frontwall => panca a posz+1.5+width/2 (interno)
                {
                    lightPosition = new Vector3(position.x, paint_max_h + 0.4f, position.z);
                }
                break;
        }
        emitter = Instantiate(light, lightPosition, Quaternion.identity);
        emitter.gameObject.GetComponent<Lights>().roomH = roomHeight;
        emitter.name = "Light";
        //ruotiamo in direzione del quadro
        emitter.transform.rotation = Quaternion.LookRotation(-direction, Vector3.up);
        emitter.transform.rotation = Quaternion.Euler(Euler);
        emitter.transform.parent = parent;
        if (emitter != null)
            emitter.SetActive(true);
        newRoom.room_lights.Add(emitter);
    }

    [ExecuteInEditMode]
    Material setMaterial(GameObject component, bool changeMaterial = true)
    {
        Material toApply = null;
        float scale_factor = 1f;
            switch (component.name)
            {
            case ("Roof"):
                if (changeMaterial)
                {
                    rmaterial_Num = roof_materials.Length;
                    rmat_indx = Random.Range(0, rmaterial_Num );
                }
                toApply = roof_materials[rmat_indx];
                scale_factor = 10f;

                break;
            case ("Floor"):
                if (changeMaterial)
                {
                    fmaterial_Num = floor_materials.Length;
                    fmat_indx = Random.Range(0, fmaterial_Num);
                }
                toApply = floor_materials[fmat_indx];
                scale_factor = 10f;
                break;
            case ("portalWall"):
                Material transparentWall = new Material(wallWithWindows);
                transparentWall.SetTexture("_MainTex", portalWall_tex);
                transparentWall.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f, 1f));
                toApply = transparentWall;
                return toApply;
            default:
                if (changeMaterial)
                {
                    wmaterial_Num = wall_materials.Length;
                    wmat_indx = Random.Range(0, wmaterial_Num); //indice random per il vettore di materiali
                }
                toApply = wall_materials[wmat_indx];
                scale_factor = 1f;
                break;

        }
        
        component.GetComponent<Renderer>().material = toApply;
        //aggiusto il tiling: dimensioni del tiles = dimensioni di scala dell'oggetto
        Vector2 tile_size = new Vector2(scale_factor*component.transform.lossyScale.z/2f,
                                        scale_factor*component.transform.lossyScale.y/2f);
        if(component.name == "Floor" || component.name == "Roof")
        {
            tile_size = randomSize/2f;
        }
        if(component.name == "portalWall" || component.name == "frontWall")
        {
            tile_size = new Vector2(randomSize.x/2f, roomHeight/2f);
        }

        component.GetComponent<Renderer>().material.mainTextureScale = tile_size;
        return toApply;
    }

    //spegne tutte le luci eccetto quella nella posizione più vicina a position
    public void turnOff_Lights(Vector3 position, Room newRoom, bool isCurrentRoom)
    {
        float minDist = float.MaxValue;
        float dist;
        GameObject nearLight = null;
        foreach (GameObject light in newRoom.room_lights)
        {
            if (light.name == "Light")//per le singole lampade
            {
                light.gameObject.GetComponent<Lights>().turnOff_PointLights();
                light.gameObject.GetComponent<Lights>().turnOff_emissiveMaterial();

                dist = Vector3.Distance(position, light.transform.position);
                if (dist <= minDist)
                {
                    minDist = dist;
                    nearLight = light;
                }
            }
            else if(light.name == "Lampadario")//lampadari
            {
                light.gameObject.GetComponent<Lights>().turnOff_PointLights();
                light.gameObject.GetComponent<Lights>().turnOff_emissiveMaterial();
            }
        }
        if(isCurrentRoom)
            nearLight.gameObject.GetComponent<Lights>().turnOn_PointLights(); //accendo le pointlight della sola luce interessata

    }

    //riaccende tutte le luci (da usare all'uscita della cinema mode)
    public void turnOn_Lights(Room newRoom)
    {
        foreach (GameObject light in newRoom.room_lights)
        {
            if (light.name == "Light")//per le singole lampade
            {
                light.gameObject.GetComponent<Lights>().turnOff_PointLights();
                light.gameObject.GetComponent<Lights>().turnOn_emissiveMaterial();
            }
            else if (light.name == "Lampadario")//lampadari
            {
                light.gameObject.GetComponent<Lights>().turnOn_PointLights();
                light.gameObject.GetComponent<Lights>().turnOn_emissiveMaterial();
            }
        }
    }
    //metodo che instanzia le barriere fisiche intorno alla playarea. Due casi;
    // 1) guardiansystem configurato: la playarea è di dimensione sempre costante. Si istanzia una volta sola per le 3 stanze (new,cur,old)
    // 2) guardiansystem non configurato. si istanzia un border a distanza di 1 metro dai muri per ogni stanza
    private void instantiateBorders(bool guardianConfigured, GameObject thisFloor, Room newRoom)
    {
        if (guardianConfigured)
        {
            Vector3 roomCentre = thisFloor.transform.position;
            float roomSize_x = thisFloor.transform.localScale.x;
            float roomSize_z = thisFloor.transform.localScale.z;


            //singleton: lo deve fare una sola volta! I border per il guardian sono STATICI!
            if (!guardian_border_singleton)
            {
                
                //istanzia i borderMarker nelle posizioni fisse delle 3 stanze: origine_x, origine_x+dist_bet_rooms, origine_x-dist_bet_rooms
                
                xmin = float.MaxValue;
                zmin = float.MaxValue;
                xmax = float.MinValue;
                zmax = float.MinValue;

                
                for (int i = 0; i < playArea_positions.Length; i++)
                {
                    Vector3 posMarker = playArea_positions[i];
                    //il player parte in pos origine_x
                    posMarker.y = roomCentre.y /*+ borderMarker.transform.localScale.y / 2f*/;
                    GameObject marker_1 = Instantiate(borderMarker, posMarker, Quaternion.identity);
                    instantiateAudioEmitter(audioEmitter, marker_1, thisFloor);
                    //non saranno figli di nessuno! Non devono essere distrutti durante l'esecuzione
                    //mi salvo x e z (min e max)
                    if (posMarker.z > zmax)
                        zmax = posMarker.z;
                    if (posMarker.z < zmin)
                        zmin = posMarker.z;
                    if (posMarker.x > xmax)
                        xmax = posMarker.x;
                    if (posMarker.x < xmin)
                        xmin = posMarker.x;

                    //stanza 2: origine_x + distance_b_room

                    posMarker = playArea_positions[i];
                    posMarker.y = roomCentre.y /*+ borderMarker.transform.localScale.y / 2f*/;
                    posMarker.x += distance_between_rooms;
                    marker_1 = Instantiate(borderMarker, posMarker, Quaternion.identity);
                    instantiateAudioEmitter(audioEmitter, marker_1, thisFloor);

                    //stanza 3: origine_x + 2*distance_b_room

                    posMarker = playArea_positions[i];
                    posMarker.y = roomCentre.y /*+ borderMarker.transform.localScale.y / 2f*/;
                    posMarker.x += 2f*distance_between_rooms;
                    marker_1 = Instantiate(borderMarker, posMarker, Quaternion.identity);
                    instantiateAudioEmitter(audioEmitter, marker_1, thisFloor);
                }
                //istanzio i delimitatori orizzontali (sono solo 3 per stanza: il bordo con zmax è l'ingresso dove c'è il portale=>E' aperto)
                //ciclo 3 volte: una per ogni stanza
                for (int j = 0; j < 3; j++)
                {
                    float offset = j * distance_between_rooms;
                    Vector3 poss = new Vector3(xmax +offset,
                       roomCentre.y + 0.75f,
                       (zmax + zmin) / 2f);
                    GameObject delimiter_1 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 0, 0));
                    delimiter_1.transform.localScale = new Vector3(0.01f, Mathf.Abs(zmax - zmin), 0.05f);

                    poss = new Vector3(xmin + offset,
                       roomCentre.y + 0.75f,
                       (zmax + zmin) / 2f);
                    GameObject delimiter_2 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 0, 0));
                    delimiter_2.transform.localScale = new Vector3(0.01f, Mathf.Abs(zmax - zmin), 0.05f);

                    poss = new Vector3((xmax + xmin) / 2f   + offset,
                                       roomCentre.y + 0.75f,
                                       zmin);
                    GameObject delimiter_3 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 90, 0));
                    delimiter_3.transform.localScale = new Vector3(0.01f, Mathf.Abs(xmax - xmin), 0.05f);
                }
                guardian_border_singleton = true; //non rientra mai più qui dentro!
            }
            distance_wall_border_x = Mathf.Abs(xmax - roomSize_x / 20f);
            distance_wall_border_z = Mathf.Abs(zmax - roomSize_z / 20f);


        }
        else
        {
            Vector3 roomCentre = thisFloor.transform.position;
            distance_wall_border_x = 1f;
            distance_wall_border_z = 1f;
            float posx, posz;
            posx = 10f * thisFloor.transform.localScale.x / 2f;
            posz = 10f * thisFloor.transform.localScale.z / 2f;

            Vector3 poss = new Vector3(roomCentre.x + posx - distance_wall_border_x,
                                       roomCentre.y /*+ borderMarker.transform.localScale.y / 2f*/,
                                       roomCentre.z + posz -0.2f);
            GameObject marker_1 = Instantiate(borderMarker, poss, Quaternion.identity);
            instantiateAudioEmitter(audioEmitter, marker_1, thisFloor, newRoom);
            marker_1.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x - posx + distance_wall_border_x,
                               roomCentre.y/* + borderMarker.transform.localScale.y / 2f*/,
                               roomCentre.z + posz - 0.2f);
            GameObject marker_2 = Instantiate(borderMarker, poss, Quaternion.identity);
            instantiateAudioEmitter(audioEmitter, marker_2, thisFloor, newRoom);
            marker_2.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x + posx - distance_wall_border_x,
                               roomCentre.y /*+ borderMarker.transform.localScale.y / 2f*/,
                               roomCentre.z - posz + distance_wall_border_z);
            GameObject marker_3 = Instantiate(borderMarker, poss, Quaternion.identity);
            instantiateAudioEmitter(audioEmitter, marker_3, thisFloor, newRoom);
            marker_3.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x - posx + distance_wall_border_x,
                               roomCentre.y /*+ borderMarker.transform.localScale.y / 2f*/,
                               roomCentre.z - posz + distance_wall_border_z);
            GameObject marker_4 = Instantiate(borderMarker, poss, Quaternion.identity);
            instantiateAudioEmitter(audioEmitter, marker_4, thisFloor, newRoom);
            marker_4.transform.parent = thisFloor.transform;

            //Nastri: saranno solo 3 (l'ingresso è libero)
            poss = new Vector3(roomCentre.x + posx - distance_wall_border_x,
                               roomCentre.y + 0.75f,
                               roomCentre.z + 0.5f);
            GameObject delimiter_1 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 0, 0));
            delimiter_1.transform.localScale = new Vector3(0.01f, posz -0.5f , 0.05f);
            delimiter_1.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x - posx + distance_wall_border_x,
                               roomCentre.y + 0.75f,
                               roomCentre.z + 0.5f);
            GameObject delimiter_2 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 0, 0));
            delimiter_2.transform.localScale = new Vector3(0.01f, posz - 0.5f, 0.05f);
            delimiter_2.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x,
                               roomCentre.y + 0.75f,
                               roomCentre.z - posz + distance_wall_border_z);
            GameObject delimiter_3 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 90, 0));
            delimiter_3.transform.localScale = new Vector3(0.01f, posx -1f, 0.05f);
            delimiter_3.transform.parent = thisFloor.transform;

            //istanziamo anche il nastro che bloccherà la porta in caso di transito all'indietro

            float x_coord = roomCentre.x + posx - distance_wall_border_x;
            Vector3 poss_mark_door = new Vector3(x_coord,
                                       roomCentre.y /*+ borderMarker.transform.localScale.y / 2f*/,
                                       roomCentre.z + posz -  2f + 0.2f);
            GameObject marker_door = Instantiate(borderMarker, poss_mark_door, Quaternion.identity);
            marker_door.transform.parent = thisFloor.transform;
            marker_door.name = "marker_door";
            marker_door.SetActive(false);
            newRoom.doorParts.Add(marker_door);
            x_coord = roomCentre.x + (posx - distance_wall_border_z) / 2f;
            Vector3 poss_delimiter_door = new Vector3(x_coord,
                               roomCentre.y + 0.75f,
                               roomCentre.z + posz - 2f + 0.2f);
            GameObject delimiterDoor = Instantiate(borderDelimiter, poss_delimiter_door, Quaternion.Euler(90, 90, 0));
            float scale = Mathf.Abs(roomCentre.x - poss_mark_door.x)/2f ;
            delimiterDoor.transform.localScale = new Vector3(0.01f, scale, 0.05f);
            delimiterDoor.transform.parent = thisFloor.transform;
            delimiterDoor.name = "DelimiterDoor";
            delimiterDoor.SetActive(false);
            newRoom.doorParts.Add(delimiterDoor);
        }
    }

    Color getRandomColorHSV()
    {
        return Random.ColorHSV();        
    }
    void setColor(GameObject component, Color color)
    {
        component.GetComponent<Renderer>().material.color = color;
    }
    //istanzia un lampadario al centro del soffitto (di cui sarà figlio)
    void instantiateChandelier(GameObject Chandelier, GameObject roof, Room newRoom)
    {
        float roofWidth, roofLenght;
        float distance_from_roof = 0.2f;
        roofWidth = roof.transform.localScale.x;
        roofLenght = roof.transform.localScale.z;
        if(roofWidth >= 13f / 10f && roofLenght <= 13f / 10f)
        {
            Vector3 position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x -= (10f * roofWidth / 4f);
            GameObject chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            chand.gameObject.GetComponent<Lights>().setLights();
            newRoom.room_lights.Add(chand);

            //secondo lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x += (10f * roofWidth / 4f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            chand.gameObject.GetComponent<Lights>().setLights();
            newRoom.room_lights.Add(chand);

        }
        else if (roofLenght >= 13f / 10f && roofWidth <= 13f / 10f)
        {
            Vector3 position = roof.transform.position;
            position.y -= distance_from_roof;
            position.z -= (10f * roofLenght / 4f);
            GameObject chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            chand.gameObject.GetComponent<Lights>().setLights();
            newRoom.room_lights.Add(chand);

            //secondo lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.z += (10f * roofLenght / 4f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            chand.gameObject.GetComponent<Lights>().setLights();
            newRoom.room_lights.Add(chand);

        }
        else if (roofWidth >= 13f / 10f && roofLenght >= 13f / 10f) //stanza molto grande: 4 lampadari
        {
            Vector3 position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x -= (10f * roofWidth / 4f);
            position.z -= (10f * roofLenght / 5f);
            GameObject chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            chand.gameObject.GetComponent<Lights>().setLights();
            newRoom.room_lights.Add(chand);

            //secondo lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            //position.x += (10f * roofWidth / 4f);
            position.z += (10f * roofLenght / 5f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            chand.gameObject.GetComponent<Lights>().setLights();
            newRoom.room_lights.Add(chand);
           
            //Terzo lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x += (10f * roofWidth / 4f);
            position.z -= (10f * roofLenght / 5f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.gameObject.GetComponent<Lights>().setLights();
            chand.transform.parent = roof.transform;
            newRoom.room_lights.Add(chand);
/* 
            //Quarto lampadario => CAUSA PROBLEMI CON LE LUCI REALTIME
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x -= (10f * roofWidth / 4f);
            position.z += (10f * roofLenght / 4f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            chand.gameObject.GetComponent<Lights>().setLights();
            newRoom.room_lights.Add(chand);
            */
        }
        else //stanza piccola: un solo lampadario
        {
            Vector3 position = roof.transform.position;
            position.y -= distance_from_roof;
            GameObject chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            chand.gameObject.GetComponent<Lights>().setLights();
            newRoom.room_lights.Add(chand);
        }

    }

    //istanzia gli emettitori audio sui border_marker
    void instantiateAudioEmitter(GameObject audioEmitter, GameObject parent, GameObject f, Room newRoom = null)
    {
        Vector3 position = parent.transform.position;
        position.y += /*(parent.transform.localScale.y)*/1f + audioEmitter.transform.localScale.y/2f -0.01f;
        GameObject audioEmit = Instantiate(audioEmitter, position, Quaternion.identity);
        if (newRoom != null)
            newRoom.emitters.Add(audioEmit);
        audioEmit.name = "AudioEmitter";
        audioEmit.GetComponent<ReduceVolume>().roomSize = randomSize;
        audioEmit.GetComponent<ReduceVolume>().minRoomSize = minsize;
        audioEmit.GetComponent<ReduceVolume>().maxRoomSize = maxsize;
        audioEmit.transform.parent = parent.transform;
        audioEmit.GetComponent<RotateSpeaker>().roomCentre = f.transform.position;
    }

    void instantiateColumns(GameObject floor, GameObject roof, float roomH)
    {
        if (roomH > 5f)
        {
            Vector3 roomCentre = floor.transform.position;
            float roof_y_coord = roof.transform.position.y;
            float h_base = 0.15f; //0.15
            float h_capitello = 0.95f; //0.95
            float offset_from_wall = 0.3f; //0.3
            float roomLenght = 10f * floor.transform.localScale.z;
            float roomWidth = 10f * floor.transform.localScale.x;
            float corpoScale = roomH - h_base - h_capitello;
            //istanziamo ai quattro angoli
            //1
            Vector3 pos_base = new Vector3(roomCentre.x + roomWidth / 2f - offset_from_wall, 
                                           roomCentre.y + h_base,
                                           roomCentre.z + roomLenght / 2f - offset_from_wall);
            Vector3 pos_capitello = new Vector3(roomCentre.x + roomWidth / 2f - offset_from_wall,
                                                roof_y_coord - h_capitello,
                                                roomCentre.z + roomLenght / 2f - offset_from_wall);
            float yCorpo = (h_base/2f + (roomH - h_capitello) / 2f);
            Vector3 pos_corpo = new Vector3(roomCentre.x + roomWidth / 2f - offset_from_wall,
                                            yCorpo,
                                            roomCentre.z + roomLenght / 2f - offset_from_wall);

            createColumn(pos_base, pos_capitello, pos_corpo, floor, roomH, h_base, h_capitello);
            //2
            pos_base = new Vector3(roomCentre.x - roomWidth / 2f + offset_from_wall,
                               roomCentre.y + h_base,
                               roomCentre.z + roomLenght / 2f - offset_from_wall);
            pos_capitello = new Vector3(roomCentre.x - roomWidth / 2f + offset_from_wall,
                                                roof_y_coord - h_capitello,
                                                roomCentre.z + roomLenght / 2f - offset_from_wall);
            yCorpo = (h_base / 2f + (roomH - h_capitello) / 2f);
            pos_corpo = new Vector3(roomCentre.x - roomWidth / 2f + offset_from_wall,
                                            yCorpo,
                                            roomCentre.z + roomLenght / 2f - offset_from_wall);

            createColumn(pos_base, pos_capitello, pos_corpo, floor, roomH, h_base, h_capitello);
            //3
            pos_base = new Vector3(roomCentre.x + roomWidth / 2f - offset_from_wall,
                               roomCentre.y + h_base,
                               roomCentre.z - roomLenght / 2f + offset_from_wall);
            pos_capitello = new Vector3(roomCentre.x + roomWidth / 2f - offset_from_wall,
                                                roof_y_coord - h_capitello,
                                                roomCentre.z - roomLenght / 2f + offset_from_wall);
            yCorpo = (h_base / 2f + (roomH - h_capitello) / 2f);
            pos_corpo = new Vector3(roomCentre.x + roomWidth / 2f - offset_from_wall,
                                            yCorpo,
                                            roomCentre.z - roomLenght / 2f + offset_from_wall);

            createColumn(pos_base, pos_capitello, pos_corpo, floor, roomH, h_base, h_capitello);
            //4
            pos_base = new Vector3(roomCentre.x - roomWidth / 2f + offset_from_wall,
                               roomCentre.y + h_base,
                               roomCentre.z - roomLenght / 2f + offset_from_wall);
            pos_capitello = new Vector3(roomCentre.x - roomWidth / 2f + offset_from_wall,
                                                roof_y_coord - h_capitello,
                                               roomCentre.z - roomLenght / 2f + offset_from_wall);
            yCorpo = (h_base / 2f + (roomH - h_capitello) / 2f);
            pos_corpo = new Vector3(roomCentre.x - roomWidth / 2f + offset_from_wall,
                                            yCorpo,
                                            roomCentre.z - roomLenght / 2f + offset_from_wall);

            createColumn(pos_base, pos_capitello, pos_corpo, floor, roomH, h_base, h_capitello);

        }
    }

    void instantiateStatue(GameObject floor, Vector2 areaSize ,float roomH)
    {
        int indx = Random.Range(0, statues.Length);
        int instantiate_statue_randomic = (int)Random.Range(0, 10f); //per aggiungere randomicità 

        bool statueDecision = (instantiate_statue_randomic % 2 == 0);
        if(statueDecision)
        {
            if (areaSize.x > 12 && areaSize.y * 10f > 12 && roomH > 5.5f)
            {
                GameObject statue = Instantiate(statues[indx], floor.transform.position, Quaternion.identity);
                statue.transform.parent = floor.transform;
                statue.name = "Statua";
            }
        }

    }
    void createColumn(Vector3 pos_base, Vector3 pos_capitello, Vector3 pos_corpo, GameObject floor, float roomH, float h_base, float h_capitello)
    {
        GameObject b = Instantiate(baseColonna, pos_base, Quaternion.identity);
        b.transform.parent = floor.transform;
        b.name = "BaseColonna";
        GameObject c = Instantiate(capitello, pos_capitello, Quaternion.identity);
        c.transform.parent = floor.transform;
        c.name = "Capitello";
        GameObject corpo = Instantiate(corpoColonna, pos_corpo, Quaternion.identity);
        //corpo.transform.localScale *= (roomH - h_base - h_capitello);
        Vector3 scale = corpo.transform.localScale;
        scale.y *= (roomH - h_base - h_capitello);
        corpo.transform.localScale = scale;
        corpo.transform.parent = floor.transform;
        corpo.name = "CorpoColonna";
    }
    //se la stanza è abbastanza alta, istanzia un ornamento per muro (sx e dx)
    void instantiateOrnaments(GameObject wall, GameObject ornament)
    {
        GameObject balcony;
        float balcony_y_pos = 4.5f;
        Vector3 balcony_position = wall.transform.position;
        balcony_position.y = balcony_y_pos;
        //spazio libero: almeno 2.5m su asse y. Fascia 1: quadri, da altezza 0 a max 3.25f. Fascia 2: da 3.5f a 5.5f.
        if (roomHeight >= 6f)
        {
            switch (wall.name)
            {
                case ("sidewall_SX"):
                    balcony = Instantiate(ornament, balcony_position, Quaternion.identity);
                    balcony.name = "Balcone";
                    balcony.transform.parent = wall.transform;
                    break;
                case ("sidewall_DX"):
                    Vector3 Euler = new Vector3(0, 180, 0);
                    balcony = Instantiate(ornament, balcony_position, Quaternion.Euler(Euler));
                    balcony.name = "Balcone";
                    balcony.transform.parent = wall.transform;
                    break;
            }
        }
    }

    //per far si che il portale sia incluso correttamente nella stanza
    // i portali saranno in posizione : (-60,0,0), (0,0,0), (60,0,0). Varia solo x, il resto è fisso
    // la posizione della stanza sarà passata esternamente. Da questa possiamo trarre la posizione dei portali:
    /* stanza1 (-60, y,z) => portale 1 ecc
     * Agiamo su posStanza.z per portarla nella posizione corretta
     */

    void instantiateTorches(GameObject wall)
    {
        GameObject torchLamp;
        
        float distance_from_wall = 0.5f;
        Vector3 wall_centre = wall.transform.position;
        Vector3 pos = wall_centre;
        pos.y = 0;
        pos.z += distance_from_wall;
        if(wall.transform.localScale.z > 10f  && wall.transform.localScale.z <15f && wall.name == "frontWall")
        {
            Debug.Log("Torch");
            pos.x = wall_centre.x + wall.transform.localScale.z / 4f;
            torchLamp = Instantiate(torch, pos, Quaternion.identity);
            torchLamp.transform.parent = wall.transform;
            torchLamp.name = "TorchLamp";
            pos.x = wall_centre.x - wall.transform.localScale.z / 4f;
            torchLamp = Instantiate(torch, pos, Quaternion.identity);
            torchLamp.transform.parent = wall.transform;
            torchLamp.name = "TorchLamp";
        }
        else if (wall.transform.localScale.z >= 15f && wall.name == "frontWall")
        {
            pos.x = wall_centre.x + wall.transform.localScale.z / 7f;
            torchLamp = Instantiate(torch, pos, Quaternion.identity);
            torchLamp.transform.parent = wall.transform;
            torchLamp.name = "TorchLamp";
            pos.x = wall_centre.x + wall.transform.localScale.z / 3f;
            torchLamp = Instantiate(torch, pos, Quaternion.identity);
            torchLamp.transform.parent = wall.transform;
            torchLamp.name = "TorchLamp";
            pos.x = wall_centre.x - wall.transform.localScale.z / 7f;
            torchLamp = Instantiate(torch, pos, Quaternion.identity);
            torchLamp.transform.parent = wall.transform;
            torchLamp.name = "TorchLamp";
            pos.x = wall_centre.x - wall.transform.localScale.z / 3f;
            torchLamp = Instantiate(torch, pos, Quaternion.identity);
            torchLamp.transform.parent = wall.transform;
            torchLamp.name = "TorchLamp";
        }
    }
    Vector3 fix_roomPosition(Vector3 oldPosition, float roomLenght = 0, float portalLenght = 0)
    {
        Vector3 currentPosition = oldPosition;
        float offset = 0f - currentPosition.z + portalLenght/2f  - roomLenght/2f;
        Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y , currentPosition.z+ offset);
        return newPosition;
    }

    void InstantiateWindows (GameObject wall, float floor_length, Transform parent)
    {
        Vector3 wallPosition = wall.transform.position;
        Vector3 portalWall_size = new Vector3(floor_length / 6, roomHeight / 3, 1.0f);
        
        Vector3 windowPos = new Vector3(wallPosition.x + ((floor_length - 3) / 4 + 0.2f), 1.9f, 0.62f); 
        GameObject win = Instantiate(window, windowPos, Quaternion.identity);
        win.transform.parent = parent;
        win.transform.localScale = portalWall_size;
        StartCoroutine(InstantiateCorniceFinestra(portalWall_size.x, portalWall_size.y, win));

        windowPos = new Vector3(wallPosition.x - ((floor_length - 3) / 4 + 0.2f), 1.9f, 0.62f); 
        win = Instantiate(window, windowPos, Quaternion.identity);
        win.transform.parent = parent;
        win.transform.localScale = portalWall_size;
        StartCoroutine(InstantiateCorniceFinestra(portalWall_size.x, portalWall_size.y, win));
    }

    private IEnumerator InstantiateCorniceFinestra(float width, float height, GameObject finestra)
    {
        GameObject new_Target;
        Vector3 posizioneQuadro = finestra.transform.position;
        //lati sinistro e destro
        new_Target = Instantiate(latoFinestra, new Vector3(posizioneQuadro.x - (width/* + 0.1f*/) / 2f, posizioneQuadro.y - (height/* + 0.1f*/) / 2f, posizioneQuadro.z/* + 0.1f*/), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, height, 1);
        new_Target.transform.parent = finestra.transform;

        new_Target = Instantiate(latoFinestra, new Vector3(posizioneQuadro.x + (width/* + 0.1f*/) / 2f, posizioneQuadro.y + (height/* + 0.1f*/) / 2f, posizioneQuadro.z/* + 0.1f*/), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, height, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
        new_Target.transform.parent = finestra.transform;

        //lati superiore e inferiore 
        new_Target = Instantiate(latoFinestra, new Vector3(posizioneQuadro.x + (width/* + 0.1f*/) / 2f, posizioneQuadro.y - (height/* + 0.1f*/) / 2f, posizioneQuadro.z/* + 0.1f*/), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, width, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, 90.0f, Space.Self);
        new_Target.transform.parent = finestra.transform;

        new_Target = Instantiate(latoFinestra, new Vector3(posizioneQuadro.x - (width/* + 0.1f*/) / 2f, posizioneQuadro.y + (height/* + 0.1f*/) / 2f, posizioneQuadro.z/* + 0.1f*/), Quaternion.identity);
        new_Target.transform.localScale = new Vector3(1, width, 1);
        new_Target.transform.Rotate(0.0f, 0.0f, -90.0f, Space.Self);
        new_Target.transform.parent = finestra.transform;


        //angoli
        new_Target = Instantiate(angoloFinestra, new Vector3(posizioneQuadro.x - (width/* + 0.1f*/) / 2f, posizioneQuadro.y + (height/* + 0.1f*/) / 2f, posizioneQuadro.z/* + 0.1f*/), Quaternion.identity);
        new_Target.transform.parent = finestra.transform;

        new_Target = Instantiate(angoloFinestra, new Vector3(posizioneQuadro.x + (width/* + 0.1f*/) / 2f, posizioneQuadro.y + (height/* + 0.1f*/) / 2f, posizioneQuadro.z/* + 0.1f*/), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 270.0f, Space.Self);
        new_Target.transform.parent = finestra.transform;

        new_Target = Instantiate(angoloFinestra, new Vector3(posizioneQuadro.x + (width/* + 0.1f*/) / 2f, posizioneQuadro.y - (height/* + 0.1f*/) / 2f, posizioneQuadro.z/* + 0.1f*/), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 180.0f, Space.Self);
        new_Target.transform.parent = finestra.transform;

        new_Target = Instantiate(angoloFinestra, new Vector3(posizioneQuadro.x - (width/* + 0.1f*/) / 2f, posizioneQuadro.y - (height/* + 0.1f*/) / 2f, posizioneQuadro.z/* + 0.1f*/), Quaternion.identity);
        new_Target.transform.Rotate(0.0f, 0.0f, 90.0f, Space.Self);
        new_Target.transform.parent = finestra.transform;

        yield return new WaitForSeconds(0);
    }

}
public class Room
{
    private GameObject room_obj;
    private List<GameObject> lights, benches, paintings, lights_benches_paints, audioEmitters;
    private List<GameObject> doorComponents;
    private Vector2 area;
    private bool player_present; //se il player si trova in questa stanza

    public Room()
    {
        this.lights = new List<GameObject>();
        this.benches = new List<GameObject>();
        this.paintings = new List<GameObject>();
        this.lights_benches_paints = new List<GameObject>();
        this.audioEmitters = new List<GameObject>();
        this.doorComponents = new List<GameObject>();
        this.area = new Vector2();
        this.player_present = false;
    }

    public bool playerpresent
    {
        get => this.player_present;
        set => this.player_present = value;
    }
    public GameObject room_GameObj
    {
        get => this.room_obj;
        set => this.room_obj = value;
    }
    public List<GameObject> emitters
    {
        get => this.audioEmitters;
        set => this.audioEmitters = value;
    }
    public List<GameObject> doorParts
    {
        get => this.doorComponents;
        set => this.doorComponents = value;
    }
    public List<GameObject> room_lights
    {
        get => this.lights;
        set => this.lights = value;
    }
    public List<GameObject> room_benches
    {
        get => this.benches;
        set => this.benches = value;
    }
    public List<GameObject> room_paintings
    {
        get => this.paintings;
        set => this.paintings = value;
    }
    public List<GameObject> room_lights_benches_paints
    {
        get => this.lights_benches_paints;
        set => this.lights_benches_paints = value;
    }
    public Vector2 Area
    {
        get => this.area;
        set => this.area = value;
    }
    //porta e cambio stanza
    public void changeName(string name)
    {
        this.room_GameObj.name = name;
        if(name == "OldRoom")
        {
            foreach(GameObject component in this.doorComponents)
            {
                component.SetActive(true); 
            }
        }
    }

    public void turnOffRoomLight()
    {
        foreach(GameObject light in this.lights)
        {
            light.GetComponent<Lights>().turnOff_PointLights();
        }
    }
    public void turnOnRoomChandeliers()
    {
        foreach (GameObject light in this.lights)
        {
            if(light.name == "Lampadario")
                light.GetComponent<Lights>().turnOff_PointLights();
        }
    }
    //audio
    public void resumeTrack(float t)
    {
        foreach(GameObject emitter in this.audioEmitters)
        {
            emitter.transform.Find("Audio Source").gameObject.GetComponent<AudioSource>().time =t;
        }
    }

    public void mute()
    {
        foreach (GameObject emitter in this.audioEmitters)
        {
            emitter.gameObject.GetComponent<ReduceVolume>().soundMuted = true;
        }
    }
    public void unmute()
    {
        foreach (GameObject emitter in this.audioEmitters)
        {
            emitter.gameObject.GetComponent<ReduceVolume>().soundMuted = false;
        }
    }
}