using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReduceVolume : MonoBehaviour
{
    public GameObject soundEmitter;
    public GameObject player;
    private float maxDistance = 30f; //corrisponde a volume = 0
    private float minDistance = 0f; // corrisponde a volume = 1
    private float minVolume = 0.3f;
    private float maxVolume = 1f;

    public Vector2 roomSize, maxRoomSize, minRoomSize; //usato per uniformare il volume nelle stanze
    float area, maxArea, minArea;

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
                mute();
            }
            else
            {
                unmute();
                regulateVolume(Distance);
            }
        }
    }
    public void regulateVolume(float dist)
    {
        float vol;
        //maxVolume = volume_scaleFactor() * maxVolume - (1 - volume_scaleFactor()) * minVolume; //normalizzo il massimo volume possibile per le dimensioni della stanza
        //vol = (float)(Mathf.Abs(maxDistance- dist)*maxVolume / maxDistance);
        vol = volume_scaleFactor()*(float)(Mathf.Abs(maxDistance - dist) * maxVolume / maxDistance) + (1-volume_scaleFactor())*minVolume;
        this.soundEmitter.GetComponent<AudioSource>().volume = vol;
        maxVolume = 1f;
    }
    float volume_scaleFactor()
    {
        area = roomSize.x * roomSize.y;
        maxArea = maxRoomSize.x * maxRoomSize.y;
        minArea = minRoomSize.x * minRoomSize.y;
        float scale = 1f - (float)(Mathf.Abs((maxArea - area)/(maxArea - minArea))); //se area == maxarea => volume massimo. Se area == minArea => volume minimo (non muto!)
        return scale;
    }
    public void mute()
    {
        this.soundEmitter.GetComponent<AudioSource>().mute = true;
    }
    public void unmute()
    {
        this.soundEmitter.GetComponent<AudioSource>().mute = false;
    }
}
