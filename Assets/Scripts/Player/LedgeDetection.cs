using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeDetection : MonoBehaviour
{
    /*
    [SerializeField] private float radius;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Player player;
    public bool ledgeDetected;

    private bool canDetected = true;

    private BoxCollider2D boxCD => GetComponent<BoxCollider2D>();

    private void Update()
    {
        if (canDetected)
            player.ledgeDetected = Physics2D.OverlapCircle(transform.position, radius, whatIsGround);

        if (canDetected)
            ledgeDetected = Physics2D.OverlapCircle(transform.position, radius, whatIsGround);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            canDetected = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCD.bounds.center, boxCD.size, 0);

        foreach (var hit in colliders)
        {
            if (hit.gameObject.GetComponent<PlatformController>() != null)
                return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            canDetected = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    */
}
