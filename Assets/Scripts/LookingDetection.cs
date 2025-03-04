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
        // Cast ray from the center of the camera
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // If the ray hits something
        if (Physics.Raycast(ray, out hit))
        {
            // If the object hit is a cube
            ObjectSeenFromCamera component = hit.collider.gameObject.GetComponent<ObjectSeenFromCamera>();
            if (component != null)
            {
                // Call the function in the ObjectSeenFromCamera script
                component.LookedAt();
            }
        }
    }
}
