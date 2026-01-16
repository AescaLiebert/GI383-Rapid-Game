using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    public int HP;
    public int level;
    [Header("Movement Stats")]
    public float jump;
    public float speed;
    public float dash;

    private string weaponName;


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

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()

    {

        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

    }




}