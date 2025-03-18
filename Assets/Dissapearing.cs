using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disappearing : MonoBehaviour
{
    public float MaxDistance;
    private MeshRenderer meshRenderer;
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        float transparency = Mathf.Min(1, distance / MaxDistance); // 10 is the maximum distance

        // Calculate transparency depending on distance
        // You have to select the property name, not the display name
        meshRenderer.material.SetFloat("_Transparency", transparency);
    }
}