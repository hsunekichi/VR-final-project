using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceRotation : MonoBehaviour
{
    public Transform targetCamera;
    // Start is called before the first frame update
    void Start()
    {
        //transform.rotation = targetCamera.rotation;
        //transform.lookAt(targetCamera.rotation);
        //transform.Rotate(0.0f, 180.0f, 0.0f);
        transform.rotation = Quaternion.LookRotation(targetCamera.forward);
    }
}
