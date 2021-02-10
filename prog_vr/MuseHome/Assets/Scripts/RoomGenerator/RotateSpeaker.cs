using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpeaker : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 roomCentre;
    void Start()
    {

        //this.gameObject.transform.rotation;
        calculateRotation();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void calculateRotation()
    {
        
        Vector3 target = roomCentre;
        target.y = this.transform.position.y;
        Vector3 targetDirection = target - (this.transform.position);
        this.transform.rotation = Quaternion.LookRotation(targetDirection);
    }
}
