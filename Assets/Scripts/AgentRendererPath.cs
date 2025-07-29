using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentRendererPath : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;

    private LineRenderer _lineRenderer;
    private NavMeshPath _path;
    private Vector3 _targetPosition;
    private bool _isShiftHeld = false;

    public Color _completePathColor = Color.green;
    public Color _partialPathColor = Color.yellow;
    public Color _invalidPathColor = Color.red;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _lineRenderer = GetComponent<LineRenderer>();
        _path = new NavMeshPath();
        _lineRenderer.positionCount = 0;
    }

    void Update()
    {
        _isShiftHeld = Input.GetKey(KeyCode.LeftShift); // <- shift sx premuto

        if (Input.GetMouseButtonDown(0)) // <- click sinistro per impostare destinazione
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _targetPosition = hit.point;

                _agent.CalculatePath(_targetPosition, _path); // <- come suggerito dalla traccia, calcola il path
                _agent.destination = _targetPosition;
                DrawPath();
                _agent.isStopped = true;
            }
        }

        if (_agent.isStopped && !_isShiftHeld) // <- se l`agent è fermo e shift non è premuto, muove l’agente
        {
            _agent.isStopped = false;
        }
    }

    void DrawPath()
    {
        if (!_agent.hasPath) _lineRenderer.enabled = false;
        else _lineRenderer.enabled = true;

        switch (_path.status) // <- colora il LineRenderer in base allo stato del path
        {
            case NavMeshPathStatus.PathComplete:
                _lineRenderer.material.color = _completePathColor;
                _lineRenderer.startColor = _completePathColor;
                _lineRenderer.endColor = _completePathColor;
                break;
            case NavMeshPathStatus.PathPartial:
                _lineRenderer.material.color = _partialPathColor;
                _lineRenderer.startColor = _partialPathColor;
                _lineRenderer.endColor = _partialPathColor;
                break;
            case NavMeshPathStatus.PathInvalid:
                _lineRenderer.material.color = _invalidPathColor;
                _lineRenderer.startColor = _invalidPathColor;
                _lineRenderer.endColor = _invalidPathColor;
                break;
        }

        _lineRenderer.positionCount = _agent.path.corners.Length;
        _lineRenderer.SetPositions(_agent.path.corners);
    }
}
