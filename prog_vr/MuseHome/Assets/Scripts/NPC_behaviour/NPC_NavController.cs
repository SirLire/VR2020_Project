using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_NavController : MonoBehaviour
{
    public List<Vector3> Targets;
    public List<Vector3> Paintings;
    public float sostaMinima = 3.0f, sostaMassima = 10.0f;
    public float randomStoppingDistance = 0.5f;
    public float rotationFacingStep = 0.1f;
    private NavMeshAgent _navMeshAgent;
    private bool routing = false;
    private int _selectedTarget = 0;

    // Start is called before the first frame update
    void Start()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _selectedTarget = Random.Range(0, Targets.Count);
        _navMeshAgent.SetDestination(Targets[_selectedTarget]);
    }

    // Update is called once per frame
    void Update()
    {
        if (routing == false)
        {
            routing = true;
            StartCoroutine( route() );
        }
        if (TargetReached())
        {
            FaceTarget(Paintings[_selectedTarget]);
        }
    }

    private IEnumerator route()
    {
        if (TargetReached())
        {
            _navMeshAgent.isStopped = true;
            yield return new WaitForSeconds( Random.Range(sostaMinima, sostaMassima) );
            _selectedTarget = Random.Range(0, Targets.Count);
            _navMeshAgent.SetDestination(Targets[_selectedTarget]);
            _navMeshAgent.isStopped = false;
        }
        yield return new WaitForSeconds(0);

        routing = false;
    }

    private bool TargetReached()
    {
        if (!_navMeshAgent.pathPending)
        {
            if (_navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance + Random.Range(-randomStoppingDistance, randomStoppingDistance))
            {
                return true;
            }
        }
        return false;
    }

    private void FaceTarget(Vector3 destination)
    {
        Vector3 lookPos = destination - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationFacingStep);
    }
}