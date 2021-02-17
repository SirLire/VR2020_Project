using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float lookSpeed = 5.0f;
    public GameObject camera;
    private Transform player;
    public float rotazione;
// Start is called before the first frame update
void Start()
    {
        player = camera.transform;
        rotazione = player.rotation.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 rotation = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        //rotation.x movimento sinistra destra
        rotazione = rotation.x * lookSpeed * Time.deltaTime;
        
        player.rotation *= Quaternion.Euler(0, rotazione, 0);
    }
}
