using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;

public class WallController : MonoBehaviour
{
    public float maxDistance = global.maxDistance;
    private MeshRenderer meshRenderer;
    private Transform mainCamera;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, mainCamera.position);
        if (meshRenderer != null)
        {
            meshRenderer.enabled = distance <= maxDistance;
        }
    }
}
