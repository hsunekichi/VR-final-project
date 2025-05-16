using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    public float maxDistance = 17f;
    private Light lightComponent;
    private Transform mainCameraTransform;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        if (mainCameraTransform == null || lightComponent == null)
            return;

        float distance = Vector3.Distance(transform.position, mainCameraTransform.position);
        lightComponent.enabled = distance <= maxDistance;
    }
}
