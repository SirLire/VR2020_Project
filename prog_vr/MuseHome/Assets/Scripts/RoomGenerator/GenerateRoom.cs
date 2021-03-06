﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenerateRoom : MonoBehaviour
{
    public GameObject sidewall, portalwall, frontwall, roof, floor, roomParent,bench;
    public GameObject emitter;
    public GameObject Chandelier;
    public GameObject audioEmitter;
    public GameObject ornament;
    public GameObject paint_marker; //paint di test: cubo nero
    private GameObject room;
    public GameObject borderMarker, borderDelimiter; //marker degli angoli e nastro tra i singoli marker intorno all'area di gioco
    public GameObject empty_4_paintBench; //empty padre di una coppia quadro/panca
    public Material[] wall_materials, floor_materials, roof_materials;//vettori di materiali per pavimento, soffitto e muri
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

    //NPC
    private List<GameObject> NPCs = new List<GameObject>();
    public int minNPCs = 1, maxNPCs = 6; //probabilmente dovrebbe dipendere dalla dimensione della stanza
    public GameObject NPC;
    private Vector3 newPosition;


    // Start is called before the first frame update
    void Start()
    {
        Vector2 min = new Vector2(6, 4);
        Vector2 max = new Vector2(20, 20);
        room = createRoom(position1, 12f, min, max);
        Vector3 pos2 = new Vector3(-60, 0, 0);
        room = createRoom(pos2, 2f, min, max);
    }
    // Update is called once per frame
    void Update()
    {
    }
    
    //METODO GENERICO PER CREARE STANZE!
    public GameObject createRoom(Vector3 pos, float roomH, Vector2 Minsize, Vector2 Maxsize ,float distance_bet_rooms = 60, Vector3[] guardianCorners = null, bool guardianConfigured = false)
    {
        this.roomHeight = roomH;
        this.minsize = Minsize;
        this.maxsize = Maxsize;
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
                            roomParent, paint_marker, bench, positions);
        generateNPCs(pos, minNPCs, maxNPCs, r);

        return r;
    }
    void generateNPCs(Vector3 position, int minNPCs, int maxNPCs, GameObject room)
    {
        this.GetComponent<NavMeshSurface>().BuildNavMesh();
        int N_NPC = Random.Range(minNPCs, maxNPCs);
        for (int i = 0; i < N_NPC; i++)
        {
            GameObject new_NPC = Instantiate(NPC, position, Quaternion.identity);
            NPC_NavController controller = new_NPC.GetComponent<NPC_NavController>();
            List<Vector3> TargetsLocations = new List<Vector3>();
            List<GameObject> benches = getBench();
            for (int j = 0; j < benches.Count; j++)
            {
                TargetsLocations.Add(benches[j].transform.position);
            }
            List<Vector3> TargetsPaintings = new List<Vector3>();
            List<GameObject> paintings = getPainting();
            for (int j = 0; j < paintings.Count; j++)
            {
                TargetsPaintings.Add(paintings[j].transform.position);
            }
            controller.Targets = TargetsLocations;
            controller.Paintings = TargetsPaintings;
            NPCs.Add(new_NPC);
            new_NPC.transform.parent = room.gameObject.transform;
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
                            GameObject painting, GameObject bench,Vector3[] guardianCorners, float distance_bet_rooms = 60, bool guardianConfigured = false)
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
        instantiateBorders(guardianConfigured, newFloor);
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

        Color col = getRandomColorHSV();
        setColor(sidewall_dx, col);
        setColor(sidewall_sx, col);

        /*3 casi distinti:
         * 1) direction=(1,0,0) -> asse +x => muro DX
         * 2) direction=(-1,0,0) -> asse-x => muro SX
         * 3) direction=(0,0,1) -> asse +z => frontWall (solo se esplicitamente richiesto)
         */
        Vector3 dx_direction = new Vector3(1, 0, 0);
        Vector3 sx_direction = new Vector3(-1, 0, 0);
        instantiatePaintings(sidewall_dx, newFloor, painting, dx_direction, bench, empty,
            floorLength, guardianConfigured, 0.5f, 6);
        instantiateOrnaments(sidewall_dx, ornament);
        if (enoughSpace) {// istanzia dipinti e panche solo se c'è abbastanza spazio per entrambi i lati
            instantiatePaintings(sidewall_sx, newFloor, painting, sx_direction, bench, empty,
                                 floorLength, guardianConfigured, 0.5f, 6);

            instantiateOrnaments(sidewall_sx, ornament);
        }
        //muri figli dell'empty
        sidewall_dx.transform.parent = empty.transform;
        sidewall_sx.transform.parent = empty.transform;

        //3)Portal wall: 
        Vector3 portalWallPos = new Vector3(position.x, position.y + roomHeight / 2f - 0.01f, position.z + floorLength / 2f); // si trova in direzione +z
        GameObject pwall = Instantiate(portalWall, portalWallPos, Quaternion.identity);
        pwall.name = "portalWall";
        //materiale muro
        setMaterial(pwall, false);

        setColor(pwall, col);

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
        instantiateChandelier(Chandelier, newRoof);
        //5) Frontwall: diverso perchè potrebbe avere materiali diversi, essere una finestra. in base a dimensione stanza, può avere o meno quadri (se troppo lontano da playarea. NO!)
        Vector3 frontwallPos = new Vector3(position.x, position.y + roomHeight / 2f - 0.01f, position.z - floorLength / 2f); // si trova in direzione -z
        GameObject fwall = Instantiate(frontWall, frontwallPos, Quaternion.identity);
        fwall.name = "frontWall";
        setMaterial(fwall, true);

        col = getRandomColorHSV();
        setColor(fwall, col);

        //spessore muri su x
        Vector3 frontwall_size = new Vector3(0.25f, roomHeight, floorWidth);
        fwall.transform.localScale = frontwall_size;
        fwall.transform.rotation = Quaternion.AngleAxis(90, Vector3.up); //per mantenere sull'asse z locale la larghezza del muro
        Vector3 fw_direction = new Vector3(0, 0, 1);

        //Per evitare panche che si compenetrano, idea semplice: pochi quadri centrati nel muro
        float distance = newFloor.transform.localScale.x*10 / 2f;
        //se ho abbastanza spazio per una bench centrale (3=totale distanza da muri, + 2 bench)
        
        if(2*distance >= 3f + 2f*bench.transform.localScale.z && randomSize.y>4.5f
                    && enoughSpace && !guardianConfigured)
            instantiatePaintings(fwall, newFloor, painting, fw_direction, bench, empty,
                floorLength, guardianConfigured, 0.75f *distance, 1);
        fwall.transform.parent = empty.transform;


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
    int instantiatePaintings(GameObject wall, GameObject floor, GameObject marker, Vector3 direction, 
                             GameObject bench, GameObject room, float playarea_len, bool guardianConfigured,
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
        Vector3 start_pos = posx + wall.transform.forward * (usable_space / 2f - space_between_paint - size.x / 2f - portal_length);
        //per evitare che compenetri nel muro
        float offset_from_wall = 0.01f;
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
                break;
        }

        //spazio del muro usato: per controllare di non eccedere
        used_space = space_between_paint + size.x+ portal_length;


        //allocazione dei quadri:
        for (int i = 0; i < paintNum && used_space <= usable_space - space_between_paint; i++)
        {
            if (used_space <= usable_space - space_between_paint)
            {
                if (i != 0)
                {
                    start_pos -= wall.transform.forward * (size.x / 2f + 1f * space_between_paint); //avanza la posizione

                }
                //istanziamo l'empty padre della coppia
                GameObject coupleParent = Instantiate(empty_4_paintBench, start_pos, Quaternion.identity);
                coupleParent.name = "Painting&Bench";
                coupleParent.transform.parent = room.transform;
                //istanziamo un singolo quadro
                GameObject mark = Instantiate(marker, start_pos, wall.transform.rotation);
                mark.name = "Painting";
                float trueSize = mark.GetComponentInChildren<Display_Image>().InstantiateImage("", size.x);
                if(size.x != trueSize)
                {
                    start_pos += wall.transform.forward * (size.x / 2f);
                    start_pos -= wall.transform.forward * (trueSize / 2f);

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
                }
                mark.transform.parent = coupleParent.transform;
                //istanzio la panca associata
                instantiateBench(bench, start_pos, direction, coupleParent.transform);
                instantiateLights(emitter, start_pos, direction, paint_max_h +0.25f, coupleParent.transform);
                paintings.Add(mark);
                paintBenchCouples.Add(coupleParent);
                paint_instances++; //calcoliamo il numero di quadri istanziati
                start_pos -= wall.transform.forward * trueSize / 2f; //avanziamo la posizione
                used_space += 1f * space_between_paint;
                //calcoliamo la dimensione massima di un quadro come min(spazio ancora disponibile e 1/4 del muro stesso)
                float maxsize = Mathf.Min((usable_space - used_space) / 1f, usable_space / 4f);
                size.x = Random.Range(1f, maxsize); //size per il prossimo quadro
                used_space += size.x;
            }
        }
        used_space += space_between_paint; //per lasciare spazio finale


        return paint_instances;
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

    GameObject instantiateBench(GameObject bench, Vector3 position, Vector3 direction, Transform parent)
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

        benches.Add(paintingBench);//aggiungo alla lista
        return paintingBench;
    }

    GameObject instantiateLights(GameObject light, Vector3 position, Vector3 direction, float  paint_height, Transform parent)
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
                lightPosition = new Vector3(position.x, paint_max_h +0.25f, position.z);
                Euler = new Vector3(0, 90, 0);
                break;
            case -1:
                //muro SX => panca a posx-1.5-width/2 (interno stanza)
                lightPosition = new Vector3(position.x, paint_max_h + 0.25f, position.z);
                Euler = new Vector3(0, -90, 0);
                break;
            case 0:
                if (direction.z == 1 && direction.y == 0) //frontwall => panca a posz+1.5+width/2 (interno)
                {
                    lightPosition = new Vector3(position.x, paint_max_h + 0.25f, position.z);
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
        Lights.Add(emitter);//aggiungo alla lista
        return emitter;
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
                    rmat_indx = Random.Range(0, rmaterial_Num - 1);
                }
                toApply = roof_materials[rmat_indx];
                scale_factor = 10f;

                break;
            case ("Floor"):
                if (changeMaterial)
                {
                    fmaterial_Num = floor_materials.Length;
                    fmat_indx = Random.Range(0, fmaterial_Num - 1);
                }
                toApply = floor_materials[fmat_indx];
                scale_factor = 10f;
                break;
            default:
                if (changeMaterial)
                {
                    wmaterial_Num = wall_materials.Length;
                    wmat_indx = Random.Range(0, wmaterial_Num - 1); //indice random per il vettore di materiali
                }
                toApply = wall_materials[wmat_indx];
                scale_factor = 1f;
                break;

        }
        
        component.GetComponent<Renderer>().material = toApply;
        //aggiusto il tiling: dimensioni del tiles = dimensioni di scala dell'oggetto
        Vector2 tile_size = new Vector2(scale_factor*component.transform.lossyScale.z,
                                        scale_factor*component.transform.lossyScale.y);
        if(component.name == "Floor" || component.name == "Roof")
        {
            tile_size = randomSize;
        }
        if(component.name == "portalWall" || component.name == "frontWall")
        {
            tile_size = new Vector2(randomSize.x, roomHeight);
        }

        component.GetComponent<Renderer>().material.mainTextureScale = tile_size;
        component.GetComponent<Renderer>().material.SetTextureScale("_DetailAlbedoMap", tile_size);
        component.GetComponent<Renderer>().material.SetTextureScale("_DetailNormalMap", tile_size);
        return toApply;
    }

    //metodo che ritorna la lista contenente tutte le coppie di quadri e panche associate PER LA STANZA CORRENTE!
    public List<GameObject> getPaintBenchLight_List()
    {
        return this.paintBenchCouples;
    }
    public List<GameObject> getLights()
    {
        return this.Lights;
    }
    public List<GameObject> getBench()
    {
        return this.benches;
    }
    public List<GameObject> getPainting()
    {
        return this.paintings;
    }

    //spegne tutte le luci eccetto quella nella posizione più vicina a position
    public void turnOff_Lights(Vector3 position)
    {
        float minDist = float.MaxValue;
        float dist;
        GameObject nearLight = null;
        foreach (GameObject light in this.Lights)
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
        nearLight.gameObject.GetComponent<Lights>().turnOn_PointLights(); //accendo le pointlight della sola luce interessata

    }

    //riaccende tutte le luci (da usare all'uscita della cinema mode)
    public void turnOn_lights()
    {
        foreach(GameObject light in this.Lights)
        {
            if (light.name == "Light")//per le singole lampade
            {
                //light.GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.white);
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
    void instantiateBorders(bool guardianConfigured, GameObject thisFloor)
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
                    posMarker.y = roomCentre.y + borderMarker.transform.localScale.y / 2f;
                    GameObject marker_1 = Instantiate(borderMarker, posMarker, Quaternion.identity);
                    instantiateAudioEmitter(audioEmitter, marker_1);
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
                    posMarker.y = roomCentre.y + borderMarker.transform.localScale.y / 2f;
                    posMarker.x += distance_between_rooms;
                    marker_1 = Instantiate(borderMarker, posMarker, Quaternion.identity);
                    instantiateAudioEmitter(audioEmitter, marker_1);

                    //stanza 3: origine_x + 2*distance_b_room

                    posMarker = playArea_positions[i];
                    posMarker.y = roomCentre.y + borderMarker.transform.localScale.y / 2f;
                    posMarker.x += 2f*distance_between_rooms;
                    marker_1 = Instantiate(borderMarker, posMarker, Quaternion.identity);
                    instantiateAudioEmitter(audioEmitter, marker_1);
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
                    delimiter_1.transform.localScale = new Vector3(0.1f, Mathf.Abs(zmax - zmin), 0.1f);

                    poss = new Vector3(xmin + offset,
                       roomCentre.y + 0.75f,
                       (zmax + zmin) / 2f);
                    GameObject delimiter_2 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 0, 0));
                    delimiter_2.transform.localScale = new Vector3(0.1f, Mathf.Abs(zmax - zmin), 0.1f);

                    poss = new Vector3((xmax + xmin) / 2f   + offset,
                                       roomCentre.y + 0.75f,
                                       zmin);
                    GameObject delimiter_3 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 90, 0));
                    delimiter_3.transform.localScale = new Vector3(0.1f, Mathf.Abs(xmax - xmin), 0.1f);
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
                                       roomCentre.y + borderMarker.transform.localScale.y / 2f,
                                       roomCentre.z + posz -0.2f);
            GameObject marker_1 = Instantiate(borderMarker, poss, Quaternion.identity);
            instantiateAudioEmitter(audioEmitter, marker_1);
            marker_1.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x - posx + distance_wall_border_x,
                               roomCentre.y + borderMarker.transform.localScale.y / 2f,
                               roomCentre.z + posz - 0.2f);
            GameObject marker_2 = Instantiate(borderMarker, poss, Quaternion.identity);
            instantiateAudioEmitter(audioEmitter, marker_2);
            marker_2.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x + posx - distance_wall_border_x,
                               roomCentre.y + borderMarker.transform.localScale.y / 2f,
                               roomCentre.z - posz + distance_wall_border_z);
            GameObject marker_3 = Instantiate(borderMarker, poss, Quaternion.identity);
            instantiateAudioEmitter(audioEmitter, marker_3);
            marker_3.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x - posx + distance_wall_border_x,
                               roomCentre.y + borderMarker.transform.localScale.y / 2f,
                               roomCentre.z - posz + distance_wall_border_z);
            GameObject marker_4 = Instantiate(borderMarker, poss, Quaternion.identity);
            instantiateAudioEmitter(audioEmitter, marker_4);
            marker_4.transform.parent = thisFloor.transform;

            //Nastri: saranno solo 3 (l'ingresso è libero)
            poss = new Vector3(roomCentre.x + posx - distance_wall_border_x,
                               roomCentre.y + 0.75f,
                               roomCentre.z + 0.5f);
            GameObject delimiter_1 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 0, 0));
            delimiter_1.transform.localScale = new Vector3(0.1f, posz -0.5f , 0.1f);
            delimiter_1.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x - posx + distance_wall_border_x,
                               roomCentre.y + 0.75f,
                               roomCentre.z + 0.5f);
            GameObject delimiter_2 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 0, 0));
            delimiter_2.transform.localScale = new Vector3(0.1f, posz - 0.5f, 0.1f);
            delimiter_2.transform.parent = thisFloor.transform;

            poss = new Vector3(roomCentre.x,
                               roomCentre.y + 0.75f,
                               roomCentre.z - posz + distance_wall_border_z);
            GameObject delimiter_3 = Instantiate(borderDelimiter, poss, Quaternion.Euler(90, 90, 0));
            delimiter_3.transform.localScale = new Vector3(0.1f, posx -1f, 0.1f);
            delimiter_3.transform.parent = thisFloor.transform;
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
    void instantiateChandelier(GameObject Chandelier, GameObject roof)
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
            this.Lights.Add(chand);

            //secondo lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x += (10f * roofWidth / 4f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            this.Lights.Add(chand);

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
            this.Lights.Add(chand);

            //secondo lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.z += (10f * roofLenght / 4f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            this.Lights.Add(chand);

        }
        else if (roofWidth >= 13f / 10f && roofLenght >= 13f / 10f) //stanza molto grande: 4 lampadari
        {
            Vector3 position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x -= (10f * roofWidth / 4f);
            position.z -= (10f * roofLenght / 4f);
            GameObject chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            this.Lights.Add(chand);

            //secondo lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x += (10f * roofWidth / 4f);
            position.z += (10f * roofLenght / 4f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            this.Lights.Add(chand);

            //Terzo lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x += (10f * roofWidth / 4f);
            position.z -= (10f * roofLenght / 4f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            this.Lights.Add(chand);

            //Quarto lampadario
            position = roof.transform.position;
            position.y -= distance_from_roof;
            position.x -= (10f * roofWidth / 4f);
            position.z += (10f * roofLenght / 4f);
            chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            this.Lights.Add(chand);

        }
        else //stanza piccola: un solo lampadario
        {
            Vector3 position = roof.transform.position;
            position.y -= distance_from_roof;
            GameObject chand = Instantiate(Chandelier, position, Quaternion.identity);
            chand.name = "Lampadario";
            chand.transform.rotation = Quaternion.Euler(-90, 0, 180);
            chand.transform.parent = roof.transform;
            this.Lights.Add(chand);
        }

    }

    //istanzia gli emettitori audio sui border_marker
    void instantiateAudioEmitter(GameObject audioEmitter, GameObject parent)
    {
        Vector3 position = parent.transform.position;
        position.y += (parent.transform.localScale.y) + audioEmitter.transform.localScale.y/2f -0.025f;
        GameObject audioEmit = Instantiate(audioEmitter, position, Quaternion.identity);
        audioEmit.name = "AudioEmitter";
        audioEmit.GetComponent<ReduceVolume>().roomSize = randomSize;
        audioEmit.GetComponent<ReduceVolume>().minRoomSize = minsize;
        audioEmit.GetComponent<ReduceVolume>().maxRoomSize = maxsize;
        audioEmit.transform.parent = parent.transform;
        audioEmit.GetComponent<RotateSpeaker>().roomCentre = floor.transform.position;
    }

    //se la stanza è abbastanza alta, istanzia un ornamento per muro (sx e dx)
    void instantiateOrnaments(GameObject wall, GameObject ornament)
    {
        GameObject balcony;
        float balcony_y_pos = 4.5f;
        Vector3 balcony_position = wall.transform.position;
        balcony_position.y = balcony_y_pos;
        //spazio libero: almeno 2.5m su asse y. Fascia 1: quadri, da altezza 0 a max 3.25f. Fascia 2: da 3.5f a 5.5f.
        if (roomHeight >= 5.5f)
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
    Vector3 fix_roomPosition(Vector3 oldPosition, float roomLenght = 0, float portalLenght = 0)
    {
        Vector3 currentPosition = oldPosition;
        float offset = 0f - currentPosition.z + portalLenght/2f  - roomLenght/2f;
        Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y , currentPosition.z+ offset);
        return newPosition;
    }
}
