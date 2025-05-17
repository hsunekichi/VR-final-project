using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using GlobalVars;

public class VRButton : MonoBehaviour
{
    UIController arrowUI;
    [SerializeField]
    private AudioSource audioSource;

    void Start()
    {
        arrowUI = GameObject.Find("Canvas").GetComponent<UIController>();
    }

    public void OnButtonPressed()
    {
        audioSource.Play();
        global.arrowCount += 10;
        if (global.arrowCount > 99) global.arrowCount = 99;
        arrowUI.SetArrowCount();
    }
}