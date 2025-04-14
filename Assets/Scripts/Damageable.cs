using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    // Event to notify when seen
    public float InvencibilityTime = 0;
    public UnityEvent Damaged;

    float InvencibilityStartTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Start without invencibility
        InvencibilityStartTime = -InvencibilityTime;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage()
    {
        float currentTime = Time.time;

        if (currentTime > InvencibilityStartTime + InvencibilityTime)
        {
            Damaged.Invoke();
            InvencibilityStartTime = currentTime;
        }
    }
}
