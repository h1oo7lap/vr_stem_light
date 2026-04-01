using UnityEngine;

public class RingOrbitZone : MonoBehaviour
{
    public Transform ringCenter;   // 👈 PHẢI có dòng này
    public float orbitSpeed = 50f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Planet"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            OrbitMovement orbit = other.GetComponent<OrbitMovement>();
            if (orbit == null)
            {
                orbit = other.gameObject.AddComponent<OrbitMovement>();
                orbit.Init(ringCenter);
            }
        }
    }
}