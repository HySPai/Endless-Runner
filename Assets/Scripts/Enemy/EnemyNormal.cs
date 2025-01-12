using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class EnemyNormal : MonoBehaviour
{
    #region
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Player player;

    #endregion

    [Header("Spawn info")]
    [SerializeField] private bool canSpawn;
    private bool justSpawned;
    [Tooltip("Dùng để xác định hướng nhảy của enemy sau khi hắn xuất hiện")]
    [SerializeField] private Vector2 spawnLaunchDirection;

    [Header("Take Damage info")]
    [SerializeField] private LayerMask layerToTakeDamage;
    [SerializeField] private float timeToDie;
    [SerializeField] private Vector2 knockbackDir;
    [HideInInspector] public bool canDie = false;

    [Header("Player Check")]
    private bool isPlayer;
    private bool canAttack = true; // Add this line

    [Header("Move")]
    [SerializeField] private float speed;
    [SerializeField] private bool canMove;

    [Header("Collision checks")]
    public bool checkPlayerMove;
    [SerializeField] private Transform playerCheckMove;
    [SerializeField] private Vector2 playerCheckRadiusMove;

    [SerializeField] private Transform playerCheck;
    [SerializeField] private float playerCheckRadius;
    [SerializeField] private LayerMask whatIsPLayer;

    [SerializeField] private Transform groundCheck;
    private bool isGrounded;

    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatIsGround;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        rb.velocity = spawnLaunchDirection;
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void Update()
    {
        if (canDie)
            return;

        enemySpawnLogic();
        checkForCollisions();
        checkForAnim();
        CheckPlayerRotage();
        CheckPlayerMove();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & layerToTakeDamage) != 0)
        {
            StartCoroutine(Die());
        }
    }

    private void CheckPlayerMove()
    {
        if (checkPlayerMove && canMove)
        {
            Move();
        }
        else if(checkPlayerMove && canSpawn)
        {
            justSpawned = true;
        }
    }

    private void Move()
    {
        if (checkPlayerMove)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
        }
    }

    private IEnumerator Die()
    {
        canDie = true;
        rb.velocity = knockbackDir;
        anim.SetBool("canDie", canDie);
        Time.timeScale = 0.7f;

        yield return new WaitForSeconds(0.1f);

        Time.timeScale = 1f;

        yield return new WaitForSeconds(1f);

        rb.velocity = new Vector2(0, 0);

        yield return new WaitForSeconds(3f);
        Destroy(gameObject);

        yield return null;
    }

    private void CheckPlayerRotage()
    {
        if (player.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (Vector2.Distance(transform.position, player.transform.position) > 50)
        {
            Destroy(this.gameObject);
        }
    }

    private void checkForAnim()
    {
        anim.SetFloat("yVelocity", rb.velocity.y);

        if (isPlayer && canAttack)
        {
            anim.SetBool("isAttack", true);
            StartCoroutine(AttackCooldown());
        }
        else
        {
            anim.SetBool("isAttack", false);
        }

        anim.SetBool("justSpawned", justSpawned);
        anim.SetBool("isGrounded", isGrounded);
    }

    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.5f); 
        canAttack = true;
    }

    private void enemySpawnLogic()
    {
        if (justSpawned)
        {
            if (rb.velocity.y < 0)
            {
                rb.gravityScale = 10;
                Time.timeScale = 1;
            }

            if (isGrounded)
            {
                stopMovement();
                rb.gravityScale = 5;
            }
        }
    }

    private void stopMovement()
    {
        rb.velocity = new Vector2(0, 0);
    }

    private void checkForCollisions()
    {
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        isPlayer = Physics2D.Raycast(playerCheck.position, Vector2.left, playerCheckRadius, whatIsPLayer);
        checkPlayerMove = Physics2D.Raycast(playerCheckMove.position, Vector2.left, whatIsPLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance, groundCheck.position.z));
        Gizmos.DrawWireSphere(playerCheck.position, playerCheckRadius);
        Gizmos.DrawWireCube(playerCheckMove.position, playerCheckRadiusMove);
    }
}
