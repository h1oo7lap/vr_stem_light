using UnityEngine;

public enum ZoomState
{
    Overview,
    Focus,
    Detail
}

public class ZoomStateManager : MonoBehaviour
{
    public static ZoomStateManager Instance { get; private set; }

    public ZoomState CurrentState { get; private set; } = ZoomState.Overview;

    [Header("Scale Thresholds")]
    public float focusMinScale = 1.5f;
    public float detailMinScale = 3f;

    [Header("References")]
    public Transform solarSystemRoot;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (solarSystemRoot == null) return;

        float currentScale = solarSystemRoot.localScale.x;

        ZoomState newState;

        if (currentScale >= detailMinScale)
            newState = ZoomState.Detail;
        else if (currentScale >= focusMinScale)
            newState = ZoomState.Focus;
        else
            newState = ZoomState.Overview;

        if (newState != CurrentState)
        {
            CurrentState = newState;
            Debug.Log($"[ZoomState] → {newState}");
        }
    }

    public bool IsInState(ZoomState state) => CurrentState == state;

    public void SetState(ZoomState newState)
    {
        CurrentState = newState;
        Debug.Log($"[ZoomState] → {newState}");
    }
}