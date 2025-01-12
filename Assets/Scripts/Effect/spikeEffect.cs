using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spikeEffect : MonoBehaviour
{
    private Player player;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            player.SpeedReset();
        }
    }
}
