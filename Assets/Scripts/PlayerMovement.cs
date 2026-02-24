using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.5f;

    [Header("Jump")]
    public float jumpHeight = 1.2f;

    [Header("Gravity")]
    public float gravity = -9.8f;
    public float groundedStick = -2f;

    [Header("Rotation")]
    public bool faceMoveDirection = true;
    public float rotateSpeed = 14f;

    [Header("Animator (ActionID System)")]
    public Animator rigAnimator;
    public string actionIdParam = "actionID";

    [Header("Action IDs")]
    public int ID_IDLE_STAND = 11;
    public int ID_WALK_FORWARD = 21;
    public int ID_RUN_FORWARD = 31;
    public int ID_JUMP = 41;
    public int ID_FALL = 61;
    public int ID_LAND = 71;

    [Header("Landing (optional)")]
    public float landLockTime = 0.12f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool wasGrounded;
    private float landLockTimer;
    private int actionIdHash;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        actionIdHash = Animator.StringToHash(actionIdParam);

        if (rigAnimator == null)
            rigAnimator = GetComponentInChildren<Animator>(true);

        wasGrounded = controller.isGrounded;

        if (rigAnimator != null)
            rigAnimator.SetInteger(actionIdHash, ID_IDLE_STAND);
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(x, 0f, z);
        if (input.sqrMagnitude > 1f) input.Normalize();

        bool grounded = controller.isGrounded;

        if (!wasGrounded && grounded)
        {
            if (landLockTime > 0f) landLockTimer = landLockTime;
        }
        wasGrounded = grounded;

        if (grounded)
        {
            if (velocity.y < 0f) velocity.y = groundedStick;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocity.y = Mathf.Sqrt(Mathf.Max(0.01f, jumpHeight) * -2f * gravity);
                landLockTimer = 0f;
            }
        }

        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = sprint ? runSpeed : walkSpeed;

        Vector3 move = input * speed;
        controller.Move(move * Time.deltaTime);

        if (faceMoveDirection && input.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(input, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        UpdateActionId(input, sprint, grounded);
    }

    private void UpdateActionId(Vector3 input, bool sprint, bool grounded)
    {
        if (rigAnimator == null) return;

        if (!grounded)
        {
            rigAnimator.SetInteger(actionIdHash, velocity.y > 0.05f ? ID_JUMP : ID_FALL);
            return;
        }

        if (landLockTimer > 0f)
        {
            landLockTimer -= Time.deltaTime;
            rigAnimator.SetInteger(actionIdHash, ID_LAND);
            return;
        }

        if (input.sqrMagnitude < 0.001f)
        {
            rigAnimator.SetInteger(actionIdHash, ID_IDLE_STAND);
            return;
        }

        bool forward = input.z > 0.1f;
        if (sprint && forward)
            rigAnimator.SetInteger(actionIdHash, ID_RUN_FORWARD);
        else
            rigAnimator.SetInteger(actionIdHash, ID_WALK_FORWARD);
    }
}