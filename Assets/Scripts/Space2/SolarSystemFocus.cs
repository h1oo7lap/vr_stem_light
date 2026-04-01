using UnityEngine;

public class SolarSystemFocus : MonoBehaviour
{
    public Transform solarRoot;

    public float zoomSpeed = 2f;
    public float targetScale = 10f;
    public float normalScale = 1f;

    Transform pivot;

    bool zoomingIn;
    bool zoomingOut;

    Vector3 pivotOffset;

    public OvalRing[] orbits;

    public void FocusPlanet(Transform planet)
    {
        pivot = planet;

        pivotOffset = solarRoot.position - planet.position;

        zoomingIn = true;
        zoomingOut = false;
    }

    public void ZoomOut()
    {
        zoomingOut = true;
        zoomingIn = false;
    }

    void Update()
    {
        if (zoomingIn)
        {
            Zoom(targetScale);
        }

        if (zoomingOut)
        {
            Zoom(normalScale);
        }
    }

    void Zoom(float target)
    {
        float current = solarRoot.localScale.x;

        float newScale = Mathf.Lerp(current, target, Time.deltaTime * zoomSpeed);

        solarRoot.localScale = Vector3.one * newScale;

        if (pivot != null)
        {
            solarRoot.position = pivot.position + pivotOffset * newScale;
        }

        UpdateOrbitVisibility(newScale);
    }

    void UpdateOrbitVisibility(float scale)
    {
        float t = Mathf.InverseLerp(1f, targetScale, scale);

        foreach (var orbit in orbits)
        {
            orbit.SetVisibility(1f - t);
        }
    }
}