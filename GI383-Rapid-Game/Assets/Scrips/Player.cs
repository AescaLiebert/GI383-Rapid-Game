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



    public SpriteRenderer spriteRenderer;
    private Vector2 moveInput;


    public Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        if (moveInput.x != 0)
        {
            spriteRenderer.flipX = moveInput.x < 0;
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

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float dashDirection = spriteRenderer.flipX ? -1f : 1f;
        rb.linearVelocity = new Vector2(dashDirection * dashspeed, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = originalGravity;
        dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }    






}