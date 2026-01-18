using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    public int HP;
    public int level;
    [Header("Movement Stats")]
    public float jump;
    public float speed;
    
    [Header("Weapon Stats")]
    private string weaponName;
    [Header("Dash Stats")]
    public float dashspeed;
    public float dashCooldown;
    public float dashTime;

    private bool canDash = true;
    public bool dashing ;



    public Animator anim;
    public SpriteRenderer spriteRenderer;
    private Vector2 moveInput;


    public Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }


    void Update()
    {
        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x > 0;
        }

        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        if (dashing)
        {
            anim.SetBool("IsIdle", false);
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsDash", true);
        }
        else if (moveInput.x != 0)
        {
            anim.SetBool("IsIdle", false);
            anim.SetBool("IsWalk", true);
            anim.SetBool("IsDash", false);
        }
        else
        {
            anim.SetBool("IsIdle", true);
            anim.SetBool("IsWalk", false);
            anim.SetBool("IsDash", false);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (IsGrounded() && value.isPressed)
        {

            rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
        }
    }

    public void OnDash (InputValue value)
    {
       if (canDash && value.isPressed)
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {

        if (dashing) return;
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {

        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

    }

    private IEnumerator Dash()
    {
        canDash = false;
        dashing = true;

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }

        StartCoroutine(ShowGhosts());

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float dashDirection = spriteRenderer.flipX ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashspeed, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        dashing = false;
        
        if (enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator ShowGhosts()
    {
        while (dashing)
        {
            GameObject ghost = new GameObject("Ghost");
            ghost.transform.position = transform.position;
            ghost.transform.rotation = transform.rotation;
            ghost.transform.localScale = transform.localScale;

            SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
            ghostSr.sprite = spriteRenderer.sprite;
            ghostSr.flipX = spriteRenderer.flipX;
            ghostSr.sortingLayerID = spriteRenderer.sortingLayerID;
            ghostSr.sortingOrder = spriteRenderer.sortingOrder - 1;

            ghost.AddComponent<GhostFade>();

            yield return new WaitForSeconds(0.038f);
        }
    }    






}