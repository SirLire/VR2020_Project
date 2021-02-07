using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Image_Spawner : MonoBehaviour
{
    public int N_Targets = 5;
    public GameObject Target;
    
    
    void Awake()
    {
        for (int i = 0; i < N_Targets; i++)
        {
            Vector3 location = new Vector3(Random.Range(-5.0f, 5.0f), Random.Range(-5.0f, 5.0f), Random.Range(-1.0f, 1.0f));
            GameObject new_Target = Instantiate(Target, location, Quaternion.identity);
        }
    }
}
