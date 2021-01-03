using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_NavController : MonoBehaviour
{
    public List<Vector3> Targets;

    private NavMeshAgent _navMeshAgent;
    private bool routing = false;

    // Start is called before the first frame update
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.SetDestination(Targets[Random.Range(0, Targets.Count)]);
    }

    // Update is called once per frame
    void Update()
    {
        if (routing == false)
        {
            StartCoroutine( route() );
        }
    }

    private IEnumerator route()
    {
        routing = true;

        if (TargetReached())
        {
            _navMeshAgent.isStopped = true;
            yield return new WaitForSeconds( Random.Range(3 , 5) );
            _navMeshAgent.SetDestination(Targets[Random.Range(0, Targets.Count)]);
            _navMeshAgent.isStopped = false;
        }
        yield return new WaitForSeconds(0);

        routing = false;
    }

    private bool TargetReached()
    {
        if (!_navMeshAgent.pathPending)
        {
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance + Random.Range(-2, 2))
            {
                //if (!_navMeshAgent.hasPath || _navMeshAgent.velocity.sqrMagnitude == 0f)
                //{
                    return true;
                //}
            }
        }
        return false;
    }
}
