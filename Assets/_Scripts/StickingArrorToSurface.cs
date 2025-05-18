using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;
using UnityEngine.XR.Interaction.Toolkit;

public class StickingArrowToSurface : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private SphereCollider myCollider;

    [SerializeField]
    private GameObject stickingArrow;

    [SerializeField]
    public AudioSource hit, miss;

    public UIController arrowUI;

    private XRBaseController Lcontroller, Rcontroller;
    [SerializeField] private float amplitude = 0.5f;      // Intensidad [0.0 - 1.0]
    [SerializeField] private float duration = 0.5f;       // En segundos

    void Start()
    {
        arrowUI = GameObject.Find("Canvas").GetComponent<UIController>();
        Lcontroller = GameObject.Find("Left Hand").GetComponent<XRBaseController>();
        Rcontroller = GameObject.Find("Right Hand").GetComponent<XRBaseController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Enemy")
        {
            rb.isKinematic = true;
            myCollider.isTrigger = true;

            GameObject arrow = Instantiate(stickingArrow);
            arrow.transform.position = transform.position;
            arrow.transform.forward = transform.forward;
            if (!global.training) arrow.transform.localScale = new Vector3(global.scale, global.scale, global.scale);
            arrow.transform.GetChild(0).localPosition = transform.GetChild(0).localPosition;
            arrow.transform.GetChild(0).localRotation = transform.GetChild(0).localRotation;

            if (collision.collider.attachedRigidbody != null)
            {
                arrow.transform.parent = collision.collider.attachedRigidbody.transform;
            }

            collision.collider.GetComponent<IHittable>()?.GetHit();

            if (collision.gameObject.tag == "Objective")
            {
                arrow.GetComponent<PlaySound>().play(true);
                Lcontroller.SendHapticImpulse(amplitude, duration);
                Rcontroller.SendHapticImpulse(amplitude, duration);
            }
            else
            {
                arrow.GetComponent<PlaySound>().play(false);
            }
        } else
        {
            Lcontroller.SendHapticImpulse(amplitude, duration);
            Rcontroller.SendHapticImpulse(amplitude, duration);
            collision.gameObject.GetComponent<Damageable>().Damage(1.0f);
            global.arrowCount++;
            arrowUI.SetArrowCount();
        }

        Destroy(gameObject);

    }
}