using UnityEngine;

public class AutumnPuzzleStart : MonoBehaviour
{
    public AutumnLeafGameManager leafGameManager;
    public float instructionDelay = 1.5f;

    public void TriggerLeafInstruction()
    {
        if (leafGameManager != null)
            leafGameManager.Invoke(nameof(AutumnLeafGameManager.ShowInstruction), instructionDelay);
    }
}
