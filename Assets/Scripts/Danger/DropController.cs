using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropController : MonoBehaviour
{
    public Rigidbody2D rb;
    private Player player;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void Update()
    {
        // Nếu vector X của đối tượng lớn hơn quá 50 so với player thì sẽ phá hủy đổi tượng này
        if (Vector2.Distance(transform.position, player.transform.position) > 50)
        {
            Destroy(this.gameObject);
        }
    }

    public void launchDropDirection(Vector2 lauchDirection)
    {
        rb.velocity = lauchDirection;
    }
}
