using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentPatrollingPath : MonoBehaviour
{
    [SerializeField] private Transform[] _waypoints;

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
        
    }

    public void SetDestinationAtNextIndex() => SetDestinationAtIndex(_waypointIndex + 1);
    public void SetDestinationAtPreviousIndex() => SetDestinationAtIndex(_waypointIndex - 1);
    public void SetDestinationAtIndex(int index)
    {
        if (index < 0)
        {
            while (index < 0) index += _waypoints.Length;
        }

        else if (index >= _waypoints.Length) index = index % _waypoints.Length;

        _waypointIndex = index;

        _agent.SetDestination(_waypoints[_waypointIndex].position);
    }

    public bool IsCloseEnoughToDestination()
    {
        float sqrStoppingDistance = _agent.stoppingDistance * _agent.stoppingDistance;
        Vector3 toDestination = _waypoints[_waypointIndex].position - transform.position;
        float sqrDistance = toDestination.sqrMagnitude;
        if (sqrDistance <= sqrStoppingDistance + Mathf.Epsilon) return true;
        return false;
    }
}
