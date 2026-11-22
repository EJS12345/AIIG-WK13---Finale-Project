using UnityEngine;

public class PopulationSpawner : MonoBehaviour
{
    [Header("100-Entity Simulation Setup")]
    public GameObject[] villagerPrefabs; 
    public int totalEntities = 100;      
    public Vector2 spawnArea = new Vector2(60f, 60f);

    void Start()
    {
        if (villagerPrefabs == null || villagerPrefabs.Length == 0)
        {
            Debug.LogError("PopulationSpawner: Please assign Villager Prefabs in the Inspector!");
            return;
        }

        for (int i = 0; i < totalEntities; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f),
                0f,
                Random.Range(-spawnArea.y / 2f, spawnArea.y / 2f)
            );

            // Cycle through agent types (Logger, WoodCutter, Miner, Blacksmith)
            GameObject selectedPrefab = villagerPrefabs[i % villagerPrefabs.Length];
            GameObject agent = Instantiate(selectedPrefab, randomPos, Quaternion.identity, transform);
            agent.name = selectedPrefab.name + "_" + (i + 1);
        }

        Debug.Log("<color=green>GOAP Simulation Started: 100 Entities Spawned.</color>");
    }
}