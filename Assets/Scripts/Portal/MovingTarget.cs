using UnityEngine;

public class MovingTarget : BaseTarget
{
    private Vector3 _initialPosition;
    public Transform _endPosition;
    public bool multiplePositionCheckpoints;
    public Transform[] checkpoints;
    public float speed = 1f;
    private int _checkpointIndex;
    private int _checkpointDirection = 1;

    private void Start()
    {
        _initialPosition = transform.position;
    }

    // left right movement 
    private void Update()
    {
        if (multiplePositionCheckpoints && checkpoints != null && checkpoints.Length > 1)
        {
            MoveAcrossCheckpoints();
            return;
        }

        if (_endPosition == null) return;

        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f; // oscillates between 0 and 1
        transform.position = Vector3.Lerp(_initialPosition, _endPosition.position, t);
    }

    private void MoveAcrossCheckpoints()
    {
        Transform target = checkpoints[_checkpointIndex];
        if (target == null)
        {
            return;
        }

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);

        if (Vector3.Distance(transform.position, target.position) > 0.01f)
        {
            return;
        }

        // Ping-pong through the array: 0 -> 1 -> 2 -> 3 -> 2 -> 1 -> 0 ...
        if (_checkpointIndex >= checkpoints.Length - 1)
        {
            _checkpointDirection = -1;
        }
        else if (_checkpointIndex <= 0)
        {
            _checkpointDirection = 1;
        }

        _checkpointIndex += _checkpointDirection;
    }
}
