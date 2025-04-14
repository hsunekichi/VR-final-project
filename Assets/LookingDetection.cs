using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookingDetection : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    void rayAttack()
    {
        // Cast ray from the center of the camera
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        // If the ray hits something
        if (Physics.Raycast(ray, out hit))
        {
            // If the object hit is a cube
            Damageable component = hit.collider.gameObject.GetComponent<Damageable>();
            if (component != null) {
                component.Damage();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check mouse is pressed
        if (Input.GetMouseButtonDown(0)) {
            rayAttack();
        }
    }
}
