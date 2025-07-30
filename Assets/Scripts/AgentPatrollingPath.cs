using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentPatrollingPath : MonoBehaviour
{
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _radius;
    [SerializeField] private LayerMask _layerMask;

    private int _waypointIndex;
    private NavMeshAgent _agent;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        SetDestinationAtIndex(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (IsCloseEnoughToDestination())
        {
            SetDestinationAtNextIndex();
            StartCoroutine(PatrolStoppingTime());
        }
        CheckPlayer();
    }

    public void SetDestinationAtNextIndex() => SetDestinationAtIndex(_waypointIndex + 1);
    public void SetDestinationAtPreviousIndex() => SetDestinationAtIndex(_waypointIndex - 1);
    public void SetDestinationAtIndex(int index) // <- per settare la destinazione
    {
        if (index < 0)
        {
            while (index < 0) index += _waypoints.Length;
        }

        else if (index >= _waypoints.Length) index = index % _waypoints.Length;

        _waypointIndex = index;

        _agent.SetDestination(_waypoints[_waypointIndex].position);
    }

    public bool IsCloseEnoughToDestination() // <- per controllare quando è abbastanza vicino al waypoint
    {
        float sqrStoppingDistance = _agent.stoppingDistance * _agent.stoppingDistance;
        Vector3 toDestination = _waypoints[_waypointIndex].position - transform.position;
        toDestination.y = 0; // <- fondamentale altrimenti l`agent sarà sempre lontano di circa 1 dalla destinazione e non raggiungerà mai la prossima destinazione (colpa del transform.position)
        float sqrDistance = toDestination.sqrMagnitude;
        if (sqrDistance <= sqrStoppingDistance + 0.1f) return true; // <- con Mathf.Epsilon non funzionava -_-
        return false;
    }

    IEnumerator PatrolStoppingTime() // <- coroutine per gestire l`attesa tra un movimento e l`altro
    {
        _agent.isStopped = true;
        yield return new WaitForSeconds(1.5f);
        _agent.isStopped = false;
    }

    private void CheckPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _layerMask);
        if (colliders != null)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    _agent.SetDestination(collider.transform.position);
                    _agent.isStopped = false;
                }
            }
        }
    }
}
