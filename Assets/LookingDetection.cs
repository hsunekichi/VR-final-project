using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingDetection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Cast ray from the camera viewing direction
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(0.5f, 0.5f, 0));
    }
}
