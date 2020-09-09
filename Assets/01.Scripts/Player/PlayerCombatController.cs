using UnityEngine;

public class PlayerCombatController : MonoBehaviour
{
    [SerializeField] private bool combatEnabled;
    public bool canBreakProjectile;

    [SerializeField] private float attackRate = 0.2f;
    private float lastInputTime = Mathf.NegativeInfinity;
    [HideInInspector] public bool gotInput, isAttacking;

    [SerializeField] private int attackCount = -1;
    [SerializeField] private int air_attackCount = -1;
    [SerializeField] private float smashForce = 40f;

    private float currentTime;
    private float comboDelay = 0.9f;

    [SerializeField] private Transform attackHitBoxPos;
    [SerializeField] private LayerMask Damageable;

    [SerializeField] private float attackDamage = 5, thirdAttackDamage = 7, smash_thirdAttackDamage = 10f;
    [SerializeField] private float attackRadius = 0.6f, air_attackRadius = 0.8f, smash_attackRadius = 2f;
    [SerializeField] private float stunDamage = 1f, smash_stunDamage = 3f;
    private float currentAttackRadius, currentAttackDamage, currentStunDamage;
    private AttackDetails attackDetails;

    private PlayerController PC;
    private PlayerStats PS;
    private Animator anim;
    private Rigidbody2D rb;

    private void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        PC = GetComponent<PlayerController>();
        PS = GetComponent<PlayerStats>();

        currentAttackDamage = attackDamage;
        currentAttackRadius = attackRadius;
        currentStunDamage = stunDamage;

        combatEnabled = true;

        anim.SetBool("canAttack", combatEnabled);
    }

    private void Update()
    {
        if(!anim.GetBool("Dead"))
        {
            CheckCombatInput();
            CheckAttacks();

            if (air_attackCount == 2)
            {
                anim.SetBool("isAttack", true);
                rb.AddForce(new Vector2(0, -smashForce));
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
    }

    private void Damage(AttackDetails attackDetails)
    {
        if(!PC.GetDashStatus() && !anim.GetBool("Dead") && air_attackCount != 2)
        {
            int direction;

            PS.DecreaseHealth(attackDetails.damageAmount);

            //Damage Player

            if(attackDetails.position.x < transform.position.x)
            {
                direction = 1;
            }
            else
            {
                direction = -1;
            }
            PC.Knockback(direction);
        }
    }

    private void CheckCombatInput()
    {
        if(Time.time >= lastInputTime + comboDelay && air_attackCount != 2)
        {
            attackCount = -1;
            air_attackCount = -1;
            anim.SetFloat("Attack_Count", attackCount);
            anim.SetFloat("Air_Attack_Count", air_attackCount);
            currentAttackDamage = attackDamage;
        }

        if(Input.GetKeyDown(KeyCode.J))
        {
            if(combatEnabled)
            {
                gotInput = true;
                lastInputTime = Time.time;
            }
        }
    }

    private void ComboAttack()
    {
        if (PC.returnGrounded()) //attack onGround
        {
            if(attackCount < 2)
            {
                currentAttackRadius = attackRadius;
                currentAttackDamage = attackDamage;
                attackCount++;
            }
            else
            {
                attackCount = 0;
                anim.SetFloat("Attack_Count", attackCount);
            }
        }
        else //attack in Air
        {
            if(air_attackCount < 2)
            {
                currentAttackRadius = air_attackRadius;
                currentAttackDamage = attackDamage;
                rb.velocity = new Vector2(rb.velocity.x, 0f);
                air_attackCount++;
            }
            else if(air_attackCount != 2)
            {
                air_attackCount = 0;
                anim.SetFloat("Air_Attack_Count", air_attackCount);
            }
        }
    }
    private void CheckAttacks()
    {
        if(gotInput)
        {
            if(!isAttacking)
            {
                ComboAttack();

                anim.SetFloat("Attack_Count", attackCount);
                anim.SetFloat("Air_Attack_Count", air_attackCount);

                gotInput = false;
                isAttacking = true;
                //rb.velocity = Vector2.zero;

                anim.SetBool("attack", true);
                anim.SetBool("isAttack", isAttacking);
            }
        }

        if(Time.time >= lastInputTime + attackRate)
        {
            gotInput = false;
        }
    }


    private void CheckAttackHitBox()
    {
        Collider2D[] detectedObjects = Physics2D.OverlapCircleAll(attackHitBoxPos.position, currentAttackRadius, Damageable);
        attackDetails.position = transform.position;

        if(air_attackCount == 2) //check smash attack
        {
            currentAttackDamage = smash_thirdAttackDamage;
            currentAttackRadius = smash_attackRadius;
            currentStunDamage = smash_stunDamage;

            detectedObjects = Physics2D.OverlapCircleAll(attackHitBoxPos.position, currentAttackRadius, Damageable);

            if(PC.returnGrounded())
            {
                Invoke("attackReset", 0.5f);
            }
        }

        if(attackCount == 2)
        {
            currentAttackDamage = thirdAttackDamage;
        }

        attackDetails.damageAmount = currentAttackDamage;
        attackDetails.stunDamageAmount = currentStunDamage;

        foreach (Collider2D collider in detectedObjects)
        {
            if(collider.tag == "Enemy")
            {
                collider.transform.parent.SendMessage("Damage", attackDetails);
            }
        }
    }

    private void FinishAttack()
    {
        isAttacking = false;
        anim.SetBool("isAttack", isAttacking);
        anim.SetBool("attack", false);
    }

    private void attackReset()
    {
        air_attackCount = 0;
        anim.SetFloat("Air_Attack_Count", air_attackCount);
        anim.SetBool("attack", false);
        currentAttackRadius = attackRadius;
        currentAttackDamage = attackDamage;
        currentStunDamage = stunDamage;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackHitBoxPos.position, currentAttackRadius);
    }

    public void Dead()
    {
        anim.SetBool("canAttack", false);
        anim.SetBool("attack", false);
    }

}
