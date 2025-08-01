using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AGENT_STATUS { PATROL, ALLERT }

public class AgentPatrollingPath : MonoBehaviour
{
    [Header("Parametri Patrol Status")]
    [SerializeField] private Transform[] _destinationWaypoints;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private LayerMask _patrolLayerMask;
    [SerializeField] private float _patrollingWaitingTime = 1.5f;

    private int _destinationWaypointIndex;
    private NavMeshAgent _agent;
    private AGENT_STATUS _currentStatus = AGENT_STATUS.PATROL;
    private Transform _playerPosition;
    private bool _isWaiting = false;

    //[Header("Parametri FOV")]
    //[SerializeField] private float _viewAngle = 60f;
    //[SerializeField] private float _viewDistance = 10f;
    //[SerializeField] private int _viewSegments = 30;
    //[SerializeField] private LineRenderer _viewRenderer;
    //[SerializeField] private LayerMask _obstacleLayerMask;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        SetDestinationAtIndex(0);
    }

    // Update is called once per frame
    void Update()
    {
        CheckAgentStatus();

        if (IsPlayerNear()) _currentStatus = AGENT_STATUS.ALLERT;

        else _currentStatus = AGENT_STATUS.PATROL;

        //DrawFOV(); // <-- disegna il cono
    }

    public void SetDestinationAtNextIndex() => SetDestinationAtIndex(_destinationWaypointIndex + 1);
    public void SetDestinationAtPreviousIndex() => SetDestinationAtIndex(_destinationWaypointIndex - 1);
    public void SetDestinationAtIndex(int index) // <- per settare la destinazione
    {
        if (index < 0)
        {
            while (index < 0) index += _destinationWaypoints.Length;
        }

        else if (index >= _destinationWaypoints.Length) index = index % _destinationWaypoints.Length;

        _destinationWaypointIndex = index;

        _agent.SetDestination(_destinationWaypoints[_destinationWaypointIndex].position);
    }

    public bool IsCloseEnoughToDestination() // <- per controllare quando � abbastanza vicino al waypoint
    {
        float sqrStoppingDistance = _agent.stoppingDistance * _agent.stoppingDistance;
        Vector3 toDestination = _destinationWaypoints[_destinationWaypointIndex].position - transform.position;
        toDestination.y = 0; // <- fondamentale altrimenti l`agent sar� sempre lontano di circa 1 dalla destinazione e non raggiunger� mai la prossima destinazione (colpa del transform.position)
        float sqrDistance = toDestination.sqrMagnitude;

        if (sqrDistance <= sqrStoppingDistance + 0.1f) return true; // <- con Mathf.Epsilon non funzionava -_-

        _agent.SetDestination(_destinationWaypoints[_destinationWaypointIndex].position);
        return false;
    }

    private void CheckAgentStatus() // < per scegliere se pattugliare o inseguire il player
    {
        switch (_currentStatus)
        {
            case AGENT_STATUS.PATROL:
                StartPatrol();
                break;
            case AGENT_STATUS.ALLERT:
                StartChase();
                break;
        }
    }

    private void StartPatrol() // <- per gestire il pattugliamento
    {
        if (_isWaiting) return;
        
        if (IsCloseEnoughToDestination())
        {
            SetDestinationAtNextIndex();
            StartCoroutine(PatrollingStopTime());
        }
    }

    IEnumerator PatrollingStopTime() // <- coroutine per gestire l`attesa tra un movimento e l`altro
    {
        _isWaiting = true;
        _agent.isStopped = true;
        yield return new WaitForSeconds(_patrollingWaitingTime);
        _agent.isStopped = false;
        _isWaiting = false;
    }

    private void StartChase() // <- per iniziare a seguire il player
    {
        if (_playerPosition != null)
        {
            _agent.SetDestination(_playerPosition.position);
            _agent.isStopped = false;
        }        
    }

    private bool IsPlayerNear() // <- per valutare se il player � nella visuale dell`agente (enemy)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _patrolLayerMask);
        if (colliders != null)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    _playerPosition = collider.transform; // <- mi tengo da parte la posizione del player
                    return true;
                }
            }
        }
        return false;
    }

    //
    // il metodo di seguito utilizza il cono di visione con LineRenerer.
    // disabilitare i commenti per usarlo e sostituire a IsPlayerNear in Update()
    // questo metodo
    //
    //private bool IsPlayerInFOV()
    //{
    //    Collider[] targetsInFOV = Physics.OverlapSphere(transform.position, _viewDistance, _patrolLayerMask);

    //    foreach (Collider target in targetsInFOV)
    //    {
    //        if (target.CompareTag("Player"))
    //        {
    //            Transform player = target.transform;
    //            Vector3 dirToPlayer = (player.position - transform.position).normalized;

    //            if (Vector3.Angle(transform.forward, dirToPlayer) < _viewAngle / 2f)
    //            {
    //                float distToPlayer = Vector3.Distance(transform.position, player.position);

    //                // Check for obstacles
    //                if (!Physics.Raycast(transform.position, dirToPlayer, distToPlayer, _obstacleLayerMask))
    //                {
    //                    _playerPosition = player;
    //                    return true;
    //                }
    //            }
    //        }
    //    }
    //    return false;
    //}

    //private void DrawFOV()
    //{
    //    if (_viewRenderer == null) return;

    //    _viewRenderer.positionCount = _viewSegments + 2; // center + points + back to center
    //    _viewRenderer.SetPosition(0, transform.position);

    //    float angleStep = _viewAngle / _viewSegments;
    //    float startAngle = -_viewAngle / 2f;

    //    for (int i = 0; i <= _viewSegments; i++)
    //    {
    //        float angle = startAngle + angleStep * i;
    //        Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
    //        Vector3 endPoint = transform.position + dir * _viewDistance;

    //        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, _viewDistance, _obstacleLayerMask))
    //            endPoint = hit.point;

    //        _viewRenderer.SetPosition(i + 1, endPoint);
    //    }
    //}
}
