using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;

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

    void Start()
    {
        arrowUI = GameObject.Find("Canvas").GetComponent<UIController>();
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
            }
            else
            {
                arrow.GetComponent<PlaySound>().play(false);
            }
        } else
        {
            collision.gameObject.GetComponent<Damageable>().Damage(1.0f);
            global.arrowCount++;
            arrowUI.SetArrowCount();
        }

        Destroy(gameObject);

    }
}