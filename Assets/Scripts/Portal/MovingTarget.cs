using UnityEngine;

public class MovingTarget : BaseTarget
{
    private Vector3 _initialPosition;
    public Transform _endPosition;
    public float speed = 1f;

    private void Start()
    {
        _initialPosition = transform.position;
    }

    // left right movement 
    private void Update()
    {
        if (_endPosition == null) return;

        float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f; // oscillates between 0 and 1
        transform.position = Vector3.Lerp(_initialPosition, _endPosition.position, t);
    }
}
