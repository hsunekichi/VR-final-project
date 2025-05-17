using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField]
    GameObject[] EnemyPrefabs;

    public float EnemyScale = 1.0f;
    Vector3 center => transform.position;
    public Vector3 SpawnSize;
    public float SpawnRate;
    public int MaxAliveEnemies;
    public int TotalEnemiesLimit;

    public int totalEnemiesSpawned = 0;

    // Nueva referencia y distancia mínima
    public Transform ReferenceObject;
    public float MinDistanceToReference = 5f;
    private bool completedRoom = false;
    private UIController arrowUI;

    void Start()
    {
        arrowUI = GameObject.Find("Canvas").GetComponent<UIController>();
    }

    void Update()
    {
        if (CanSpawn)
        {
            SpawnItem();
        }
        else
        {
            if (!completedRoom && totalEnemiesSpawned >= TotalEnemiesLimit && CurrentEnemies == 0)
            {
                global.arrowCount += 20;
                if (global.arrowCount > 99) global.arrowCount = 99;
                arrowUI.SetArrowCount();

                completedRoom = true;
            }
        }
    }

    public void SpawnItem()
    {
        //Debug.Log(CurrentEnemies.ToString() + " " + totalEnemiesSpawned.ToString() + " " + TotalEnemiesLimit.ToString());
        if (totalEnemiesSpawned >= TotalEnemiesLimit)
        {
            return;
        }

        // Verifica que ReferenceObject esté dentro de SpawnSize
        if (ReferenceObject != null)
        {
            Vector3 localPos = ReferenceObject.position - center;
            Vector3 halfSize = SpawnSize / 2f;
            if (Mathf.Abs(localPos.x) > halfSize.x ||
                Mathf.Abs(localPos.y) > halfSize.y ||
                Mathf.Abs(localPos.z) > halfSize.z)
            {
                // ReferenceObject está fuera del área de SpawnSize
                return;
            }
        }

        Vector3 spawnPos;
        int attempts = 0;
        const int maxAttempts = 20;

        var spawnSizeBorder = SpawnSize - new Vector3(0.5f, 0.7f, 0.5f);

        do
        {
            spawnPos = center + new Vector3(
                Random.Range(-spawnSizeBorder.x / 2, spawnSizeBorder.x / 2),
                Random.Range(-spawnSizeBorder.y / 2, spawnSizeBorder.y / 2),
                Random.Range(-spawnSizeBorder.z / 2, spawnSizeBorder.z / 2)
            );
            attempts++;
        }
        while (ReferenceObject != null && Vector3.Distance(spawnPos, ReferenceObject.position) < MinDistanceToReference && attempts < maxAttempts);

        if (ReferenceObject != null && Vector3.Distance(spawnPos, ReferenceObject.position) < MinDistanceToReference)
            return; // No se encontró una posición válida


        GameObject prefabToUse = null;
        if (EnemyPrefabs != null && EnemyPrefabs.Length > 0)
        {
            var idx = Random.Range(0, EnemyPrefabs.Length);
            prefabToUse = EnemyPrefabs[idx];
        }

        GameObject item = Instantiate(prefabToUse, spawnPos, Quaternion.identity);
        item.transform.localScale = Vector3.one * EnemyScale;
        var ghostScript = item.GetComponent<Sample.GhostScript>();
        if (ghostScript != null && ghostScript.moveSettings != null)
        {
            ghostScript.moveSettings.AttackDistance *= EnemyScale;
            ghostScript.moveSettings.SpeedToTarget *= EnemyScale;
        }

        lastSpawnTime = Time.time;
        totalEnemiesSpawned++;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(center, SpawnSize);
        Gizmos.color = new Color(0, 1, 0, 0.5f);
    }

    float lastSpawnTime;
    bool CanSpawn => CooldownIsOver && CurrentEnemies < MaxAliveEnemies && totalEnemiesSpawned < TotalEnemiesLimit;
    bool CooldownIsOver => Time.time - lastSpawnTime > 1.0f / SpawnRate;
    int CurrentEnemies => GameObject.FindGameObjectsWithTag("Enemy").Length;
}
