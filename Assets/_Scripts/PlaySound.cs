using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField]
    AudioSource hit, miss;

    public void play(bool isHit)
    {
        if (isHit)
        {
            hit.Play();

        }
        else
        {
            miss.Play();
        }
    }
}
