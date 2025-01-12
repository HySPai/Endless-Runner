using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] protected float chanceToSpawn = 60;
    private Player player;

    protected virtual void Start()
    {
        bool canSpawn = chanceToSpawn >= Random.Range(0, 100);

        player = GameObject.Find("Player").GetComponent<Player>();

        if (!canSpawn)
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Vector2.Distance(transform.position, player.transform.position) > 50)
        {
            Destroy(this.gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null && !player.canBeKnocked)
            collision.GetComponent<Player>().Damage();
    }
}
