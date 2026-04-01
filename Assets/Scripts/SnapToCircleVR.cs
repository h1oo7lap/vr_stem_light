using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
[RequireComponent(typeof(Rigidbody))]
public class SnapToCircleVR : MonoBehaviour
{
    public RingSnapManager snapManager;

    public float rotateSpeed = 50f; // tốc độ quay (degree/second)

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    private bool isOrbiting = false;
    private float currentRadius;
    private float currentAngle;

    private void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        grabInteractable.selectExited.AddListener(OnRelease);
        grabInteractable.selectEntered.AddListener(OnGrab);
    }

    private void OnDisable()
    {
        grabInteractable.selectExited.RemoveListener(OnRelease);
        grabInteractable.selectEntered.RemoveListener(OnGrab);
    }

    private void Update()
    {
        if (isOrbiting && snapManager != null)
        {
            Orbit();
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Khi cầm lại → dừng quay
        isOrbiting = false;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (snapManager == null) return;

        if (snapManager.TryGetSnapPosition(transform.position, out Vector3 snappedPos))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            transform.position = snappedPos;

            StartOrbit(snappedPos);
        }
    }

    private void StartOrbit(Vector3 snappedPos)
    {
        Vector3 center = snapManager.centerPoint.position;

        Vector3 dir = snappedPos - center;
        dir.y = 0;

        currentRadius = dir.magnitude;

        currentAngle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

        isOrbiting = true;
    }

    private void Orbit()
    {
        currentAngle += rotateSpeed * Time.deltaTime;

        Vector3 center = snapManager.centerPoint.position;

        float rad = currentAngle * Mathf.Deg2Rad;

        float x = Mathf.Cos(rad) * currentRadius;
        float z = Mathf.Sin(rad) * currentRadius;

        Vector3 newPos = new Vector3(
            center.x + x,
            transform.position.y,
            center.z + z
        );

        transform.position = newPos;
    }
}