using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemSpawner : MonoBehaviour
{
    // Properties that can be changed from the Inspetor tab
    public GameObject ItemToSpawn; // Item to be spawned
    Vector3 center => transform.position; // Center of the cube to spawn
    public Vector3 SpawnSize; // Size of the cube to spawn
    public float CameraRange;
    public float SpawnRate;

    void Awake()
    {
        SpawnItem();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Q) && CanSpawn) // When Q is pressed, an item is spawned
            SpawnItem();
    }
    public void SpawnItem()
    {
        // Create new gameObject
        GameObject item = Instantiate(ItemToSpawn, center + new Vector3(Random.Range(-SpawnSize.x / 2, SpawnSize.x / 2), Random.Range(-SpawnSize.y / 2, SpawnSize.y / 2), Random.Range(-SpawnSize.z / 2, SpawnSize.z / 2)), Quaternion.identity);
        lastSpawnTime = Time.time;
    }

    // Debug funcion. In the Scene tab you can see a Red Box, which is the
    // volume where the object is going to be spawned.
    void OnDrawGizmosSelected()
    {
        // Draw one gizmos for the spawn volume and another for the camera range
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(center, SpawnSize);
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireSphere(center, CameraRange);
    }

    float lastSpawnTime;
    bool CanSpawn => CameraInRange && CooldownIsOver;
    bool CameraInRange => Vector3.Distance(Camera.main.transform.position, center) < CameraRange;
    bool CooldownIsOver => Time.time - lastSpawnTime > 1.0f/SpawnRate;
}
