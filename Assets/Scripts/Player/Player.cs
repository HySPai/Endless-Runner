using System.Collections;
using UnityEngine;
using GG.Infrastructure.Utils.Swipe;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Player : MonoBehaviour
{
    #region components
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;

    private BoxCollider2D cd;
    [SerializeField] private SwipeListener swipeListener;
    #endregion

    #region Life
    [Header("Life info")]
    private bool isDead;
    [HideInInspector] public bool playerUnlocked;
    [HideInInspector] public bool extraLife;
    [SerializeField] private float timeLife;
    #endregion

    #region VFX
    [Header("VFX")]
    [SerializeField] private ParticleSystem dustFx;
    [SerializeField] private ParticleSystem bloodFx;
    #endregion

    #region Attack
    [Header("Attack info")]
    [SerializeField] private GameObject hitAttack;
    [SerializeField] private float foreceAttack;
    private bool canAttackDown;
    private bool canAttack = false;
    #endregion

    #region Knockback
    [Header("Knockback info")]
    [SerializeField] private Vector2 knockbackDir;
    [SerializeField] private float knockbackPower;
    private bool isKnocked;
    [HideInInspector] public bool canBeKnocked = true;
    #endregion

    #region Move
    [Header("Move info")]
    public float moveSpeedNeededToSurive;
    public float moveSpeed;
    public float maxMoveSpeed;
    private float defaultMoveSpeed;
    [HideInInspector] public bool canRun = false;

    private float speedMilestone;
    [SerializeField] private float speedMultipler;
    [SerializeField] private float speedIncreaseMilestone;
    private float defaultSpeedIncreaseMilestone;

    private bool canRoll;
    private bool wallRun;
    #endregion

    #region Jump
    [Header("Jump info")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float doubleJumpForce;
    [SerializeField] private float gravityReductionSpeed;
    [SerializeField] private float minGravity;
    private bool canDoubleJump;
    private float currentGravity;
    #endregion

    #region Slide
    [Header("Slide info")]
    [SerializeField] private float slideSpeed;
    [SerializeField] private float slideTime;
    [SerializeField] private float slideCooldown;
    [HideInInspector] public float slideCooldownCounter;
    private float slideTimeCounter;
    private bool isSliding;
    #endregion

    #region Collision
    [Header("Collision detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform bottomWallCheck;
    [SerializeField] private Transform ceillingCheck;

    public float wallCheckDistance;
    public float groundCheckRadius;
    public LayerMask whatIsGround;

    private bool isGrounded;
    private bool isWallDetected;
    private bool isBottomWallDetected;
    private bool isCeillingDetected;
    #endregion

    #region Ledge
    [Header("Ledge Climb Info")]

    [SerializeField] private Transform ledgeCheck;

    private bool isTouchingLedge;
    private bool isLedgeDetected;
    private bool canClimbLedge;

    private Vector2 ledgePosBot;
    private Vector2 ledgePos1; // position to hold the player before animations ends
    private Vector2 ledgePos2; // position where to move the player after animations ends

    public float ledgeClimb_Xoffset1 = 0f;
    public float ledgeClimb_Yoffset1 = 0f;
    public float ledgeClimb_Xoffset2 = 0f;
    public float ledgeClimb_Yoffset2 = 0f;
    #endregion

    #region Item
    [Header("Coin Attraction")]
    public float attractionRadius;
    public float speedToAttract;
    [HideInInspector] public bool isAttract;
    [SerializeField] private LayerMask coinLayer;

    [Header("Shield")]
    [HideInInspector] public bool isShield = false;

    [Header("Winged shoes")]
    [HideInInspector] public bool isWingedshoes = false;
    #endregion
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        cd = GetComponent<BoxCollider2D>();

        settingDefaultValues();
    }

    void Update()
    {
        CheckCollision();
        AnimatorControllers();

        slideTimeCounter -= Time.deltaTime;
        slideCooldownCounter -= Time.deltaTime;

        extraLife = moveSpeed >= moveSpeedNeededToSurive;

        if (isDead)
            return;

        if (isKnocked)
            return;

        if (isGrounded)
            canDoubleJump = true;

        if (rb.velocity.y < 0)
            currentGravity = 5f;

        if (isGrounded && rb.velocity.y < -20)
            canRoll = true;

        if (isWallDetected)
        {
            canAttackDown = false;
            canAttack = false;
        }
        else
            canRoll = false;

        if (isGrounded)
        {
            canAttackDown = false;
        }

        SpeedController();
        SetupMovement();
        AttractCoins();
        CheckForSlideCancel();
        checkForLedgeClimb();
        CheckInput();
    }
    private void settingDefaultValues()
    {
        defaultMoveSpeed = moveSpeed;
        defaultSpeedIncreaseMilestone = speedIncreaseMilestone;
        speedMilestone = defaultSpeedIncreaseMilestone;
    }

    private void OnEnable()
    {
        swipeListener.OnSwipe.AddListener(OnSwipe);
    }

    private void OnDisable()
    {
        swipeListener.OnSwipe.RemoveListener(OnSwipe);
    }

    #region Attack
    public void PlayerAttack()
    {
        if (isSliding || isDead || isKnocked || isWallDetected)
            return;
        canAttack = true;
    }


    private void AttackFinish()
    {
        canAttack = false;
    }

    private void AttackDownButton()
    {
        if (isSliding || isDead || isKnocked || isWallDetected)
            return;
        if (!isGrounded)
        {
            canAttackDown = true;
            AttackDownForce(foreceAttack);
        }
    }

    private void AttackDownForce(float force)
    {
        rb.velocity = new Vector2(rb.velocity.x, force);
    }
    #endregion

    public void Damage()
    {
        if (!isShield)
        {
            bloodFx.Play();
            if (extraLife)
                Knockback();
            else
            {
                StartCoroutine(Die());
            }
        }
        StartCoroutine(ShieldCoolDown());
    }
    private IEnumerator ShieldCoolDown()
    {
        yield return new WaitForSeconds(1f);
        isShield = false;
    }

    private IEnumerator Die()
    {
        AudioManager.instance.PlaySFX(3);
        isDead = true;
        canBeKnocked = false;
        rb.velocity = knockbackDir;
        anim.SetBool("isDead", true);

        Time.timeScale = .6f;

        yield return new WaitForSeconds(1f);

        Time.timeScale = 1f;
        rb.velocity = new Vector2(0, 0);
        GameManager.instance.GameEnded();

        yield return null;
    }


    #region Knockback
    private IEnumerator Invincibility()
    {
        Color originalColor = sr.color, darkenColor = new Color(sr.color.r, sr.color.g, sr.color.b, .5f);
        canBeKnocked = false;
        sr.color = darkenColor;

        for (float i = 0.1f; i <= 0.4f; i += 0.1f)
        {
            yield return new WaitForSeconds(i);
            sr.color = originalColor;
            yield return new WaitForSeconds(i);
            sr.color = darkenColor;
        }

        sr.color = originalColor;
        canBeKnocked = true;
    }

    private void Knockback()
    {
        if (!canBeKnocked)
            return;

        if (!isShield && !isDead)
        {
            SpeedReset();
            StartCoroutine(Invincibility());
            extraLife = false;
            isKnocked = true;
            rb.velocity = knockbackDir;
        }
    }

    private void CancelKnockback() => isKnocked = false;

    #endregion

    #region SpeedControll
    public void SpeedReset()
    {
        moveSpeed = defaultMoveSpeed;
        speedIncreaseMilestone = defaultSpeedIncreaseMilestone;
    }

    private void SpeedController()
    {
        if (transform.position.x > speedMilestone)
        {
            speedMilestone = speedMilestone + speedIncreaseMilestone;


            moveSpeed = moveSpeed * speedMultipler;
            speedIncreaseMilestone = speedIncreaseMilestone * speedMultipler;

            if (moveSpeed > maxMoveSpeed)
            {
                moveSpeed = maxMoveSpeed;
            }
        }
    }
    #endregion

    #region Ledge Climb Region
    private void checkForLedgeClimb()
    {
        if (isLedgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;
            wallRun = false;

            ledgePos1 = new Vector2((ledgePosBot.x + wallCheckDistance) + ledgeClimb_Xoffset1, (ledgePosBot.y) + ledgeClimb_Yoffset1);
            ledgePos2 = new Vector2(ledgePosBot.x + wallCheckDistance + ledgeClimb_Xoffset2, (ledgePosBot.y) + ledgeClimb_Yoffset2);

            canRun = false;
        }

        if (canClimbLedge)
        {
            transform.position = ledgePos1;
        }
    }

    private void checkIfLedgeClimbFinished()
    {
        transform.position = ledgePos2;
        canClimbLedge = false;
        canRun = true;
        isLedgeDetected = false;
    }
    #endregion

    private void SetupMovement()
    {
        if (isKnocked && canBeKnocked)
        {
            canBeKnocked = false;
            canRun = false;
            rb.velocity = knockbackDir * knockbackPower;
        }

        if (canRun)
        {
            if (isWallDetected && !canClimbLedge && !isSliding)
            {
                rb.velocity = new Vector2(rb.velocity.x, moveSpeed / 2);
                wallRun = true;
            }
            else
            {
                wallRun = false;
                if (isSliding)
                    rb.velocity = new Vector2(moveSpeed * 1.5f, rb.velocity.y);
                else
                    rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            }
        }
    }


    #region Inputs
    public void SlideButton()
    {
        if (isDead)
            return;

        if (rb.velocity.x != 0 && slideCooldownCounter < 0 && isGrounded)
        {
            dustFx.Play();
            canBeKnocked = false;
            isSliding = true;
            slideTimeCounter = slideTime;
            slideCooldownCounter = slideCooldown;
        }
    }

    private void CheckForSlideCancel()
    {
        if (slideTimeCounter < 0 && !isCeillingDetected)
        {
            canBeKnocked = false;
            isSliding = false;
        }
    }

    public void JumpButton()
    {
        if (isSliding || isDead || canAttack)
            return;

        RollAnimFinished();

        if (isGrounded)
        {
            Jump(jumpForce);
            currentGravity = rb.gravityScale;
            StartCoroutine(UpdateGravity());
        }
        else if (canDoubleJump && isWingedshoes)
        {
            canDoubleJump = false;
            Jump(doubleJumpForce);
            currentGravity = rb.gravityScale;
            StartCoroutine(UpdateGravity());
        }
    }

    private void Jump(float force)
    {
        dustFx.Play();
        AudioManager.instance.PlaySFX(Random.Range(1, 2));
        rb.velocity = new Vector2(rb.velocity.x, force);
    }

    private IEnumerator UpdateGravity()
    {
        while (rb.velocity.y > 0)
        {
            rb.gravityScale = Mathf.Max(rb.gravityScale - gravityReductionSpeed * Time.deltaTime, minGravity);
            yield return null;
        }
        rb.gravityScale = currentGravity; // Reset gravity when falling down
    }

    private void CheckInput()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            PlayerAttack();
        }

        if (Input.GetButtonDown("Jump"))
        {
            JumpButton();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            SlideButton();
        }
    }

    private void OnSwipe(string swipe)
    {
        switch(swipe)
        {
            case "Left":
                SlideButton();
                break;
            case "Right":
                PlayerAttack();
                break;
            case "Up":
                JumpButton();
                break;
            case "Down":
                AttackDownButton();
                break;
        }
    }
    #endregion
    #region Animations

    private void AnimatorControllers()
    {
        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetFloat("speed", moveSpeed);

        anim.SetBool("canDoubleJump", canDoubleJump);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("canClimb", canClimbLedge);
        anim.SetBool("isKnocked", isKnocked);
        anim.SetBool("canAttack", canAttack);
        anim.SetBool("canAttackDown", canAttackDown);
        anim.SetBool("isRunning", canRun);
        anim.SetBool("wallRun", wallRun);

        anim.SetBool("canRoll", canRoll);
    }

    private void RollAnimFinished() => anim.SetBool("canRoll", false);

    #endregion
    private void AttractCoins()
    {
        Collider2D[] coins = Physics2D.OverlapCircleAll(transform.position, attractionRadius, coinLayer);
        foreach (Collider2D coin in coins)
        {
            // Bú đồng xu về phía player
            isAttract = true;
            Vector2 direction = (transform.position - coin.transform.position).normalized;
            coin.transform.position = Vector2.MoveTowards(coin.transform.position, transform.position, speedToAttract * Time.deltaTime);
        }
        isAttract = false;
    }

    private void CheckCollision()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
        isBottomWallDetected = Physics2D.Raycast(bottomWallCheck.position, Vector2.right, wallCheckDistance, whatIsGround);
        isCeillingDetected = Physics2D.Raycast(ceillingCheck.position, Vector2.up, wallCheckDistance + 1, whatIsGround);

        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, Vector2.right, wallCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(wallCheck.position, Vector2.right, wallCheckDistance, whatIsGround);

        if (isWallDetected && !isTouchingLedge && !isLedgeDetected)
        {
            isLedgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
        Gizmos.DrawLine(ledgeCheck.position, new Vector3(ledgeCheck.position.x + wallCheckDistance, ledgeCheck.position.y, ledgeCheck.position.z));
        Gizmos.DrawLine(bottomWallCheck.position, new Vector3(bottomWallCheck.position.x + wallCheckDistance, bottomWallCheck.position.y, bottomWallCheck.position.z));
        Gizmos.DrawLine(ceillingCheck.position, new Vector3(ceillingCheck.position.x, ceillingCheck.position.y + wallCheckDistance + 1, ceillingCheck.position.z));

        // Draw the attraction radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
