using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrManagerWolf : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public float spawnInterval;
    
    private Vector2 spawnAreaMin; // Minimum spawn area
    private Vector2 spawnAreaMax; // Maximum spawn area
    
    private float timeSinceLastSpawn;
    
    // Start is called before the first frame update
    void Start()
    {
        spawnAreaMin = GetComponent<BoxCollider2D>().bounds.min;
        spawnAreaMax = GetComponent<BoxCollider2D>().bounds.max;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnPrefab();
            timeSinceLastSpawn = 0f;
        }
    }
    
    void SpawnPrefab()
    {
        // Generate a random position within the spawn area
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomY = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        Vector2 spawnPosition = new Vector2(randomX, randomY);

        // Instantiate the prefab at the random position
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        print("Instantiated a Wolf");
    }
}
