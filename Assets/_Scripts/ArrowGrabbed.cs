using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GlobalVars;

public class ArrowGrabbed : XRGrabInteractable
{
    //private XRInteractionManager interactionManager;
    private CollisionWithBowHandler collisionHandler;

    [SerializeField]
    private GameObject arrowPrefab;

    public UIController arrowUI;

    public bool beenReleased;

    protected override void Awake()
    {
        base.Awake();
        collisionHandler = this.gameObject.GetComponent<CollisionWithBowHandler>();
        arrowUI = GameObject.Find("Canvas").GetComponent<UIController>();
        beenReleased = true;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (collisionHandler != null)
        {
            collisionHandler.SetInteractor(args.interactorObject as XRBaseInteractor, interactionManager);
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        this.gameObject.SetActive(false);

        if (beenReleased)
        {
            GameObject arrow = Instantiate(arrowPrefab);
            arrow.transform.position = transform.position;
            arrow.transform.forward = transform.forward;
            arrow.transform.GetChild(0).localPosition = transform.GetChild(0).localPosition;
            arrow.transform.GetChild(0).localRotation = transform.GetChild(0).localRotation;
            //arrow.transform.Rotate(90.0f, -90.0f, 0.0f);

            global.arrowCount--;

            arrowUI.SetArrowCount();
        }


        if (collisionHandler != null)
        {
            collisionHandler.ClearInteractor();
        }
        Destroy(gameObject);
    }
}
