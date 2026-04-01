using UnityEngine;

public class RingSnapManager : MonoBehaviour
{
    public Transform centerPoint;
    public float snapThreshold = 0.3f;

    public float[] radii = new float[]
    {
        0.75f, 1.25f, 1.75f, 2.25f, 3f, 4f, 5f
    };

    public bool TryGetSnapPosition(Vector3 objectPosition, out Vector3 snappedPosition)
    {
        snappedPosition = Vector3.zero;

        if (centerPoint == null) return false;

        Vector3 centerPos = centerPoint.position;
        Vector3 direction = objectPosition - centerPos;

        // Snap trên mặt phẳng XZ
        direction.y = 0;

        float distance = direction.magnitude;

        float nearestRadius = -1;
        float minDifference = Mathf.Infinity;

        foreach (float radius in radii)
        {
            float diff = Mathf.Abs(distance - radius);

            if (diff < minDifference)
            {
                minDifference = diff;
                nearestRadius = radius;
            }
        }

        if (minDifference <= snapThreshold)
        {
            snappedPosition = centerPos + direction.normalized * nearestRadius;
            snappedPosition.y = centerPos.y;
            return true;
        }

        return false;
    }
}