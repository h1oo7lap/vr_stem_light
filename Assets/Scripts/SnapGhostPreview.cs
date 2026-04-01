using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class SnapGhostPreview : MonoBehaviour
{
    [Header("References")]
    public RingSnapManager snapManager;

    [Header("Ghost Settings")]
    public Material ghostMaterial;
    public float ghostScale = 1f;

    private XRGrabInteractable grabInteractable;
    private GameObject ghostObject;
    private MeshFilter sourceMeshFilter;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        sourceMeshFilter = GetComponent<MeshFilter>();
    }

    private void OnEnable()
    {
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
        HideGhost();
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        CreateGhost();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        HideGhost();
    }

    private void Update()
    {
        if (!grabInteractable.isSelected) return;
        if (snapManager == null || ghostObject == null) return;

        if (snapManager.TryGetSnapPosition(transform.position, out Vector3 snapPos))
        {
            ghostObject.SetActive(true);
            ghostObject.transform.position = snapPos;
        }
        else
        {
            ghostObject.SetActive(false);
        }
    }

    private void CreateGhost()
    {
        if (ghostObject != null) return;
        if (sourceMeshFilter == null || sourceMeshFilter.sharedMesh == null) return;

        ghostObject = new GameObject("GhostPreview");

        var mf = ghostObject.AddComponent<MeshFilter>();
        mf.sharedMesh = sourceMeshFilter.sharedMesh;

        var mr = ghostObject.AddComponent<MeshRenderer>();
        mr.material = ghostMaterial;

        ghostObject.transform.localScale = transform.localScale * ghostScale;
        ghostObject.SetActive(false);
    }

    private void HideGhost()
    {
        if (ghostObject != null)
        {
            Destroy(ghostObject);
            ghostObject = null;
        }
    }
}