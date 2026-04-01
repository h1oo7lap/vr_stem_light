using System;
using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    public static BillboardUI Instance;
    public Transform playerCamera;

    public float minScale = 0.01f;
    private void Awake()
    {
        Instance = this;
    }

    void LateUpdate()
    {
        transform.LookAt(playerCamera);
        transform.Rotate(0,180,0);
    }
}