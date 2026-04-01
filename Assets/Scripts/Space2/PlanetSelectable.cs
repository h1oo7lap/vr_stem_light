using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlanetSelectable : MonoBehaviour
{
    public OvalRing orbit;
    public SolarSystemFocus focusSystem;
    public float originScale;

    private void Awake()
    {
        originScale = transform.localScale.x;
    }

    void Start()
    {
        GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>().selectEntered.AddListener(OnSelect);
    }

    void OnSelect(SelectEnterEventArgs args)
    {
        focusSystem.FocusPlanet(transform);
    }

    private void Update()
    {
        transform.position = orbit.GetPoint(0);
        
        //Scale
        float orbitRadius = orbit.GetRadius();
        float size = orbitRadius * originScale * 0.001f;
        transform.localScale = Vector3.one * size;
    }
}