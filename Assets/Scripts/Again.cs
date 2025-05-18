using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;
using UnityEngine.SceneManagement;

public class Again : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void changeScene()
    {
        audioSource.Play();
        global.arrowCount = 20;
        global.training = true;
        global.HP = 10.0f;
        StartCoroutine(WaitAndLoadScene());
    }

    private IEnumerator WaitAndLoadScene()
    {
        // Espera mientras el audio esté reproduciéndose
        yield return new WaitWhile(() => audioSource.isPlaying);

        // Carga la escena
        SceneManager.LoadScene("ArcheryScene", LoadSceneMode.Single);
    }
}