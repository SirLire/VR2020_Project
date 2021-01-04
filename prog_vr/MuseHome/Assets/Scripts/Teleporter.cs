using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Main Settings")]
    public Teleporter linkedPortal_next;
    public Teleporter linkedPortal_previous;

    [Header("Advanced Settings")]

    // Private variables
    List<PortalTraveller> trackedTravellers;

    void Awake()
    {
        //playerCam = Camera.main;
        trackedTravellers = new List<PortalTraveller>();
    }

    void LateUpdate()
    {
        HandleTravellers();
    }

    void HandleTravellers()
    {

        for (int i = 0; i < trackedTravellers.Count; i++)
        {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerT = traveller.transform;

            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign(Vector3.Dot(offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));
            // Teleport the traveller if it has crossed from one side of the portal to the other
            if (portalSide != portalSideOld)
            {
                int piC = GameObject.Find("Room_Boundary_Instantiator").GetComponent<Boundary>().player_in_CurrentRoom;
                var linkedPortal = linkedPortal_next;
                //[Cannillo]
                /*piC:
                 * => 1  :se dalla current room procedo in avanti (nella new room)
                 * => -1 :se passo dalla current room alla old room 
                 * => 0  :se dalla old room passo alla current room (non devo quindi modificare le stanze, ma ritornare nella current già visitata)
                 */
                //se piC == -1, sono tornato indietro alla old room. Se piC == 0, dalla old room sono tornato alla current => nessuno dei due teletrasporti deve modificare le stanze
                
                //[/Cannillo]
                if (portalSide > 0) //procedo in avanti
                {
                    linkedPortal = linkedPortal_previous;

                    if( piC == -1 || piC == 0 )
                        GameObject.Find("Room_Boundary_Instantiator").GetComponent<Boundary>().player_in_CurrentRoom++;
                }

                else //sono tornato indietro
                {
                    GameObject.Find("Room_Boundary_Instantiator").GetComponent<Boundary>().player_in_CurrentRoom = -1;
                }
                var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;
                traveller.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);

                //[Cannillo]
                GameObject.Find("Room_Boundary_Instantiator").GetComponent<Boundary>()._roomChanged = true; //stanza cambiata
                //[/Cannillo]

                // Can't rely on OnTriggerEnter/Exit to be called next frame since it depends on when FixedUpdate runs
                linkedPortal.OnTravellerEnterPortal(traveller);
                trackedTravellers.RemoveAt(i);
                i--;

            }
            else
            {
                //UpdateSliceParams (traveller);
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    void OnTravellerEnterPortal(PortalTraveller traveller)
    {
        if (!trackedTravellers.Contains(traveller))
        {
            traveller.EnterPortalThreshold();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add(traveller);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller)
        {
            OnTravellerEnterPortal(traveller);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var traveller = other.GetComponent<PortalTraveller>();
        if (traveller && trackedTravellers.Contains(traveller))
        {
            traveller.ExitPortalThreshold();
            trackedTravellers.Remove(traveller);
        }
    }

    int SideOfPortal(Vector3 pos)
    {
        return System.Math.Sign(Vector3.Dot(pos - transform.position, transform.forward));
    }

    bool SameSideOfPortal(Vector3 posA, Vector3 posB)
    {
        return SideOfPortal(posA) == SideOfPortal(posB);
    }

    void OnValidate()
    {
        if (linkedPortal_next != null)
        {
            linkedPortal_next.linkedPortal_previous = this;
        }
        if (linkedPortal_previous != null)
        {
            linkedPortal_previous.linkedPortal_next = this;
        }
    }
}
