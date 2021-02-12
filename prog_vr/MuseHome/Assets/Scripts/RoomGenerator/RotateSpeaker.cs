using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpeaker : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 roomCentre;
    private Vector3 oldRoomCentre;
    void Start()
    {
        oldRoomCentre = new Vector3();
        //this.gameObject.transform.rotation;
        calculateRotation();
    }
    private void Awake()
    {
        calculateRotation();
    }
    // Update is called once per frame
    void Update()
    {
        if (roomCentre != oldRoomCentre)
        {
            calculateRotation();
            oldRoomCentre = roomCentre;
        }
    }

    void calculateRotation()
    {
        
        Vector3 target = roomCentre;
        target.y = this.transform.position.y;
        Vector3 targetDirection = target - (this.transform.position);
        this.transform.rotation = Quaternion.LookRotation(targetDirection);
    }
}
