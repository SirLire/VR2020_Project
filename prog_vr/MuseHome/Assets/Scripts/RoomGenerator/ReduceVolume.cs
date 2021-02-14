﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReduceVolume : MonoBehaviour
{
    public GameObject soundEmitter;
    public GameObject player;
    private float maxDistance = 30f; //corrisponde a volume = 0
    private float minVolume = 0.3f;
    private float maxVolume = 0.5f;
    private float riseTime = 10f;
    private float fixedRiseTime = 10f;

    public Vector2 roomSize, maxRoomSize, minRoomSize; //usato per uniformare il volume nelle stanze
    float area, maxArea, minArea;
    public bool soundMuted;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("OVRPlayerController_edited");
        if(player== null)
            player = GameObject.Find("Main Camera"); //SOLO PER TEST nella scena roomGenerators
        
    }

    // Update is called once per frame
    void Update()
    {
        this.riseTime -= Time.deltaTime;
        if (soundMuted)
            mute();
        toggleVolume();
    }
    private void Awake()
    {
        mute();
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
                if (!soundMuted)
                {
                    unmute();
                    regulateVolume(Distance);
                }
            }


        }
    }
    public void regulateVolume(float dist)
    {
        float vol;
        vol = volume_scaleFactor() * maxVolume + (1 - volume_scaleFactor()) * minVolume; //normalizzo il massimo volume possibile per le dimensioni della stanza
        vol = (float)(Mathf.Abs(maxDistance- dist)*maxVolume / maxDistance);
        vol = Mathf.Max((float)(Mathf.Abs(fixedRiseTime - riseTime)*vol / fixedRiseTime), vol/2f);
        vol = Mathf.Min(vol, maxVolume);
        this.soundEmitter.GetComponent<AudioSource>().volume = vol;
        
    }
    float volume_scaleFactor()
    {
        float scale;
        area = roomSize.x * roomSize.y;
        maxArea = maxRoomSize.x * maxRoomSize.y;
        minArea = minRoomSize.x * minRoomSize.y;
        if (maxArea == minArea)
            scale = 0f;
        else
            scale = 1f - (float)(Mathf.Abs(maxArea - area) / maxArea);
        return Mathf.Abs(scale);
    }
    public void mute()
    {
        this.soundEmitter.GetComponent<AudioSource>().mute = true;
        this.riseTime = fixedRiseTime;
    }
    public void unmute()
    {
        this.soundEmitter.GetComponent<AudioSource>().mute = false;

    }

}
