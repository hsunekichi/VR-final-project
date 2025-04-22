using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Bow : MonoBehaviour
{
    public Transform bowHand;
    public Transform pullHand;
    public Transform stringAttachPoint;
    public float maxForce = 100f;
    public float minForce = 10f;
    private GameObject currentArrow;
    private bool arrowIsAttached = false;

    private XRGrabInteractable arrowGrabbed;

    private void OnTriggerEnter(Collider other)
    {
        if (arrowIsAttached && currentArrow != null) return;

        var arrow = other.GetComponent<XRGrabInteractable>();
        if (arrow)
        {
            currentArrow = arrow.gameObject;
            arrowGrabbed = arrow;
            arrowGrabbed.trackPosition = false;
            arrowGrabbed.trackRotation = false;
            currentArrow.transform.SetParent(stringAttachPoint);
            currentArrow.transform.localPosition = Vector3.zero;
            currentArrow.transform.localRotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (currentArrow && arrowGrabbed && !arrowGrabbed.isSelected)
        {
            FireArrow();
        }
    }

    private void FireArrow()
    {
        Rigidbody rb = currentArrow.GetComponent<Rigidbody>();
        float pullDistance = Vector3.Distance(bowHand.position, pullHand.position);
        Vector3 force = stringAttachPoint.forward * (pullDistance * maxForce);

        currentArrow.transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.Impulse);
        rb.useGravity = true;
        currentArrow = null;
        arrowIsAttached = false;
        arrowGrabbed = null;
        currentArrow = null;
    }
}