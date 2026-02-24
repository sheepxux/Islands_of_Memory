using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    public enum Mode { Walk, Boat }

    [Header("Target")]
    public Transform target;

    [Header("Mode")]
    public Mode mode = Mode.Walk;

    [Header("Walk Camera")]
    public float walkHeight = 16f;
    public float walkBack = 16f;
    public float walkPitch = 45f;
    public float walkYaw = 0f;
    public bool walkFollowYaw = false;

    [Header("Boat Camera (behind the boat)")]
    public Transform boatForwardRef;
    public float boatHeight = 8f;
    public float boatBack = 10f;
    public float boatPitch = 20f;
    public float boatYawOffset = 0f;

    [Header("Smoothing")]
    public float walkPosSmoothTime = 0.12f;
    public float boatPosSmoothTime = 0.03f;
    public float rotLerpSpeed = 12f;
    public bool boatUseFixedFollow = true;

    private Vector3 velocity;
    private bool followYawRuntime;
    private Vector3 fixedDesiredPos;
    private Quaternion fixedDesiredRot;

    private void Start()
    {
        ApplyModeSettings(mode);
        Snap();
    }

    private void FixedUpdate()
    {
        if (!boatUseFixedFollow) return;
        if (mode != Mode.Boat) return;
        if (target == null) return;

        fixedDesiredPos = ComputeDesiredPosition();
        fixedDesiredRot = ComputeDesiredRotation();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos;
        Quaternion desiredRot;

        if (mode == Mode.Boat && boatUseFixedFollow)
        {
            desiredPos = fixedDesiredPos;
            desiredRot = fixedDesiredRot;
        }
        else
        {
            desiredPos = ComputeDesiredPosition();
            desiredRot = ComputeDesiredRotation();
        }

        float posSmooth = (mode == Mode.Boat) ? boatPosSmoothTime : walkPosSmoothTime;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocity,
            posSmooth,
            Mathf.Infinity,
            Time.deltaTime
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRot,
            1f - Mathf.Exp(-rotLerpSpeed * Time.deltaTime)
        );
    }
    public void SetTarget(Transform newTarget) => target = newTarget;

    public void SetMode(Mode m)
    {
        mode = m;
        ApplyModeSettings(m);
    }

    public void SetBoatForwardRef(Transform forwardRef) => boatForwardRef = forwardRef;

    public void Snap()
    {
        if (target == null) return;
        velocity = Vector3.zero;

        Vector3 p = ComputeDesiredPosition();
        Quaternion r = ComputeDesiredRotation();

        transform.position = p;
        transform.rotation = r;

        fixedDesiredPos = p;
        fixedDesiredRot = r;
    }

    private void ApplyModeSettings(Mode m)
    {
        followYawRuntime = (m == Mode.Walk) ? walkFollowYaw : true;
    }

    private Vector3 GetBoatForward()
    {
        if (boatForwardRef != null) return boatForwardRef.forward;
        return target.forward;
    }

    private float GetBoatYaw()
    {
        if (boatForwardRef != null) return boatForwardRef.eulerAngles.y;
        return target.eulerAngles.y;
    }

    private Vector3 ComputeDesiredPosition()
    {
        if (mode == Mode.Boat)
        {
            Vector3 forward = GetBoatForward();
            Vector3 backDir = -forward;
            return target.position + backDir * boatBack + Vector3.up * boatHeight;
        }
        else
        {
            Vector3 backDir = Quaternion.Euler(0f, walkYaw, 0f) * Vector3.back;
            return target.position + backDir * walkBack + Vector3.up * walkHeight;
        }
    }

    private Quaternion ComputeDesiredRotation()
    {
        float yaw;
        float pitch;

        if (mode == Mode.Boat)
        {
            pitch = boatPitch;
            yaw = GetBoatYaw() + boatYawOffset;
        }
        else
        {
            pitch = walkPitch;
            yaw = followYawRuntime ? target.eulerAngles.y : walkYaw;
        }

        return Quaternion.Euler(pitch, yaw, 0f);
    }
}