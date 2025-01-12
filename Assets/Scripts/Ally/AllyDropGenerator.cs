using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyDropGenerator : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private Vector2 dropDirection;

    [SerializeField] private float dropCooldown;
    [SerializeField] private float lastDropTime;

    private Enemy enemy;

    private bool canDrop = false;

    private bool dropCoinDie = true;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Update()
    {
        if (enemy.characterStop)
        {
            if (canDrop)
            {
                if (Time.time > lastDropTime + dropCooldown)
                {
                    lastDropTime = Time.time;
                    dropCoin();
                }
            }
        }

        if (enemy.canDie)
        {
            canDrop = false;
        }

        if (enemy.canDie && dropCoinDie)
        {
            enemy.dropSpike();
            dropCoinDie = false;
        }
    }

    public void canDropCoin() => canDrop = true;

    private void dropCoin()
    {
        GameObject coin = Instantiate(objectToSpawn, transform.position, transform.rotation, transform.parent);
        DropController coinScript = coin.GetComponent<DropController>();

        coinScript.launchDropDirection(dropDirection);
    }
}
