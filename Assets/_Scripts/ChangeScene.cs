using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Arrow"))
        {
            audioSource.Play();
            global.arrowCount = 20;
            global.training = false;
            SceneManager.LoadScene("Dungeon3D", LoadSceneMode.Single);
        }
    }
}