using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region components
    private Rigidbody2D rb; // rb - rigid body
    private Animator anim;
    private SpriteRenderer sr;
    private Coroutine hurtAnimCoroutine;

    private BoxCollider2D cd;
    #endregion



    [Header("Movement Info")]
    public float moveSpeedNeededToSurive;
    public float moveSpeed;
    public float maxMoveSpeed;
    private float defaultMoveSpeed;
    private bool isRunning;
    private bool canRun = false; // bool boolean true || false


    private float speedMilestone;
    [SerializeField] private float speedMultipler;
    [SerializeField] private float speedIncreaseMilestone;
    private float defaultSpeedIncreaseMilestone;


    private bool canRoll;

    [Header("Jump info")]
    public float jumpForce;
    public float doubleJumpForce;

    private float defaultJumpForce;
    private bool canDoubleJump;

    [Header("Knockback info")]
    [SerializeField] private Vector2 knockbackDirection;
    [SerializeField] private float knockbackPower;

    private bool canBeKnocked = true;
    private bool isKnocked;

    [Header("Slide info")]
    public float slideSpeedMultipler;
    private bool isSliding;
    private bool canSlide;

    [SerializeField] private float slidingCooldown;
    [SerializeField] private float slidingTime;
    private float slidingBegun;


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


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        cd = GetComponent<BoxCollider2D>();

        settingDefaultValues();
    }

    // Update is called once per frame // if you have 60fps -  60 times per second
    void Update()
    {

        if (Input.anyKey && !isKnocked)
        {
            canRun = true;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            speedReset();
        }


        checkForRun();
        checkForJump();
        checkForSlide();
        checkForSpeedingUp();
        checkForLedgeClimb();


        checkForCollisions();
        AnimationControllers();
    }

    private void settingDefaultValues()
    {
        defaultJumpForce = jumpForce;
        defaultMoveSpeed = moveSpeed;
        defaultSpeedIncreaseMilestone = speedIncreaseMilestone;
        speedMilestone = defaultSpeedIncreaseMilestone;
    }

    private void AnimationControllers()
    {

        anim.SetFloat("xVelocity", rb.velocity.x);
        anim.SetFloat("yVelocity", rb.velocity.y);
        anim.SetBool("isRunning", isRunning);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("canClimbLedge", canClimbLedge);
        anim.SetBool("canDoubleJump", canDoubleJump);
        anim.SetBool("canRoll", canRoll);
        anim.SetBool("isKnocked", isKnocked);

        if (rb.velocity.y < -25)
        {
            canRoll = true;
        }

    }

    private void rollAnimationFinished()
    {
        canRoll = false;
    }

    private void knockbackAnimationFinished()
    {
        isKnocked = false;
        canRun = true;
    }

    public void knockback()
    {
        if (canBeKnocked)
        {
            isKnocked = true;
            hurtVFX();
            speedReset();
        }
    }

    private void checkForRun()
    {
        if (isKnocked & canBeKnocked)
        {
            canBeKnocked = false;
            canRun = false;
            rb.velocity = knockbackDirection * knockbackPower;
        }

        if (canRun)
        {
            if (isBottomWallDetected || isWallDetected && !isSliding)
            {
                speedReset();
            }
            else if (isSliding)
            {
                rb.velocity = new Vector2(moveSpeed * slideSpeedMultipler, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
            }
        }


        if (rb.velocity.x > 0)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }
    }
    private void checkForJump()
    {
        if (Input.GetKeyDown("space") && !isKnocked)
        {
            if (isGrounded)
            {
                jump();
            }
            else if (canDoubleJump)
            {
                canDoubleJump = false;

                jumpForce = doubleJumpForce;
                jump();
            }

        }

        if (isGrounded)
        {
            jumpForce = defaultJumpForce;
            canDoubleJump = true;
        }
    }

    private void checkForSlide()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canSlide && isGrounded && rb.velocity.x > defaultMoveSpeed)
        {
            isSliding = true;
            canSlide = false;
            slidingBegun = Time.time;
        }

        if (Time.time > slidingBegun + slidingTime && !isCeillingDetected)
        {
            isSliding = false;
        }

        if (Time.time > slidingBegun + slidingCooldown)
        {
            canSlide = true;
        }


    }

    private void checkForSpeedingUp()
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

    private void checkForLedgeClimb()
    {
        if (isLedgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;

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

    private void jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void speedReset()
    {

        moveSpeed = defaultMoveSpeed;
        speedIncreaseMilestone = defaultSpeedIncreaseMilestone;
    }


    private void checkForCollisions()
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
    }

    private IEnumerator hurtVFXRoutine()
    {
        Color originalColor = sr.color;
        Color darkenColor = new Color(sr.color.r, sr.color.g, sr.color.b, 0.6f);

        sr.color = darkenColor;

        yield return new WaitForSeconds(0.2f);
        sr.color = originalColor;

        yield return new WaitForSeconds(0.2f);
        sr.color = darkenColor;

        yield return new WaitForSeconds(0.2f);
        sr.color = originalColor;

        yield return new WaitForSeconds(0.2f);
        sr.color = darkenColor;

        yield return new WaitForSeconds(0.3f);
        sr.color = originalColor;

        yield return new WaitForSeconds(0.3f);
        sr.color = darkenColor;

        yield return new WaitForSeconds(0.4f);
        sr.color = originalColor;

        yield return new WaitForSeconds(0.2f);

        canBeKnocked = true; // makes player valnurable again


        hurtAnimCoroutine = null; // stops coroutine 
    }

    public void hurtVFX()
    {
        // stops actve coroutine before activating new one
        if (hurtAnimCoroutine != null)
        {
            StopCoroutine(hurtAnimCoroutine);
        }

        // starts coroutine with reference to it
        hurtAnimCoroutine = StartCoroutine(hurtVFXRoutine());
    }

}
