using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Target_Spawner : MonoBehaviour
{
    public int N_NPC = 4;
    public GameObject NPC;
    public int N_Targets = 10;
    public GameObject Target;

    
    private List<GameObject> NPCs;
    private List<GameObject> Targets;
    private List<Vector3> TargetsLocations;

    // Start is called before the first frame update
    void Awake()
    {
        NPCs = new List<GameObject>();
        Targets = new List<GameObject>();
        TargetsLocations = new List<Vector3>();

        for (int i = 0; i < N_Targets; i++)
        {
            Vector3 location = new Vector3(Random.Range(-30.0f, 30.0f), 0, Random.Range(-30.0f, 30.0f));
            GameObject new_Target = Instantiate(Target, location, Quaternion.identity);
            Targets.Add(new_Target);
            TargetsLocations.Add(location);
        }
        for(int i=0; i<N_NPC; i++)
        {
            GameObject new_NPC = Instantiate(NPC);
            NPC_NavController controller = new_NPC.GetComponent<NPC_NavController>();
            controller.Targets = TargetsLocations;
            NPCs.Add(new_NPC);
        }
    }
    
}
