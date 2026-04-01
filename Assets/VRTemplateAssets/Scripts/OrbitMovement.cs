using UnityEngine;

public class OrbitMovement : MonoBehaviour
{
    Transform center;
    public float speed = 50f;

    float radius;
    float angle;
    bool isOrbiting = false;

    public void Init(Transform orbitCenter)
    {
        center = orbitCenter;

        Vector3 offset = transform.position - center.position;
        offset.y = 0;

        radius = offset.magnitude;
        angle = Mathf.Atan2(offset.z, offset.x);

        // Cố định Y theo tâm
        transform.position = new Vector3(
            transform.position.x,
            center.position.y,
            transform.position.z
        );

        isOrbiting = true;

        // Tắt physics nếu có
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }

    void Update()
    {
        if (!isOrbiting) return;

        angle += speed * Time.deltaTime * Mathf.Deg2Rad;

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        transform.position = new Vector3(
            center.position.x + x,
            center.position.y,
            center.position.z + z
        );
    }
}