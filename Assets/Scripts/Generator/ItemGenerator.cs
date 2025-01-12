using System.Collections;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    [SerializeField] private GameObject wingedShoePrefab;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject magnetPrefab;

    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        SpawnItem();
    }

    private void SpawnItem()
    {
        ItemType randomItemType = (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length);

        // Select a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instantiate the corresponding item prefab
        GameObject itemPrefab = null;
        switch (randomItemType)
        {
            case ItemType.WingedShoe:
                itemPrefab = wingedShoePrefab;
                break;
            case ItemType.Shield:
                itemPrefab = shieldPrefab;
                break;
            case ItemType.Magnet:
                itemPrefab = magnetPrefab;
                break;
        }

        if (itemPrefab != null)
        {
            Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
