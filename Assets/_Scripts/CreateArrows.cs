using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;
using UnityEngine.XR.Interaction.Toolkit;

public class CreateArrows : XRGrabInteractable
{
    [SerializeField]
    public GameObject objectToSpawn;  // El prefab que quieres instanciar

    //private XRGrabInteractable grabInteractable;
    //private XRInteractionManager interactionManager;

    //private void Start()
    //{
    //    //grabInteractable = GetComponent<XRGrabInteractable>();
    //    //grabInteractable.onSelectEntered.AddListener(OnObjectSelected);

    //    // Obtener el Interaction Manager
    //    //interactionManager = FindObjectOfType<XRInteractionManager>();
    //}

    // Este método se ejecuta cuando el objeto es seleccionado

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (global.arrowCount > 0)
        {
            // Crear la nueva instancia
            GameObject newObject = Instantiate(objectToSpawn, transform.position + Vector3.up * 0.5f, Quaternion.Euler(0, 0, 0));

            XRGrabInteractable newGrabInteractable = newObject.GetComponent<XRGrabInteractable>();


            if (newGrabInteractable != null)
            {
                GameObject attachPoint = new GameObject("AttachPoint");
                attachPoint.transform.SetParent(newObject.transform);
                attachPoint.transform.localPosition = Vector3.zero;
                attachPoint.transform.localRotation = Quaternion.Euler(0, 90, 0);

                newGrabInteractable.attachTransform = attachPoint.transform;

                // Hacer que el interactor libere el objeto actual (opcional si quieres que suelte el original)
                interactionManager.SelectExit(args.interactorObject, args.interactableObject);

                // Forzar que el interactor seleccione la nueva instancia
                interactionManager.SelectEnter(args.interactorObject, newGrabInteractable);
            }
        }
        else
        {
            interactionManager.SelectExit(args.interactorObject, args.interactableObject);
        }
    }
}