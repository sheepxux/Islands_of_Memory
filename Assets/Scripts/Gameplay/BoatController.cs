using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatController : MonoBehaviour
{
    [Header("Refs (optional but recommended)")]
    public Transform forwardRef;
    public OarSystem oarSystem;

    [Header("Keys")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    [Header("Stroke Timing")]
    public float minStrokeInterval = 0.08f;

    [Header("Forward Movement")]
    public float forwardPerStroke = 0.7f; 
    public float maxForwardSpeed = 6.5f;  
    public float forwardDamping = 2.5f;   

    [Header("Turning (ONLY when same side repeated)")]
    public int repeatToStartTurning = 2;
    public float yawPerRepeatStroke = 38f;     
    public float maxYawSpeed = 70f;            
    public float yawDamping = 8f;

    [Header("Turn Smooth (optional)")]
    public float yawResponse = 6f;             

    private Rigidbody rb;

    private float forwardVel;
    private float yawVel;
    private float yawVelTarget;

    private float lastStrokeTime;
    private int lastSide;
    private int sameSideCount;

    private Transform ForwardT => forwardRef != null ? forwardRef : transform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void OnEnable()
    {
        forwardVel = 0f;
        yawVel = 0f;
        yawVelTarget = 0f;

        lastSide = 0;
        sameSideCount = 0;
        lastStrokeTime = -999f;
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        ApplyMotion(Time.fixedDeltaTime);
    }

    private void HandleInput()
    {
        if (Time.time - lastStrokeTime < minStrokeInterval) return;

        if (Input.GetKeyDown(leftKey)) Stroke(-1);
        else if (Input.GetKeyDown(rightKey)) Stroke(+1);
    }

    private void Stroke(int side)
    {
        lastStrokeTime = Time.time;

        if (side == lastSide) sameSideCount++;
        else sameSideCount = 1;
        lastSide = side;

        forwardVel += forwardPerStroke;
        forwardVel = Mathf.Clamp(forwardVel, 0f, maxForwardSpeed);

        if (sameSideCount >= repeatToStartTurning)
        {
            float yawSign = (side == -1) ? +1f : -1f;
            yawVelTarget += yawSign * yawPerRepeatStroke;
            yawVelTarget = Mathf.Clamp(yawVelTarget, -maxYawSpeed, maxYawSpeed);
        }

        if (oarSystem != null)
        {
            if (side == -1) oarSystem.StrokeLeft();
            else oarSystem.StrokeRight();
        }
    }

    private void ApplyMotion(float dt)
    {
        forwardVel = Mathf.MoveTowards(forwardVel, 0f, forwardDamping * dt);
        yawVelTarget = Mathf.MoveTowards(yawVelTarget, 0f, yawDamping * dt);
        yawVel = Mathf.Lerp(yawVel, yawVelTarget, 1f - Mathf.Exp(-yawResponse * dt));

        Vector3 fwd = ForwardT.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = transform.forward;
        fwd.Normalize();

        Vector3 newPos = rb.position + fwd * forwardVel * dt;
        Quaternion newRot = rb.rotation;

        if (Mathf.Abs(yawVel) > 0.001f)
        {
            newRot = Quaternion.AngleAxis(yawVel * dt, Vector3.up) * newRot;
        }

        rb.MovePosition(newPos);
        rb.MoveRotation(newRot);
    }
}