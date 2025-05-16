using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CollisionWithBowHandler : MonoBehaviour
{

    private XRBaseInteractor currentInteractor;
    private XRInteractionManager interactionManager;
    private ArrowGrabbed thisGrabInteractable;

    [SerializeField]
    Rigidbody rb;

    float collisionCheckDistance;

    private void Awake()
    {
        thisGrabInteractable = this.gameObject.GetComponent<ArrowGrabbed>();
    }

    public void SetInteractor(XRBaseInteractor interactor, XRInteractionManager manager)
    {
        currentInteractor = interactor;
        interactionManager = manager;
    }

    public void ClearInteractor()
    {
        currentInteractor = null;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Bow") && currentInteractor != null)
        {
            thisGrabInteractable.beenReleased = false;
            // Forzar el cambio de selección
            interactionManager.SelectExit(currentInteractor, thisGrabInteractable);

            interactionManager.SelectEnter(currentInteractor, collision.transform.Find("MidPointParent").Find("MidPointGrabObject").GetComponent<XRGrabInteractable>());
            this.gameObject.SetActive(false);

            Destroy(gameObject);
        }
    }

}
