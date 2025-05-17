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

    [SerializeField]
    public TextMeshProUGUI heartText;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = arrowText.transform.localScale;
        SetArrowCount();
        SetHeartCount();
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

    public void SetHeartCount()
    {
        heartText.text = $"x {global.HP}";
        StartCoroutine(AnimateHeart());
    }

    private IEnumerator AnimateHeart()
    {
        heartText.transform.localScale = originalScale * 1.3f;
        heartText.color = Color.yellow;

        yield return new WaitForSeconds(0.2f);

        heartText.transform.localScale = originalScale;
        heartText.color = Color.white;
    }
}

