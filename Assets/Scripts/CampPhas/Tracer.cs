using UnityEngine;

public class Tracer : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lifetime = 0.05f;

    private float despawnTime;

    private void Awake()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
    }

    private void OnEnable()
    {
        despawnTime = Time.time + lifetime;
    }

    private void Update()
    {
        if (Time.time >= despawnTime)
        {
            Destroy(gameObject);
        }
    }

    public void SetPositions(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null)
        {
            return;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
}
