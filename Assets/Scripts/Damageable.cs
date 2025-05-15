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
    public float HP = 100.0f;

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

    public void Damage(float damage=0.0f)
    {
        float currentTime = Time.time;

        if (currentTime > InvencibilityStartTime + InvencibilityTime)
        {
            // Apply damage
            HP -= damage;
            Damaged.Invoke();
            InvencibilityStartTime = currentTime;
        }
    }
}
