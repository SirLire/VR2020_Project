using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReduceVolume : MonoBehaviour
{
    public GameObject soundEmitter;
    public GameObject player;
    private float maxDistance = 30f; //corrisponde a volume = 0
    private float minDistance = 0f; // corrisponde a volume = 1
    private float minVolume = 0f;
    private float maxVolume = 1f;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Main Camera"); //da cambiare col player effettivo
    }

    // Update is called once per frame
    void Update()
    {
        toggleVolume();
    }

    public void toggleVolume()
    {
        if (player != null)
        {
            float Distance = Vector3.Distance(player.transform.position, this.transform.position);
            if (Distance > 30f)
            {
                this.soundEmitter.GetComponent<AudioSource>().mute = true;
            }
            else
            {
                this.soundEmitter.GetComponent<AudioSource>().mute = false;
                regulateVolume(Distance);
            }
        }
    }
    public void regulateVolume(float dist)
    {
        float vol;
        vol = (float)(Mathf.Abs(maxDistance- dist)*maxVolume / maxDistance);
        this.soundEmitter.GetComponent<AudioSource>().volume = vol;
    }
}
