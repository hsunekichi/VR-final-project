using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullScreenShaderController : MonoBehaviour
{
    [Header("Time Stats")]
    [SerializeField] private float _hurtDisplayTime = 1.5f;
    [SerializeField] private float _hurtFadeOutTime = 0.5f;

    [Header("References")]
    [SerializeField] private ScriptableRendererFeature _fullScreenDamage;
    [SerializeField] private Material _material;

    [Header("Intensity Stats")]
    [SerializeField] private float _voronoiIntensityStat = 2.5f;
    [SerializeField] private float _vignetteIntensityStat = 2.5f;

    private int _voronoiIntensity = Shader.PropertyToID("_VoronoiIntensity");
    private int _vignetteIntensity = Shader.PropertyToID("_VignetteIntensity");

    private void Start()
    {
        _fullScreenDamage.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            getHurt();
        }
    }

    public void getHurt()
    {
        StartCoroutine(Hurt());
    }

    private IEnumerator Hurt()
    {
        _fullScreenDamage.SetActive(true);
        _material.SetFloat(_voronoiIntensity, _voronoiIntensityStat);
        _material.SetFloat(_vignetteIntensity, _vignetteIntensityStat);

        yield return new WaitForSeconds(_hurtDisplayTime);

        float elapsedTime = 0f;
        while (elapsedTime < _hurtFadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _hurtFadeOutTime);
            _material.SetFloat(_voronoiIntensity, Mathf.Lerp(_voronoiIntensityStat, 0f, t));
            _material.SetFloat(_vignetteIntensity, Mathf.Lerp(_vignetteIntensityStat, 0f, t));
            yield return null;
        }

        _fullScreenDamage.SetActive(false);
    }
}
