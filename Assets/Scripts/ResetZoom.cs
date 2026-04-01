using UnityEngine;
using UnityEngine.InputSystem;

public class ResetZoom : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference buttonB;

    [Header("References")]
    public Transform solarSystemRoot;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = solarSystemRoot.localScale;
    }

    private void OnEnable()
    {
        if (buttonB != null)
            buttonB.action.performed += OnButtonB;
    }

    private void OnDisable()
    {
        if (buttonB != null)
            buttonB.action.performed -= OnButtonB;
    }

    private void OnButtonB(InputAction.CallbackContext ctx)
    {
        solarSystemRoot.localScale = originalScale;

        if (ZoomStateManager.Instance != null)
            ZoomStateManager.Instance.SetState(ZoomState.Overview);

        Debug.Log("[ResetZoom] Reset về Overview!");
    }
}