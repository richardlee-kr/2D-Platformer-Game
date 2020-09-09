using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboAttack : MonoBehaviour
{
    private Animator animator;
    public int noOfClicks = 0;
    public int noOfClicks_Air = 1;
    private float lastClickedTime = 0;
    private float maxComboDelay = 0.9f;

    private Rigidbody2D rigidbody;
    public float attackForce;

    [SerializeField] private float attackRate = 0.5f;
    private float currentTime;

    public Transform hitBox;
    public Vector2 boxSize;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - lastClickedTime > maxComboDelay)
        {
            noOfClicks = 1;
        }

        if(currentTime <= 0)
        {
            if (Input.GetKeyDown(KeyCode.J))
            {
                lastClickedTime = Time.time;
                if(!animator.GetBool("Grounded"))
                {
                    if (noOfClicks_Air < 3)
                    {
                        rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);
                        //rigidbody.AddForce(new Vector2(0f, attackForce), ForceMode2D.Impulse);
                        noOfClicks_Air++;
                    }
                }
                else
                {
                    if (noOfClicks < 3)
                        noOfClicks++;
                }


                Collider2D[] colliders = Physics2D.OverlapBoxAll(hitBox.position, boxSize, 0);
                foreach (Collider2D collider in colliders)
                {
                    if(collider.tag == "Enemy")
                    {
                        Debug.Log("Hit");
                        //collider.GetComponent<Enemy>().TakeDamage(damage);
                    }
                    Debug.Log(collider.tag);
                }

                currentTime = attackRate;
                animator.SetBool("isAttack", true);
                PlayAnimation(noOfClicks, noOfClicks_Air);
            }
        }
        else
        {
            currentTime -= Time.deltaTime;
        }
        if ((noOfClicks_Air > 4 || animator.GetBool("Grounded")))
        {
            noOfClicks_Air = 0;
            animator.SetBool("isAttack", false);
        }
        if(noOfClicks == 3)
        {
            noOfClicks = 0;
            animator.SetBool("isAttack", false);
        }
        if(noOfClicks_Air == 3)
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, -attackForce);
        }
   }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(hitBox.position, boxSize);
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
