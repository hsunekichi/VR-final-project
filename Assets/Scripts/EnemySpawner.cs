using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject EnemyPrefab;
    Vector3 center => transform.position;
    public Vector3 SpawnSize;
    public float SpawnRate;
    public int MaxAliveEnemies;
    public int TotalEnemiesLimit;

    private int totalEnemiesSpawned = 0;

    // Nueva referencia y distancia mínima
    public Transform ReferenceObject;
    public float MinDistanceToReference = 5f;

    void Update()
    {
        if (CanSpawn)
            SpawnItem();
    }

    public void SpawnItem()
    {
        if (totalEnemiesSpawned >= TotalEnemiesLimit)
            return;

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

        GameObject item = Instantiate(EnemyPrefab, spawnPos, Quaternion.identity);
        //item.transform.localScale = Vector3.one * 0.25f;
        
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
