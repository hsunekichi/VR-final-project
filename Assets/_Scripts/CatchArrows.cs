using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;
using UnityEngine.XR.Interaction.Toolkit;

public class SingleUseGrabWithDelay : XRGrabInteractable
{
    private bool hasBeenUsed = false;

    public UIController arrowUI;

    void Start()
    {
        arrowUI = GameObject.Find("Canvas").GetComponent<UIController>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (hasBeenUsed) return;

        hasBeenUsed = true;

        colliders[0].enabled = false;
        interactionManager.UnregisterInteractable(this);

        global.arrowCount++;
        arrowUI.SetArrowCount();

        StartCoroutine(DestroySafely());
    }

    private IEnumerator DestroySafely()
    {
        yield return null; // Espera un frame completo
        Destroy(gameObject);
    }
}
