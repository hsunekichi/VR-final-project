using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treasureController : MonoBehaviour
{
    public GameObject player;
    public float rotationSpeed;
    public AnimationCurve updownSpeed;
    public float maxTpDistance;

    public float updownSpeedModifier;
    public float maxUpdownFactor;
    public float maxUpdownDistance;
    float init_y_t;
    float updown_sign = 1.0f;
    Vector3 init_pos;

    // Start is called before the first frame update
    void Start()
    {
        init_y_t = Time.time;
        init_pos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float d = Vector3.Distance(player.transform.position, transform.position);
        if (d < 15.0f)
        {
            // Start rotation in global up axis
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);

            float updownFactor = Mathf.Min(1 + (updownSpeedModifier / Mathf.Max(1.0f, d * d)), maxUpdownFactor);
            float yDistance = transform.position.y - init_pos.y;


            // IF animation curve finished
            if (Time.time - init_y_t > updownSpeed.keys[updownSpeed.length - 1].time)
            {
                init_y_t = Time.time;
                updown_sign *= -1.0f;
            }

            float speed = updownSpeed.Evaluate(Time.time - init_y_t);
            //float speed = 0.5f;
            transform.Translate(Vector3.up * speed * updown_sign * updownFactor * Time.deltaTime);
        }

        if (d < 2.0f)
        {
            // Teleport random distance on x
            float x = Random.Range(-maxTpDistance, maxTpDistance);
            float z = Random.Range(-maxTpDistance, maxTpDistance);

            transform.position = new Vector3(init_pos.x + x, init_pos.y, init_pos.z + z);
        }

    }
}
