using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Quiver : MonoBehaviour
{
    public GameObject arrowPrefab;
    public Transform spawnPoint;
    // Start is called before the first frame update
    void Start()
    {
        //por si limitamos el número de arrows
    }
    private void OnTriggerEnter(Collider other)
    {
        var interactor = other.GetComponent<XRBaseControllerInteractor>();
        if (interactor && interactor.selectTarget == null)
        {
            GameObject arrow = Instantiate(arrowPrefab, spawnPoint.position, spawnPoint.rotation);
            XRGrabInteractable grab = arrow.GetComponent<XRGrabInteractable>();
            grab.interactionManager = FindObjectOfType<XRInteractionManager>();

            // Optional: Force grab it
            interactor.interactionManager.SelectEnter(interactor, grab);
        }
    }
}