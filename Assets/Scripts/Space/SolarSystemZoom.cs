using UnityEngine;

public class SolarSystemZoom : MonoBehaviour
{
    public CustomKnob knob;
    public Transform solarSystemRoot;

    void Start()
    {
        knob.onZoomChanged.AddListener(UpdateZoom);
    }

    void UpdateZoom(float zoom)
    {
        solarSystemRoot.localScale = Vector3.one * zoom;
    }
}
