using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;
using UnityEngine.XR.Interaction.Toolkit;

public class ArrowController : MonoBehaviour
{
    [SerializeField]
    private GameObject midPointVisual, arrowPrefab, arrowSpawnPoint;

    [SerializeField]
    private float arrowMaxSpeed = 10;

    [SerializeField]
    private AudioSource bowReleaseAudioSource;

    public UIController arrowUI;

    void Start()
    {
        arrowUI = GameObject.Find("Canvas").GetComponent<UIController>();
    }

    public void PrepareArrow()
    {
        if (global.arrowCount > 0) {
            midPointVisual.SetActive(true);
        }
    }

    public void ReleaseArrow(float strength)
    {
        bowReleaseAudioSource.Play();
        if (global.arrowCount > 0)
        {
            midPointVisual.SetActive(false);

            GameObject arrow = Instantiate(arrowPrefab);
            arrow.transform.position = arrowSpawnPoint.transform.position;
            arrow.transform.rotation = midPointVisual.transform.rotation;
            if (!global.training) arrow.transform.localScale = new Vector3(global.scale, global.scale, global.scale);
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            rb.AddForce(midPointVisual.transform.forward * strength * arrowMaxSpeed, ForceMode.Impulse);
            global.arrowCount--;

            arrowUI.SetArrowCount();
        }
    }
}