using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Boundary : MonoBehaviour
{
    public GameObject wallMarker;
    public float AreaSize = 0;
    [SerializeField] private Vector3 playArea_dimensions;
    [SerializeField] private bool configured;
    // Start is called before the first frame update
    void Start()
    {
        configured = OVRManager.boundary.GetConfigured();
        GameObject[] plane = GameObject.FindGameObjectsWithTag("Plane");
        GameObject onlyPlane = plane[0];
        //TODO: force user to use guardian system
        if (configured)
        {
            //Grab all the boundary points. Setting BoundaryType to OuterBoundary is necessary
            Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);

            var planeRenderer = onlyPlane.GetComponent<Renderer>();
            if (planeRenderer != null)
                planeRenderer.material.SetColor("_Color", Color.red);
            //Generate a bunch of tall thin cubes to mark the outline
            foreach (Vector3 pos in boundaryPoints)
            {
                Instantiate(wallMarker, pos, Quaternion.identity);
            }

            //TODO: get Area size and check if it's smaller than room size
            playArea_dimensions = OVRManager.boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);
            //OVRBoundary.GetDimensions() returns a Vector3 containing the width, height, and depth in tracking space units, with height always returning 0.
            AreaSize = playArea_dimensions[0] * playArea_dimensions[2]; //m^2


        }
        else
        {
            //if guardian system is not configured

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

/*
public GameObject wallMarker;
//Check if the boundary is configured
bool configured = OVRManager.boundary.GetConfigured();
        if (configured)
        {
            //Grab all the boundary points. Setting BoundaryType to OuterBoundary is necessary
            Vector3[] boundaryPoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
     
             //Generate a bunch of tall thin cubes to mark the outline
            foreach (Vector3 pos in boundaryPoints)
            {    
                Instantiate(wallMarker, pos, Quaternion.identity);
            }
        } 
*/