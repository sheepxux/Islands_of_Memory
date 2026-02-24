using UnityEngine;

public class OldManActionDriver : MonoBehaviour
{
    [Header("Reference")]
    public Animator animator;

    [Header("Animator Param (MUST match your controller)")]
    public string actionParamName = "actionID";

    [Header("Action IDs (confirm these are correct for your pack)")]
    public int idleStand = 11;
    public int walkForward = 21;
    public int runForward = 31;

    private int _actionHash;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        _actionHash = Animator.StringToHash(actionParamName);
    }

    private void Update()
    {
        if (animator == null) return;

        bool forward = Input.GetKey(KeyCode.W);
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (!forward)
        {
            animator.SetInteger(_actionHash, idleStand);
        }
        else
        {
            animator.SetInteger(_actionHash, sprint ? runForward : walkForward);
        }
    }
}