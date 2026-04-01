using UnityEngine;
using UnityEngine.InputSystem;

public class ThumbstickZoom : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference rightThumbstick;

    [Header("Zoom Settings")]
    public float zoomSpeed = 0.5f;
    public float minScale = 0.1f;
    public float maxScale = 5f;

    [Header("References")]
    public Transform solarSystemRoot;

    private void Update()
    {
        // Chỉ hoạt động khi Focus hoặc Detail
        if (ZoomStateManager.Instance == null) return;
        if (ZoomStateManager.Instance.IsInState(ZoomState.Overview)) return;

        if (rightThumbstick == null || solarSystemRoot == null) return;

        float input = rightThumbstick.action.ReadValue<Vector2>().y;

        if (Mathf.Abs(input) > 0.1f)
        {
            float newScale = solarSystemRoot.localScale.x + input * zoomSpeed * Time.deltaTime;
            newScale = Mathf.Clamp(newScale, minScale, maxScale);
            solarSystemRoot.localScale = Vector3.one * newScale;
        }
    }
}