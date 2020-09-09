using UnityEngine.Events;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController_Failed : MonoBehaviour
{
    [SerializeField] private float jumpForce = 400f; //force added when palyer jumps

    [Range(0, .3f)] [SerializeField] private float movementSmoothing = 0.05f; //How much to smooth out the movement
    [Range(0, 1)] [SerializeField] private float crouchSpeed = 0.36f; //speed applied to movement when crouching
    [SerializeField] private bool AirControl = true;
    private Vector3 Velocity = Vector3.zero;

    private bool isRight = true; //where player is facing
    private int right = 1;

    [SerializeField] private LayerMask GroundLayer; //Layer which is Ground
    [SerializeField] private Transform GroundCheck; //Position of object where to check ground
    [SerializeField] private Transform CeilingCheck; //Position fo object where to check ceiling
    [SerializeField] private Collider2D CrouchDisableCollider; //Collider will be disalbed while crouching

    const float GroundRadius = 0.2f; //Radius of overlap Circle to determine isGround
    const float CeilingRadius = 0.2f; //Radius of overlap Circle to determine if player can stand up
    [HideInInspector] public bool isGround = true; //whether player is on ground
    private bool wasCrouching = false;

    public Transform wallCheck; //Position of Object where to check wall
    public float wallCheckDistance; //Distance of checking wall
    public LayerMask wallLayer; //Wall Layer
    private bool isWall; //On wall
    private bool isWallJump; //Jumping from wall
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float slidingSpeed;
    [SerializeField] private float freezeTime = 0.3f;

    [SerializeField] Text velocity_Text;

    //Components
    public Rigidbody2D myRigidbody;
    public Animator animator;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();


        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();

    }

    private void Update()
    {
        isWall = Physics2D.Raycast(wallCheck.position, Vector2.right * right, wallCheckDistance, wallLayer); //wallCheck
        animator.SetBool("Sliding", isWall);
        animator.SetBool("onGround", isGround);
        //Debug.Log(isWallJump);
    }

    private void FixedUpdate()
    {
        velocity_Text.text = myRigidbody.velocity.ToString();

        //Ground Checking
        bool wasGround = isGround;
        isGround = false;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(GroundCheck.position, GroundRadius, GroundLayer);
        bool Grounded = Physics2D.OverlapCircle(GroundCheck.position, GroundRadius, GroundLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if(colliders[i].gameObject != gameObject)
            {
                isGround = true;
                if (!wasGround && Grounded)
                {
                    OnLandEvent.Invoke();
                    //Debug.Log("OnLandEvent");
                }
            }
        }

        //falling animation
        if(!Grounded && !isWall)
        {
            //Debug.Log("Flying");
            animator.SetBool("onFly", true);
        }

        //OnGround Event
        if(Grounded)
        {
            OnLandEvent.Invoke();
        }

        //Debug.Log(Grounded);
        //Debug.Log(myRigidbody.velocity.y);
        //Debug.Log("wasGround:" + wasGround + "//isGround: " + isGround);
    }


    //public function used in PlayerMovement.cs
    public void Move(float move, bool crouch, bool jump)
    {
        if(!crouch)
        {
            if(Physics2D.OverlapCircle(CeilingCheck.position, CeilingRadius, GroundLayer)) //checking Ceiling
            {
                crouch = true;
            }
        }

        //Movement
        if(isGround || AirControl)
        {
            //Croucing
            if(crouch)
            {
                if(!wasCrouching)
                {
                    wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }
                move *= crouchSpeed;

                if (CrouchDisableCollider != null)
                    CrouchDisableCollider.enabled = false;
            }
            else
            {
                if (CrouchDisableCollider != null)
                    CrouchDisableCollider.enabled = true;
                if(wasCrouching)
                {
                    wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            //Horizontal Movement
            if(!isWallJump && !isWall)
            {
                //myRigidbody.velocity = new Vector2(move * 10f, myRigidbody.velocity.y);
                Vector3 targetVelocity = new Vector2(move * 10f, myRigidbody.velocity.y);
                myRigidbody.velocity = Vector3.SmoothDamp(myRigidbody.velocity, targetVelocity, ref Velocity, movementSmoothing);
                if (move != 0)
                    Debug.Log("Moving");
            }

            if(move > 0 && !isRight) //Flip when player facing right
            {
                Flip();
            }
            else if(move < 0 && isRight) //Flip when player facing left
            {
                Flip();
            }
        }

        if(isGround && jump && !crouch && !isWall) //when press jump on ground
        {
            isGround = false;
            myRigidbody.AddForce(new Vector2(0f, jumpForce));
            Debug.Log("Jump");
        }

        if (isWall && !isGround)
        {
            isWallJump = false;
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, myRigidbody.velocity.y * slidingSpeed);
            animator.SetBool("onFly", false);
            Debug.Log("sliding");

            if (jump && isWall) //when press jump on wall
            {
                isWallJump = true;
                Invoke("FreezeX", freezeTime); //Freeze movement by isWallJump
                //myRigidbody.velocity = new Vector2(-right * wallJumpForce, 1.2f * wallJumpForce);
                myRigidbody.AddForce(new Vector2(-right * wallJumpForce, 1.2f * wallJumpForce), ForceMode2D.Impulse);
                Debug.Log("WallJump");
                Flip();
            }
        }
    }

    private void FreezeX()
    {
        isWallJump = false;
        Debug.Log("isWallJump==false");
    }

    private void Flip() //Flip player right or left
    {
        isRight = !isRight;
        right *= -1;

        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(wallCheck.position, Vector2.right * right * wallCheckDistance);
    }
}
