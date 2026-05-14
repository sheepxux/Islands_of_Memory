using System.Collections;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform initialRespawnPoint;
    public TopDownCamera topDownCamera;
    public DeathRespawnUI deathRespawnUI;

    [Header("Respawn")]
    public float verticalOffset = 0.05f;
    public float inputLockTime = 0.15f;
    public bool snapCameraOnRespawn = true;
    public bool playDeathSequence = true;
    public float sinkDepth = 1.4f;
    public float sinkDuration = 1.2f;

    private Vector3 respawnPosition;
    private Quaternion respawnRotation = Quaternion.identity;
    private bool hasRespawnPoint;
    private bool isRespawning;

    private CharacterController characterController;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        ResolveReferences();

        if (initialRespawnPoint != null)
            SetRespawnPoint(initialRespawnPoint);
        else if (player != null)
            SetRespawnPoint(player.position, player.rotation);
    }

    public void SetRespawnPoint(Transform point)
    {
        if (point == null) return;
        SetRespawnPoint(point.position, point.rotation);
    }

    public void SetRespawnPoint(Vector3 position, Quaternion rotation)
    {
        respawnPosition = position;
        respawnRotation = rotation;
        hasRespawnPoint = true;
    }

    public void Respawn(GameObject target = null)
    {
        if (isRespawning) return;

        if (target != null)
            player = target.transform;

        ResolveReferences();

        if (player == null || !hasRespawnPoint)
            return;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        bool movementWasEnabled = playerMovement != null && playerMovement.enabled;
        bool controllerWasEnabled = characterController != null && characterController.enabled;

        if (playerMovement != null)
        {
            playerMovement.ResetMotion();
            playerMovement.enabled = false;
        }

        if (characterController != null)
            characterController.enabled = false;

        Coroutine sinkRoutine = null;
        if (sinkDepth > 0f && sinkDuration > 0f)
            sinkRoutine = StartCoroutine(SinkPlayer());

        bool showDeathSequence = playDeathSequence && deathRespawnUI != null && deathRespawnUI.IsReady;

        if (showDeathSequence)
            yield return deathRespawnUI.PlayIntroAndWait();

        if (sinkRoutine != null)
            StopCoroutine(sinkRoutine);

        player.SetParent(null, true);
        player.position = respawnPosition + Vector3.up * verticalOffset;
        player.rotation = respawnRotation;

        if (characterController != null)
            characterController.enabled = controllerWasEnabled;

        if (playerMovement != null)
            playerMovement.ResetMotion();

        if (snapCameraOnRespawn && topDownCamera != null)
        {
            topDownCamera.SetTarget(player);
            topDownCamera.SetMode(TopDownCamera.Mode.Walk);
            topDownCamera.Snap();
        }

        yield return new WaitForSecondsRealtime(inputLockTime);

        if (showDeathSequence)
            yield return deathRespawnUI.PlayOutro();

        if (playerMovement != null)
            playerMovement.enabled = movementWasEnabled;

        isRespawning = false;
    }

    private IEnumerator SinkPlayer()
    {
        if (player == null)
            yield break;

        Vector3 start = player.position;
        Vector3 end = start + Vector3.down * sinkDepth;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, sinkDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = t * t * (3f - 2f * t);
            player.position = Vector3.Lerp(start, end, eased);
            yield return null;
        }
    }

    private void ResolveReferences()
    {
        if (player == null)
        {
            PlayerMovement foundMovement = FindFirstObjectByType<PlayerMovement>();
            if (foundMovement != null)
                player = foundMovement.transform;
        }

        if (topDownCamera == null)
            topDownCamera = FindFirstObjectByType<TopDownCamera>();

        if (deathRespawnUI == null)
            deathRespawnUI = FindFirstObjectByType<DeathRespawnUI>();

        if (player != null)
        {
            characterController = player.GetComponent<CharacterController>();
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }
}
