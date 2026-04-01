using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class OvalRing : MonoBehaviour
{
    [Header("Ellipse Shape (Base Shape)")]
    public float radiusX = 2f;
    public float radiusY = 1f;

    [Header("Ring Settings")]
    [Range(10, 200)]
    public int segments = 100;
    public float lineWidth = 0.05f;

    private LineRenderer lineRenderer;
    private MeshCollider meshCollider;

    private Vector3[] points;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.widthMultiplier = lineWidth;
    }

    void Start()
    {
        DrawOval();
    }
    
    public Vector3 GetPoint(float angle)
    {
        float x = Mathf.Cos(angle) * radiusX;
        float z = Mathf.Sin(angle) * radiusY;

        return transform.TransformPoint(new Vector3(x, 0, z));
    }

    void DrawOval()
    {
        lineRenderer.positionCount = segments;
        points = new Vector3[segments];

        float angle = 0f;

        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Cos(angle) * radiusX;
            float z = Mathf.Sin(angle) * radiusY;

            Vector3 pos = new Vector3(x, 0, z);
            points[i] = pos;

            lineRenderer.SetPosition(i, pos);

            angle += (2 * Mathf.PI) / segments;
        }
    }
    

    // SCALE METHOD (tối ưu)
    public void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, 1f, scale);
    }
    
    [ContextMenu("Rebuild Ring")]
    public void RebuildRing()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();

        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.widthMultiplier = lineWidth;

        DrawOval();
    }

    public void SetVisibility(float alpha)
    {
        Color c = lineRenderer.startColor;
        c.a = alpha;

        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }
    
    public float GetRadius()
    {
        float scaleX = transform.lossyScale.x;
        float scaleZ = transform.lossyScale.z;

        float rX = radiusX * scaleX;
        float rY = radiusY * scaleZ;

        return (rX + rY) * 0.5f;
    }
}

