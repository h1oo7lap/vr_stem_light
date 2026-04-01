using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CustomKnob : XRBaseInteractable
{
    [Header("Handle")]
    public Transform handle;

    [Header("Rotation")]
    public float minAngle = -90f;
    public float maxAngle = 90f;

    [Header("Zoom Range")]
    public float minZoom = 1f;
    public float maxZoom = 50f;

    [Header("Current Value")]
    [Range(0,1)]
    public float value = 0.5f;

    [System.Serializable]
    public class ZoomEvent : UnityEvent<float>{}
    public ZoomEvent onZoomChanged;

    IXRSelectInteractor interactor;

    void Start()
    {
        UpdateKnob();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        selectEntered.AddListener(StartGrab);
        selectExited.AddListener(EndGrab);
    }

    protected override void OnDisable()
    {
        selectEntered.RemoveListener(StartGrab);
        selectExited.RemoveListener(EndGrab);
        base.OnDisable();
    }

    void StartGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject;
    }

    void EndGrab(SelectExitEventArgs args)
    {
        interactor = null;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && interactor != null)
        {
            UpdateRotation();
        }
    }

    void UpdateRotation()
    {
        Transform t = interactor.GetAttachTransform(this);

        Vector3 dir = t.position - transform.position;
        dir = transform.InverseTransformDirection(dir);

        float angle = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;

        float tValue = Mathf.InverseLerp(minAngle, maxAngle, angle);
        value = Mathf.Clamp01(tValue);

        UpdateKnob();
    }

    void UpdateKnob()
    {
        float angle = Mathf.Lerp(minAngle, maxAngle, value);

        if (handle != null)
            handle.localEulerAngles = new Vector3(0, angle, 0);

        float zoom = Mathf.Lerp(minZoom, maxZoom, value);

        onZoomChanged?.Invoke(zoom);
    }

    public float GetZoom()
    {
        return Mathf.Lerp(minZoom, maxZoom, value);
    }
}