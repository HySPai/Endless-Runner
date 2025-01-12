using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject npcToSpawn;
    [SerializeField] private Transform spawnLocation;
    [SerializeField] private int chanceToSpawn;

    private bool hasSpawned = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(hasSpawned && collision.tag == "Player")
        {
            int randomRoll = Random.Range(0, 100);

            if(randomRoll < chanceToSpawn)
            {
                Instantiate(npcToSpawn, spawnLocation.position, transform.rotation);
                Time.timeScale = 0.6f;
                hasSpawned = false;
            }
        }
    }
}
