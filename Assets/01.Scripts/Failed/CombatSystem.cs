using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rigidbody;
    private PlayerController PC;
    private PlayerStats PS;

    private int noOfClicks = 0;
    private int noOfClicks_Air = 0;

    private float currentTime;
    private float lastClickedTime = 0;
    private float maxComboDelay = 0.9f;

    public float attackForce;
    private float currentAttackRadius;
    public float attackRadius = 0.6f;
    public float attackRadius_Air = 2f;
    public float attackRadius_Last;
    public float attackDamage;
    public float thirdAttackDamage;
    private AttackDetails attackDetails;

    [SerializeField] private float attackRate = 0.2f;
    public float thirdAir_AttackDamageMulitplier = 2.0f;

    public LayerMask Damagable;
    public Transform hitBox;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        PC = GetComponent<PlayerController>();
        PS = GetComponent<PlayerStats>();

        currentAttackRadius = attackRadius;
        animator.SetBool("isAttack", false);
    }

    // Update is called once per frame
    void Update()
    {
        if(!animator.GetBool("Dead"))
            Attack();
    }

    private void function()
    {
        animator.SetBool("isAttack", false);
    }

    private void function2()
    {
        PC.canMove = true;
    }

    private void Damage(AttackDetails attackDetails)
    {
        if(!PC.GetDashStatus() && !animator.GetBool("Dead") && noOfClicks_Air != 3)
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


    private void Attack()
    {
        if(Time.time - lastClickedTime > maxComboDelay)
        {
            noOfClicks = 0;
            noOfClicks_Air = 0;
            animator.SetFloat("Attack_Count", 1f);
            animator.SetFloat("Air_Attack_Count", 1f);
            animator.SetBool("isAttack", false);
        }

        if(currentTime <= 0)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                rigidbody.velocity = Vector2.zero;
                PC.canMove = false;
                lastClickedTime = Time.time;
                if(!animator.GetBool("Grounded"))
                {
                    if (noOfClicks_Air < 3 && noOfClicks_Air != 3)
                    {
                        currentAttackRadius = attackRadius_Air;
                        rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);
                        noOfClicks_Air++;
                    }
                    else
                        noOfClicks_Air = 0;
                }
                else
                {
                    if (noOfClicks < 3)
                    {
                        currentAttackRadius = attackRadius;
                        noOfClicks++;
                    }
                    else
                        noOfClicks = 0;
                }


                Collider2D[] detectedObejcts = Physics2D.OverlapCircleAll(hitBox.position, currentAttackRadius, Damagable);
                attackDetails.damageAmount = attackDamage;
                attackDetails.position= transform.position;
                foreach (Collider2D collider in detectedObejcts)
                {
                    if(collider.tag == "Enemy")
                    {
                        if (noOfClicks == 3)
                        {
                            attackDetails.damageAmount = thirdAttackDamage;
                            collider.transform.parent.SendMessage("Damage", attackDetails);
                        }
                        else
                        {
                            collider.transform.parent.SendMessage("Damage", attackDetails);

                        }
                    }
                }

                currentTime = attackRate;
                animator.SetBool("isAttack", true);
                PlayAnimation(noOfClicks, noOfClicks_Air);
                Invoke("function2", 0.7f);
            }
        }
        else
        {
            currentTime -= Time.deltaTime;
        }

        if(noOfClicks_Air == 3)
        {
            //rigidbody.velocity = new Vector2(rigidbody.velocity.x, -attackForce);
            rigidbody.AddForce(new Vector2(0f, -attackForce));
            currentAttackRadius = attackRadius_Last;
            animator.SetBool("isAttack", true);

            Collider2D[] detectedObejcts = Physics2D.OverlapCircleAll(hitBox.position, currentAttackRadius, Damagable);
            attackDetails.damageAmount = thirdAttackDamage * thirdAir_AttackDamageMulitplier;
            attackDetails.position = transform.position;
            foreach (Collider2D collider in detectedObejcts)
            {
                if (collider.tag == "Enemy")
                {
                    collider.transform.parent.SendMessage("Damage", attackDetails);
                    Invoke("function", 0.4f);
                    noOfClicks_Air = 0;
                }
            }
            if (animator.GetBool("Grounded"))
            {
                Invoke("function", 0.4f);
            }
        }


        if (!animator.GetBool("isAttack") )
        {
            currentAttackRadius = attackRadius;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(hitBox.position, currentAttackRadius);
    }
    private void PlayAnimation(int atkNum, int atkNum_Air)
    {
        if(!animator.GetBool("Grounded"))
        {
            animator.SetFloat("Air_Attack_Count", atkNum_Air);
        }
        else
        {
            animator.SetFloat("Attack_Count", atkNum);
        }
        animator.SetTrigger("Attack");
    }

}
