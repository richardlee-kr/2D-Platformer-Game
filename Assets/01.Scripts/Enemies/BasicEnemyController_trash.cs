using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyController_trash: MonoBehaviour
{
    private enum State
    {
        Moving,
        Knockback,
        Dead
    }

    private State currentState;

    //movement
    [SerializeField] private float movementSpeed;
    private Vector2 movement;

    //Health
    [SerializeField] private float maxHealth;
    private float currentHealth;

    //Damage
    [SerializeField] GameObject hitParticle;
    [SerializeField] private float knockbackDuration;
    [SerializeField] private Vector2 knockbackSpeed;
    private float knockbackStartTime;

    //Touch Attack
    [SerializeField] private float lastTouchDamageTime, touchDamageCooldown;
    [SerializeField] private float touchDamage;
    [SerializeField] private float touchDamageWidth, touchDamageHeight;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Transform touchDamageCheck;
    private float[] attackDetails = new float[2];
    private Vector2 touchDamageBotLeft, touchDamageTopRight;

    //Check Surroundings
    private bool groundDetected, wallDetected, wallDeteted_bottom;
    [SerializeField] private float groundCheckDistance, wallCheckDistance;
    [SerializeField] private Transform groundCheck, wallCheck, wallCheck_bottom;
    [SerializeField] private LayerMask groundLayer;

    //Directions
    private int facingDirection, damageDirection;

    //Components
    [SerializeField] private string aliveName;
    private GameObject alive;
    private Rigidbody2D aliveRb;
    private Animator aliveAnim;

    private void Start()
    {
        alive = transform.Find(aliveName).gameObject;
        aliveRb = alive.GetComponent<Rigidbody2D>();
        aliveAnim = alive.GetComponent<Animator>();

        currentHealth = maxHealth;

        facingDirection = 1;
    }

    private void Update()
    {
        switch(currentState)
        {
            case State.Moving:
                UpdateMovingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.Dead:
                UpdateDeadState();
                break;
        }
    }

    //--WALKING STATE
    private void EnterMovingState()
    {

    }
    private void UpdateMovingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right * facingDirection, wallCheckDistance, groundLayer);
        wallDeteted_bottom = Physics2D.Raycast(wallCheck_bottom.position, transform.right * facingDirection, wallCheckDistance, groundLayer);

        CheckTouchDamage();

        if((!groundDetected || wallDetected || wallDeteted_bottom) && currentState != State.Knockback)
        {
            Flip();
        }
        else
        {
            movement.Set(movementSpeed * facingDirection, aliveRb.velocity.y);
            aliveRb.velocity = movement;
        }
    }
    private void ExitMovingState()
    {

    }

    //--KNOCKBACK STATE
    private void EnterKnockbackState()
    {
        knockbackStartTime = Time.time;
        movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
        aliveRb.velocity = movement;
        aliveAnim.SetBool("Knockback", true);

    }
    private void UpdateKnockbackState()
    {
        if(Time.time >= knockbackStartTime + knockbackDuration)
        {
            SwitchState(State.Moving);
        }
    }
    private void ExitKnockbackState()
    {
        aliveAnim.SetBool("Knockback", false);
        Invoke("ExitDeadState", 2f);
    }

    //--DEAD STATE
    private void EnterDeadState()
    {
        aliveAnim.SetBool("Dead", true);
        Destroy(gameObject, 3f);
    }
    private void UpdateDeadState()
    {

    }
    private void ExitDeadState()
    {
    }

    //-- OTHER FUNCTION

    private void Damage(float[] attackDetails)
    {
        currentHealth -= attackDetails[0];
        Instantiate(hitParticle, alive.transform.position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)));

        if(attackDetails[1] > alive.transform.position.x)
        {
            damageDirection = -1;
        }
        else
        {
            damageDirection = 1;
        }

        //Hit Particle

        if(currentHealth > 0.0f && currentState != State.Dead)
        {
            SwitchState(State.Knockback);
        }
        else if(currentHealth <= 0.0f)
        {
            SwitchState(State.Dead);
        }
    }

    private void CheckTouchDamage()
    {
        if(Time.time >= lastTouchDamageTime + touchDamageCooldown && currentState != State.Knockback)
        {
            touchDamageBotLeft.Set(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
            touchDamageTopRight.Set(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

            Collider2D hit = Physics2D.OverlapArea(touchDamageBotLeft, touchDamageTopRight, playerLayer);

            if(hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails[0] = touchDamage;
                attackDetails[1] = alive.transform.position.x;
                hit.SendMessage("Damage", attackDetails);
            }
        }
    }

    private void Flip()
    {
        facingDirection *= -1;
        alive.transform.Rotate(0, 180, 0);

        //Vector3 theScale = transform.localScale;
        //theScale.x *= -1;
        //transform.localScale = theScale;

    }
    private void SwitchState(State state)
    {
        switch(currentState)
        {
            case State.Moving:
                ExitMovingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.Dead:
                ExitDeadState();
                break;
        }

        switch (state)
        {
            case State.Moving:
                EnterMovingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.Dead:
                EnterDeadState();
                break;
        }

        currentState = state;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));
        Gizmos.DrawLine(wallCheck_bottom.position, new Vector2(wallCheck_bottom.position.x + wallCheckDistance, wallCheck_bottom.position.y));


        Vector2 botLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 botRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 topLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));
        Vector2 topRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2), touchDamageCheck.position.y + (touchDamageHeight / 2));

        Gizmos.DrawLine(botLeft, botRight);
        Gizmos.DrawLine(botRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, botLeft);
    }
}
