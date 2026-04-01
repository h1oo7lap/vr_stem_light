using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;

public class TwoHandZoom : MonoBehaviour
{
    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Zoom Settings")]
    public float minScale = 0.1f;
    public float maxScale = 5f;
    public float zoomSensitivity = 1f;

    [Header("Thumbstick Zoom (1 tay)")]
    public InputActionReference leftThumbstick;
    public InputActionReference rightThumbstick;
    public float thumbstickZoomSpeed = 0.5f;

    [Header("Haptics")]
    public float hapticAmplitude = 0.3f;
    public float hapticDuration = 0.1f;

    private float previousDistance = -1f;
    private bool isTwoHandActive = false;

    private XRBaseInputInteractor leftInteractor;
    private XRBaseInputInteractor rightInteractor;

    private void Start()
    {
        if (leftController != null)
            leftInteractor = leftController.GetComponentInChildren<XRBaseInputInteractor>();
        if (rightController != null)
            rightInteractor = rightController.GetComponentInChildren<XRBaseInputInteractor>();
    }

    private void Update()
    {
        bool leftGrip = IsGripping(leftInteractor);
        bool rightGrip = IsGripping(rightInteractor);

        if (leftGrip && rightGrip)
        {
            // ---- 2 TAY ZOOM ----
            float currentDistance = Vector3.Distance(
                leftController.position,
                rightController.position
            );

            if (!isTwoHandActive)
            {
                previousDistance = currentDistance;
                isTwoHandActive = true;
            }
            else
            {
                float delta = currentDistance - previousDistance;
                ApplyScale(delta * zoomSensitivity);
                previousDistance = currentDistance;
            }
        }
        else
        {
            isTwoHandActive = false;
            previousDistance = -1f;

            // ---- 1 TAY THUMBSTICK ZOOM ----
            float input = 0f;

            if (leftThumbstick != null)
                input += leftThumbstick.action.ReadValue<Vector2>().y;
            if (rightThumbstick != null)
                input += rightThumbstick.action.ReadValue<Vector2>().y;

            input = Mathf.Clamp(input, -1f, 1f);

            if (Mathf.Abs(input) > 0.1f)
            {
                ApplyScale(input * thumbstickZoomSpeed * Time.deltaTime);
            }
        }
    }

    private void ApplyScale(float delta)
    {
        float newScale = transform.localScale.x + delta;

        bool hitLimit = false;
        if (newScale < minScale) { newScale = minScale; hitLimit = true; }
        if (newScale > maxScale) { newScale = maxScale; hitLimit = true; }

        transform.localScale = Vector3.one * newScale;

        if (hitLimit)
        {
            SendHaptic(leftInteractor);
            SendHaptic(rightInteractor);
        }
    }

    private bool IsGripping(XRBaseInputInteractor interactor)
    {
        if (interactor == null) return false;
        return interactor.isSelectActive;
    }

    private void SendHaptic(XRBaseInputInteractor interactor)
    {
        if (interactor == null) return;
        interactor.SendHapticImpulse(hapticAmplitude, hapticDuration);
    }
}