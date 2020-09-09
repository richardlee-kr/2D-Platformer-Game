using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Horizontal movement
    private float movementInputDirection;
    public float movementSpeed = 10.0f;
    public float movementForceInAir;
    public float airDragMultiplier = 0.95f;
    public float turnTimerSet = 0.1f;
    private float turnTimer;
    private bool isWalking;
    public bool canMove;
    private bool canFlip;

    //Normal Jump
    public float jumpForce = 16.0f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float jumpTimerSet = 0.15f;
    public int amountOfJumps = 1;
    private int amountOfJumpsLeft;
    private float jumpTimer;
    private bool isGrounded;
    private bool canNormalJump;
    private bool isAttemptingToJump;
    private bool checkJumpMultiplier;

    //facing Direction
    private int facingDirection = 1;
    private bool isFacingRight = true;
    private int lastWallJumpDirection;

    //Wall Jump
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool canWallJump;
    private bool hasWallJumped;
    private float wallJumpTimer;
    public float wallJumpTimerSet = 0.5f;
    public float wallSlideSpeed = 0.5f;
    public Vector2 wallHopDirection;
    public Vector2 wallJumpDirection;
    public float wallHopForce;
    public float wallJumpForce;

    //Ledge Climb
    public float ledgeClimbXOffset1 = 0f;
    public float ledgeClimbYOffset1 = 0f;
    public float ledgeClimbXOffset2 = 0f;
    public float ledgeClimbYOffset2 = 0f;

    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDetected;
    private Vector2 ledgePosBot;
    private Vector2 ledgePos1;
    private Vector2 ledgePos2;

    //Check Surroundings
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform ledgeCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius;
    public float wallCheckDistance;

    //Dash
    private bool isDashing;
    private float dashTimeLeft;
    private float lastImageXpos;
    private float lastDash = -100f;
    private int amountOfDashLeft;
    public float dashTime;
    public float dashSpeed;
    public float distanceBetweenImages;
    public float dashCoolDown;
    public int amountOfDash = 2;


    //Damage
    private bool knockback;
    private float knockbackStartTime;
    [SerializeField] private float knockbackDuration;
    [SerializeField] private Vector2 knockbackSpeed;


    //Components
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerCombatController PC;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        PC = GetComponent<PlayerCombatController>();

        amountOfJumpsLeft = amountOfJumps;
        amountOfDashLeft = amountOfDash;

        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();
    }

    void Update()
    {
        CheckInput();
        CheckMovementDirection();
        CheckDash();

        CheckSurroudings();
        CheckKnockback();

        CheckIfCanJump();
        CheckJump();
        CheckIfWallSliding();
        CheckLedgeClimb();

        CheckDeath();

        UpdateAnimation();
        Debugging();


        if(Time.time >= (lastDash + dashCoolDown) && amountOfDashLeft <= 0)
            amountOfDashLeft = amountOfDash;

    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void Debugging()
    {
    }

    public bool GetDashStatus()
    {
        return isDashing;
    }

    public void Knockback(int direction)
    {
        knockback = true;
        knockbackStartTime = Time.time;
        rb.velocity = new Vector2(knockbackSpeed.x * direction, knockbackSpeed.y);
        anim.SetTrigger("Damaged");
    }

    private void CheckKnockback()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
            knockback = false;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void CheckLedgeClimb()
    {
        if(ledgeDetected && !canClimbLedge)
        {
            canClimbLedge = true;
            if(isFacingRight)
            {
                ledgePos1 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) - ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Floor(ledgePosBot.x + wallCheckDistance) + ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }
            else
            {
                ledgePos1 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) + ledgeClimbXOffset1, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset1);
                ledgePos2 = new Vector2(Mathf.Ceil(ledgePosBot.x - wallCheckDistance) - ledgeClimbXOffset2, Mathf.Floor(ledgePosBot.y) + ledgeClimbYOffset2);
            }

            canMove = false;
            canFlip = false;

            anim.SetBool("canClimbLedge", canClimbLedge);
        }

        if(canClimbLedge)
        {
            transform.position = ledgePos1;
            transform.position = Vector2.Lerp(ledgePos1, ledgePos2, Time.deltaTime);
        }
    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canFlip = true;
        ledgeDetected = false;

        anim.SetBool("canClimbLedge", canClimbLedge);
    }

    private void CheckSurroudings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, facingDirection * Vector2.right, wallCheckDistance, groundLayer);

        isTouchingLedge = Physics2D.Raycast(ledgeCheck.position, facingDirection * Vector2.right, wallCheckDistance, groundLayer);

        if(isTouchingWall && !isTouchingLedge && !ledgeDetected)
        {
            ledgeDetected = true;
            ledgePosBot = wallCheck.position;
        }
    }
    public bool returnGrounded()
    {
        return isGrounded;
    }

    private void CheckIfWallSliding()
    {
        if(isTouchingWall && movementInputDirection == facingDirection && rb.velocity.y < 0 && !canClimbLedge)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void CheckIfCanJump()
    {
        if(isGrounded && rb.velocity.y <= 0.01f)
        {
            amountOfJumpsLeft = amountOfJumps;
        }

        if(isTouchingWall)
        {
            canWallJump = true;
        }

        if(amountOfJumpsLeft <= 0)
        {
            canNormalJump = false;
        }
        else
        {
            canNormalJump = true;
        }
    }

    private void CheckMovementDirection()
    {
        if (isFacingRight && movementInputDirection < 0)
        {
            Flip();
        }
        else if (!isFacingRight && movementInputDirection > 0)
        {
            Flip();
        }

        if(Mathf.Abs(rb.velocity.x) >= 0.01f)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void UpdateAnimation()
    {
        anim.SetBool("Walking", isWalking);
        anim.SetBool("Grounded", isGrounded);
        anim.SetBool("WallSliding", isWallSliding);
        anim.SetBool("Dashing", isDashing);
        anim.SetFloat("y_velocity", rb.velocity.y);
    }

    private void CheckInput()
    {
        if(!anim.GetBool("isAttack"))
        {
            movementInputDirection = Input.GetAxisRaw("Horizontal");
        }
        else if(anim.GetFloat("Air_Attack_Count") < 0)
        {
            movementInputDirection = 0;
            rb.velocity = Vector2.zero;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if(isGrounded || (amountOfJumpsLeft > 0 && isTouchingWall))
            {
                NormalJump();
            }
            else
            {
                jumpTimer = jumpTimerSet;
                isAttemptingToJump = true;
            }
        }

        if(Input.GetButtonDown("Horizontal") && isTouchingWall)
        {
            if(!isGrounded && movementInputDirection != facingDirection)
            {
                canMove = false;
                canFlip = false;

                turnTimer = turnTimerSet;
            }
        }

        if(turnTimer >= 0)
        {
            turnTimer -= Time.deltaTime;

            if(turnTimer <= 0)
            {
                canMove = true;
                canFlip = true;
            }
        }

        if(checkJumpMultiplier && !Input.GetButton("Jump"))
        {
            checkJumpMultiplier = false;
            rb.velocity = new Vector2(rb.velocity.x, variableJumpHeightMultiplier * rb.velocity.y);
        }

        if(Input.GetButtonDown("Dash"))
        {
            if(amountOfDashLeft > 0)
                if (Time.time >= (lastDash + dashCoolDown) || amountOfDashLeft > 0)
                {
                    AttemptToDash();
                }
        }
    }

    private void AttemptToDash()
    {
        isDashing = true;
        dashTimeLeft = dashTime;
        lastDash = Time.time;
        amountOfDashLeft--;

        PlayerAfterImagePool.Instance.GetFromPool();
        lastImageXpos = transform.position.x;
    }

    public int GetFacingDirection()
    {
        return facingDirection;
    }

    private void CheckDash()
    {
        if(isDashing)
        {
            if(dashTimeLeft > 0)
            {
                canMove = false;
                canFlip = false;
                rb.velocity = new Vector2(dashSpeed * facingDirection, 0);
                dashTimeLeft -= Time.deltaTime;

                if(Mathf.Abs(transform.position.x - lastImageXpos) > distanceBetweenImages)
                {
                    PlayerAfterImagePool.Instance.GetFromPool();
                    lastImageXpos = transform.position.x;
                }
            }
            
            if((dashTimeLeft <= 0 || isTouchingWall))
            {
                isDashing = false;
                canMove = true;
                canFlip = true;
                rb.velocity = Vector2.zero;
            }
        }
    }

    private void CheckJump()
    {
        if(jumpTimer > 0)
        {
            if(!isGrounded && isTouchingWall && movementInputDirection != 0 && movementInputDirection == -facingDirection)
            {
                wallJump();
            }
            else if(isGrounded || amountOfJumpsLeft > 0)
            {
                NormalJump();
            }
        }
        
        if(isAttemptingToJump)
        {
            jumpTimer = Time.deltaTime;
        }

        if(wallJumpTimer > 0)
        {
            if(hasWallJumped && movementInputDirection == -lastWallJumpDirection)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0.0f);
                hasWallJumped = false;
            }
            else if(wallJumpTimer <= 0)
            {
                hasWallJumped = false;
            }
            else
            {
                wallJumpTimer -= Time.deltaTime;
            }
        }
    }

    private void NormalJump()
    {
        if(canNormalJump && !isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetTrigger("Jump");
            amountOfJumpsLeft--;

            jumpTimer = 0;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
        }
    }

    private void wallJump()
    {
        if(canWallJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.0f);
            isWallSliding = false;
            amountOfJumpsLeft = amountOfJumps;
            anim.SetTrigger("Jump");
            amountOfJumpsLeft--;
            Vector2 forceToAdd = new Vector2(wallJumpForce * wallJumpDirection.x * movementInputDirection, wallJumpForce * wallJumpDirection.y);
            rb.AddForce(forceToAdd, ForceMode2D.Impulse);

            jumpTimer = 0;
            turnTimer = 0;
            canMove = true;
            canFlip = true;
            isAttemptingToJump = false;
            checkJumpMultiplier = true;
            hasWallJumped = true;
            wallJumpTimer = wallJumpTimerSet;
            lastWallJumpDirection = -facingDirection;

            Flip();
        }
    }

    private void ApplyMovement()
    {
        if(!isGrounded && !isWallSliding && movementInputDirection == 0 && !knockback)
        {
            rb.velocity = new Vector2(rb.velocity.x * airDragMultiplier, rb.velocity.y);
        }
        else if(canMove && !knockback)
        {
            rb.velocity = new Vector2(movementSpeed * movementInputDirection, rb.velocity.y);
        }

        if(isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, wallSlideSpeed * rb.velocity.y);
        }
    }

    public void DisableMovement()
    {
        canFlip = false;
        canMove = false;
        //Debug.Log("disable movement");
    }
    public void EnableMovement()
    {
        canFlip = true;
        canMove = true;
        //Debug.Log("enable movement");
    }
    public void DisableFlip()
    {
        canFlip = false;
        //Debug.Log("disable flip");
    }
    public void EnableFlip()
    {
        canFlip = true;
        //Debug.Log("enable flip");
    }

    private void Flip()
    {
        if(!isWallSliding && canFlip && !knockback)
        {
            isFacingRight = !isFacingRight;
            facingDirection *= -1;

            transform.Rotate(0, 180, 0);

            //Vector3 theScale = transform.localScale;
            //theScale.x *= -1;
            //transform.localScale = theScale;
        }
    }

    private void CheckDeath()
    {
        if(anim.GetBool("Dead"))
        {
            canMove = false;
            canFlip = false;
            PC.Dead();
            rb.velocity = Vector2.zero;
            this.enabled = false;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(wallCheck.position, Vector2.right * facingDirection * wallCheckDistance);
        Gizmos.color = Color.blue;
        //Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + wallCheckDistance, wallCheck.position.y, wallCheck.position.z));
    }
}
