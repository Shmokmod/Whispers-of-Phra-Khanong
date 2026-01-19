using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float runSpeed = 5f;

    [Header("References")]
    [SerializeField] private Rigidbody rb;

    private Vector2 moveInput;
    private Animator animator;
    private bool isRunning;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        isRunning = Input.GetKey(KeyCode.LeftShift) && moveInput != Vector2.zero;
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        Vector2 normalizedInput = moveInput.normalized;
        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        // ขยับตัวละคร
        Vector3 newVelocity = new Vector3(
            normalizedInput.x * currentSpeed,
            rb.linearVelocity.y,
            normalizedInput.y * currentSpeed
        );
        rb.linearVelocity = newVelocity;

        // ตั้งค่า Animation
        bool isMoving = moveInput != Vector2.zero;
        animator.SetBool("isWalking", isMoving);

        // บันทึก direction และอัปเดต parameters
        if (isMoving)
        {
            // อัปเดต input parameters ขณะกำลังเคลื่อนที่
            animator.SetFloat("InputX", normalizedInput.x);
            animator.SetFloat("InputY", normalizedInput.y);

            // บันทึก direction สุดท้ายไว้
            animator.SetFloat("LastInputX", normalizedInput.x);
            animator.SetFloat("LastInputY", normalizedInput.y);
        }
        else
        {
            // เมื่อหยุด ให้ Input เป็น 0
            animator.SetFloat("InputX", 0);
            animator.SetFloat("InputY", 0);
            // LastInput จะคงค่าเดิมที่บันทึกไว้ตอนเคลื่อนที่
        }
    }
}