using UnityEngine;

public class AEDWireRenderer : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    public float wireWidth = 0.01f;
    public int curvePointCount = 12;
    public float sagAmount = 0.08f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = curvePointCount;
        lineRenderer.startWidth = wireWidth;
        lineRenderer.endWidth = wireWidth;
        lineRenderer.useWorldSpace = true;
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null)
            return;

        for (int i = 0; i < curvePointCount; i++)
        {
            float t = i / (float)(curvePointCount - 1);

            Vector3 pos = Vector3.Lerp(startPoint.position, endPoint.position, t);

            float sag = Mathf.Sin(t * Mathf.PI) * sagAmount;
            pos += Vector3.down * sag;

            lineRenderer.SetPosition(i, pos);
        }
    }
}