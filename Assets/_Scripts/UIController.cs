using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GlobalVars;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI arrowText;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = arrowText.transform.localScale;
        SetArrowCount();
    }

    public void SetArrowCount()
    {
        arrowText.text = $"x {global.arrowCount}";
        StartCoroutine(AnimateText());
    }

    private IEnumerator AnimateText()
    {
        arrowText.transform.localScale = originalScale * 1.3f;
        arrowText.color = Color.yellow;

        yield return new WaitForSeconds(0.2f);

        arrowText.transform.localScale = originalScale;
        arrowText.color = Color.white;
    }
}

