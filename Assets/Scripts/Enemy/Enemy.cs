using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private Player player;
    public CameraShake cameraShake;

    #endregion
    [Header("Spawn info")]
    private bool justSpawned = true;
    [Tooltip("Dùng để xác định hướng nhảy của enemy sau khi hắn xuất hiện")]
    [SerializeField] private Vector2 spawnLaunchDirection;

    [SerializeField] private int amountOfSpikes;
    [SerializeField] private GameObject objectToSpawn;
    private Vector2 startPos;

    [Header("Movement info")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float speedFrontPlayer;
    [SerializeField] private float speedBehindPlayer;

    private bool isRunning;
    public bool canRun;
    private bool canJump;
    private bool canDoubleJump;
    [HideInInspector] public bool characterStop = true;

    [Header("Hit Damage info")]
    [SerializeField] private LayerMask layerToTakeDamage;
    [SerializeField] private Vector2 knockbackDir;
    [SerializeField] private float timeToDie;
    [HideInInspector] public bool canDie = false;

    [Header("Ledge Climb Info")]

    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private Transform ledgeWallCheck;

    private bool isTouchingLedge;
    private bool isLedgeDetected;
    private bool isLedgeWallDetected;
    private bool canClimbLedge;

    private Vector2 ledgePosBot;
    private Vector2 ledgePos1; // vị trí để giữ player trước khi animation kết thúc
    private Vector2 ledgePos2; // vị trí di chuyển player sau khi animation kết thúc

    public float ledgeClimb_Xoffset1 = 0f;
    public float ledgeClimb_Yoffset1 = 0f;
    public float ledgeClimb_Xoffset2 = 0f;
    public float ledgeClimb_Yoffset2 = 0f;

    [Header("Collision checks")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform groundCheckAhead;
    [SerializeField] private Transform wallCheck;

    private bool isGrounded;
    private bool isGroundAhead;
    private bool isWallDetected;

    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float groundCheckAheadDistance; 
    [SerializeField] private float ledgeWallCheckDistance;
    [SerializeField] private float wallCheckRadius;

    [SerializeField] private LayerMask whatIsGround;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.Find("Player").GetComponent<Player>();
        sr = GetComponent<SpriteRenderer>();

        rb.velocity = spawnLaunchDirection;
        startPos = transform.position;
    }

    private void Update()
    {
        if (canDie)
            return;
        enemySpawnLogic();
        checkForAnim();
        checkIfShouldStop();
        checkForMoveSpeed();
        checkForMove();
        checkForCollisions();
        checkForLedgeClimb();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & layerToTakeDamage) != 0)
        {
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        canDie = true;
        
        anim.SetBool("canDie", canDie);
        Time.timeScale = 0.7f;

        checkIfShouldStop() ;

        if (player.transform.position.x < transform.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            rb.velocity = knockbackDir;
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
            rb.velocity = -knockbackDir;
        }

        yield return new WaitForSeconds(0.2f);

        Time.timeScale = 1f;

        yield return new WaitForSeconds(1f);

        rb.velocity = new Vector2(0, 0);

        yield return new WaitForSeconds(3f);
        Destroy(gameObject);

        yield return null;
    }

    public void dropSpike()
    {
        for(int i = 0; i < amountOfSpikes; i++)
        {
            GameObject spike = Instantiate(objectToSpawn, transform.position, objectToSpawn.transform.rotation, transform.parent);
            DropController spikeController = spike.GetComponent<DropController>();

            spikeController.launchDropDirection(new Vector2(-3 + i, 10) * 2);
        }
    }

    private void checkIfShouldStop()
    {
        if (Vector2.Distance(startPos, transform.position) > 100 && isGrounded)
        {
            characterStop = false;

            stopMovement();
            canRun = false;

            if (player.transform.position.x < transform.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }

            if (Vector2.Distance(transform.position, player.transform.position) > 50)
            {
                Destroy(this.gameObject);
            }
        }
    }

    public void enemyCanMove()
    {
        justSpawned = false;
        canRun = true;
    }

    private void checkForAnim()
    {
        anim.SetFloat("yVelocity", rb.velocity.y);

        anim.SetBool("justSpawned", justSpawned);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("canClimbLedge", canClimbLedge);
        anim.SetBool("canDie", canDie);

        if (rb.velocity.x != 0) 
            isRunning = true;
        else
            isRunning = false;
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

    private void checkForMoveSpeed()
    {
        if (player.transform.position.x > transform.position.x)
        {
            moveSpeed = speedBehindPlayer;
        }
        else
        {
            moveSpeed = speedFrontPlayer;
        }
    }

    private void checkForMove()
    {
        if (canRun)
        {
            if (isWallDetected && canJump)
            {
                jump();
            }
            else if (isGrounded && !isGroundAhead)
            {
                jump();
            }
            else if(canDoubleJump && !isGroundAhead && rb.velocity.y < -10)
            {
                canDoubleJump = false;
                jump();
            }

            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            if(isGrounded)
            {
                canJump = true;
                canDoubleJump = true;
            }
            else
            {
                canJump = false;
            }
        }
    }

    private void jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void stopMovement()
    {
        rb.velocity = new Vector2(0, 0);
    }

    private void checkForLedgeClimb()
    {
        if (isLedgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

            ledgePos1 = new Vector2((ledgePosBot.x + ledgeWallCheckDistance) + ledgeClimb_Xoffset1, (ledgePosBot.y) + ledgeClimb_Yoffset1);
            ledgePos2 = new Vector2(ledgePosBot.x + ledgeWallCheckDistance + ledgeClimb_Xoffset2, (ledgePosBot.y) + ledgeClimb_Yoffset2);

            canRun = false;
        }

        if (canClimbLedge)
        {
            transform.position = ledgePos1;
            canJump = false;
            stopMovement();
        }
    }

    public void checkIfLedgeClimbFinished()
    {
        transform.position = ledgePos2;
        canClimbLedge = false;
        canRun = true;
        isLedgeDetected = false;
    }

    private void checkForCollisions()
    {
        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        isGroundAhead = Physics2D.Raycast(groundCheckAhead.position, Vector2.down, groundCheckAheadDistance, whatIsGround);

        if(isGrounded)
        {
            isWallDetected = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, whatIsGround);
        }

        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, Vector2.right, ledgeWallCheckDistance, whatIsGround);
        isLedgeWallDetected = Physics2D.Raycast(ledgeWallCheck.position, Vector2.right, ledgeWallCheckDistance, whatIsGround);

        if(rb.velocity.y < 0)
        {
            ledgeClimb_Yoffset1 = -1.156f;
        }
        else
        {
            ledgeClimb_Yoffset1 = -0.77f;
        }

        if (isLedgeWallDetected && !isTouchingLedge && !isLedgeDetected)
        {
            isLedgeDetected = true;
            ledgePosBot = ledgeWallCheck.position;
            stopMovement();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector3(groundCheck.position.x, groundCheck.position.y - groundCheckDistance, groundCheck.position.z));
        Gizmos.DrawLine(groundCheckAhead.position, new Vector3(groundCheckAhead.position.x, groundCheckAhead.position.y - groundCheckAheadDistance, groundCheckAhead.position.z));
        Gizmos.DrawLine(ledgeWallCheck.position, new Vector3(ledgeWallCheck.position.x + ledgeWallCheckDistance, ledgeWallCheck.position.y, ledgeWallCheck.position.z));
        Gizmos.DrawLine(ledgeCheck.position, new Vector3(ledgeCheck.position.x + ledgeWallCheckDistance, ledgeCheck.position.y, ledgeCheck.position.z));
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
    }
}
